using D2Traderie.Project.Models;
using System;
using System.Collections.Generic;

namespace D2Traderie.Project.AppServices
{
    class EndpointService
    {
        public EndpointService() { }

        public string GetItemsEndpoint(int page)
        {
            IEndpoint e = new Endpoint();
            e = new EndpointItemsDecorator(e);
            e = new EndpointQuestionMarkDecorator(e);
            e = new EndpointItemPropertiesDecorator(e);
            e = new EndpointAndDecorator(e);
            e = new EndpointPageDecorator(e, page);
            return e.GetEndpointURL();
        }

        public string GetTagsEndpoint()
        {
            IEndpoint e = new Endpoint();
            e = new EndpointTagsDecorator(e);
            e = new EndpointItemsDecorator(e);
            return e.GetEndpointURL();
        }

        public string GetListingEndpoint(ulong itemID, int page, SearchSettings settings = null)
        {
            if (settings == null)
                settings = new SearchSettings();

            IEndpoint e = new Endpoint();
            e = new EndpointListingsDecorator(e);
            // EndpointItemDecorator dodaje ?item=ID — od tej pory używamy tylko &
            e = new EndpointItemDecorator(e, itemID.ToString());

            var parameters = BuildParameters(settings, page);
            foreach (var (key, value) in parameters)
            {
                e = new EndpointAndDecorator(e);
                e = new EndpointPropertyDecorator(e, key, value);
            }

            string url = e.GetEndpointURL();
            Console.WriteLine($"[ENDPOINT] {url}");
            return url;
        }

        private List<(string key, string value)> BuildParameters(SearchSettings s, int page)
        {
            var p = new List<(string, string)>();

            string platform = s.GetPlatformParam();
            if (!string.IsNullOrEmpty(platform))
                p.Add(("prop_Platform", Uri.EscapeDataString(platform)));

            string mode = s.GetModeParam();
            if (!string.IsNullOrEmpty(mode))
                p.Add(("prop_Mode", Uri.EscapeDataString(mode)));

            string ladder = s.GetLadderParam();
            if (!string.IsNullOrEmpty(ladder))
                p.Add(("prop_Ladder", Uri.EscapeDataString(ladder)));

            string gameVersion = s.GetGameVersionParam();
            if (!string.IsNullOrEmpty(gameVersion))
                p.Add(("prop_Game%20version", gameVersion));

            string unidentified = s.GetUnidentifiedParam();
            if (!string.IsNullOrEmpty(unidentified))
                p.Add(("prop_Unidentified", unidentified));

            string ethereal = s.GetEtherealParam();
            if (!string.IsNullOrEmpty(ethereal))
                p.Add(("prop_Ethereal", ethereal));

            string makeOffer = s.GetMakeOfferParam();
            if (!string.IsNullOrEmpty(makeOffer))
                p.Add(("makeOffer", makeOffer));

            // page zawsze na końcu
            p.Add(("page", page.ToString()));

            return p;
        }
    }
}