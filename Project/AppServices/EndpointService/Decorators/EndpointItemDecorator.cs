using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    class EndpointItemDecorator : EndpointDecorator
    {
        string itemID;
        public EndpointItemDecorator(IEndpoint endpoint, string itemID) : base(endpoint) { this.itemID = itemID; }

        public override string GetEndpointURL()
        {
            string url = base.GetEndpointURL();
            url += $"?item={itemID}";
            return url;
        }
    }
}
