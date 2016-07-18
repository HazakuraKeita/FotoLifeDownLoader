using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FotoLifeDownLoader
{
    public class HatenaUser
    {
        public List<string> Images { get; private set; }
        private byte[] data;
        private CookieContainer cookie;

        public HatenaUser(string userId, string password)
        {
            Images = new List<string>();
            cookie = new CookieContainer();

            var vals = new Hashtable();
            vals["name"] = userId;
            vals["password"] = password;
            vals["persistent"] = 1;
            vals["location"] = @"http://f.hatena.ne.jp/";

            string param = "";
            foreach (string k in vals.Keys)
            {
                param += String.Format("{0}={1}&", k, vals[k]);
            }
            data = Encoding.ASCII.GetBytes(param);
        }

        public async Task Connect()
        {
            var url = "https://www.hatena.ne.jp/login";
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            req.CookieContainer = cookie;

            var reqStream = req.GetRequestStream();
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();

            var res = await req.GetResponseAsync();
            var resStream = res.GetResponseStream();
            var reader = new StreamReader(resStream, Encoding.GetEncoding("UTF-8"));
            var result = reader.ReadToEnd();
            reader.Close();
            resStream.Close();
        }

        public async Task LoadAllImageUri()
        {           
            var uri = "http://f.hatena.ne.jp/hazakurakeita/Hatena%20Blog/rss";
            var result = await LoadRss(uri);
            var nameOpenSearch = XNamespace.Get("http://a9.com/-/spec/opensearchrss/1.0/");
            var channel = XDocument.Parse(result);
            var totalResults = int.Parse((from nodes in channel.Descendants(nameOpenSearch + "totalResults") select nodes).Single().Value);
            var itemPerPage = int.Parse((from nodes in channel.Descendants(nameOpenSearch + "itemsPerPage") select nodes).Single().Value);
            var numOfPages = (totalResults / itemPerPage) + 1;

            for (var i = 1; i <= numOfPages; i++)
            {
                result = await LoadRss(uri + "?page=" + i);

                var nameRss = XNamespace.Get("http://purl.org/rss/1.0/");
                var rss = XDocument.Parse(result);
                var items = from nodes in rss.Descendants(nameRss + "item")
                            select nodes;
                foreach (var item in items)
                {
                    var nameHatena = XNamespace.Get("http://www.hatena.ne.jp/info/xmlns#");
                    var image = (from nodes in item.Descendants(nameHatena + "imageurl") select nodes).Single().Value;
                    Images.Add(image);
                }
            }     
        }

        public async Task DownLoad(string imageUri, string saveUri)
        {
            var req = (HttpWebRequest)WebRequest.Create(imageUri);
            req.CookieContainer = cookie;
            var res = await req.GetResponseAsync();
            var resStream = res.GetResponseStream();

            byte[] buffer = new byte[65535];
            var memStream = new MemoryStream();
            while (true)
            {
                int rb = resStream.Read(buffer, 0, buffer.Length);
                if (rb > 0)
                {
                    memStream.Write(buffer, 0, rb);
                }
                else
                {
                    break;
                }
            }

            var fileName = imageUri.Remove(0, imageUri.LastIndexOf('/') + 1);
            var fileStream = new FileStream(saveUri + @"\" + fileName + "img.jpg", FileMode.Create);
            byte[] wbuf = new byte[memStream.Length];
            memStream.Seek(0, SeekOrigin.Begin);
            memStream.Read(wbuf, 0, wbuf.Length);
            fileStream.Write(wbuf, 0, wbuf.Length);
            fileStream.Close();
        }

        private async Task<string> LoadRss(string uri)
        {
            var req = (HttpWebRequest)WebRequest.Create(uri);
            req.CookieContainer = cookie;
            var res = await req.GetResponseAsync();
            var resStream = res.GetResponseStream();
            var reader = new StreamReader(resStream, Encoding.GetEncoding("UTF-8"));
            var result = reader.ReadToEnd();
            reader.Close();
            resStream.Close();

            return result;
        }
    }
}
