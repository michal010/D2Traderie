using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    class EndpointListingsDecorator : EndpointDecorator
    {
        public EndpointListingsDecorator(IEndpoint endpoint) : base(endpoint) { }

        public override string GetEndpointURL()
        {
            string url = base.GetEndpointURL();
            url += "listings"; //move to settings
            return url;
        }
    }
}
