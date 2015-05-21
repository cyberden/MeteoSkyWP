using MeteoSkyWP.Common;
using MeteoSkyWP.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoSkyWP.DAL
{
    public class DataSourceProvider
    {
        public static string FavoritesFileName = "favorites.xml";

        private static DataSourceProvider _dataSourceProvider = new DataSourceProvider();

        private ObservableCollection<ForecastSearchResponseElement> _favorites;

        public ObservableCollection<ForecastSearchResponseElement> Favorites
        {
            get { return _favorites; }
        }

        public static async Task<IEnumerable<ForecastSearchResponseElement>> GetFavorites(bool forceReadFromFile = false)
        {
            if ((_dataSourceProvider != null && _dataSourceProvider.Favorites == null)
                || forceReadFromFile)
                await _dataSourceProvider.GetFavoritesModelAsync();

            return _dataSourceProvider.Favorites;
        }

        public static async Task SaveFavorite(ForecastSearchResponseElement favorite)
        {
            var favorites = await GetFavorites();

            var previous = favorites.FirstOrDefault(f => f.ElementUrl == favorite.ElementUrl);
            if (previous != null)
            {
                _dataSourceProvider.Favorites.Remove(previous);
            }

            _dataSourceProvider.Favorites.Add(favorite);

            await StorageHelper.Save<ObservableCollection<ForecastSearchResponseElement>>(FavoritesFileName, _dataSourceProvider.Favorites);
        }

        public static async Task DeleteFavorite(string url)
        {
            var previous = _dataSourceProvider.Favorites.FirstOrDefault(f => f.ElementUrl == url);
            if (previous != null)
            {
                _dataSourceProvider.Favorites.Remove(previous);
            }

            await StorageHelper.Save<ObservableCollection<ForecastSearchResponseElement>>(FavoritesFileName, _dataSourceProvider.Favorites);
        }

        private async Task GetFavoritesModelAsync()
        {
            _favorites = await StorageHelper.Load<ObservableCollection<ForecastSearchResponseElement>>(FavoritesFileName);
        }

    }
}
