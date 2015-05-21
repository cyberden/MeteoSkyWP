using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoSkyWP.DataModel
{
    public class ForecastResult 
    {
        public string City { get; set; }

        public string Header { get; set; }

        public List<ForecastElement> ForecastElements { get; set; }

        public string TemperatureChartUrl { get; set; }

        public string RainChartUrl { get; set; }

        public bool IsLongForecast { get; set; }
    }
    
    public class ForecastElement
    {
        public string WeekDay { get; set; }

        public string Date { get; set; }

        public string Hour { get; set; }

        public TimeSpan Time { get; set; }

        public string Temperature { get; set; }

        public string TemperatureColor { get; set; }

        public string Rain { get; set; }

        public string RainColor { get; set; }

        public string RainForegroundColor { get; set; }

        public string Weather { get; set; }

        public string WeatherUrl { get; set; }

        public string WeatherIconPath { get; set; }

        public string WindDir { get; set; }

        public string WindDirUrl { get; set; }

        public string WindDirIconPath { get; set; }

        public string WindAverage { get; set; }

        public string WindPeak { get; set; }

        public string WindColor { get; set; }

        public string WindAverageForegroundColor { get; set; }

        public string WindPeakForegroundColor { get; set; }

        public string TileWeatherIconPath { get; set; }
    }
}
