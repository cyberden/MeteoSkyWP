using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MeteoSkyWP.Tools
{
    public class IsStringNotNullToVisibilityConverter : IValueConverter
    {
        public bool Not { get; set; }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is string && !string.IsNullOrEmpty((string)value)) ^ Not? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
