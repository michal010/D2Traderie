using D2Traderie.Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    public enum FilterOption { Null, Lower, Higher, Equal, LowerOrEqual, HigherOrEqual}
    public enum SortOption
    {
        None,
        PriceAscending,
        PriceDescending,
        PropertyAscending,
        PropertyDescending
    }

    class ItemFilterService
    {
        Services services;
        
        public ItemFilterService(Services services)
        {
            this.services = services;
        }

        public List<ListingEntity> GetSortedListings(List<ListingEntity> listings, SortOption sort, string propertyName = null)
        {
            switch (sort)
            {
                case SortOption.PriceAscending:
                    return listings
                        .OrderBy(l => l.GetPriceValues().Count > 0 ? l.GetPriceValues().Min() : ulong.MaxValue)
                        .ToList();
                case SortOption.PriceDescending:
                    return listings
                        .OrderByDescending(l => l.GetPriceValues().Count > 0 ? l.GetPriceValues().Min() : 0)
                        .ToList();
                case SortOption.PropertyAscending:
                    return listings
                        .OrderBy(l => GetPropertyValue(l, propertyName))
                        .ToList();
                case SortOption.PropertyDescending:
                    return listings
                        .OrderByDescending(l => GetPropertyValue(l, propertyName))
                        .ToList();
                default:
                    return listings;
            }
        }

        private int GetPropertyValue(ListingEntity listing, string propertyName)
        {
            if (listing.NumericProperties == null || propertyName == null) return 0;
            var prop = listing.NumericProperties.FirstOrDefault(p => p.Property == propertyName);
            return prop?.Number ?? 0;
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
            if (listing.NumericProperties == null)
            {
                bool anyFilterSet = itemProperties.Any(p =>
                    (p.Min != null && p.Min > 0) || (p.Max != null && p.Max > 0));
                return !anyFilterSet;
            }

            foreach (var itemProp in itemProperties)
            {
                // Pomiń właściwości bez ustawionych filtrów
                bool hasMin = itemProp.Min != null && itemProp.Min > 0;
                bool hasMax = itemProp.Max != null && itemProp.Max > 0;

                if (!hasMin && !hasMax)
                    continue;

                PropertyEntity listingProperty = listing.NumericProperties
                    .Where(p => p.Property == itemProp.Property)
                    .FirstOrDefault();

                if (listingProperty == null)
                    return false;

                if (hasMin && listingProperty.Number < itemProp.Min)
                    return false;

                if (hasMax && listingProperty.Number > itemProp.Max)
                    return false;
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
