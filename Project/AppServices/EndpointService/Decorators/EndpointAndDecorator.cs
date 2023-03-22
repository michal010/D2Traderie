using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    class EndpointAndDecorator : EndpointDecorator
    {
        public EndpointAndDecorator(IEndpoint endpoint) : base(endpoint) { }

        public override string GetEndpointURL()
        {
            string url = base.GetEndpointURL();
            url += "&"; //move to settings
            return url;
        }
    }
}
