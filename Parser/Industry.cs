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

        public Industry()
        {
            Companies = new List<Company>();
        }

        public Industry(string url, string title) : this()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("title", title);
            data.Add("url", url);
            ID = Program.Adapter.InsertRow("industries", data);
            Url = url;
            Title = title;
        }
        public void InitCompanies()
        {
            InitCompaniesFromDB();

            string pagePattern = @"""totalRecords"":(.*?),""pageSize"":(.*?)},";
            string url = $"{Program.FinamDomen}/api/quotesOnline?menuName=stocks_{Url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[2]}&pageNumber=";

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
                    if (match.Value != string.Empty && Companies.Find(c => c.Url == match.Groups[2].Value && c.Title == match.Groups[1].Value) == null)
                    {
                        Company company = new Company(match.Groups[2].Value, match.Groups[1].Value, "", this.ID);
                        company.InitNews();
                        company.InitEvents();
                        company.InitStockExchanges();
                        company.GetQuotes();
                        Companies.Add(company);
                    }
                }
            }
        }

        public void InitCompaniesFromDB()
        {
            List<Dictionary<string, string>> result = Program.Adapter.GetQueryResult("select * from companies where industry_id = " + ID);
            foreach (Dictionary<string, string> row in result)
            {
                Company company = new Company();
                company.ID = long.Parse(row["id"]);
                company.Title = row["title"];
                company.Url = row["url"];
                company.Code = row["code"];
                company.InitNews();
                company.InitEvents();
                company.InitStockExchanges();
                company.GetQuotes();
                if (Companies.Find(c => c.Title == company.Title) == null)
                    Companies.Add(company);
            }
        }
    }
}
