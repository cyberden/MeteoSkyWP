using MeteoSkyWP.Common;
using MeteoSkyWP.DataModel;
using MeteoSkyWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Input;
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
    public sealed partial class RootPage : BasePage
    {
        float _defaultMapZoomFactor = 0;
        float _defaultForecastMapZoomFactor = 0;
        public RootPage()
        {
            this.DefaultViewModel = new RootPageViewModel();
            this.Loaded += RootPage_Loaded;
            this.InitializeComponent();
        }

        void RootPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_defaultMapZoomFactor == 0)
            {
                _defaultMapZoomFactor = (float)(scrl.ActualWidth / 768);
                scrl.MaxHeight = 768 * _defaultMapZoomFactor;
                scrl.MinZoomFactor = _defaultMapZoomFactor;
                scrl.ZoomToFactor(_defaultMapZoomFactor);

                _defaultForecastMapZoomFactor = (float)(scrl2.ActualWidth / 512);
                scrl2.MaxHeight = 450 * _defaultForecastMapZoomFactor;
                scrl2.MinZoomFactor = _defaultMapZoomFactor;
                scrl2.ZoomToFactor(_defaultForecastMapZoomFactor);
            }
        }

        public override async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            await ((RootPageViewModel)DefaultViewModel).InitializeViewModel();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ((RootPageViewModel)DefaultViewModel).GoToForecastExecute(((ForecastSearchResponseElement)e.ClickedItem));
        }

        private void ExtraLstViewItemClick(object sender, ItemClickEventArgs e)
        {
            ((RootPageViewModel)DefaultViewModel).GoToExtraExecute(((ExtraViewModel)e.ClickedItem));
        }

        private void StackPanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState != HoldingState.Started) return;

            FrameworkElement element = sender as FrameworkElement;
            if (element == null) return;

            // If the menu was attached properly, we just need to call this handy method
            FlyoutBase.ShowAttachedFlyout(element);
        }

        #region secondary tile
        public static Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }

        #endregion

        private void scrl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (scrl.ZoomFactor != _defaultMapZoomFactor)
            {
                scrl.ZoomToFactor(_defaultMapZoomFactor);
            }
            else
            {
                scrl.ZoomToFactor(_defaultMapZoomFactor * 2);
            }
        }

        private void scrl2_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (scrl2.ZoomFactor != _defaultForecastMapZoomFactor)
            {
                scrl2.ZoomToFactor(_defaultForecastMapZoomFactor);
            }
            else
            {
                scrl2.ZoomToFactor(_defaultForecastMapZoomFactor * 2);
            }
        }

        private void mapsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (scrl.ZoomFactor != _defaultMapZoomFactor)
            {
                scrl.ZoomToFactor(_defaultMapZoomFactor);
            }
        }

        private void forecastMapsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (scrl2.ZoomFactor != _defaultForecastMapZoomFactor)
            {
                scrl2.ZoomToFactor(_defaultForecastMapZoomFactor);
            }
        }
        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

    }
}
