using MeteoSkyWP.Common;
using MeteoSkyWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MeteoSkyWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ForecastPage : BasePage
    {
        public ForecastPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var url = (string)e.Parameter;

            var dc = new ForecastPageViewModel();
            this.DefaultViewModel = dc;

            if (string.IsNullOrEmpty(url)) //géoposition
            {
                dc.ShowCurrentPositionForecast = true;
                await dc.Initialize();
            }
            else
                await dc.Initialize(url);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (DefaultViewModel != null))
                ((ForecastPageViewModel)DefaultViewModel).ToggleWindVisibilityCommand.Execute(sender);
        }
    }
}
