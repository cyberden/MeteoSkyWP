using MeteoSkyWP.Business;
using MeteoSkyWP.Business.Common;
using MeteoSkyWP.Business.TileNotifications;
using MeteoSkyWP.Common;
using MeteoSkyWP.DAL;
using MeteoSkyWP.DataModel;
using MeteoSkyWP.Tools;
using NotificationsExtensions.TileContent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MeteoSkyWP.ViewModels
{
    public class ForecastPageViewModel : BaseViewModel
    {
        #region properties

        protected ForecastElement CurrentPinnedTileElement { get; set; }

        protected Geolocator Geo { get; set; }

        protected string TileId { get { return string.Format("MeteoSkySecTile_{0}", City.Replace(" ", "").Replace("(", "").Replace(")", "")); } }
        protected string Url { get; set; }

        protected string City { get; set; }

        private string _header;

        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Tuple<string, object>> _forecastElements;

        public ObservableCollection<Tuple<string, object>> ForecastElements
        {
            get { return _forecastElements; }
            set
            {
                _forecastElements = value;
                RaisePropertyChanged();
            }
        }

        private bool _isFavorite;

        public bool IsFavorite
        {
            get { return _isFavorite; }
            set
            {
                _isFavorite = value;
                RaisePropertyChanged();
            }
        }

        private bool _isPinned;

        public bool IsPinned
        {
            get { return _isPinned; }
            set
            {
                _isPinned = value;
                RaisePropertyChanged();
            }
        }

        private bool _isShowingLongForecast;

        public bool IsShowingLongForecast
        {
            get { return _isShowingLongForecast; }
            set
            {
                _isShowingLongForecast = value;
                RaisePropertyChanged();

                if (_isShowingLongForecast)
                    LongForecastToggleButtonGlyph = "3";
                else
                    LongForecastToggleButtonGlyph = "7";
            }
        }

        private Visibility _windVIsibility;
        public Visibility WindVisibility
        {
            get { return _windVIsibility; }
            set
            {
                _windVIsibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _temperatureAndRainVIsibility;
        public Visibility TemperatureAndRainVIsibility
        {
            get { return _temperatureAndRainVIsibility; }
            set
            {
                _temperatureAndRainVIsibility = value;
                RaisePropertyChanged();
            }
        }

        private string _longForecastToggleButtonGlyph;

        public string LongForecastToggleButtonGlyph
        {
            get { return _longForecastToggleButtonGlyph; }
            set
            {
                _longForecastToggleButtonGlyph = value;
                RaisePropertyChanged();
            }
        }

        private string _togglePerHourLabel;

        public string TogglePerHourLabel
        {
            get { return _togglePerHourLabel; }
            set
            {
                _togglePerHourLabel = value;
                RaisePropertyChanged();
            }
        }

        protected bool IsHourDetail { get; set; }

        private Visibility _toggleHourBtnVisibility;
        public Visibility ToggleHourBtnVisibility
        {
            get { return _toggleHourBtnVisibility; }
            set
            {
                _toggleHourBtnVisibility = value;
                RaisePropertyChanged();
            }
        }

        public bool ShowCurrentPositionForecast { get; set; }


        #endregion

        #region Commands
        public ICommand AddToFavoriteCommand { get; private set; }

        public ICommand ToggleLongShortForecastCommand { get; private set; }

        public ICommand TogglePinForecastCommand { get; private set; }

        public ICommand GoHomeCommand { get; private set; }

        public ICommand TogglePerHourViewCommand { get; private set; }

        public ICommand ToggleWindVisibilityCommand { get; private set; }
        #endregion

        #region CTOR
        public ForecastPageViewModel()
        {
            AddToFavoriteCommand = new RelayCommand(AddToFavoriteExecute);
            ToggleLongShortForecastCommand = new RelayCommand(ToggleLongShortForecastExecute);
            TogglePinForecastCommand = new RelayCommand(TogglePinForecastExecute);
            GoHomeCommand = new RelayCommand(GoHomeExecute);
            TogglePerHourViewCommand = new RelayCommand(TogglePerHourViewExecute);
            ToggleWindVisibilityCommand = new RelayCommand(ToggleWindVisibility);
        }
        #endregion

        #region Initialization

        public async Task Initialize()
        {
            IsLoading = true;

            string url = string.Empty;

            // Retrieve loc.
            if (Geo == null)
            {
                Geo = new Geolocator();
            }

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

            bool forecastFound = false;
            string errorMessage = string.Empty;

            if (result.Locations.Any() && result.Locations[0].Address != null)
            {
                // here also it should be checked if there result isn't null and what to do in such a case
                var searchData = await new MeteocielProvider().SearchForecastData(result.Locations[0].Address.Town);

                if (searchData != null && searchData.Any())
                {
                    forecastFound = true;

                    url = searchData.First().ElementUrl;
                    await Initialize(url);
                }
                else
                {
                    errorMessage = "Aucune prévision n'a été trouvée pour votre position ou aucune connexion n'a été trouvée.";
                }
            }
            else
            {
                errorMessage = "Votre position n'a pas pu être déterminée.";
            }

            if (IsLoading)
                IsLoading = false;

            if (!forecastFound)
            {
                var messageDialog = new MessageDialog(errorMessage);
                messageDialog.Commands.Add(new UICommand("Fermer", new UICommandInvokedHandler(this.CommandInvokedHandler)));
                messageDialog.DefaultCommandIndex = 0;
                messageDialog.CancelCommandIndex = 0;

                await messageDialog.ShowAsync();
            }
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            GoHomeExecute(null);
        }


        public async Task Initialize(string url)
        {
            Url = url;

            IsLoading = true;

            var favorites = await DataSourceProvider.GetFavorites();
            IsFavorite = favorites.Any(f => f.ElementUrl == url.Replace("tendances", "previsions"));

            var response = await new MeteocielProvider().GetForecast(url, IsHourDetail);
            
            WindVisibility = Visibility.Collapsed;
            TemperatureAndRainVIsibility = Visibility.Visible;

            IsShowingLongForecast = response.IsLongForecast;

            if (IsShowingLongForecast)
            {
                IsHourDetail = false;
                ToggleHourBtnVisibility = Visibility.Collapsed;
            }
            else
            {
                ToggleHourBtnVisibility = Visibility.Visible;

                if (IsHourDetail)
                    TogglePerHourLabel = "Prévisions à 3 heures";
                else
                    TogglePerHourLabel = "Prévisions à l'heure";
            }

            Header = response.Header;
            City = response.City;
            IsPinned = Windows.UI.StartScreen.SecondaryTile.Exists(TileId);

            var elts = new ObservableCollection<Tuple<string, object>>();

            foreach (var elt in response.ForecastElements.GroupBy(elt => elt.WeekDay))
            {
                elts.Add(new Tuple<string, object>(string.Format("{0} ({1})", elt.Key, elt.First().Date), new ObservableCollection<ForecastElement>(elt)));
            }

            if (!IsHourDetail)
                elts.Add(new Tuple<string, object>("graphes", new ChartsForecastViewModel() { TemperatureChartUrl = response.TemperatureChartUrl, RainChartUrl = response.RainChartUrl }));

            ForecastElements = elts;

            if (IsPinned && !IsShowingLongForecast)
                NotifyTile(TileId);

            IsLoading = false;
        }
        #endregion

        #region Command Handlers
        public async void AddToFavoriteExecute(object parameter)
        {
            if (IsFavorite)
            {
                await DataSourceProvider.DeleteFavorite(Url);
                IsFavorite = false;
            }
            else
            {
                var favoriteData = new ForecastSearchResponseElement() { ElementUrl = Url.Replace("tendances", "previsions"), Name = City };

                await DataSourceProvider.SaveFavorite(favoriteData);
                IsFavorite = true;
            }
        }

        public async void ToggleLongShortForecastExecute(object parameter)
        {
            string url = string.Empty;
            if (Url.Contains("tendances"))
            {
                url = Url.Replace("tendances", "previsions");
            }
            else
            {
                url = Url.Replace("previsions", "tendances");
            }

            await Initialize(url);
        }

        public async void TogglePinForecastExecute(object parameter)
        {
            if (Windows.UI.StartScreen.SecondaryTile.Exists(TileId))
            {
                // First prepare the tile to be unpinned
                SecondaryTile secondaryTile = new SecondaryTile(TileId);
                // Now make the delete request.
                bool isUnpinned = await secondaryTile.RequestDeleteAsync();
                if (isUnpinned)
                    IsPinned = false;
            }
            else
            {
                IsPinned = true;

                IsLoading = true;
                if (IsShowingLongForecast)
                {
                    var element = await new MeteocielProvider().GetForecastForTileUpdate(Url);
                    CurrentPinnedTileElement = element.Item2;
                }
                else
                {
                    var elements = (ObservableCollection<ForecastElement>)ForecastElements.First().Item2;
                    CurrentPinnedTileElement = elements.LastOrDefault(elt => elt.Time < DateTime.Now.TimeOfDay) ?? elements.First();
                }
                IsLoading = false;

                await TileHelper.PinSecondaryTile(TileId, City.Split(new string[] { " (" }, StringSplitOptions.None).First(), Url, UpdateTile);
            }
        }

        private async void UpdateTile()
        {
            ForecastTilesNotificationHelper.NotifyTile(TileId, CurrentPinnedTileElement, City);
        }

        public void GoHomeExecute(object parameter)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
                rootFrame.Navigate(typeof(RootPage));
        }

        public async void TogglePerHourViewExecute(object parameter)
        {
            IsHourDetail = !IsHourDetail;

            await Initialize(this.Url);
        }

        public void ToggleWindVisibility(object parameter)
        {
            WindVisibility = WindVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            TemperatureAndRainVIsibility = TemperatureAndRainVIsibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion

        #region Tiles
        public void NotifyTile(string tileId)
        {
            var elements = (ObservableCollection<ForecastElement>)ForecastElements.First().Item2;
            var currentElement = elements.LastOrDefault(elt => elt.Time < DateTime.Now.TimeOfDay) ?? elements.First();

            ForecastTilesNotificationHelper.NotifyTile(tileId, currentElement, City);
        }
        #endregion
    }

    public class ChartsForecastViewModel : BaseViewModel
    {
        private string _temperatureChartUrl;
        public string TemperatureChartUrl
        {
            get { return _temperatureChartUrl; }
            set
            {
                _temperatureChartUrl = value;
                RaisePropertyChanged();
            }
        }

        private string _rainChartUrl;
        public string RainChartUrl
        {
            get { return _rainChartUrl; }
            set
            {
                _rainChartUrl = value;
                RaisePropertyChanged();
            }
        }
    }
}

