using MeteoSkyWP.Business;
using MeteoSkyWP.Business.TileNotifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MeteoSkyWPruntimeComponent
{
    public sealed class UpdateForecastBackgroundTask : IBackgroundTask //XamlRenderingBackgroundTask
    {
        BackgroundTaskDeferral _deferral = null;
        IBackgroundTaskInstance _taskInstance = null;

        //protected async override void OnRun(IBackgroundTaskInstance taskInstance)
        //{
        //   _deferral = taskInstance.GetDeferral();
        //    _taskInstance = taskInstance;

        //    // parcourir les tiles
        //    var tiles = await Windows.UI.StartScreen.SecondaryTile.FindAllForPackageAsync();
        //    foreach (var tile in tiles)
        //    {
        //        var result = await new MeteocielProvider().GetForecastForTileUpdate(tile.Arguments);

        //        ForecastTilesNotificationHelper.NotifyTile(tile.TileId, result.Item2, result.Item1);
        //    }

        //    _deferral.Complete();
        //}

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            _taskInstance = taskInstance;

            // parcourir les tiles
            var tiles = await Windows.UI.StartScreen.SecondaryTile.FindAllForPackageAsync();
            foreach (var tile in tiles)
            {
                var targetUrl = tile.Arguments;

                bool isCurrentLocation = false;
                isCurrentLocation = tile.Arguments == "CurrentLocation";

                if (isCurrentLocation)
                {
                    isCurrentLocation = true;

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
                            targetUrl = searchData.First().ElementUrl;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(targetUrl))
                {
                    var result = await new MeteocielProvider().GetForecastForTileUpdate(targetUrl);

                    ForecastTilesNotificationHelper.NotifyTile(tile.TileId, result.Item2, isCurrentLocation ? "Lieu actuel" : result.Item1);
                }
            }

            _deferral.Complete();
        }
    }
}
