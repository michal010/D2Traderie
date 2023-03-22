using D2Traderie.Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    class SettingsService
    {
        private Services services;
        private SearchSettings searchSettings;


        public SettingsService(Services services)
        {
            searchSettings = new SearchSettings();
            services.MainWindow.SearchSettings = searchSettings;
        }
    }
}
