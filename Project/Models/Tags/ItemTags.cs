using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.Models.Tags
{
    [JsonObject(Title = "tags")]
    class ItemTags
    {
        [JsonProperty("Weapon Type")]
        public List<ItemTagEntity> WeaponTypeTags { get; set; }
        [JsonProperty("Skills")]
        public List<ItemTagEntity> SkillTags { get; set; }
        [JsonProperty("Item Type")]
        public List<ItemTagEntity> ItemTypeTags { get; set; }
        [JsonProperty("Body Location")]
        public List<ItemTagEntity> BodyLocationTags { get; set; }
        [JsonProperty("Craft Type")]
        public List<ItemTagEntity> CraftTypeTags { get; set; }
        [JsonProperty("Gem Type")]
        public List<ItemTagEntity> GemTypeTags { get; set; }
        [JsonProperty("Tier")]
        public List<ItemTagEntity> TierTags { get; set; }
    }
}
