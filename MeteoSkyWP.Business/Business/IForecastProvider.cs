using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace MeteoSkyWP.Business
{
    interface IForecastProvider
    {
        Task<List<Tuple<string, string>>> GetForecastmaps();
        System.Threading.Tasks.Task<MeteoSkyWP.DataModel.ForecastResult> GetForecast(string targetUrl, bool hourDetail);
        System.Threading.Tasks.Task<System.Collections.Generic.List<MeteoSkyWP.DataModel.ForecastSearchResponseElement>> SearchForecastData(string searchString);
    }
}
