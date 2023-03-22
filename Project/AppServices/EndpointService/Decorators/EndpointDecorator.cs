using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    class EndpointDecorator : IEndpoint
    {
        private IEndpoint endpoint;

        public EndpointDecorator(IEndpoint endpoint)
        {
            this.endpoint = endpoint;
        }

        public virtual string GetEndpointURL()
        {
            return endpoint.GetEndpointURL();
        }
    }
}
