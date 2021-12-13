using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser
{
    class Company
    {
        public long ID { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public List<StockExchange> StockExchanges { get; set; }
        public List<CompanyEvent> Events { get; set; }
        public List<CompanyNews> News { get; set; }
        public Dictionary<StockExchange, List<Quote>> Quotes { get; set; }

        public Company()
        {
            StockExchanges = new List<StockExchange>();
            Events = new List<CompanyEvent>();
            News = new List<CompanyNews>();
            Quotes = new Dictionary<StockExchange, List<Quote>>();
        }

        public Company(string url, string title) : this()
        {
            Url = url;
            Title = title;
        }

        public Company(string url, string title, string code, long industryId) :this(url, title)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("title", title);
            data.Add("url", url);
            data.Add("code", code);
            data.Add("industry_id", industryId);
            ID = Program.Adapter.InsertRow("companies", data);
        }

        public void InitStockExchanges()
        {
            string allMarketsPattern = @"markets&quot;:(.+?)}]}]}";
            string currentMarketPattern = @"{&quot;id.*?title&quot;:&quot;([^\/]+?)&quot;,&quot;quotes&quot;:\[";

            string text = Parser.GetStringFromHtml(Program.FinamDomen + Url, Encoding.GetEncoding(1251));
            text = Regex.Matches(text, allMarketsPattern, RegexOptions.IgnoreCase)[0].Groups[1].Value;
            foreach (Match match in Regex.Matches(text, currentMarketPattern, RegexOptions.IgnoreCase))
            {
                StockExchange stockExchange = Program.StockExchanges.Find(s => s.Title == match.Groups[1].Value);
                stockExchange.Companies.Add(this);
                StockExchanges.Add(stockExchange);
            }
        }
        public void InitNews()
        {
            InitNewsFromDB();
            DateTime date;
            string subject;
            string url;

            string pattern = @"<tr class=""news"">[\W\w]+?date"">(.*?)<[\w\W]+?subject"">(.*?) <a href=""(.*?)"">(.*?)<";
            int startYear = Program.DateFrom.Year;

            List<Dictionary<string, string>> result = Program.Adapter.GetQueryResult("SELECT `date` FROM company_news where company_id = " + ID + " order by `date` desc limit 1");
            if (result.Count > 0)
                startYear = DateTime.ParseExact(result[0]["date"], "dd.MM.yyyy H:mm:ss", null).Year;

            for (int year = startYear; year < Program.DateTo.Year; year++)
            {
                string text = Parser.GetStringFromHtml(Program.FinamDomen + Url.Replace("quote", "profile") + $"/mixed/?start-date={year}-01-01&end-date={year}-12-31", Encoding.GetEncoding(1251));
                foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
                {
                    date = DateTime.ParseExact(match.Groups[1].Value, "dd.MM.yyyy", null);
                    subject = match.Groups[2].Value + " " + match.Groups[4].Value;
                    url = match.Groups[3].Value;

                    string textPattern = @"handmade mid f-newsitem-text[\s\S]+?<p>([\S\s]*?)<\/p";
                    text = Parser.GetStringFromHtml((url.Contains("http") ? "" : Program.FinamDomen) + url, Encoding.UTF8);
                    foreach (Match match1 in Regex.Matches(text, textPattern, RegexOptions.IgnoreCase))
                    {
                        if (News.Find(n => n.Subject == subject && n.Date == date) == null)
                        {
                            CompanyNews news = new CompanyNews(date, subject, match1.Groups[1].Value, ID);
                            News.Add(news);
                        }
                    }
                }
            }
        }

        public void InitNewsFromDB()
        {
            List<Dictionary<string, string>> result = Program.Adapter.GetQueryResult("select * from company_news where company_id = " + ID);
            foreach (Dictionary<string, string> row in result)
            {
                CompanyNews news = new CompanyNews();
                news.ID = long.Parse(row["id"]);
                news.Subject = row["subject"];
                news.Text = row["text"];
                news.Date = DateTime.ParseExact(row["date"], "dd.MM.yyyy H:mm:ss", null);
                if (!News.Contains(news))
                    News.Add(news);
            }
        }

        public void InitEvents()
        {
            InitEventsFromDB();

            DateTime date;
            string subject;
            string url;

            string pattern = @"<tr class=""news"">[\W\w]+?date"">(.*?)<[\w\W]+?subject"">(.*?) <a href=""(.*?)"">(.*?)<";

            int startYear = Program.DateFrom.Year;

            List<Dictionary<string, string>> result = Program.Adapter.GetQueryResult("SELECT `date` FROM company_events where company_id = " + ID + " order by `date` desc limit 1");
            if (result.Count > 0)
                startYear = DateTime.ParseExact(result[0]["date"], "dd.MM.yyyy H:mm:ss", null).Year;

            for (int year = startYear; year < Program.DateTo.Year; year++)
            {
                string text = Parser.GetStringFromHtml(Program.FinamDomen + Url.Replace("quote", "profile") + $"/corporate/?start-date={year}-01-01&end-date={year}-12-31", Encoding.GetEncoding(1251));
                foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
                {
                    date = DateTime.ParseExact(match.Groups[1].Value, "dd.MM.yyyy", null);
                    subject = match.Groups[2].Value + " " + match.Groups[4].Value;
                    url = match.Groups[3].Value;

                    string textPattern = @"handmade mid f-newsitem-text[\s\S]+?<p>([\S\s]*?)<\/p";
                    text = Parser.GetStringFromHtml((url.Contains("http") ? "" : Program.FinamDomen) + url, Encoding.GetEncoding(1251));
                    foreach (Match match1 in Regex.Matches(text, textPattern, RegexOptions.IgnoreCase))
                    {
                        if (Events.Find(e => e.Subject == subject && e.Date == date) == null)
                        {
                            Events.Add(new CompanyEvent(date, subject, match1.Groups[1].Value, ID));
                        }
                    }
                }
            }            
        }
        public void InitEventsFromDB()
        {
            List<Dictionary<string, string>> result = Program.Adapter.GetQueryResult("select * from company_events where company_id = " + ID);
            foreach (Dictionary<string, string> row in result)
            {
                CompanyEvent companyEvent = new CompanyEvent();
                companyEvent.ID = long.Parse(row["id"]);
                companyEvent.Subject = row["subject"];
                companyEvent.Text = row["text"];
                companyEvent.Date = DateTime.ParseExact(row["date"], "dd.MM.yyyy H:mm:ss", null);
                if (!Events.Contains(companyEvent))
                    Events.Add(companyEvent);
            }
        }

        public void GetQuotes()
        {
            GetQuotesFromDB();
            string paramsPattern = @"Finam.IssuerProfile.Main.issue[\w\W]+?""id"": (.*?), ""code"": ""(.*?)""[\w\W]+?""market"": {""id"": (.*?), ""title"": ""(.*?)""";

            string text = Parser.GetStringFromHtml(Program.FinamDomen + Url.Replace("quote", "profile") + "export", Encoding.GetEncoding(1251));

            string stockId = string.Empty;
            string stockName = string.Empty;
            string companyId = string.Empty;
            string companyCode = string.Empty;

            foreach (Match match in Regex.Matches(text, paramsPattern, RegexOptions.IgnoreCase))
            {
                companyId= match.Groups[1].Value;
                companyCode = match.Groups[2].Value;
                stockId = match.Groups[3].Value;
                stockName = match.Groups[4].Value;
            }
            Code = companyCode;
            Program.Adapter.UpdateRow("companies", ID, new Dictionary<string, object>() { { "code", companyCode } });

            StockExchange stockExchange = StockExchanges.Find(s => s.Title == stockName);

            if (!Quotes.Keys.Contains(stockExchange))
            {
                List<Quote> quotes = new List<Quote>();

                string filepath = Parser.ParseQuotes(this);
                using (var reader = new StreamReader(filepath))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(',');
                        if (values.Length == 1)
                            continue;
                        string date = values[2];
                        string time = values[3];
                        DateTime timestamp = DateTime.ParseExact(date + " " + time, "yyyyMMdd HHmmss", null);
                        double open = double.Parse(values[4].Replace('.', ','));
                        double close = double.Parse(values[7].Replace('.', ','));
                        double low = double.Parse(values[6].Replace('.', ','));
                        double high = double.Parse(values[5].Replace('.', ','));
                        long count = long.Parse(values[8]);

                        quotes.Add(new Quote(timestamp, open, close, low, high, count, ID, stockExchange.ID));
                    }
                }
                Quotes.Add(stockExchange, quotes);
                stockExchange.Quotes.Add(this, quotes);
            }
        }

        public void GetQuotesFromDB()
        {
            foreach (StockExchange stockExchange in StockExchanges)
            {
                List<Dictionary<string, string>> result = Program.Adapter.GetQueryResult($"select * from quotes where company_id = '{ID}' and  stock_exchanges_id = '{stockExchange.ID}'");
                foreach (Dictionary<string, string> row in result)
                {
                    Quote quote = new Quote();
                    quote.ID = long.Parse(row["id"]);
                    quote.Open = double.Parse(row["open"]);
                    quote.Close = double.Parse(row["close"]);
                    quote.Low = double.Parse(row["low"]);
                    quote.High = double.Parse(row["high"]);
                    quote.Count = long.Parse(row["count"]);
                    quote.Timestamp = DateTime.ParseExact(row["timestamp"], "dd.MM.yyyy H:mm:ss", null);

                    if (!Quotes.Keys.Contains(stockExchange))
                        Quotes.Add(stockExchange, new List<Quote>());

                    if (!Quotes[stockExchange].Contains(quote))
                        Quotes[stockExchange].Add(quote);
                }
            }
        }
    }
}
