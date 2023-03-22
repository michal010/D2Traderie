using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.Models.Tags
{
    class ItemTagEntity
    {
        [JsonProperty("tag_id")]
        public string TagID { get; set; }
        [JsonProperty("tag")]
        public string Tag { get; set; }
        [JsonProperty("category")]
        public string Category { get; set; }
        [JsonProperty("format")]
        public string? Format { get; set; }

    }
}
