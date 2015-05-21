using MeteoSkyWP.Business.DataModel;
using MeteoSkyWP.Common;
using MeteoSkyWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MeteoSkyWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SkiPage : BasePage
    {
        public SkiPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var dc = new SkiPageViewModel();
            this.DefaultViewModel = dc;

            await dc.Initialize();
        }

        private void StationLstItemClicked(object sender, ItemClickEventArgs e)
        {
            ((SkiPageViewModel)DefaultViewModel).GoToSkiStationReportsExecute(((SkiStationReportElement)e.ClickedItem));
        }
    }
}
