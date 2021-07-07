using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Parser
{
    class Program
    {
        //как получить ссылку для перехода к отрасли?
        // https://www.finam.ru/cache/N72Hgd54/icharts/icharts.js !!!
        // "2596154":.*?,"'(.*?)'": ""};

        //about <tr name="about">.*?href="(.*?)"

        static string url = "https://www.finam.ru/analysis/bundle00006/";
        static string companyNamesUrl = "https://www.finam.ru/analysis/bundle0000200001/";
        static string industriesNamesUrl = "https://www.finam.ru/cache/N72Hgd54/icharts/icharts.js";
        static string industriesNewsUrl = "https://www.finam.ru/analysis/bundle00006/?mode=0";

        static List<string> industriesNames;
        static List<string> industriesNews;
        static List<string> companyNames;

        static void Main(string[] args)
        {
            industriesNames = GetIndustriesNames(industriesNamesUrl);
            //industriesNews = GetIndustriesNews(industriesNewsUrl);
            companyNames = GetCompanyNames(companyNamesUrl);
        }

        static List<string> GetIndustriesNames(string url)
        {
            string pattern = @"(?<=реСторБ1Р1',').*?(?='];)";
            string text = GetStringFromHtml(url);
            foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
            {
                return new List<string>(match.ToString().Split(new string[] { "','" }, StringSplitOptions.RemoveEmptyEntries));
            }

            return null;
        }

        static List<string> GetCompanyNames(string url)
        {
            string pattern = @"<div class=""profile-list-item"">.*?"">(.*?)<";

            List<string> companyNames = new List<string>();

            string text = GetStringFromHtml(url);
            foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
            {
                if (match.Value != string.Empty)
                {
                    companyNames.Add(match.Groups[1].Value);
                }
            }

            return companyNames;
        }

        static int GetCountIndustriesNewsPages(string url)
        {
            string pattern = @"class=""pager"".*?count=""(.*?)""";

            string text = GetStringFromHtml(url);
            foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
            {
                if (match.Value != string.Empty)
                {
                    return int.Parse(match.Groups[1].Value);
                }
            }

            return -1;
        }

        static List<string> GetIndustriesNews(string url)
        {
            string pattern = @"(?<=""subject"">).*?(?=<\/a>)";

            List<string> news = new List<string>();
            int count = GetCountIndustriesNewsPages(url);

            for (int i = 1; i <= 30/*count*/; i++)
            {
                Console.WriteLine($"Парсим {i} страницу");
                Thread.Sleep(2000);
                string pageUrl = url.Substring(0, url.IndexOf('?')) + "?page=" + i.ToString();
                string text = GetStringFromHtml(pageUrl);
                foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
                {
                    if (match.Value != string.Empty)
                    {
                        string temp = string.Empty;

                        int beginId = match.Value.IndexOf("<a");
                        int endId = match.Value.IndexOf(">");

                        temp += match.Value.Substring(0, beginId);

                        temp += match.Value.Substring(endId + 1);

                        news.Add(temp);
                    }
                }
            }

            return news;
        }

        static string GetStringFromHtml(string url)
        {
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.GetEncoding(1251);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                byte[] htmlData = client.DownloadData(url);
                return Encoding.GetEncoding(1251).GetString(htmlData);
            }
        }
    }
}
