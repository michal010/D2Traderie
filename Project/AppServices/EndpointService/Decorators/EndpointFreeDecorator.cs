using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    class EndpointFreeDecorator : EndpointDecorator
    {
        bool free;
        public EndpointFreeDecorator(IEndpoint endpoint, bool free) : base(endpoint) { this.free = free; }

        public override string GetEndpointURL()
        {
            string url =  base.GetEndpointURL();
            url += $"&free={free.ToString().ToLower()}"; //move to settings
            return url;
        }
    }
}
