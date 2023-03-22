using D2Traderie.Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    public enum FilterOption { Null, Lower, Higher, Equal, LowerOrEqual, HigherOrEqual}

    class ItemFilterService
    {
        Services services;
        
        public ItemFilterService(Services services)
        {
            this.services = services;
        }

        public List<ListingEntity> GetFilteredListings(List<ListingEntity> listings, List<ItemPropertyEntity> itemProperties , FilterOption option, ulong RuneValue)
        {
            List<ListingEntity> filteredListings = listings;

            switch(option)
            {
                case FilterOption.Equal:
                    filteredListings = listings.Where(l => IsListingValueEqual(l, RuneValue)).ToList();
                    break;
                case FilterOption.Lower:
                    filteredListings = listings.Where(l => IsListingValueLower(l, RuneValue)).ToList();
                    break;
                case FilterOption.Higher:
                    filteredListings = listings.Where(l => IsListingValueHigher(l, RuneValue)).ToList();
                    break;
                case FilterOption.LowerOrEqual:
                    filteredListings = listings.Where(l => IsListingValueLowerOrEqual(l, RuneValue)).ToList();
                    break;
                case FilterOption.HigherOrEqual:
                    filteredListings = listings.Where(l => IsListingValueHigherOrEqual(l, RuneValue)).ToList();
                    break;
                case FilterOption.Null:

                    break;
            }

            if(itemProperties != null && itemProperties.Count > 0)
            {
                filteredListings = filteredListings.Where(l => PropertyCheck(l, itemProperties)).ToList();
            }

            return filteredListings;
        }

        private bool PropertyCheck(ListingEntity listing, List<ItemPropertyEntity> itemProperties)
        {
            foreach(var itemProp in itemProperties)
            {
                PropertyEntity listingProperty = listing.NumericProperties.Where(p => p.Property == itemProp.Property).FirstOrDefault();
                if (listingProperty == null && (itemProp.Min != null || itemProp.Max != null))
                    return false;
                if (itemProp.Min != null)
                {
                    if (listingProperty.Number >= itemProp.Min)
                        continue;
                    else return false;
                }
                if(itemProp.Max != null)
                {
                    if (listingProperty.Number <= itemProp.Max)
                        continue;
                    else return false;
                }
            }
            return true;
        }

        private bool IsListingValueEqual(ListingEntity listing, ulong value)
        {
            List<ulong> values = listing.GetPriceValues();
            return IsValueEqual(value, values);
        }

        private bool IsValueEqual(ulong value, List<ulong> toCompare)
        {
            foreach (var el in toCompare)
            {
                if (el == value)
                    return true;
            }
            return false;
        }

        private bool IsListingValueLower(ListingEntity listing, ulong value)
        {
            List<ulong> values = listing.GetPriceValues();
            return IsValueLower(value, values);
        }

        private bool IsValueLower(ulong value, List<ulong> toCompare)
        {
            foreach(var el in toCompare)
            {
                if (el < value)
                    return true;
            }
            return false;
        }


        private bool IsListingValueHigher(ListingEntity listing, ulong value)
        {
            List<ulong> values = listing.GetPriceValues();
            return IsValueHigher(value, values);
        }

        private bool IsValueHigher(ulong value, List<ulong> toCompare)
        {
            foreach (var el in toCompare)
            {
                if (el > value)
                    return true;
            }
            return false;
        }

        private bool IsListingValueHigherOrEqual(ListingEntity listing, ulong value)
        {
            List<ulong> values = listing.GetPriceValues();
            return IsValueHigherOrEqual(value, values);
        }

        private bool IsValueHigherOrEqual(ulong value, List<ulong> toCompare)
        {
            foreach (var el in toCompare)
            {
                if (el >= value)
                    return true;
            }
            return false;
        }

        private bool IsListingValueLowerOrEqual(ListingEntity listing, ulong value)
        {
            List<ulong> values = listing.GetPriceValues();
            return IsValueLowerOrEqual(value, values);
        }

        private bool IsValueLowerOrEqual(ulong value, List<ulong> toCompare)
        {
            foreach (var el in toCompare)
            {
                if (el <= value)
                    return true;
            }
            return false;
        }
    }


}
