using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Parser
{
    class Industry
    {
        public long ID { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public List<Company> Companies { get; set; }

        public Industry(string url, string title)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("Title", title);
            data.Add("Url", url);
            ID = Program.Adapter.InsertRow("industries", data);
            Url = url;
            Title = title;
            Companies = new List<Company>();
            GetCompanies();
        }
        public void GetCompanies()
        {
            string pagePattern = @"""totalRecords"":(.*?),""pageSize"":(.*?)},";
            string url = $"https://www.finam.ru/api/quotesOnline?menuName=stocks_{Url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[2]}&pageNumber=";

            string text = Parser.GetStringFromHtml(url + 1, Encoding.GetEncoding(1251));
            int pageCount = 1;
            foreach (Match match in Regex.Matches(text, pagePattern, RegexOptions.IgnoreCase))
            {
                if (match.Value != string.Empty)
                {
                    pageCount = (int)(double.Parse(match.Groups[1].Value) / double.Parse(match.Groups[2].Value));
                    if (double.Parse(match.Groups[1].Value) / double.Parse(match.Groups[2].Value) > pageCount)
                        pageCount++;
                }
            }

            string companyPattern = @"name"":""(.*?)"",""url"":""(.*?)""";

            for (int i = 1; i <= pageCount; i++)
            {
                text = Parser.GetStringFromHtml(url + i, Encoding.UTF8);
                foreach (Match match in Regex.Matches(text, companyPattern, RegexOptions.IgnoreCase))
                {
                    if (match.Value != string.Empty)
                    {
                        Companies.Add(new Company(match.Groups[2].Value, match.Groups[1].Value, ID));
                    }
                }
            }
        }
    }
}
