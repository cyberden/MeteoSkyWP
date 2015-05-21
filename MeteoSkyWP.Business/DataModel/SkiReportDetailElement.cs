using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoSkyWP.Business.DataModel
{
    public class SkiReportDetailElement
    {
        public string Name { get; set; }

        public string Month { get; set; }

        public string Year { get; set; }

        public string SnowDepthDiagramUrl { get; set; }

        public string FreshSnowDepthDiagramUrl { get; set; }

        public List<StationReportElement> Reports {get;set;}
    }

    public class StationReportElement
    {
        public int Day { get; set; }

        public string SnowDepth { get; set; }

        public string FreshSnowDepth { get; set; }

        public string Temperature { get; set; }

        public string WindIconUrl { get; set; }

        public string Wind { get; set; }

        public string MaxTemperature { get; set; }

        public string MinTemperature { get; set; }
    }
}
