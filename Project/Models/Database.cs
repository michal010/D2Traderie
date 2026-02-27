using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using D2Traderie.Project.AppServices;
using D2Traderie.Project.Consts;
using D2Traderie.Project.Models.Tags;
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

            // DODAJ:
            if (!services.FileService.FileExist(Settings.ItemTagsDataFullName))
                await DonwloadAndSaveItemTags();
            else
                LoadObjectFromJsonFile<ItemTags>(out ItemTags, Settings.ItemTagsDataFullName);

            services.MainWindow.ItemNames = GetItemNames();
        }
        public async Task RefreshDatabase()
        {
            Items = new List<ItemEntity>();
            await DownloadAndSaveItems();
            await DonwloadAndSaveItemTags();
            services.MainWindow.ItemNames = GetItemNames();
        }


        public async Task DownloadListings(ulong itemID, uint DepthOfSearch) //additional options to be passed.
        {
            uint maxPages = DepthOfSearch;
            int pageIndex = 0;
            string json = string.Empty;
            string endpointURL = string.Empty;
            HttpResponseMessage response;
            Listings = new List<ListingEntity>();
            while (true)
            {
                endpointURL = services.EndpointService.GetListingEndpoint(itemID, pageIndex);
                response = await services.HttpSerivce.Get(endpointURL);
                json = await response.Content.ReadAsStringAsync();
                PagedListingEntity pagedListings = new PagedListingEntity();
                pagedListings = JsonConvert.DeserializeObject<PagedListingEntity>(json);
                if (pagedListings != null && pagedListings.pagedListings.Count > 0)
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
            Console.WriteLine();
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
                Console.WriteLine($"Fetching: {endpointURL}"); // <- dodaj
                response = await services.HttpSerivce.Get(endpointURL);
                Console.WriteLine($"Status: {response.StatusCode}"); // <- dodaj
                json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response length: {json.Length}, preview: {json.Substring(0, Math.Min(200, json.Length))}"); // <- dodaj
                PagedItemsEntity pagedItems = JsonConvert.DeserializeObject<PagedItemsEntity>(json);
                if (pagedItems != null && pagedItems.pagedItems.Count > 0)
                {
                    Console.WriteLine($"Page {pageIndex}: got {pagedItems.pagedItems.Count} items"); // <- dodaj
                    Items.AddRange(pagedItems.pagedItems);
                    services.MainWindow.UpdateLoadingMessage($"Pobrano {Items.Count} itemów...");
                }
                else
                {
                    Console.WriteLine($"No more items at page {pageIndex}, stopping."); // <- dodaj
                    break;
                }
                pageIndex++;
            }
            Console.WriteLine($"Total items downloaded: {Items.Count}"); // <- dodaj
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
            //Add filtering options
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
            {
                return Items.Where(i => i.Name == itemName).FirstOrDefault().Id;
            }
            else
                return default;
        }

        public string GetItemName(uint id)
        {
            return "";
        }

        public string GetItemImage(ItemEntity item)
        {
            return item.Img;
        }
    }
}
