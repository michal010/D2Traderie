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

            services.MainWindow.ItemNames = GetItemNames();
            //LoadObjectFromJsonFile<ItemTags>(out ItemTags, Settings.ItemTagsDataFullName);
        }

        public async Task DownloadListings(uint itemID, uint DepthOfSearch) //additional options to be passed.
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
                    Listings.AddRange(pagedListings.pagedListings);
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
            //jakies zabezpieczenie przed spamem.
            int pageIndex = 0;
            string json = string.Empty;
            string endpointURL = string.Empty;
            HttpResponseMessage response;
            while(true)
            {
                endpointURL = services.EndpointService.GetItemsEndpoint(pageIndex);
                response = await services.HttpSerivce.Get(endpointURL);
                json = await response.Content.ReadAsStringAsync();
                PagedItemsEntity pagedItems = new PagedItemsEntity();
                pagedItems = JsonConvert.DeserializeObject<PagedItemsEntity>(json);
                if (pagedItems != null && pagedItems.pagedItems.Count > 0)
                    Items.AddRange(pagedItems.pagedItems);
                else
                    break;
                pageIndex++;
            }
            await ConvertAndSaveDataAsJson<List<ItemEntity>>(Items, Settings.ItemsFileDataFullName);
        }

        public async void DonwloadAndSaveItemTags()
        {
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

        public List<ItemPropertyEntity> GetItemPropertyNumericEntities(uint itemID)
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

        public ItemEntity GetItem(uint itemID)
        {
            return Items.Where(i => i.Id == itemID).FirstOrDefault();
        }

        public uint GetItemID(string itemName)
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
