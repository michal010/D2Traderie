using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.Models
{
    class PagedListingEntity
    {
        [JsonProperty("listings")]
        public List<ListingEntity> pagedListings { get; set; }
    }
}
