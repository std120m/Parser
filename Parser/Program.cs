using Parser.Adapter;
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
        public static IDataAdapter Adapter;
        public static List<Industry> Industries;
        public static List<StockExchange> StockExchanges;

        static void Main(string[] args)
        {
            Adapter = new MySQLDataAdapter();
            Connect();

            StockExchanges = GetStockExchanges();
            Industries = GetIndustriesNames();
        }
        public static IDataAdapter Connect()
        {
            Adapter.Connect(
                new ConnectionSettings
                {
                    CharSet = "UTF8",

                    Host = "127.0.0.1",
                    Port = "3306",
                    User = "root",
                    Password = "1234",
                    DefaultSchema = "stock_exchange",
                });
            return Adapter;
        }

        static List<StockExchange> GetStockExchanges()
        {
            string pattern = @"title: '(.*?)'";

            List<StockExchange> stockExchanges = new List<StockExchange>();

            string text = Parser.GetStringFromHtml("https://www.finam.ru/profile/np-rts/honeywell-international-inc/export/", Encoding.GetEncoding(1251));
            foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
            {
                stockExchanges.Add(new StockExchange(match.Groups[1].Value));
            }

            return stockExchanges;
        }

        static List<Industry> GetIndustriesNames()
        {
            string pattern = @"<li class=""index__item--Igi""><a class=""index__menuItem--rwD"" href=""(.*?)"">(.*?)<";

            List<Industry> industries = new List<Industry>();

            string text = Parser.GetStringFromHtml("https://www.finam.ru/quotes/stocks/", Encoding.GetEncoding(1251));
            foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
            {
                industries.Add(new Industry(match.Groups[1].Value, match.Groups[2].Value));
            }

            return industries;
        }
    }
}
