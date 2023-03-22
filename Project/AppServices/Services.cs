using D2Traderie.Project.Models;

namespace D2Traderie.Project.AppServices
{
    class Services
    {
        public HttpService HttpSerivce;
        public SettingsService SettingsService;
        public EndpointService EndpointService;
        public FileService FileService;
        public Database Database;
        public ItemFilterService FilterService;
        public MainWindow MainWindow; //pass it to some views service.

        public Services(MainWindow windowReference)
        {
            this.MainWindow = windowReference;
            HttpSerivce = new HttpService();
            SettingsService = new SettingsService(this);
            EndpointService = new EndpointService();
            FileService = new FileService();
            FilterService = new ItemFilterService(this);
            Database = new Database(this);
        }
    }
}