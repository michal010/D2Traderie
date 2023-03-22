using D2Traderie.Project.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    class Endpoint : IEndpoint
    {
        public string GetEndpointURL()
        {
            return Settings.BaseEndpoint;
        }
    }
}
