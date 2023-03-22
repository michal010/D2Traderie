using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    class EndpointPropertyDecorator : EndpointDecorator
    {
        private string propertyName;
        private string propertyValue;

        public EndpointPropertyDecorator(IEndpoint endpoint, string propertyName, string propertyValue) : base(endpoint) 
        { 
            this.propertyName = propertyName; 
            this.propertyValue = propertyValue; 
        }

        public override string GetEndpointURL()
        {
            string url = base.GetEndpointURL();
            url += $"{propertyName}={propertyValue}"; //move to settings
            return url;
        }
    }
}
