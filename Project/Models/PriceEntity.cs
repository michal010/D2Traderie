using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.Models
{
    public class PriceEntity
    {
        [JsonProperty("img")]
        public string Img { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("group")]
        public int Group { get; set; } //grupa oferty tj. np um + mal będą w jednej grupie, or Ber, np. będzie w grupie innej
        [JsonProperty("item_id")]
        public uint ItemID { get; set; }
        [JsonProperty("quantity")]
        public uint Quantity { get; set; }
        [JsonProperty("listing_id")]
        public uint ListingID { get; set; }
        //[JsonProperty("properties")]
        //public PropertiesEntity { get; set; }
        [JsonProperty("variant_id")]
        public uint? VariantID{ get; set; }
        [JsonProperty("variant_image")]
        public string? VariantImage{ get; set; }
        [JsonProperty("variant_name")]
        public string? VariantName{ get; set; }
    }
}
