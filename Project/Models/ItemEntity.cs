using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.Models
{
    class ItemEntity
    {
        [JsonProperty("id")]
        public uint Id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("buy_price")]
        public string BuyPrice { get; set; }
        [JsonProperty("img")]
        public string Img { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("active")]
        public bool Active { get; set; }
        [JsonProperty("mode")]
        public string Mode { get; set; }
        [JsonProperty("unlocked_at")]
        public string UnlockedAt { get; set; }
        [JsonProperty("created_at")]
        public string CreatedAt{ get; set; }
        [JsonProperty("name")]
        public string Name{ get; set; }
        [JsonProperty("properties")]
        public List<ItemPropertyEntity> Properties { get; set; }
    }
}
