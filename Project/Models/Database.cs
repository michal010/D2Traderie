using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using D2Traderie.Project.AppServices;
using D2Traderie.Project.Consts;
using D2Traderie.Project.Models.Tags;
using FuzzySharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace D2Traderie.Project.Models
{
    class Database
    {
        public List<ItemEntity> Items;
        public List<ListingEntity> Listings;
        public ItemTags ItemTags;

        Services services;

        public Database(Services services)
        {
            this.services = services;
            Items = new List<ItemEntity>();
            ItemTags = new ItemTags();
        }

        public async void DownloadAndSaveDatabase()
        {
            DownloadAndSaveItems();
            DonwloadAndSaveItemTags();
        }

        public async void LoadDatabase()
        {
            if (!services.FileService.FileExist(Settings.ItemsFileDataFullName))
                await DownloadAndSaveItems();
            else
                LoadObjectFromJsonFile<List<ItemEntity>>(out Items, Settings.ItemsFileDataFullName);

            if (!services.FileService.FileExist(Settings.ItemTagsDataFullName))
                await DonwloadAndSaveItemTags();
            else
                LoadObjectFromJsonFile<ItemTags>(out ItemTags, Settings.ItemTagsDataFullName);

            await LoadOrDownloadValues();

            services.MainWindow.ItemNames = GetItemNames();
        }

        public async Task RefreshDatabase()
        {
            Items = new List<ItemEntity>();
            await DownloadAndSaveItems();
            await DonwloadAndSaveItemTags();
            services.MainWindow.ItemNames = GetItemNames();
        }

        public async Task DownloadListings(ulong itemID, uint DepthOfSearch, SearchSettings settings = null)
        {
            uint maxPages = DepthOfSearch;
            int pageIndex = 0;
            Listings = new List<ListingEntity>();

            while (true)
            {
                // Przekaż settings do endpointu
                string endpointURL = services.EndpointService.GetListingEndpoint(itemID, pageIndex, settings);
                var response = await services.HttpSerivce.Get(endpointURL);
                string json = await response.Content.ReadAsStringAsync();

                PagedListingEntity pagedListings = Newtonsoft.Json.JsonConvert.DeserializeObject<PagedListingEntity>(json);
                if (pagedListings?.pagedListings != null && pagedListings.pagedListings.Count > 0)
                {
                    Listings.AddRange(pagedListings.pagedListings);
                    services.MainWindow.UpdateLoadingMessage($"Pobrano {Listings.Count} listingów (strona {pageIndex})...");
                }
                else
                    break;

                pageIndex++;
                if (maxPages < pageIndex)
                    break;
            }
            services.MainWindow.Listings = GetListings();
        }

        public async Task DownloadAndSaveItems()
        {
            int pageIndex = 0;
            string json = string.Empty;
            string endpointURL = string.Empty;
            HttpResponseMessage response;
            while (true)
            {
                endpointURL = services.EndpointService.GetItemsEndpoint(pageIndex);
                Console.WriteLine($"Fetching: {endpointURL}");
                response = await services.HttpSerivce.Get(endpointURL);
                Console.WriteLine($"Status: {response.StatusCode}");
                json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response length: {json.Length}, preview: {json.Substring(0, Math.Min(200, json.Length))}");
                PagedItemsEntity pagedItems = JsonConvert.DeserializeObject<PagedItemsEntity>(json);
                if (pagedItems != null && pagedItems.pagedItems.Count > 0)
                {
                    Console.WriteLine($"Page {pageIndex}: got {pagedItems.pagedItems.Count} items");
                    Items.AddRange(pagedItems.pagedItems);
                    services.MainWindow.UpdateLoadingMessage($"Pobrano {Items.Count} itemów...");
                }
                else
                {
                    Console.WriteLine($"No more items at page {pageIndex}, stopping.");
                    break;
                }
                pageIndex++;
            }
            Console.WriteLine($"Total items downloaded: {Items.Count}");
            await ConvertAndSaveDataAsJson<List<ItemEntity>>(Items, Settings.ItemsFileDataFullName);
        }

        public async Task DonwloadAndSaveItemTags()
        {
            services.MainWindow.UpdateLoadingMessage("Pobieranie tagów...");
            string json = string.Empty;
            string endpointURL = services.EndpointService.GetTagsEndpoint();
            HttpResponseMessage response = await services.HttpSerivce.Get(endpointURL);
            json = await response.Content.ReadAsStringAsync();

            ItemTagsRootObject root = JsonConvert.DeserializeObject<ItemTagsRootObject>(json);
            ItemTags = root.Tags;

            await ConvertAndSaveDataAsJson<ItemTags>(ItemTags, Settings.ItemTagsDataFullName);
        }

        private async Task ConvertAndSaveDataAsJson<T>(T obj, string fullName)
        {
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            await services.FileService.SaveToFileAsync<string>(json, fullName);
        }

        public void LoadObjectFromJsonFile<T>(out T obj, string fullName)
        {
            string json = services.FileService.LoadStringFromFile(fullName);
            obj = JsonConvert.DeserializeObject<T>(json);
        }

        public List<string> GetItemNames()
        {
            return Items.Select(o => o.Name).ToList();
        }

        public List<ListingEntity> GetListings()
        {
            return Listings;
        }

        public List<ItemPropertyEntity> GetItemPropertyNumericEntities(ulong itemID)
        {
            var item = Items.Where(i => i.Id == itemID).FirstOrDefault();
            var numericProperties = item.Properties.Where(p => p.Type == "number").ToList();
            return numericProperties;
        }

        public List<ItemPropertyEntity> GetItemPropertyNumericEntities(ItemEntity item)
        {
            var numericProperties = item.Properties.Where(p => p.Type == "number").ToList();
            return numericProperties;
        }

        public ItemEntity GetItem(string itemName)
        {
            return Items.Where(i => i.Name == itemName).FirstOrDefault();
        }

        public ItemEntity GetItem(ulong itemID)
        {
            return Items.Where(i => i.Id == itemID).FirstOrDefault();
        }

        public ulong GetItemID(string itemName)
        {
            var item = Items.Where(i => i.Name == itemName).FirstOrDefault();
            if (item != null)
                return Items.Where(i => i.Name == itemName).FirstOrDefault().Id;
            else
                return default;
        }

        public string GetItemName(uint id) => "";

        public string GetItemImage(ItemEntity item) => item.Img;

        // =====================================================================
        // NOWE: Fuzzy matching OCR właściwości do propertiów itemu
        // =====================================================================

        /// <summary>
        /// Dopasowuje właściwości odczytane przez OCR do znanych propertiów itemu
        /// używając fuzzy matchingu (tolerancja błędów OCR).
        /// Zwraca listę gotowych filtrów z ustawionymi wartościami Min.
        /// </summary>
        public List<ItemPropertyEntity> MatchDetectedPropertiesToItem(
            ItemEntity item,
            List<DetectedProperty> detectedProperties)
        {
            var numericProps = GetItemPropertyNumericEntities(item);

            // Baza wyjściowa — wszystkie properties z null (użytkownik uzupełnia ręcznie)
            var result = numericProps.Select(p => new ItemPropertyEntity
            {
                Property = p.Property,
                PropertyId = p.PropertyId,
                Type = p.Type,
                Min = null,
                Max = null
            }).ToList();

            if (detectedProperties == null || detectedProperties.Count == 0)
                return result;

            // Znormalizowane nazwy propertiów z bazy do fuzzy comparisona
            var normalizedKnown = numericProps.Select(p => new
            {
                Original = p,
                Normalized = NormalizePropertyForMatching(p.Property)
            }).ToList();

            foreach (var detected in detectedProperties)
            {
                string detectedNorm = NormalizePropertyForMatching(detected.RawText);
                if (string.IsNullOrWhiteSpace(detectedNorm)) continue;

                // Fuzzy match — szukamy najbliższego dopasowania
                int bestScore = 0;
                string bestPropKey = null;

                foreach (var known in normalizedKnown)
                {
                    // PartialRatio dobrze radzi sobie z przypadkami gdzie OCR
                    // odczytał fragment właściwości lub przestawił wyrazy
                    int score = Fuzz.PartialRatio(detectedNorm, known.Normalized);

                    // TokenSortRatio jako drugi scorer — lepszy gdy wyrazy poprzestawiane
                    int tokenScore = Fuzz.TokenSortRatio(detectedNorm, known.Normalized);

                    int combinedScore = Math.Max(score, tokenScore);

                    if (combinedScore > bestScore)
                    {
                        bestScore = combinedScore;
                        bestPropKey = known.Original.Property;
                    }
                }

                // Próg 62 — toleruje typowe błędy OCR (zamienione litery, brakujące znaki)
                if (bestScore < 62 || bestPropKey == null)
                {
                    Console.WriteLine($"[OCR Match] BRAK dopasowania: '{detected.RawText}' → norm: '{detectedNorm}' (best: {bestScore})");
                    continue;
                }

                Console.WriteLine($"[OCR Match] '{detected.RawText}' → '{bestPropKey}' (score: {bestScore}, value: {detected.Value})");

                var target = result.FirstOrDefault(r => r.Property == bestPropKey);
                if (target != null)
                {
                    // Min = odczytana wartość (szukamy "co najmniej tyle")
                    // Max = null (nie ograniczamy z góry)
                    target.Min = (uint)Math.Abs(detected.Value);
                    target.Max = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Normalizuje template property do prostego tekstu do fuzzy porównania.
        /// "{{value}}% Enhanced Defense" → "enhanced defense"
        /// "+{{value}} to Fire Resistance" → "to fire resistance"
        /// </summary>
        private string NormalizePropertyForMatching(string property)
        {
            if (string.IsNullOrEmpty(property)) return "";

            // Usuń placeholdery {{value}}, {{min}}, itp.
            string norm = Regex.Replace(property, @"\{\{[^}]+\}\}", "");

            // Usuń znaki specjalne (%, +, -, nawiasy itd.)
            norm = Regex.Replace(norm, @"[^a-zA-Z\s]", " ");

            // Zwiń wielokrotne spacje
            norm = Regex.Replace(norm, @"\s+", " ").Trim().ToLowerInvariant();

            return norm;
        }


        #region Values
        public async Task LoadOrDownloadValues()
        {
            if (!services.FileService.FileExist(Settings.ValuesFileDataFullName))
                await DownloadAndSaveValues();
            else
                ApplyValuesFromFile();
        }

        public async Task DownloadAndSaveValues()
        {
            services.MainWindow.UpdateLoadingMessage("Pobieranie wartości walut...");

            var response = await services.HttpSerivce.Get(Settings.ValuesEndpoint);
            string json = await response.Content.ReadAsStringAsync();

            // Zapisz surowy JSON
            await services.FileService.SaveToFileAsync<string>(json, Settings.ValuesFileDataFullName);

            // Zastosuj do RuneValues
            ApplyValuesFromJson(json);

            Console.WriteLine("[Values] Pobrano i zastosowano wartości walut.");
        }

        public async Task RefreshValues()
        {
            await DownloadAndSaveValues();
        }

        private void ApplyValuesFromFile()
        {
            try
            {
                string json = services.FileService.LoadStringFromFile(Settings.ValuesFileDataFullName);
                ApplyValuesFromJson(json);
                Console.WriteLine("[Values] Załadowano wartości walut z pliku.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Values] Błąd ładowania z pliku: {ex.Message}");
            }
        }

        private void ApplyValuesFromJson(string json)
        {
            var root = Newtonsoft.Json.JsonConvert.DeserializeObject<D2Traderie.Project.Models.ItemValuesRoot>(json);
            if (root?.Prices == null) return;

            // Przelicz user_value (float w Berach) → ulong (* 10000)
            // i nadpisz słownik RuneValues
            var newValues = new Dictionary<string, ulong>();
            foreach (var entry in root.Prices)
            {
                if (string.IsNullOrEmpty(entry.Name)) continue;
                ulong val = (ulong)System.Math.Round(entry.UserValue * 10000);
                if (val == 0) val = 1; // minimum 1 żeby nie było 0
                newValues[entry.Name] = val;
            }

            // Nadpisz statyczny słownik
            D2Traderie.Project.Consts.RunesValue.RuneValues = newValues;

            // Zaktualizuj listę w UI (Runes property jest teraz .Keys.ToList())
            services.MainWindow.Dispatcher.Invoke(() =>
            {
                services.MainWindow.Runes = D2Traderie.Project.Consts.RunesValue.Runes;
            });

            Console.WriteLine($"[Values] Zastosowano {newValues.Count} wartości walut.");
        }
        #endregion
    }
}