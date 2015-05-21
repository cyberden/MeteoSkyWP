using MeteoSkyWP.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MeteoSkyWP.Tools
{
    public class ForecastTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ForecastTemplate { get; set; }

        public DataTemplate ChartsTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var tuple = item as Tuple<string, object>;

            if (tuple != null && tuple.Item2 is ChartsForecastViewModel)
                return ChartsTemplate;
            else
                return ForecastTemplate;
        }
    }
}
