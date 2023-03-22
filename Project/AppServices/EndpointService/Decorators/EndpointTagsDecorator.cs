using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    class EndpointTagsDecorator : EndpointDecorator
    {
        public EndpointTagsDecorator(IEndpoint endpoint) : base(endpoint) { }

        public override string GetEndpointURL()
        {
            string url = base.GetEndpointURL();
            url += "tags/"; //move to settings
            return url;
        }
    }
}
