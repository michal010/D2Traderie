using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.Models
{
    public class ItemPropertyEntity
    {
        [JsonProperty("property_id")]
        public ulong PropertyId { get; set; }

        [JsonProperty("property")]
        public string Property { get; set; }

        [JsonProperty("img")]
        public string? Img { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("required")]
        public bool? Required { get; set; }

        [JsonProperty("min")]
        public uint? Min { get; set; }

        [JsonProperty("max")]
        public uint? Max { get; set; }

        [JsonIgnore]
        public string DisplayName => Property?.Replace("{{value}}", "#") ?? "";
    }
}
