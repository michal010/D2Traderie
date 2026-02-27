using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;
using FuzzySharp.SimilarityRatio;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Tesseract;
using FuzzySharp;

namespace D2Traderie.Project.AppServices
{
    using DrawingColor = System.Drawing.Color;
    using DrawingBitmap = System.Drawing.Bitmap;
    using DrawingRectangle = System.Drawing.Rectangle;
    using DrawingPoint = System.Drawing.Point;
    using DrawingPixelFormat = System.Drawing.Imaging.PixelFormat;
    using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
    using DrawingGraphics = System.Drawing.Graphics;
    using WinFormsScreen = System.Windows.Forms.Screen;

    public class ItemOcrResult
    {
        public string ItemName { get; set; }
        public bool Success { get; set; }
        public string RawText { get; set; }
    }

    class OcrService
    {
        private Services services;
        private string tessDataPath;

        // Win32 API do globalnego hotkey
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Modifiers
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_ALT = 0x0001;
        private const int HOTKEY_ID = 9000;
        private const uint VK_D = 0x44;

        public OcrService(Services services)
        {
            this.services = services;
            tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "tessdata");
        }

        public void RegisterHotkey(IntPtr windowHandle)
        {
            RegisterHotKey(windowHandle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, VK_D);
        }

        public void UnregisterHotkey(IntPtr windowHandle)
        {
            UnregisterHotKey(windowHandle, HOTKEY_ID);
        }

        public bool IsOurHotkey(int id) => id == HOTKEY_ID;

        public ItemOcrResult CaptureAndRecognize()
        {
            try
            {
                var screen = WinFormsScreen.PrimaryScreen.Bounds;
                using var fullBitmap = new DrawingBitmap(screen.Width, screen.Height, DrawingPixelFormat.Format32bppArgb);
                using var graphics = DrawingGraphics.FromImage(fullBitmap);
                graphics.CopyFromScreen(DrawingPoint.Empty, DrawingPoint.Empty, screen.Size);

                var tooltipRect = FindTooltipFast(fullBitmap);

                if (tooltipRect == null)
                {
                    fullBitmap.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_screenshot.png"), DrawingImageFormat.Png);
                    return new ItemOcrResult { Success = false, RawText = "Nie znaleziono tooltipa" };
                }

                using var tooltipBitmap = fullBitmap.Clone(tooltipRect.Value, DrawingPixelFormat.Format32bppArgb);
                tooltipBitmap.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_screenshot.png"), DrawingImageFormat.Png);

                return RunOcr(tooltipBitmap);
            }
            catch (Exception ex)
            {
                return new ItemOcrResult { Success = false, RawText = ex.Message };
            }
        }

        private unsafe DrawingRectangle? FindTooltipFast(DrawingBitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new DrawingRectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                DrawingPixelFormat.Format32bppArgb);

            int width = bitmap.Width;
            int height = bitmap.Height;
            int stride = bitmapData.Stride;
            byte* ptr = (byte*)bitmapData.Scan0;

            // Ignoruj 15% ekranu od lewej i prawej — tam jest UI gry (ramki, paski)
            int marginX = (int)(width * 0.15);
            int marginY = (int)(height * 0.15);

            int bestX = -1, bestY = -1;
            int bestScore = 0;

            // Szukaj obszaru który wygląda jak tooltip:
            // - ciemne tło przez kilka pikseli z rzędu
            // - potem jasny tekst (biały/złoty/zielony)
            for (int y = marginY; y < height - marginY; y += 2)
            {
                for (int x = marginX; x < width - marginX; x += 2)
                {
                    byte* pixel = ptr + y * stride + x * 4;
                    byte b = pixel[0];
                    byte g = pixel[1];
                    byte r = pixel[2];

                    // Sprawdź czy to może być nazwa itemu
                    // Złoty (rare): R>180, G>120, B<100
                    // Zielony (set): R<80, G>150, B<80  
                    // Biały (normal): R>200, G>200, B>200
                    bool isItemNameColor =
                        (r > 180 && g > 120 && b < 100) ||  // złoty
                        (r < 80 && g > 150 && b < 80) ||     // zielony
                        (r > 200 && g > 200 && b > 200);      // biały

                    if (!isItemNameColor) continue;

                    // Sprawdź czy nad tym pikselem jest ciemne tło tooltipa
                    // (co najmniej 5 pikseli ciemnych z rzędu powyżej)
                    int darkCount = 0;
                    for (int dy = 1; dy <= 8; dy++)
                    {
                        if (y - dy < 0) break;
                        byte* above = ptr + (y - dy) * stride + x * 4;
                        if (above[0] < 25 && above[1] < 25 && above[2] < 25)
                            darkCount++;
                    }

                    if (darkCount < 4) continue;

                    // Sprawdź czy w okolicy jest więcej pikseli w tym samym kolorze
                    // (żeby uniknąć pojedynczych pikseli)
                    int score = 0;
                    for (int dx = -10; dx <= 10; dx += 2)
                    {
                        if (x + dx < 0 || x + dx >= width) continue;
                        byte* nearby = ptr + y * stride + (x + dx) * 4;
                        bool nearbyMatch =
                            (nearby[2] > 180 && nearby[1] > 120 && nearby[0] < 100) ||
                            (nearby[2] < 80 && nearby[1] > 150 && nearby[0] < 80) ||
                            (nearby[2] > 200 && nearby[1] > 200 && nearby[0] > 200);
                        if (nearbyMatch) score++;
                    }

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestX = x;
                        bestY = y;
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);

            if (bestX == -1 || bestScore < 3) return null;

            // Tooltip zaczyna się trochę nad znalezionym tekstem
            int tooltipTop = Math.Max(0, bestY - 180);
            // Cofnij się do lewej krawędzi tooltipa (szukaj ciemnego obszaru)
            int tooltipLeft = Math.Max(marginX, bestX - 250);
            int tooltipWidth = 650;
            int tooltipHeight = 650;

            // Upewnij się że nie wychodzimy poza ekran
            tooltipLeft = Math.Min(tooltipLeft, width - tooltipWidth);
            tooltipTop = Math.Min(tooltipTop, height - tooltipHeight);

            return new DrawingRectangle(
                Math.Max(0, tooltipLeft),
                Math.Max(0, tooltipTop),
                tooltipWidth,
                tooltipHeight);
        }

        private bool IsTooltipStartPixel(Bitmap bmp, int x, int y)
        {
            // Sprawdź czy piksel jest ciemny (tło tooltipa D2R)
            var color = bmp.GetPixel(x, y);
            bool isDark = color.R < 30 && color.G < 30 && color.B < 30;
            if (!isDark) return false;

            // Sprawdź czy gdzieś blisko jest złoty/żółty tekst (nazwa itemu)
            for (int dy = 0; dy < 30; dy++)
            {
                if (y + dy >= bmp.Height) break;
                for (int dx = 0; dx < 200; dx++)
                {
                    if (x + dx >= bmp.Width) break;
                    var c = bmp.GetPixel(x + dx, y + dy);
                    // Złoty kolor D2R: R > 180, G > 140, B < 80
                    if (c.R > 180 && c.G > 140 && c.B < 80)
                        return true;
                }
            }
            return false;
        }

        private int FindTooltipWidth(Bitmap bmp, int startX, int startY)
        {
            int maxWidth = 500;
            for (int x = startX; x < Math.Min(startX + maxWidth, bmp.Width); x++)
            {
                var color = bmp.GetPixel(x, startY);
                if (color.R > 50 || color.G > 50 || color.B > 50)
                    return x - startX;
            }
            return maxWidth;
        }

        private int FindTooltipHeight(Bitmap bmp, int startX, int startY)
        {
            int maxHeight = 600;
            for (int y = startY; y < Math.Min(startY + maxHeight, bmp.Height); y++)
            {
                var color = bmp.GetPixel(startX, y);
                if (color.R > 50 || color.G > 50 || color.B > 50)
                    return y - startY;
            }
            return maxHeight;
        }

        private ItemOcrResult RunOcr(Bitmap bitmap)
        {
            // Preprocessing — zwiększ kontrast dla lepszego OCR
            var processed = PreprocessForOcr(bitmap);

            using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
            engine.SetVariable("tessedit_char_whitelist",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 +-%'");

            using var memStream = new MemoryStream();
            processed.Save(memStream, DrawingImageFormat.Png);
            memStream.Position = 0;

            using var pix = Pix.LoadFromMemory(memStream.ToArray());
            using var page = engine.Process(pix);

            string rawText = page.GetText();
            string itemName = ExtractItemName(rawText);

            // Dopasuj do znanych nazw itemów
            string matchedName = MatchToKnownItem(itemName);

            return new ItemOcrResult
            {
                Success = matchedName != null,
                ItemName = matchedName ?? itemName,
                RawText = rawText
            };
        }

        private Bitmap PreprocessForOcr(Bitmap original)
        {
            var result = new Bitmap(original.Width * 2, original.Height * 2);
            using var g = Graphics.FromImage(result);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.DrawImage(original, 0, 0, original.Width * 2, original.Height * 2);

            // Zwiększ kontrast - zamień ciemne tło na białe, tekst na czarny
            for (int x = 0; x < result.Width; x++)
            {
                for (int y = 0; y < result.Height; y++)
                {
                    var c = result.GetPixel(x, y);
                    // Jasny tekst → czarny
                    float brightness = c.GetBrightness();
                    result.SetPixel(x, y, brightness > 0.3f
                        ? DrawingColor.Black
                        : DrawingColor.White);
                }
            }
            return result;
        }

        private string ExtractItemName(string ocrText)
        {
            if (string.IsNullOrWhiteSpace(ocrText)) return null;

            var lines = ocrText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(l => l.Trim())
                               .Where(l => l.Length >= 5 && l.Any(char.IsLetter)) // minimum 5 znaków + co najmniej 1 litera
                               .ToArray();

            if (lines.Length == 0) return null;

            // często nazwa jest pierwsza i składa się głównie z wielkich liter
            return lines[0];
        }

        private string MatchToKnownItem(string ocrItemName)
        {
            if (string.IsNullOrWhiteSpace(ocrItemName))
                return null;

            var knownNames = services.Database.GetItemNames();
            if (knownNames == null || !knownNames.Any())
                return null;

            ocrItemName = ocrItemName.Trim();

            // Najpierw dokładne dopasowanie (case-insensitive)
            var exact = knownNames.FirstOrDefault(n =>
                string.Equals(n, ocrItemName, StringComparison.OrdinalIgnoreCase));

            if (exact != null)
                return exact;

            // Jeśli OCR dał za krótki tekst – nie próbujemy fuzzy (blokuje "A", "re", "of" itp.)
            if (ocrItemName.Length < 5)
                return null;

            // Szukamy najlepszego dopasowania z progiem ~75–82%
            var bestMatch = Process.ExtractOne(
               ocrItemName,
               knownNames,
               cutoff: 72
               );

            if (bestMatch != null && bestMatch.Score >= 72)
            {
                // opcjonalnie – logujemy do debugu
                Console.WriteLine($"Fuzzy match: {ocrItemName} → {bestMatch.Value} (score: {bestMatch.Score})");
                return bestMatch.Value;
            }

            return null; // nic sensownego nie znaleziono
        }
    }
}