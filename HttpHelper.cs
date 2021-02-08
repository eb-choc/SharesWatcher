using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SinaFinance_7X24
{
    public class HttpHelper
    {
        public string GetRemoteData(string url, string method = "", string contenttype = "")
        {
            try
            {
                if (!InternetGetConnectedState(0, 0)) return "";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = string.IsNullOrEmpty(method) ? "GET" : method;
                request.ContentType = string.IsNullOrEmpty(contenttype) ? "application/json;charset=utf-8" : contenttype;
                request.Timeout = 9000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream ResponseStream = response.GetResponseStream();
                StreamReader StreamReader = new StreamReader(ResponseStream, Encoding.Default);
                string re = StreamReader.ReadToEnd();
                StreamReader.Close();
                ResponseStream.Close();
                return re;
            }
            catch
            {
                return "";
            }
        }
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(int Description, int ReservedValue);
    }
}
