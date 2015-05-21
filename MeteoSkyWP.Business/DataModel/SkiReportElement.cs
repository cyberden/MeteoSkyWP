using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoSkyWP.Business.DataModel
{
    public class SkiReportElement
    {
        public string Department { get; set; }

        public List<SkiStationReportElement> Stations { get; set; }
    }

    public class SkiStationReportElement
    {
        public string Code { get; set; }

        public DateTime ReportDate { get; set; }

        public string ReportDateStr 
        {
            get { return ReportDate.ToString("dd/MM"); }
        }

        public string StationName { get; set; }

        public string Height { get; set; }

        public string SnowDepth { get; set; }

        public string FreshSnowDepth { get; set; }

        public string Temperature { get; set; }
    }
}
