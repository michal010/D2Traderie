using D2Traderie.Project.Consts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.Models
{
    public class ListingEntity
    {
        [JsonProperty("id")]
        public uint Id { get; set; }
        [JsonProperty("seller_id")]
        public string SellerID{ get; set; }
        [JsonProperty("amount")]
        public string Amount { get; set; }
        [JsonProperty("completed")]
        public bool? Completed{ get; set; }
        [JsonProperty("active")]
        public bool? Active{ get; set; }
        [JsonProperty("item_id")]
        public uint ItemID { get; set; }
        [JsonProperty("variant_id")]
        public uint? VariantID { get; set; }
        [JsonProperty("selling")]
        public bool? Selling{ get; set; }
        [JsonProperty("make_offer")]
        public bool? MakeOffer{ get; set; }
        [JsonProperty("accept_listing_price")]
        public bool? AcceptListingPrice{ get; set; }
        [JsonProperty("updated_at")]
        public string UpdatedAt{ get; set; }
        [JsonProperty("wishlist_id")]
        public string WhishlistID{ get; set; }
        [JsonProperty("offer_wishlist")]
        public bool? OfferWhishlist{ get; set; }
        [JsonProperty("offer_wishlist_id")]
        public string OfferWishlistId{ get; set; }
        [JsonProperty("prices")]
        public List<PriceEntity> Prices { get; set; }
        [JsonProperty("properties")]
        public List<PropertyEntity> Properties { get; set; }
        [JsonProperty("standing")]
        public bool? standing { get; set; }
        [JsonProperty("stock")]
        public bool? Stock { get; set; }
        //[JsonProperty("seller")]
        //public SellerEntity Seller { get; set; }

        [JsonIgnore()]
        public string PriceGroups { get; set; }

        [JsonIgnore()]
        public string LisingURL
        {
            get
            {
                return String.Format("https://traderie.com/diablo2resurrected/listing/" + Id);
            }
        }

        [JsonIgnore()]
        public List<PropertyEntity> NumericProperties 
        { 
            get
            {
                if (Properties == null || Properties.Count < 1)
                    return null;

                return Properties.Where(p => p.Type == "number").ToList();
            }
        }

        [OnDeserialized]
        internal void ConvertPricesToGroupsString(StreamingContext context)
        {
            if (Prices == null || Prices.Count < 1)
            {
                PriceGroups = "Make offers";
            }

            StringBuilder sb = new StringBuilder();
            int groupIndex = 0;
            List<PriceEntity> priceGroup = new List<PriceEntity>();
            priceGroup = GetPriceGroup(groupIndex);

            while (priceGroup != null && priceGroup.Count > 0)
            {
                if (groupIndex != 0)
                    sb.Append(" OR \n");
                foreach (var price in priceGroup)
                {
                    sb.Append(String.Format("{0} X {1} \n", price.Quantity, price.Name));
                }
                sb.Remove(sb.Length - 1, 1);
                groupIndex++;
                priceGroup = GetPriceGroup(groupIndex);
            }
            PriceGroups = sb.ToString();
        }

        public List<PriceEntity> GetPriceGroup(int groupIndex)
        {
            return Prices.Where(p => p.Group == groupIndex).ToList();
        }

        public List<ulong> GetPriceValues()
        {
            List<ulong> result = new List<ulong>();
            for (int i = 0; i < Prices.Count; i++)
            {
                ulong value = ulong.MaxValue;
                var prices = GetPriceGroup(i);
                foreach (var p in prices)
                {
                    if(RunesValue.RuneValues.ContainsKey(p.Name))
                    {
                        ulong RuneValue = RunesValue.RuneValues[p.Name];
                        value += RuneValue * p.Quantity;
                    }
                }
                if(value != ulong.MaxValue)
                    result.Add(value);
            }
            return result;
        }
    }
}
