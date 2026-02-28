using D2Traderie.Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    public enum FilterOption { Null, Lower, Higher, Equal, LowerOrEqual, HigherOrEqual }
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
            Console.WriteLine($"[SORT] Option={sort}, Property='{propertyName}', Listings count={listings?.Count}");

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
                    if (string.IsNullOrEmpty(propertyName))
                    {
                        Console.WriteLine("[SORT] PropertyAscending: brak propertyName, pomijam sortowanie");
                        return listings;
                    }
                    return listings
                        .OrderBy(l => GetPropertyValue(l, propertyName))
                        .ToList();

                case SortOption.PropertyDescending:
                    if (string.IsNullOrEmpty(propertyName))
                    {
                        Console.WriteLine("[SORT] PropertyDescending: brak propertyName, pomijam sortowanie");
                        return listings;
                    }
                    return listings
                        .OrderByDescending(l => GetPropertyValue(l, propertyName))
                        .ToList();

                default:
                    return listings;
            }
        }

        private int GetPropertyValue(ListingEntity listing, string propertyName)
        {
            if (listing.NumericProperties == null || string.IsNullOrEmpty(propertyName))
                return 0;

            // Szukaj dokładnego dopasowania
            var prop = listing.NumericProperties.FirstOrDefault(p => p.Property == propertyName);

            // Fallback: dopasowanie case-insensitive (API czasem zwraca różne wielkości liter)
            if (prop == null)
                prop = listing.NumericProperties.FirstOrDefault(p =>
                    string.Equals(p.Property, propertyName, StringComparison.OrdinalIgnoreCase));

            return prop?.Number ?? 0;
        }

        public List<ListingEntity> GetFilteredListings(
            List<ListingEntity> listings,
            List<ItemPropertyEntity> itemProperties,
            FilterOption option,
            ulong RuneValue)
        {
            List<ListingEntity> filteredListings = listings;

            // Filtr ceny (runa)
            switch (option)
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

            // Filtr właściwości — tylko jeśli jakiś filtr jest faktycznie ustawiony
            if (itemProperties != null && itemProperties.Count > 0)
            {
                bool anyFilterActive = itemProperties.Any(p =>
                    (p.Min.HasValue && p.Min.Value > 0) ||
                    (p.Max.HasValue && p.Max.Value > 0));

                if (anyFilterActive)
                    filteredListings = filteredListings.Where(l => PropertyCheck(l, itemProperties)).ToList();
            }

            return filteredListings;
        }

        private bool PropertyCheck(ListingEntity listing, List<ItemPropertyEntity> itemProperties)
        {
            // Listing bez właściwości — przepuść tylko jeśli żaden filtr nie jest ustawiony
            if (listing.NumericProperties == null)
            {
                bool anyFilterSet = itemProperties.Any(p =>
                    (p.Min.HasValue && p.Min.Value > 0) ||
                    (p.Max.HasValue && p.Max.Value > 0));
                return !anyFilterSet;
            }

            foreach (var itemProp in itemProperties)
            {
                bool hasMin = itemProp.Min.HasValue && itemProp.Min.Value > 0;
                bool hasMax = itemProp.Max.HasValue && itemProp.Max.Value > 0;

                if (!hasMin && !hasMax)
                    continue;

                PropertyEntity listingProperty = listing.NumericProperties
                    .FirstOrDefault(p => p.Property == itemProp.Property);

                if (listingProperty == null)
                    return false;

                // Rzutowanie na long żeby uniknąć problemów z int? vs uint? przy wartościach ujemnych
                long propValue = listingProperty.Number ?? 0;

                if (hasMin && propValue < (long)itemProp.Min.Value)
                    return false;

                if (hasMax && propValue > (long)itemProp.Max.Value)
                    return false;
            }
            return true;
        }

        private bool IsListingValueEqual(ListingEntity listing, ulong value)
            => IsValueEqual(value, listing.GetPriceValues());

        private bool IsValueEqual(ulong value, List<ulong> toCompare)
            => toCompare.Any(el => el == value);

        private bool IsListingValueLower(ListingEntity listing, ulong value)
            => IsValueLower(value, listing.GetPriceValues());

        private bool IsValueLower(ulong value, List<ulong> toCompare)
            => toCompare.Any(el => el < value);

        private bool IsListingValueHigher(ListingEntity listing, ulong value)
            => IsValueHigher(value, listing.GetPriceValues());

        private bool IsValueHigher(ulong value, List<ulong> toCompare)
            => toCompare.Any(el => el > value);

        private bool IsListingValueHigherOrEqual(ListingEntity listing, ulong value)
            => IsValueHigherOrEqual(value, listing.GetPriceValues());

        private bool IsValueHigherOrEqual(ulong value, List<ulong> toCompare)
            => toCompare.Any(el => el >= value);

        private bool IsListingValueLowerOrEqual(ListingEntity listing, ulong value)
            => IsValueLowerOrEqual(value, listing.GetPriceValues());

        private bool IsValueLowerOrEqual(ulong value, List<ulong> toCompare)
            => toCompare.Any(el => el <= value);
    }
}