using Parser.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.IO;

namespace Parser
{
    class Program
    {
        public static IDataAdapter Adapter;
        public static List<Industry> Industries = new List<Industry>();
        public static List<StockExchange> StockExchanges = new List<StockExchange>();
        public static List<WorldNews> WorldNews = new List<WorldNews>();
        public const string FinamDomen = "https://www.finam.ru";
        public const string RiaDomen = "https://ria.ru";
        public const string DownloadsPath = @"C:\Users\sdt1\Downloads";
        public static DateTime DateFrom = new DateTime(2015, 7, 10);
        public static DateTime DateTo = new DateTime(2021, 7, 10);

        public static string[] AllowStocks = new string[] 
        {
            "МосБиржа акции",
            "Санкт-Петербургская биржа",
            "МосБиржа Архив",
            "Санкт-Петербургская биржа Архив"
        };

        static void Main(string[] args)
        {
            Adapter = new MySQLDataAdapter();
            Connect();

            InitStockExchanges();
            InitIndustriesNames();

            WorldNewsController worldNewsController = new WorldNewsController();
            WorldNews = worldNewsController.Parse();
        }
        public static IDataAdapter Connect()
        {
            Adapter.Connect(
                new ConnectionSettings
                {
                    CharSet = "UTF8",

                    //Host = "127.0.0.1",
                    //Port = "3306",
                    //User = "root",
                    //Password = "1234",
                    //DefaultSchema = "stock_exchange",

                    Host = "s2.kts.tu-bryansk.ru",
                    Port = "3306",
                    User = "17IAS-AMISI.AndronovMP",
                    Password = "tV/0@lQRddb-#fk9",
                    DefaultSchema = "17IAS-AMISI_AndronovMP",
                });
            return Adapter;
        }

        static void InitStockExchanges()
        {
            List<Dictionary<string, string>> result = Adapter.GetQueryResult("select * from stock_exchanges");
            foreach (Dictionary<string, string> row in result)
            {
                StockExchange stock = new StockExchange();
                stock.ID = long.Parse(row["id"]);
                stock.Title = row["title"];
                if (!StockExchanges.Contains(stock))
                    StockExchanges.Add(stock);
            }

            string pattern = @"title: '(.*?)'";

            string text = Parser.GetStringFromHtml(FinamDomen + "/profile/np-rts/honeywell-international-inc/export/", Encoding.GetEncoding(1251));
            foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
            {
                if (StockExchanges.Find(s => s.Title == match.Groups[1].Value) == null)
                    StockExchanges.Add(new StockExchange(match.Groups[1].Value));
            }
        }

        static void InitIndustriesNames()
        {
            List<Dictionary<string, string>> result = Adapter.GetQueryResult("select * from industries");
            foreach (Dictionary<string, string> row in result)
            {
                Industry industry = new Industry();
                industry.ID = long.Parse(row["id"]);
                industry.Title = row["title"];
                industry.Url = row["url"];
                industry.InitCompanies();
                if (!Industries.Contains(industry))
                    Industries.Add(industry);
            }

            string pattern = @"<li class=""index__item--Igi""><a class=""index__menuItem--rwD"" href=""(.*?)"">(.*?)<";

            string text = Parser.GetStringFromHtml(FinamDomen + "/quotes/stocks/", Encoding.GetEncoding(1251));
            foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
            {
                if (Industries.Find(i => i.Url == match.Groups[1].Value && i.Title == match.Groups[2].Value) == null)
                {
                    Industry industry = new Industry(match.Groups[1].Value, match.Groups[2].Value);
                    industry.InitCompanies();
                    Industries.Add(industry);
                }
            }
        }
    }
}
