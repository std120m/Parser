using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parser
{
    static class Parser
    {
        public static string Domen = "https://www.finam.ru";
        public static string GetStringFromHtml(string url, Encoding encoding)
        {
            byte[] htmlData = new byte[0];
            using (WebClient client = new WebClient())
            {
                client.Encoding = encoding;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                try
                {
                    htmlData = client.DownloadData(url);
                    Console.WriteLine($"{url} --- was parsed");
                    Thread.Sleep(2000);
                }catch(Exception e)
                {
                    Console.WriteLine($"Error --- {e}");

                    Thread.Sleep(15000);
                    GetStringFromHtml(url, encoding);
                }
                return encoding.GetString(htmlData);
            }
        }
    }
}
