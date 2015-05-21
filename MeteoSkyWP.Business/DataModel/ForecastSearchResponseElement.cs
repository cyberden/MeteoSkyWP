using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MeteoSkyWP.DataModel
{
    [DataContract]
    public class ForecastSearchResponseElement
    {
        [DataMember]
        public string ElementUrl { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}
