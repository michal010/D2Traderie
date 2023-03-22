using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    class EndpointPageDecorator : EndpointDecorator
    {
        int page;
        public EndpointPageDecorator(IEndpoint endpoint, int page) : base(endpoint) { this.page = page; }

        public override string GetEndpointURL()
        {
            string url = base.GetEndpointURL();
            url += $"page={page.ToString()}"; //move to settings
            return url;
        }
    }
}
