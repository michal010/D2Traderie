using Newtonsoft.Json;

namespace D2Traderie.Project.Models
{
    /// <summary>
    /// Pojedynczy wpis z API /items/values — interesuje nas tylko name i user_value.
    /// </summary>
    public class ItemValueEntity
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("user_value")]
        public double UserValue { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("item_id")]
        public string ItemId { get; set; }
    }

    public class ItemValuesRoot
    {
        [JsonProperty("prices")]
        public System.Collections.Generic.List<ItemValueEntity> Prices { get; set; }
    }
}