using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.Models.Tags
{
    class ItemTagsRootObject
    {
        [JsonProperty("tags")]
        public ItemTags Tags { get; set;}
    }
}
