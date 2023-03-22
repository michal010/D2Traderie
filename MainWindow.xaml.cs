using D2Traderie.Project;
using D2Traderie.Project.AppServices;
using D2Traderie.Project.Consts;
using D2Traderie.Project.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace D2Traderie
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<string> _runes;

        public List<string> Runes
        {
            get { return _runes;  }
            set
            {
                _runes = value;
                OnPropertyChanged();
            }
        }

        private uint _depthOfSearch;

        public uint DepthOfSearch
        {
            get { return _depthOfSearch; }
            set
            {
                _depthOfSearch = value;
                OnPropertyChanged();
            }
        }

        private string _selectedRuneValue;

        public string SelectedRuneValue
        {
            get { return _selectedRuneValue; }
            set
            {
                _selectedRuneValue = value;
                OnPropertyChanged();
            }
        }

        private List<FilterOption> _filterOptions;

        public List<FilterOption> FilterOptions
        {
            get { return _filterOptions; }
            set
            {
                _filterOptions = value;
                OnPropertyChanged();
            }
        }

        private FilterOption _selectedFilterOption;

        public FilterOption SelectedFilterOption
        {
            get { return _selectedFilterOption; }
            set
            {
                _selectedFilterOption = value;
                OnPropertyChanged();
            }
        }

        private string _selectedItemName;

        public string SelectedItemName
        {
            get { return _selectedItemName; }
            set
            {
                _selectedItemName = value;
                OnPropertyChanged();
            }
        }

        private List<string> _itemNames;

        public List<string> ItemNames
        {
            get { return _itemNames; }
            set
            {
                _itemNames = value;
                OnPropertyChanged();
            }
        }

        private List<ListingEntity> _listings;

        public List<ListingEntity> Listings
        {
            get { return _listings; }
            set
            {
                _listings = value;
                OnPropertyChanged();
            }
        }

        private ListingEntity _selectedListing;

        public ListingEntity SelectedListing
        {
            get { return _selectedListing; }
            set
            {
                _selectedListing = value;
                OnPropertyChanged();
            }
        }

        private List<ItemPropertyEntity> _selectedItemProperties;

        public List<ItemPropertyEntity> SelectedItemProperties
        {
            get { return _selectedItemProperties; }
            set
            {
                _selectedItemProperties = value;
                OnPropertyChanged();
            }
        }

        private string _selectedItemImageURL;

        public string SelectedItemImageURL
        {
            get { return _selectedItemImageURL; }
            set
            {
                _selectedItemImageURL = value;
                OnPropertyChanged();
            }
        }

        private SearchSettings _searchSettings;

        public SearchSettings SearchSettings
        {
            get { return _searchSettings; }
            set
            {
                _searchSettings = value;
                OnPropertyChanged();
            }
        }

        Services services;

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // for .NET Core you need to add UseShellExecute = true
            // see https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute#property-value
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        public MainWindow()
        {
            DataContext = this;
            FilterOptions = new List<FilterOption>() { FilterOption.Null, FilterOption.Lower, FilterOption.LowerOrEqual, FilterOption.Equal, FilterOption.Higher, FilterOption.HigherOrEqual };
            Runes = RunesValue.Runes;
            DepthOfSearch = 1;
            services = new Services(this);
            //Add some gif to notify about loading.
            services.Database.LoadDatabase();
            InitializeComponent();
        }

        private async void OnSearchButtonClicked(object sender, RoutedEventArgs e)
        {
            searchButton.IsEnabled = false;
            //services.Database.DownloadAndSaveItems();
            uint itemID = services.Database.GetItemID(SelectedItemName);
            await services.Database.DownloadListings(itemID, DepthOfSearch);
            //process request 
            //send
            searchButton.IsEnabled = true;
        }

        private void RefreshSelectedItemUI()
        {
            if (SelectedItemName == null)
                return;
            uint itemID = services.Database.GetItemID(SelectedItemName);
            var item = services.Database.GetItem(itemID);
            SelectedItemProperties = services.Database.GetItemPropertyNumericEntities(item);
            SelectedItemImageURL = services.Database.GetItemImage(item);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnItemNameSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSelectedItemUI();
        }

        private void OnFilterButtonClicked(object sender, RoutedEventArgs e)
        {
            if (services.Database.Listings == null || services.Database.Listings.Count < 0)
                return;
            ulong value = 0;
            if(SelectedRuneValue != null)
                value = RunesValue.RuneValues[SelectedRuneValue];
            Listings = services.FilterService.GetFilteredListings(services.Database.Listings, SelectedItemProperties, SelectedFilterOption, value);
        }
    }
}
