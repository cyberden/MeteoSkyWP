using MeteoSkyWP.Business;
using MeteoSkyWP.Business.TileNotifications;
using MeteoSkyWP.Common;
using MeteoSkyWP.DAL;
using MeteoSkyWP.DataModel;
using MeteoSkyWP.Tools;
using MeteoSkyWPruntimeComponent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MeteoSkyWP.ViewModels
{
    public class RootPageViewModel : BaseViewModel
    {
        #region Properties
        private string _searchString;
        public string SearchString
        {
            get { return _searchString; }
            set 
            {
                _searchString = value;
                RaisePropertyChanged();
            }
        }

        protected static string PinCurrentLocationTileText = "épingler la tuile pour la position courante";
        protected static string UnPinCurrentLocationTileText = "Desépingler la tuile pour la position courante";

        private string _toggleCurrentLocationPinText = PinCurrentLocationTileText;

        public string ToggleCurrentLocationPinText
        {
            get { return _toggleCurrentLocationPinText; }
            set
            {
                _toggleCurrentLocationPinText = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Tuple<string, string>> _observationsMapsCollections;

        public ObservableCollection<Tuple<string, string>> ObservationsMapsCollections
        {
            get
            {
                if (_observationsMapsCollections == null)
                {
                    _observationsMapsCollections = new ObservableCollection<Tuple<string, string>>();
                    _observationsMapsCollections.Add(new Tuple<string, string>("France", @"http://www.meteociel.fr/cartes_obs/temp2_1h.png"));
                    _observationsMapsCollections.Add(new Tuple<string, string>("Zoom Nord-Ouest", @"http://meteociel.com/cartes_obs/temp2no_1h.png"));
                    _observationsMapsCollections.Add(new Tuple<string, string>("Zoom Nord-Est", @"http://meteociel.com/cartes_obs/temp2ne_1h.png"));
                    _observationsMapsCollections.Add(new Tuple<string, string>("Zoom Sud-Ouest", @"http://meteociel.com/cartes_obs/temp2so_1h.png"));
                    _observationsMapsCollections.Add(new Tuple<string, string>("Zoom Sud-Est", @"http://meteociel.com/cartes_obs/temp2se_1h.png"));
                    _observationsMapsCollections.Add(new Tuple<string, string>("Zoom RP", @"http://meteociel.com/cartes_obs/temp2rp_1h.png"));
                    MapSelectedIndex = 0;   
                }

                return _observationsMapsCollections;
            }
        }

        private ObservableCollection<Tuple<string, string>> _forecastMapsCollections;

        public ObservableCollection<Tuple<string, string>> ForecastMapsCollections
        {
            get
            {
                
                return _forecastMapsCollections;
            }
            set
            {
                _forecastMapsCollections = value;
                RaisePropertyChanged();
            }
        }
        

        private int _mapSelectedIndex;

        public int MapSelectedIndex
        {
            get { return _mapSelectedIndex; }
            set
            {
                _mapSelectedIndex = value;
                RaisePropertyChanged();
            }
        }

        private int _forecastMapSelectedIndex;

        public int ForecastMapSelectedIndex
        {
            get { return _forecastMapSelectedIndex; }
            set
            {
                _forecastMapSelectedIndex = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ForecastSearchResponseElement> _searchResponseElements = new ObservableCollection<ForecastSearchResponseElement>();

        public ObservableCollection<ForecastSearchResponseElement> SearchResponseElements
        {
            get { return _searchResponseElements;}
            set
            {
                _searchResponseElements = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ForecastSearchResponseElement> _favoritesResponseElements = new ObservableCollection<ForecastSearchResponseElement>();

        public ObservableCollection<ForecastSearchResponseElement> FavoritesResponseElements
        {
            get { return _favoritesResponseElements; }
            set
            {
                _favoritesResponseElements = value;
                RaisePropertyChanged();
            }
        }

        public Tuple<string, ForecastElement> CurrentLocationForecast { get; set; }

        public ObservableCollection<ExtraViewModel> _availableExtras = new ObservableCollection<ExtraViewModel>() { new ExtraViewModel() { Type = ExtrasEnum.Ski, Label = "Enneigement"} };
        public ObservableCollection<ExtraViewModel> AvailableExtras
        {
            get { return _availableExtras; }
            set
            {
                _availableExtras = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand SearchForecastCommand { get; private set; }

        public ICommand GoToForecastCommand { get; private set; }

        public ICommand DeleteFavoriteCommand { get; private set; }

        public ICommand ToggleCurrentLocationPinCommand { get; private set; }

        public ICommand GoToExtraCommand { get; private set; }

        #endregion

        #region ctor
        public RootPageViewModel()
        {
            SearchForecastCommand = new RelayCommand(SearchForecastExecute);
            GoToExtraCommand = new RelayCommand(GoToExtraExecute);
            DeleteFavoriteCommand = new RelayCommand(DeleteFavoriteExecute);
            ToggleCurrentLocationPinCommand = new RelayCommand(ToggleCurrentLocationPinExecute);
        }
        #endregion

        #region Loading
        public async Task InitializeViewModel()
        {
            ToggleCurrentLocationPinText = Windows.UI.StartScreen.SecondaryTile.Exists("CurrentLocation") ? UnPinCurrentLocationTileText : PinCurrentLocationTileText;

            FavoritesResponseElements = new ObservableCollection<ForecastSearchResponseElement>(await DataSourceProvider.GetFavorites(true));
            FavoritesResponseElements.Insert(0, new ForecastSearchResponseElement() { ElementUrl = string.Empty, Name = "Position actuelle" });

            ForecastMapsCollections = new ObservableCollection<Tuple<string, string>>(await new MeteocielProvider().GetForecastmaps());

            ForecastMapSelectedIndex = 0;
        }
        #endregion

        #region Command handlers
        public async void SearchForecastExecute(object parameter)
        {
            if (string.IsNullOrEmpty(SearchString))
                return;

            IsLoading = true;
            SearchResponseElements = new ObservableCollection<ForecastSearchResponseElement>(await new MeteocielProvider().SearchForecastData(SearchString));

            if (SearchResponseElements.Count == 1)
            {
                GoToForecastExecute(SearchResponseElements.First());
            }

            IsLoading = false;
        }

        public void GoToForecastExecute(object parameter)
        {
            if (parameter is ForecastSearchResponseElement)
            {
                SearchString = null;
                SearchResponseElements = null;

                string url = ((ForecastSearchResponseElement)parameter).ElementUrl;

                Frame rootFrame = Window.Current.Content as Frame;

                if (rootFrame != null)
                    rootFrame.Navigate(typeof(ForecastPage), url);
            }
        }

        public void GoToExtraExecute(object parameter)
        {
            if (parameter is ExtraViewModel)
            {
                var vm = (ExtraViewModel)parameter;

                Type navigateToType = null;

                switch (vm.Type)
	            {
		            case ExtrasEnum.Ski:
                        navigateToType = typeof(SkiPage);
                        break;
	            }

                Frame rootFrame = Window.Current.Content as Frame;

                if (rootFrame != null && navigateToType != null)
                    rootFrame.Navigate(navigateToType);
            }
        }

        public async void DeleteFavoriteExecute(object parameter)
        {
            var fav = parameter as ForecastSearchResponseElement;

            if (fav != null)
            {
                FavoritesResponseElements.Remove(fav);
                await DataSourceProvider.DeleteFavorite(fav.ElementUrl);
            }
        }

        public async void ToggleCurrentLocationPinExecute(object parameter)
        {
            // Is Pinned?
            var isPinned = Windows.UI.StartScreen.SecondaryTile.Exists("CurrentLocation");

            if (!isPinned)
            {
                IsLoading = true;
                ToggleCurrentLocationPinText = UnPinCurrentLocationTileText;

                CurrentLocationForecast = await new MeteocielProvider().GetForecastForTileUpdate(await RetrieveTargetUrlForCurrentLocation());
                IsLoading = false;

                await TileHelper.PinSecondaryTile("CurrentLocation", "Lieu actuel", "CurrentLocation", UpdateTile);
            }
            else
            {
                ToggleCurrentLocationPinText = PinCurrentLocationTileText;

                SecondaryTile secondaryTile = new SecondaryTile("CurrentLocation");
                await secondaryTile.RequestDeleteAsync();
            }
        }

        private void UpdateTile()
        {
            if (CurrentLocationForecast != null)
            {
                ForecastTilesNotificationHelper.NotifyTile("CurrentLocation", CurrentLocationForecast.Item2, "Lieu actuel");
            }
        }

        /// <summary>
        /// Ce code est copié malheureusement car sinon c'est chiant, il manque toujours quelque chose.
        /// </summary>
        /// <returns></returns>
        public static async Task<string> RetrieveTargetUrlForCurrentLocation()
        {
            var geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 1000;
            Geoposition position = await geolocator.GetGeopositionAsync();

            // reverse geocoding
            BasicGeoposition myLocation = new BasicGeoposition
            {
                Longitude = position.Coordinate.Longitude,
                Latitude = position.Coordinate.Latitude
            };

            Geopoint pointToReverseGeocode = new Geopoint(myLocation);
            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAtAsync(pointToReverseGeocode);

            string errorMessage = string.Empty;

            if (result.Locations.Any() && result.Locations[0].Address != null)
            {
                // here also it should be checked if there result isn't null and what to do in such a case
                var searchData = await new MeteocielProvider().SearchForecastData(result.Locations[0].Address.Town);

                if (searchData != null && searchData.Any())
                {
                    return searchData.First().ElementUrl;
                }
            }

            return null;
        }
        
        #endregion
    }

    public class ExtraViewModel
    {
        public string Label { get; set; }

        public ExtrasEnum Type { get; set; }
    }

    public enum ExtrasEnum
    {
        Ski
    }
}
