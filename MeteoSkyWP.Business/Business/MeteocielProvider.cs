using HtmlAgilityPack;
using MeteoSkyWP.Business.Common;
using MeteoSkyWP.Business.DataModel;
using MeteoSkyWP.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MeteoSkyWP.Business
{
    public class MeteocielProvider : IForecastProvider
    {
        private static string RootUrl = "http://www.meteociel.fr/";

        public static Dictionary<string, string> IconMappings { get; set; }

        static MeteocielProvider()
        {
            IconMappings = new Dictionary<string, string>();

            IconMappings.Add("averse_grele.gif", "hail-{0}.png");
            IconMappings.Add("averse_neige.gif", "little_snow-{0}.png");
            IconMappings.Add("averse_orage.gif", "chance_of_storm-{0}.png");
            IconMappings.Add("averse_pluie.gif", "chances_of_rain-{0}.png");
            IconMappings.Add("averse_pluiefaible.gif", "chances_of_rain-{0}.png");
            IconMappings.Add("averse_pluieneige.gif", "sleet-{0}.png");
            IconMappings.Add("avserse_verglas.gif", "sleet-2-{0}.png");
            IconMappings.Add("brouillard.gif", "fog_day-{0}.png");
            IconMappings.Add("grele.gif", "hail-{0}.png");
            IconMappings.Add("mitige.gif", "partly_cloudy_day-{0}.png");
            IconMappings.Add("neige.gif", "snow-{0}.png");
            IconMappings.Add("nuageux.gif", "cloud-{0}.png");
            IconMappings.Add("oragefaible.gif", "storm-{0}.png");
            IconMappings.Add("peu_nuageux.gif", "small_clouds-{0}.png");
            IconMappings.Add("pluie.gif", "rain-{0}.png");
            IconMappings.Add("pluie_neige.gif", "sleet-{0}.png");
            IconMappings.Add("soleil.gif", "sun-{0}.png");
            IconMappings.Add("verglas.gif", "sleet-2-{0}.png");

            IconMappings.Add("sso.png", "sso.png");
            IconMappings.Add("sse.png", "sse.png");
            IconMappings.Add("so.png", "so.png");
            IconMappings.Add("se.png", "se.png");
            IconMappings.Add("s.png", "s.png");
            IconMappings.Add("oso.png", "oso.png");
            IconMappings.Add("ono.png", "ono.png");
            IconMappings.Add("o.png", "o.png");
            IconMappings.Add("nno.png", "nno.png");
            IconMappings.Add("nne.png", "nne.png");
            IconMappings.Add("ne.png", "ne.png");
            IconMappings.Add("n.png", "n.png");
            IconMappings.Add("ene.png", "ene.png");
            IconMappings.Add("e.png", "e.png");
        }

        public async Task<List<Tuple<string, string>>> GetForecastmaps()
        {
            var forecastSearchElements = new List<ForecastSearchResponseElement>();
            try
            {
                string htmlPage = string.Empty;

                // Appel à la page de recherche.
                htmlPage = await GetStringAsync(RootUrl + "prevision/genprev0.php");

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlPage);

                var node = htmlDocument.DocumentNode;

                var nodes = node.Descendants("a").Where(n => n.GetAttributeValue("href", string.Empty)
                                                 .StartsWith("genprev"))
                                                 .Select(n => new Tuple<string, string>
                                                     (
                                                        n.InnerText, 
                                                        RootUrl + "prevision/" + n.GetAttributeValue("href", string.Empty).Replace("gen", "").Replace(".php", ".png")
                                                     ));

                return new List<Tuple<string, string>>(nodes);

            }
            catch (Exception ex)
            {

            }

            return new List<Tuple<string, string>>();
        }
        public async Task<List<ForecastSearchResponseElement>> SearchForecastData(string searchString)
        {
            var forecastSearchElements = new List<ForecastSearchResponseElement>();
            try
            {
                string searchUrl = "prevville.php?action=getville&ville={0}";
                string chosenUrl = null;

                string htmlPage = string.Empty;

                // Appel à la page de recherche.
                htmlPage = await GetStringAsync(RootUrl + string.Format(searchUrl, StringHelper.RemoveDiacritics(searchString.ToLower(), StringHelper.DictionaryDef.French)));

                var reg = new Regex("previsions/\\d*/\\w*.htm");
                var matches = reg.Matches(htmlPage);

                // cas où on a directement trouvé la correspondance.
                if (matches.Count == 1)
                {
                    chosenUrl = matches[0].Value;
                    forecastSearchElements.Add(new ForecastSearchResponseElement() { ElementUrl = chosenUrl });
                }
                else // sinon choix des villes.
                {
                    HtmlDocument htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(htmlPage);
                    List<string> matchesLinks = new List<string>();

                    foreach (Match match in matches)
                        matchesLinks.Add(match.Value);

                    var links = htmlDocument.DocumentNode.Descendants("a").Where(a => matchesLinks.Contains(a.Attributes["href"].Value));

                    for (int i = 0; i < links.Count(); i++)
                    {
                        forecastSearchElements.Add(new ForecastSearchResponseElement() { ElementUrl = matchesLinks[i], Name = links.ElementAt(i).InnerText + links.ElementAt(i).NextSibling.InnerText });
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return forecastSearchElements;
        }

        public async Task<ForecastResult> GetForecast(string targetUrl, bool hourDetail)
        {
            string htmlPage;
            if (hourDetail)
                htmlPage = await GetStringAsync(RootUrl + targetUrl.Replace("previsions", "previsions-wrf-1h"));
            else
                htmlPage = await GetStringAsync(RootUrl + targetUrl);

            if (htmlPage.Contains("Données non disponibles"))
                htmlPage = await GetStringAsync(RootUrl + targetUrl);

            var result = ExtractWeatherForecast(htmlPage);

            result.IsLongForecast = targetUrl.Contains("tendances");
            return result;
        }

        public async Task<Tuple<string, ForecastElement>> GetForecastForTileUpdate(string targetUrl)
        {
            var result = await GetForecast(targetUrl.Replace("tendances", "previsions"), false);
            return new Tuple<string, ForecastElement>(result.City, result.ForecastElements.GroupBy(elt => elt.WeekDay).First().LastOrDefault(elt => elt.Time < DateTime.Now.TimeOfDay) ?? result.ForecastElements.First());
        }

        private ForecastResult ExtractWeatherForecast(string htmlPage)
        {
            try
            {
                ForecastResult result = new ForecastResult() { ForecastElements = new List<ForecastElement>() };

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlPage);

                var node = htmlDocument.DocumentNode;

                //result.City = node.Descendants("tr").First(t => t.Attributes.Contains("class") && t.Attributes.Any(att => att.Name == "class" && att.Value == "titre_categorie")).ChildNodes[1].FirstChild.Descendants("b").First().InnerText.Split(new string[] { "ville" }, StringSplitOptions.None)[1].Trim();

                var childNode = node.Descendants("td").First(t => t.Attributes.Contains("width") && t.Attributes.Any(att => att.Name == "width" && att.Value == "797")).ChildNodes[5].FirstChild;

                if (childNode.ChildNodes.Count > 1)
                    result.Header = childNode.ChildNodes[1].InnerText;
                else
                    result.Header = childNode.FirstChild.InnerText;

                result.City = result.Header.Split(new string[] { "jours pour" }, StringSplitOptions.None)[1].Trim();

                result.TemperatureChartUrl = node.Descendants("img").First(t => t.Attributes.Contains("title") && t.Attributes.Any(att => att.Name == "title" && att.Value.ToLower().StartsWith("graphe des températures"))).GetAttributeValue("src", null);

                result.RainChartUrl = node.Descendants("img").First(t => t.Attributes.Contains("title") && t.Attributes.Any(att => att.Name == "title" && att.Value.ToLower().StartsWith("graphe des précipitations"))).GetAttributeValue("src", null);

                var table = node.Descendants("table").First(t => t.Attributes.Contains("border") && t.Attributes.Any(att => att.Name == "border" && att.Value == "1"));

                // First two rows are headers.
                string currentWeekDay = null;
                string currentDate = null;

                int cpt = 0;
                foreach (var row in table.Descendants("tr"))
                {
                    cpt++;

                    if (cpt > 2)
                    {

                        int cptCol = 0;
                        var currentForecastElement = new ForecastElement();


                        var descendants = row.Descendants("td");
                        if (descendants.Count() != 10)
                        {
                            cptCol = 1;
                        }
                        else
                        {
                            currentWeekDay = null;
                            currentDate = null;
                        }

                        foreach (var col in descendants)
                        {
                            switch ((ForecastColumnsEnum)cptCol)
                            {
                                case ForecastColumnsEnum.Day:
                                    {
                                        currentWeekDay = col.InnerHtml.Split(new string[] { "<br>" }, StringSplitOptions.None)[0].ToLower();
                                        currentDate = col.InnerHtml.Split(new string[] { "<br>" }, StringSplitOptions.None)[1].ToLower();
                                    }
                                    break;
                                case ForecastColumnsEnum.Time:
                                    {
                                        currentForecastElement.Hour = col.InnerText;
                                        currentForecastElement.Time = TimeSpan.Parse(currentForecastElement.Hour);
                                    }
                                    break;
                                case ForecastColumnsEnum.Temperature:
                                    {
                                        currentForecastElement.Temperature = col.InnerText;
                                        currentForecastElement.TemperatureColor = col.GetAttributeValue("bgcolor", "#000000");
                                    }
                                    break;
                                case ForecastColumnsEnum.WindDir:
                                    {
                                        var img = col.Descendants("img").First();
                                        currentForecastElement.WindDir = img.GetAttributeValue("title", null);
                                        currentForecastElement.WindDirUrl = img.GetAttributeValue("src", null);

                                        var shortenUrl = currentForecastElement.WindDirUrl.Split('/').Last();

                                        currentForecastElement.WindDirIconPath = string.Format("/Assets/icons/{0}", currentForecastElement.WindDirUrl.Split('/').Last());
                                    }
                                    break;
                                case ForecastColumnsEnum.WindAverage:
                                    {
                                        currentForecastElement.WindAverage = col.InnerText;
                                        currentForecastElement.WindColor = col.GetAttributeValue("bgcolor", "#000000");
                                    }
                                    break;
                                case ForecastColumnsEnum.WindMax:
                                    {
                                        currentForecastElement.WindPeak = col.InnerText;
                                    }
                                    break;
                                case ForecastColumnsEnum.Rain:
                                    {
                                        currentForecastElement.Rain = col.InnerText;
                                        currentForecastElement.RainColor = col.GetAttributeValue("bgcolor", "#000000");

                                        if (currentForecastElement.Rain == "--")
                                            currentForecastElement.RainForegroundColor = "white";
                                        else
                                            currentForecastElement.RainForegroundColor = "black";
                                    }
                                    break;
                                case ForecastColumnsEnum.Humidity:
                                    break;
                                case ForecastColumnsEnum.Pressure:
                                    break;
                                case ForecastColumnsEnum.Weather:
                                    {
                                        var img = col.Descendants("img").First();
                                        currentForecastElement.Weather = img.GetAttributeValue("title", null);
                                        currentForecastElement.WeatherUrl = img.GetAttributeValue("src", null);

                                        var shortenUrl = currentForecastElement.WeatherUrl.Split('/').Last();

                                        currentForecastElement.WeatherIconPath = string.Format("/Assets/icons/{0}", currentForecastElement.WeatherUrl.Split('/').Last());
                                        currentForecastElement.TileWeatherIconPath = "/Assets/square150x150Tile.png";

                                        if (IconMappings.ContainsKey(shortenUrl))
                                        {
                                            currentForecastElement.WeatherIconPath = string.Format("/Assets/MetroIcons/White/{0}", string.Format(IconMappings[shortenUrl], 26));
                                            currentForecastElement.TileWeatherIconPath = string.Format("/Assets/MetroIcons/White/{0}", string.Format(IconMappings[shortenUrl], 256));
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            cptCol++;
                        }

                        currentForecastElement.WeekDay = currentWeekDay;
                        currentForecastElement.Date = currentDate;

                        result.ForecastElements.Add(currentForecastElement);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<SkiReportElement>> GetSkiReports()
        {
            string htmlPage;

            htmlPage = await GetStringAsync(RootUrl + "/obs/neige_stations_ski.php");

            var result = ExtractSkiReports(htmlPage);

            return result;
        }

        public List<SkiReportElement> ExtractSkiReports(string htmlPage)
        {
            try
            {
                List<SkiReportElement> result = new List<SkiReportElement>();

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlPage);

                var node = htmlDocument.DocumentNode;

                var childNode = node.Descendants("td").Where(t => t.Attributes.Contains("width") && t.Attributes.Any(att => att.Name == "width" && att.Value == "797")).ElementAt(1);
                var subChild = childNode.ChildNodes.ElementAt(1);

                SkiReportElement currentElement = null;
                foreach(var child in subChild.ChildNodes.Where(s => s.OriginalName == "b" || s.OriginalName == "table"))
                {
                    if (child.OriginalName == "b")
                    {
                        currentElement = new SkiReportElement() { Department = child.InnerText.Replace(':', ' ').Trim(), Stations = new List<SkiStationReportElement>() };
                        result.Add(currentElement);
                    }
                    else
                    {
                        int cpt = 0;
                        foreach (var row in child.Descendants("tr"))
                        {
                            int cptColumns = 0;
                            if (cpt > 0)
                            {
                                var currentStationElement = new SkiStationReportElement();

                                foreach(var desc in row.Descendants("td"))
                                {
                                    switch ((SkiForecastColumnsEnum)cptColumns)
                                    {
                                        case SkiForecastColumnsEnum.Day:
                                            {
                                                currentStationElement.ReportDate = DateTime.Parse(desc.InnerText);
                                            }
                                            break;
                                        case SkiForecastColumnsEnum.Station:
                                            {
                                                currentStationElement.StationName = desc.InnerText;
                                                currentStationElement.StationName = currentStationElement.StationName.Substring(0, currentStationElement.StationName.LastIndexOf('(')).Trim();

                                                currentStationElement.Code = desc.FirstChild.Attributes["href"].Value.Split('=')[1];
                                            }
                                            break;
                                        case SkiForecastColumnsEnum.Height:
                                            {
                                                currentStationElement.Height = desc.InnerText;
                                            }
                                            break;
                                        case SkiForecastColumnsEnum.SnowDepth:
                                            {
                                                currentStationElement.SnowDepth = desc.InnerText;
                                            }
                                            break;
                                        case SkiForecastColumnsEnum.FreshSnowDepth:
                                            {
                                                currentStationElement.FreshSnowDepth = desc.InnerText;
                                            }
                                            break;
                                        case SkiForecastColumnsEnum.Temperature:
                                            {
                                                currentStationElement.Temperature = desc.InnerText;
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                    cptColumns++;
                                }

                                currentElement.Stations.Add(currentStationElement);
                            }

                            cpt++;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<SkiReportDetailElement> GetSkiReportsDetail(string url)
        {
            string htmlPage;

            htmlPage = await GetStringAsync(RootUrl + url);

            var result = ExtractSkiReportDetail(htmlPage);

            return result;
        }

        public SkiReportDetailElement ExtractSkiReportDetail(string htmlPage)
        {
            try
            {
                htmlPage = htmlPage.Replace("</tr><td", "</tr><tr><td");

                var result = new SkiReportDetailElement() { Reports = new List<StationReportElement>() };

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlPage);

                var node = htmlDocument.DocumentNode;

                var childNode = node.Descendants("td").Where(t => t.Attributes.Contains("width") && t.Attributes.Any(att => att.Name == "width" && att.Value == "797")).ElementAt(1);
                var subChild = childNode.ChildNodes.ElementAt(1);

                result.Name = subChild.FirstChild.InnerText;

                foreach(var select in childNode.Descendants("select"))
                {
                    if (select.GetAttributeValue("name", string.Empty) == "mois")
                    {
                        var selectedElement = select.Descendants("option").FirstOrDefault(o => o.Attributes.Contains("selected"));
                        result.Month = selectedElement.InnerText;
                    }
                    else if (select.GetAttributeValue("name", string.Empty) == "annee")
                    {
                        var selectedElement = select.Descendants("option").FirstOrDefault(o => o.Attributes.Contains("selected"));
                        result.Year = selectedElement.InnerText;
                    }
                }

                var cpt = 0;
                foreach(var table in childNode.Descendants("table"))
                {
                    if (cpt == 0)
                    {
                        var row = table.Descendants("tr").First();

                        var cptImg = 0;
                        foreach(var img in row.Descendants("img"))
                        {
                            if (cptImg == 0)
                                result.SnowDepthDiagramUrl = img.Attributes["src"].Value;
                            else if (cptImg == 1)
                                result.FreshSnowDepthDiagramUrl = img.Attributes["src"].Value;

                            cptImg++;
                        }
                    }
                    else if (cpt == 1)
                    {
                        var cptRow = 0;
                        foreach(var row in table.Descendants("tr"))
                        {
                            var cptCol = 0;

                            if (cptRow > 0)
                            {
                                if (row.FirstChild.GetAttributeValue("colspan", 0) == 7)
                                    break;

                                var report = new StationReportElement();

                                foreach (var col in row.Descendants("td"))
                                {
                                    switch ((SkiForecastDetailColumnsEnum)cptCol)
                                    {
                                        case SkiForecastDetailColumnsEnum.Day:
                                            {
                                                report.Day = int.Parse(col.InnerText);
                                            }
                                            break;
                                        case SkiForecastDetailColumnsEnum.FreshSnowDepth:
                                            {
                                                report.FreshSnowDepth = col.InnerText;
                                            }
                                            break;
                                        case SkiForecastDetailColumnsEnum.SnowDepth:
                                            {
                                                report.SnowDepth = col.InnerText;
                                            }
                                            break;
                                        case SkiForecastDetailColumnsEnum.Temperature:
                                            {
                                                report.Temperature = col.InnerText;
                                            }
                                            break;
                                        case SkiForecastDetailColumnsEnum.Wind:
                                            {
                                                report.Wind = col.InnerText.Trim();
                                            }
                                            break;
                                        case SkiForecastDetailColumnsEnum.MaxTemp:
                                            {
                                                report.MaxTemperature = col.InnerText;
                                            }
                                            break;
                                        case SkiForecastDetailColumnsEnum.MinTemp:
                                            {
                                                report.MinTemperature = col.InnerText;
                                            }
                                            break;
                                        default:
                                            break;
                                    }

                                    cptCol++;
                                }

                                result.Reports.Add(report);
                            }

                            cptRow++;
                        }
                    }

                    cpt++;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<string> GetStringAsync(string url)
        {
            using (var client = new HttpClient())
            {
                var byteData = await client.GetByteArrayAsync(url);
                return WebUtility.HtmlDecode(Encoding.GetEncoding("iso-8859-1").GetString(byteData, 0, byteData.Length - 1));
            }
        }
    }

    public enum ForecastColumnsEnum
    {
        Day = 0,
        Time = 1,
        Temperature = 2,
        WindDir = 3,
        WindAverage = 4,
        WindMax = 5,
        Rain = 6,
        Humidity = 7,
        Pressure = 8,
        Weather = 9
    }

    public enum SkiForecastColumnsEnum
    {
        Day = 0,
        Station = 1,
        Height = 2,
        SnowDepth = 3,
        FreshSnowDepth = 4,
        Temperature = 5
    }

    public enum SkiForecastDetailColumnsEnum
    {
        Day = 0,
        FreshSnowDepth = 1,
        SnowDepth = 2,
        Temperature = 3,
        Wind = 4,
        MaxTemp = 5,
        MinTemp = 6
    }
}
