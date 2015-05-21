using MeteoSkyWP.Business;
using MeteoSkyWP.Business.DataModel;
using MeteoSkyWP.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MeteoSkyWP.ViewModels
{
    public class SkiDetailPageViewModel : BaseViewModel
    {
        #region Properties
        public string Url { get; set; }

        private SkiReportDetailElement _report;
        public SkiReportDetailElement Report
        {
            get { return _report; }
            set
            {
                _report = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand GoToNextMonthCommand { get; private set; }

        public ICommand GoToLastMonthCommand { get; private set; }
        #endregion

        #region Ctor
        public SkiDetailPageViewModel()
        {
            GoToNextMonthCommand = new RelayCommand(GoToNextMonthExecute);
            GoToLastMonthCommand = new RelayCommand(GoToLastMonthExecute);
        }
        #endregion

        #region Command Handlers
        public void GoToNextMonthExecute(object parameter)
        {
            string url = GetNextMonthUrl();

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
                rootFrame.Navigate(typeof(SkiDetailPage), url);
        }

        public void GoToLastMonthExecute(object parameter)
        {
            string url = GetLastMonthUrl();

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
                rootFrame.Navigate(typeof(SkiDetailPage), url);
        }
        #endregion

        #region Methods
        public async Task Initialize(string url)
        {
            IsLoading = true;

            Report = await new MeteocielProvider().GetSkiReportsDetail(url);

            IsLoading = false;
        }

        public string GetNextMonthUrl()
        {
            var m = Regex.Matches(Url, "\\d+");

            string code = m[0].Value;
            int month = int.Parse(m[2].Value);
            int year = int.Parse(m[3].Value);

            var nextMonthDt = new DateTime(year, month, 1).AddMonths(1);

            return string.Format("/obs/neige_stations_ski.php?code={0}&heure=0&mois={1}&annee={2}", code, nextMonthDt.Month, nextMonthDt.Year);
        }

        public string GetLastMonthUrl()
        {
            var m = Regex.Matches(Url, "\\d+");

            string code = m[0].Value;
            int month = int.Parse(m[2].Value);
            int year = int.Parse(m[3].Value);

            var lastMonthDt = new DateTime(year, month, 1).AddMonths(-1);

            return string.Format("/obs/neige_stations_ski.php?code={0}&heure=0&mois={1}&annee={2}", code, lastMonthDt.Month, lastMonthDt.Year);
        }
        #endregion
    }
}
