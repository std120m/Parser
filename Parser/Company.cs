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
        public List<StockExchange> StockExchanges { get; set; }
        public List<CompanyEvent> Events { get; set; }
        public List<CompanyNews> News { get; set; }
        public Dictionary<StockExchange, List<Quote>> Quotes { get; set; }

        public Company(string url, string title)
        {
            Url = url;
            Title = title;
            StockExchanges = new List<StockExchange>();
            Events = new List<CompanyEvent>();
            News = new List<CompanyNews>();
            Quotes = new Dictionary<StockExchange, List<Quote>>();
        }

        public Company(string url, string title, long id):this(url, title)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("Title", title);
            data.Add("Url", url);
            data.Add("Industry_id", id);
            ID = Program.Adapter.InsertRow("companies", data);
            InitNews();
            InitEvents();
            InitStockExchanges();
            GetQuotes();
        }

        public void InitStockExchanges()
        {
            string pattern = @"markets&quot;:[\w\W]+?title&quot;:&quot;(.*?)&quot;,&quot;quotes";

            string text = Parser.GetStringFromHtml(Parser.Domen + Url, Encoding.GetEncoding(1251));
            foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
            {
                StockExchange stockExchange = Program.StockExchanges.Find(s => s.Title == match.Groups[1].Value);
                stockExchange.Companies.Add(this);
                StockExchanges.Add(stockExchange);
            }
        }
        public void InitNews()
        {
            DateTime date;
            string subject;
            string url;

            string pattern = @"<tr class=""news"">[\W\w]+?date"">(.*?)<[\w\W]+?subject"">(.*?) <a href=""(.*?)"">(.*?)<";
            for (int year = 2015; year <= 2021; year++)
            {
                string text = Parser.GetStringFromHtml(Parser.Domen + Url + $"/mixed/?start-date={year}-01-01&end-date={year}-12-31", Encoding.GetEncoding(1251));
                foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
                {
                    date = DateTime.ParseExact(match.Groups[1].Value, "dd.MM.yyyy", null);
                    subject = match.Groups[2].Value + " " + match.Groups[4].Value;
                    url = match.Groups[3].Value;

                    string textPattern = @"handmade mid f-newsitem-text[\s\S]+?<p>([\S\s]*?)<\/p";
                    text = Parser.GetStringFromHtml(Parser.Domen + url, Encoding.GetEncoding(1251));
                    foreach (Match match1 in Regex.Matches(text, textPattern, RegexOptions.IgnoreCase))
                    {
                        News.Add(new CompanyNews(date, subject, match1.Groups[1].Value, ID));
                    }
                }
            }
        }
        public void InitEvents()
        {
            DateTime date;
            string subject;
            string url;

            string pattern = @"<tr class=""news"">[\W\w]+?date"">(.*?)<[\w\W]+?subject"">(.*?) <a href=""(.*?)"">(.*?)<";
            for (int year = 2015; year <= 2021; year++)
            {
                string text = Parser.GetStringFromHtml(Parser.Domen + Url + $"/corporate/?start-date={year}-01-01&end-date={year}-12-31", Encoding.GetEncoding(1251));
                foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
                {
                    date = DateTime.ParseExact(match.Groups[1].Value, "dd.MM.yyyy", null);
                    subject = match.Groups[2].Value + " " + match.Groups[4].Value;
                    url = match.Groups[3].Value;

                    string textPattern = @"handmade mid f-newsitem-text[\s\S]+?<p>([\S\s]*?)<\/p";
                    text = Parser.GetStringFromHtml(Parser.Domen + url, Encoding.GetEncoding(1251));
                    foreach (Match match1 in Regex.Matches(text, textPattern, RegexOptions.IgnoreCase))
                    {
                        Events.Add(new CompanyEvent(date, subject, match1.Groups[1].Value, ID));
                    }
                }
            }            
        }

        public void GetQuotes()
        {
            string paramsPattern = @"Finam.IssuerProfile.Main.issue[\w\W]+?""id"": (.*?), ""code"": ""(.*?)""[\w\W]+?""market"": {""id"": (.*?), ""title"": ""(.*?)""";

            string text = Parser.GetStringFromHtml(Parser.Domen + Url + "export", Encoding.GetEncoding(1251));

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
            DateTime dateFrom = new DateTime(2015, 7, 9);
            DateTime dateTo = new DateTime(2021, 7, 9);
            
            //Period keys
            //1  -  тики,
            //2  -  1 мин.,
            //3  -  5 мин.,
            //4  -  10 мин.,
            //5  -  15 мин.,
            //6  -  30 мин.,
            //7  -  1 час,
            //8  -  1 день,
            //9  -  1 неделя,
            //10 -  1 месяц
            int period = 7;
            string ext = ".txt";

            //date format keys
            //1 — ггггммдд,
            //2 — ггммдд,
            //3 — ддммгг,
            //4 — дд/мм/гг,
            //5 — мм/дд/гг
            int dtf = 4;

            //time format keys
            //1 — ччммсс,
            //2 — ччмм,
            //3 — чч:мм:сс,
            //4 — чч:мм
            int tmf = 4;

            //msor keys
            //0 — начала свечи,
            //1 — окончания свечи
            int msor = 1;

            //separator keys
            //1 — запятая(,),
            //2 — точка(.),
            //3 — точка с запятой(;),
            //4 — табуляция(»),
            //5 — пробел()
            int sep = 3;

            StockExchange stockExchange = StockExchanges.Find(s => s.Title == stockName);

            string url = $"http://export.finam.ru/{companyCode}_{dateFrom.ToString("yyMMdd")}_{dateTo.ToString("yyMMdd")}{ext}?market={stockId}&em={companyId}&code={companyCode}&apply=0&df={dateFrom.Day}&mf={dateFrom.Month - 1}&yf={dateFrom.Year}&from={dateFrom.ToString("dd.MM.yyyy")}&dt={dateTo.Day}&mt={dateTo.Month - 1}&yt={dateTo.Year}&to={dateTo.ToString("dd.MM.yyyy")}&p={period}&f={companyCode}_{dateFrom.ToString("yyMMdd")}_{dateTo.ToString("yyMMdd")}&e={ext}&cn={companyCode}&dtf={dtf}&tmf={tmf}&MSOR={msor}&mstime=on&mstimever=1&sep={sep}&sep2=1&datf=1&at=0";

            text = Parser.GetStringFromHtml(url, Encoding.UTF8);
            List<Quote> quotes = new List<Quote>();
            string[] lines = text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string[] tokens = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string date = tokens[2];
                string time = tokens[3];
                DateTime timestamp = DateTime.ParseExact(date + " " + time, "dd/MM/yy HH:mm", null);
                double open = double.Parse(tokens[4].Replace('.', ','));
                double close = double.Parse(tokens[7].Replace('.', ','));
                double low = double.Parse(tokens[6].Replace('.', ','));
                double high = double.Parse(tokens[5].Replace('.', ','));
                int count = int.Parse(tokens[8]);
                quotes.Add(new Quote(timestamp, open, close, low, high, count, ID, stockExchange.ID));
            }

            Quotes.Add(stockExchange, quotes);
            stockExchange.Quotes.Add(this, quotes);
        }
    }
}
