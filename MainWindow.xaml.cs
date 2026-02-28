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
    // Nowa klasa filtra - tylko do UI
    public class PropertyFilter
    {
        public string Property { get; set; }
        public string DisplayName => Property?.Replace("{{value}}", "#") ?? "";
        public uint? Min { get; set; } = null;
        public uint? Max { get; set; } = null;
    }
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

        private List<SortOption> _sortOptions;
        public List<SortOption> SortOptions
        {
            get { return _sortOptions; }
            set { _sortOptions = value; OnPropertyChanged(); }
        }

        private SortOption _selectedSortOption;
        public SortOption SelectedSortOption
        {
            get { return _selectedSortOption; }
            set { _selectedSortOption = value; OnPropertyChanged(); }
        }

        private string _selectedSortProperty;
        public string SelectedSortProperty
        {
            get { return _selectedSortProperty; }
            set { _selectedSortProperty = value; OnPropertyChanged(); }
        }
        public List<string> SelectedItemPropertyNames
        {
            get
            {
                return SelectedItemProperties?
                    .Select(p => p.Property)
                    .ToList() ?? new List<string>();
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
            SortOptions = new List<SortOption>
            {
                SortOption.None,
                SortOption.PriceAscending,
                SortOption.PriceDescending,
                SortOption.PropertyAscending,
                SortOption.PropertyDescending
            };

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
            ShowLoading("Pobieranie listingów...");
            ulong itemID = services.Database.GetItemID(SelectedItemName);
            await services.Database.DownloadListings(itemID, DepthOfSearch, SearchSettings);
            HideLoading();
            searchButton.IsEnabled = true;
        }

        private async void OnRefreshDbClicked(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Czy na pewno chcesz odświeżyć bazę danych?\nOperacja może potrwać kilka minut.",
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            refreshDbButton.IsEnabled = false;
            ShowLoading("Pobieranie bazy itemów...");
            await services.Database.RefreshDatabase();
            HideLoading();
            refreshDbButton.IsEnabled = true;
        }
        private async void OnRefreshValuesClicked(object sender, RoutedEventArgs e)
        {
            refreshValuesButton.IsEnabled = false;
            ShowLoading("Pobieranie wartości walut...");
            await services.Database.RefreshValues();
            HideLoading();
            refreshValuesButton.IsEnabled = true;

            MessageBox.Show(
                "Wartości walut zostały zaktualizowane.",
                "Gotowe",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void RefreshSelectedItemUI()
        {
            if (SelectedItemName == null) return;
            ulong itemID = services.Database.GetItemID(SelectedItemName);
            var item = services.Database.GetItem(itemID);

            // Twórz puste filtry - tylko nazwy właściwości, Min/Max = null
            SelectedItemProperties = services.Database
                .GetItemPropertyNumericEntities(item)
                .Select(p => new ItemPropertyEntity
                {
                    Property = p.Property,
                    PropertyId = p.PropertyId,
                    Type = p.Type,
                    Min = null,  // zawsze null - użytkownik wpisuje sam
                    Max = null   // zawsze null
                })
                .ToList();

            SelectedItemImageURL = services.Database.GetItemImage(item);
            OnPropertyChanged(nameof(SelectedItemPropertyNames));
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

            if (services.Database.Listings == null || services.Database.Listings.Count <= 0)
                return;

            ulong value = 0;
            if (SelectedRuneValue != null && RunesValue.RuneValues.ContainsKey(SelectedRuneValue))
                value = RunesValue.RuneValues[SelectedRuneValue];

            Console.WriteLine($"=== FILTER DEBUG ===");
            Console.WriteLine($"Listings w DB: {services.Database.Listings.Count}");
            Console.WriteLine($"FilterOption: {SelectedFilterOption}, RuneValue: {value}");
            Console.WriteLine($"SelectedItemProperties count: {SelectedItemProperties?.Count}");
            if (SelectedItemProperties != null)
                foreach (var p in SelectedItemProperties)
                    Console.WriteLine($"  Property: {p.Property}, Min: {p.Min}, Max: {p.Max}");

            // najpierw tylko filtr ceny
            var afterPriceFilter = services.FilterService.GetFilteredListings(
                services.Database.Listings, null, SelectedFilterOption, value);
            Console.WriteLine($"Po filtrze CENY (bez property): {afterPriceFilter.Count}");

            // potem z property
            var filtered = services.FilterService.GetFilteredListings(
                services.Database.Listings, SelectedItemProperties, SelectedFilterOption, value);
            Console.WriteLine($"Po filtrze WSZYSTKIM: {filtered.Count}");

            string sortProp = (SelectedSortOption == SortOption.PropertyAscending ||
                               SelectedSortOption == SortOption.PropertyDescending)
                               ? SelectedSortProperty : null;

            Listings = services.FilterService.GetSortedListings(filtered, SelectedSortOption, sortProp);
            Console.WriteLine($"Listings w UI po sortowaniu: {Listings.Count}");
        }
        public void ShowLoading(string message = "Pobieranie danych...")
        {
            loadingOverlay.Visibility = Visibility.Visible;
            loadingStatusLabel.Content = message;
        }

        public void HideLoading()
        {
            loadingOverlay.Visibility = Visibility.Collapsed;
        }

        public void UpdateLoadingMessage(string message)
        {
            if(loadingStatusLabel != null)
                loadingStatusLabel.Content = message;
        }

        #region ORC
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            services.OcrService.RegisterHotkey(helper.Handle);

            // Podłącz handler wiadomości Windows
            System.Windows.Interop.HwndSource source =
                System.Windows.Interop.HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
        }

        protected override void OnClosed(EventArgs e)
        {
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            services.OcrService.UnregisterHotkey(helper.Handle);
            base.OnClosed(e);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY && services.OcrService.IsOurHotkey(wParam.ToInt32()))
            {
                OnHotkeyPressed();
                handled = true;
            }
            return IntPtr.Zero;
        }

        private async void OnHotkeyPressed()
        {
            ShowLoading("Rozpoznawanie itemu...");
            var result = await Task.Run(() => services.OcrService.CaptureAndRecognize());
            HideLoading();

            if (result.Success)
            {
                SelectedItemName = result.ItemName;
                SearchBar.Text = result.ItemName;

                // Pobierz item z bazy
                ulong itemID = services.Database.GetItemID(result.ItemName);
                var item = services.Database.GetItem(itemID);

                if (item != null)
                {
                    // === NOWE: fuzzy match OCR properties → auto-uzupełnij filtry ===
                    var matchedProperties = services.Database.MatchDetectedPropertiesToItem(
                        item,
                        result.DetectedProperties
                    );

                    SelectedItemProperties = matchedProperties;
                    SelectedItemImageURL = services.Database.GetItemImage(item);
                    OnPropertyChanged(nameof(SelectedItemPropertyNames));

                    int filledCount = matchedProperties.Count(p => p.Min != null);
                    Console.WriteLine($"[OCR] Auto-uzupełniono {filledCount}/{matchedProperties.Count} właściwości");
                }
                else
                {
                    // Fallback: brak itemu w bazie, normalny refresh UI
                    RefreshSelectedItemUI();
                }

                ShowLoading($"Pobieranie listingów dla: {result.ItemName}");
                await services.Database.DownloadListings(itemID, DepthOfSearch);
                HideLoading();

                Console.WriteLine($"[OCR] Sukces: {result.ItemName}");
                Console.WriteLine($"[OCR] Raw text: {result.RawText.Replace("\n", " | ")}");
            }
            else
            {
                HideLoading();
                Console.WriteLine($"[OCR] Niepowodzenie: {result.RawText}");
            }
        }
        #endregion
    }
}
