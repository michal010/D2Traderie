using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;
using FuzzySharp.SimilarityRatio;
using System;
using System.Collections.Generic;
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

    public class DetectedProperty
    {
        public string RawText { get; set; }
        public int Value { get; set; }
    }

    public class ItemOcrResult
    {
        public string ItemName { get; set; }
        public bool Success { get; set; }
        public string RawText { get; set; }
        public List<DetectedProperty> DetectedProperties { get; set; } = new();
    }

    class OcrService
    {
        private Services services;
        private string tessDataPath;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }

        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_ALT = 0x0001;
        private const int HOTKEY_ID = 9000;
        private const uint VK_D = 0x44;

        // Wycinamy pas ekranu: od (kursor - 15% szerokosci ekranu) do (kursor + 15% szerokosci ekranu)
        // Wysokosc: caly ekran (od 0 do height) - tooltip moze byc przy gornej lub dolnej krawedzi
        private const double MARGIN_RATIO = 0.15; // 15% szerokosci ekranu na kazda strone kursora

        public OcrService(Services services)
        {
            this.services = services;
            tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "tessdata");
        }

        public void RegisterHotkey(IntPtr windowHandle) =>
            RegisterHotKey(windowHandle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, VK_D);

        public void UnregisterHotkey(IntPtr windowHandle) =>
            UnregisterHotKey(windowHandle, HOTKEY_ID);

        public bool IsOurHotkey(int id) => id == HOTKEY_ID;

        public ItemOcrResult CaptureAndRecognize()
        {
            try
            {
                // Pobierz aktualna pozycje kursora
                GetCursorPos(out POINT cursor);

                var screen = WinFormsScreen.PrimaryScreen.Bounds;

                // X: kursor - 15% szerokosci ekranu ... kursor + 15% szerokosci ekranu
                int margin = (int)(screen.Width * MARGIN_RATIO);
                int captureX = Math.Max(0, cursor.X - margin);
                int captureRight = Math.Min(screen.Width, cursor.X + margin);
                int captureWidth = captureRight - captureX;

                // Y: caly ekran - tooltip moze byc przy gornej lub dolnej krawedzi
                int captureY = 0;
                int captureHeight = screen.Height;

                Console.WriteLine($"[OCR] Kursor: ({cursor.X}, {cursor.Y}), Capture: x={captureX}..{captureRight} ({captureWidth}px), pelna wysokosc ekranu");

                using var captureBitmap = new DrawingBitmap(captureWidth, captureHeight, DrawingPixelFormat.Format32bppArgb);
                using (var graphics = DrawingGraphics.FromImage(captureBitmap))
                {
                    graphics.CopyFromScreen(
                        new DrawingPoint(captureX, captureY),
                        DrawingPoint.Empty,
                        new System.Drawing.Size(captureWidth, captureHeight));
                }

                captureBitmap.Save(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_screenshot.png"),
                    DrawingImageFormat.Png);

                return RunOcr(captureBitmap);
            }
            catch (Exception ex)
            {
                return new ItemOcrResult { Success = false, RawText = ex.Message };
            }
        }

        private ItemOcrResult RunOcr(Bitmap bitmap)
        {
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
            Console.WriteLine($"[OCR] Raw text:\n{rawText}");

            string itemName = ExtractItemName(rawText);
            string matchedName = MatchToKnownItem(itemName);
            var detectedProps = ExtractProperties(rawText);

            return new ItemOcrResult
            {
                Success = matchedName != null,
                ItemName = matchedName ?? itemName,
                RawText = rawText,
                DetectedProperties = detectedProps
            };
        }

        private Bitmap PreprocessForOcr(Bitmap original)
        {
            var scaled = new Bitmap(original.Width * 2, original.Height * 2);
            using (var g = Graphics.FromImage(scaled))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(original, 0, 0, original.Width * 2, original.Height * 2);
            }

            for (int x = 0; x < scaled.Width; x++)
            {
                for (int y = 0; y < scaled.Height; y++)
                {
                    var c = scaled.GetPixel(x, y);
                    float brightness = c.GetBrightness();
                    scaled.SetPixel(x, y, brightness > 0.3f ? DrawingColor.Black : DrawingColor.White);
                }
            }
            return scaled;
        }

        private List<DetectedProperty> ExtractProperties(string ocrText)
        {
            var result = new List<DetectedProperty>();
            if (string.IsNullOrWhiteSpace(ocrText)) return result;

            var lines = ocrText
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => l.Length >= 4)
                .Skip(1)
                .ToArray();

            var simplePattern = new Regex(@"^[+\-]?(\d+)[%+\s]*(.{4,})$");
            var addsPattern = new Regex(@"[Aa]dds\s+\d+[-\s]+(\d+)\s+(.+?)\s*[Dd]amage", RegexOptions.IgnoreCase);
            var chargesPattern = new Regex(@"[Ll]evel\s+(\d+)\s+(.+?)\s*\(", RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                var addsMatch = addsPattern.Match(line);
                if (addsMatch.Success && int.TryParse(addsMatch.Groups[1].Value, out int maxDmg))
                {
                    result.Add(new DetectedProperty
                    {
                        RawText = $"Adds {addsMatch.Groups[2].Value.Trim()} Damage",
                        Value = maxDmg
                    });
                    continue;
                }

                var chargesMatch = chargesPattern.Match(line);
                if (chargesMatch.Success && int.TryParse(chargesMatch.Groups[1].Value, out int lvl))
                {
                    result.Add(new DetectedProperty
                    {
                        RawText = chargesMatch.Groups[2].Value.Trim(),
                        Value = lvl
                    });
                    continue;
                }

                var simpleMatch = simplePattern.Match(line);
                if (simpleMatch.Success && int.TryParse(simpleMatch.Groups[1].Value, out int val))
                {
                    string propText = simpleMatch.Groups[2].Value.Trim();
                    if (propText.Length >= 4 && propText.Any(char.IsLetter))
                        result.Add(new DetectedProperty { RawText = propText, Value = val });
                }
            }

            Console.WriteLine($"[OCR] Wykryte wlasciwosci ({result.Count}):");
            foreach (var p in result)
                Console.WriteLine($"  '{p.RawText}' = {p.Value}");

            return result;
        }

        private string ExtractItemName(string ocrText)
        {
            if (string.IsNullOrWhiteSpace(ocrText)) return null;

            return ocrText
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => l.Length >= 5 && l.Any(char.IsLetter))
                .FirstOrDefault();
        }

        private string MatchToKnownItem(string ocrItemName)
        {
            if (string.IsNullOrWhiteSpace(ocrItemName)) return null;

            var knownNames = services.Database.GetItemNames();
            if (knownNames == null || !knownNames.Any()) return null;

            ocrItemName = ocrItemName.Trim();

            var exact = knownNames.FirstOrDefault(n =>
                string.Equals(n, ocrItemName, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;

            if (ocrItemName.Length < 5) return null;

            var bestMatch = Process.ExtractOne(ocrItemName, knownNames, cutoff: 72);
            if (bestMatch != null && bestMatch.Score >= 72)
            {
                Console.WriteLine($"[OCR] Fuzzy match: '{ocrItemName}' -> '{bestMatch.Value}' (score: {bestMatch.Score})");
                return bestMatch.Value;
            }

            return null;
        }
    }
}