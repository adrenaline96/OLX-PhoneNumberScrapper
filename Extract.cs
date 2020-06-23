using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OLX_Scraper
{

    class Extract
    {
        
        
        public Extract()
        {
            
        }

        public List<String> ExtractAd(List<String> input, BackgroundWorker bgw, List<String> proxyList)
        {
           ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;
            List<String> ads = new List<String>();
            foreach (String link in input)
            {
                if (bgw.CancellationPending)
                {
                    //e.Cancel = true;
                    break;

                }
                try {

                    var url = link;
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                    //var client = new WebClient();
                    if (proxyList.Count > 0)
                    {
                        Random rnd = new Random();
                        int r = rnd.Next(proxyList.Count);
                        string proxyString = proxyList[r];
                        string[] proxyInfo = proxyString.Split(':');

                        //webRequest.KeepAlive = false;

                        // webRequest.ServicePoint.Expect100Continue = false;
                        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                       // webRequest.Timeout = 3000;
                       
                        webRequest.Proxy = new WebProxy(proxyInfo[0], Convert.ToInt32(proxyInfo[1]));
                        if (proxyInfo.Length > 2)
                        {
                            webRequest.Proxy.Credentials = new NetworkCredential(proxyInfo[2], proxyInfo[3]);
                        }

                    }
                    webRequest.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                    webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    webRequest.UserAgent="Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                    HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                    StreamReader responseReader = new StreamReader(response.GetResponseStream());
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains("marginright5 link linkWithHash"))
                            {

                                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                                doc.LoadHtml(line);
                                HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//a[@href]");

                                foreach (HtmlNode node in collection)
                                {

                                    ads.Add(node.GetAttributeValue("href", "default"));
                                }

                            }

                        }
                    }
                }
                catch(Exception ex)
                {
                    Logger.Log("ExtractAd: " + ex.Message);

                    continue;
                }

            }
            return ads;
        }
        public void ExtractNumbers(List<String> ads, BackgroundWorker bgw, ListView lv, List<String> proxyList)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;

            foreach (String ad in ads)
            {
                if (bgw.CancellationPending)
                {
                    
                    break;

                }
                try
                {
                    CookieContainer cookies = new CookieContainer();
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(anunt);
                    

                    String proxyHost = String.Empty;
                    String proxyPort = String.Empty;
                    String proxyUser = String.Empty;
                    String proxyPass = String.Empty;

                    if (proxyList.Count > 0)
                    {
                        Random rnd = new Random();
                        int r = rnd.Next(proxyList.Count);
                        string proxyString = proxyList[r];

                        //webRequest.KeepAlive = false;
                       // webRequest.ServicePoint.Expect100Continue = false;

                        string[] proxyInfo = proxyString.Split(':');
                        proxyHost = proxyInfo[0];
                        proxyPort = proxyInfo[1];

                       
                        webRequest.Proxy = new WebProxy(proxyInfo[0], Convert.ToInt32(proxyInfo[1]));

                        if (proxyInfo.Length > 2)
                        {
                            webRequest.Proxy.Credentials = new NetworkCredential(proxyInfo[2], proxyInfo[3]);
                            proxyUser = proxyInfo[2];
                            proxyPass = proxyInfo[3];
                        }
                    }
                   // webRequest.Timeout = 5000;
                    webRequest.CookieContainer = cookies;
                    
                    webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";
                    HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                    StreamReader responseReader = new StreamReader(response.GetResponseStream());

                    string sResponseHTML = responseReader.ReadToEnd();
                

                    string pattern = @"var phoneToken = \'(.*?)\';";
                    string token = string.Empty;
                    MatchCollection matches = Regex.Matches(sResponseHTML, pattern);
                    foreach (Match match in matches)
                    {
                        token = match.Groups[1].Value;
                    }



                    string patternName = "block brkword xx-large" + Convert.ToChar(34)+@">(.*)<";
                    string name = string.Empty;
                    MatchCollection matchesName = Regex.Matches(sResponseHTML, patternName);
                    foreach (Match match in matchesName)
                    {
                        name = match.Groups[1].Value;
                    }


                    string patternID = @"ID(.*)\.html";
                    string id = string.Empty;

                    MatchCollection matchesID = Regex.Matches(anunt, patternID);
                    foreach (Match match in matchesID)
                    {
                        id = match.Groups[1].Value;
                    }



                    Uri target = new Uri("https://www.olx.ro/ajax/misc/contact/phone/" + id + "/?pt=" + token);
                    HttpWebRequest nrRequest = (HttpWebRequest)WebRequest.Create("https://www.olx.ro/ajax/misc/contact/phone/" + id + "/?pt=" + token);

                    if (proxyHost != String.Empty && proxyPort != String.Empty)
                    {
                        nrRequest.Proxy = new WebProxy(proxyHost, Convert.ToInt32(proxyPort));
                        //nrRequest.KeepAlive = false;
                       // nrRequest.ServicePoint.Expect100Continue = false;

                        if (proxyUser != String.Empty && proxyPass != String.Empty)
                        {
                            nrRequest.Proxy.Credentials = new NetworkCredential(proxyUser, proxyPass);
                        }
                    }
                   // nrRequest.Timeout = 5000;
                    nrRequest.CookieContainer = cookies;
                    nrRequest.CookieContainer.Add(new Cookie("pt", token) { Domain = target.Host });
                    nrRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";

                    HttpWebResponse nrResponse = (HttpWebResponse)nrRequest.GetResponse();

                    StreamReader nrReader = new StreamReader(nrResponse.GetResponseStream());

                    string nrResponseHTML = nrReader.ReadToEnd();

                    string patternNr = "{"+Convert.ToChar(34)+"value"+Convert.ToChar(34)+":"+Convert.ToChar(34)+"(.*)\"}";
                    string number = string.Empty;

                    MatchCollection matchesNr = Regex.Matches(nrResponseHTML, patternNr);
                    foreach (Match match in matchesNr)
                    {
                        number = match.Groups[1].Value;
                    }

                    if (name != String.Empty && number != String.Empty && number != "0000 000 00")
                    {
                        if (number.Contains("span"))
                        {
                            String res = String.Empty;
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(number);
                            res = WebUtility.HtmlDecode(doc.DocumentNode.InnerText).Replace(@"<\/span>", "/");

                            ListViewItem lvis = new ListViewItem();

                            lvis.Text = name;
                            lvis.SubItems.Add(RemoveLast(res, "/"));
                            lv.Items.Add(lvis);
                        }
                        else
                        {
                            ListViewItem lvi = new ListViewItem();

                            lvi.Text = name;
                            lvi.SubItems.Add(number);
                            lv.Items.Add(lvi);
                        }
                    }

                }
                catch(Exception ex)
                {
                    Logger.Log("ExtractNumbers: "+ex.Message);
                    continue;
                }
            }

        }

        public static string RemoveLast(string text, string character)
        {
            if (text.Length < 1) return text;
            return text.Remove(text.ToString().LastIndexOf(character), character.Length);
        }
        public static IEnumerable<IEnumerable<T>> Partition<T>(IEnumerable<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.AsEnumerable();
            return splits;
        }
        public static void AddRange<T, S>(Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Collection is null");
            }

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                {
                    source.Add(item.Key, item.Value);
                }
                else
                {
                    // handle duplicate key issue here
                }
            }
        }

    }
}
