using MeteoSkyWP.Business;
using MeteoSkyWP.Business.DataModel;
using MeteoSkyWP.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MeteoSkyWP.ViewModels
{
    public class SkiPageViewModel : BaseViewModel
    {
        #region properties
        private List<SkiReportElement> _observations;
        public List<SkiReportElement> Observations
        {
            get { return _observations; }
            set
            {
                _observations = value;
                RaisePropertyChanged();
            }
        }

        private SkiReportElement _currentSelectedElement;

        public SkiReportElement CurrentSelectedElement
        {
            get { return _currentSelectedElement; }
            set
            {
                _currentSelectedElement = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand GoHomeCommand { get; private set; }

        public ICommand GoToSkiStationReportsCommand { get; private set; }
        #endregion

         #region CTOR
        public SkiPageViewModel()
        {
            GoHomeCommand = new RelayCommand(GoHomeExecute);
            GoToSkiStationReportsCommand = new RelayCommand(GoToSkiStationReportsExecute);
        }
        #endregion

        #region Methods
        public async Task Initialize()
        {
            IsLoading = true;
         
            Observations = await new MeteocielProvider().GetSkiReports();
            CurrentSelectedElement = Observations.FirstOrDefault();

            IsLoading = false;
        }
        #endregion

        #region Command handlers
        public void GoHomeExecute(object parameter)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
                rootFrame.Navigate(typeof(RootPage));
        }

        public void GoToSkiStationReportsExecute(object parameter)
        {
            if (parameter is SkiStationReportElement)
            {
                string url = string.Format("/obs/neige_stations_ski.php?code={0}&heure=0&mois={1}&annee={2}", ((SkiStationReportElement)parameter).Code, DateTime.Now.Month, DateTime.Now.Year);

                Frame rootFrame = Window.Current.Content as Frame;

                if (rootFrame != null)
                    rootFrame.Navigate(typeof(SkiDetailPage), url);
            }
        }
        #endregion
    }
}
