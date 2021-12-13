using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser
{
    class WorldNewsController
    {
        public WorldNewsController() { }

        public List<WorldNews> Parse()
        {
            List<WorldNews> worldNews = new List<WorldNews>();
            InitNewsFromDB(worldNews);

            string dateFormat = "yyyyMMdd";
            DateTime date = Program.DateFrom;

            List<Dictionary<string, string>> result = Program.Adapter.GetQueryResult("SELECT `timestamp` FROM world_news order by `timestamp` desc limit 1");
            if (result.Count > 0)
                date = DateTime.ParseExact(result[0]["timestamp"], "dd.MM.yyyy H:mm:ss", null);


            while (date != Program.DateTo)
            {
                string pattern = @"list-item__content.*?<a.*?<a.*?list-item__title.*?>(.*?)<.*?list-item__date.*?>(.*?)" + (IsCurrentYear(date) ? " " : " (.*?) ") + "(.*?), (.*?)<";
                string url = $"/services/tagsearch/?date_start={date.ToString(dateFormat)}&date={date.ToString(dateFormat)}&tags%5B%5D=world";
                string text = Parser.GetStringFromHtml(Program.RiaDomen + url, Encoding.GetEncoding(65001));
                foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
                {
                    try
                    {
                        int timeIndex = IsCurrentYear(date) ? 4 : 5;
                        string title = match.Groups[1].Value;
                        DateTime newsDate = new DateTime(IsCurrentYear(date) ? DateTime.Now.Year : int.Parse(match.Groups[4].Value),
                                                        int.Parse(DateHelper.MonthToNumber[match.Groups[3].Value]),
                                                        int.Parse(match.Groups[2].Value),
                                                        int.Parse(match.Groups[timeIndex].Value.Split(':')[0]),
                                                        int.Parse(match.Groups[timeIndex].Value.Split(':')[1]), 0);

                        WorldNews news = new WorldNews(newsDate, title);
                        worldNews.Add(news);
                    }
                    catch (Exception e) { Console.WriteLine("Error: " + e.Message); }
                }
                date = date.AddDays(1);
            }

            return worldNews;
        }

        public void InitNewsFromDB(List<WorldNews> worldNews)
        {
            List<Dictionary<string, string>> result = Program.Adapter.GetQueryResult("select * from world_news");
            foreach (Dictionary<string, string> row in result)
            {
                WorldNews news = new WorldNews();
                news.ID = long.Parse(row["id"]);
                news.Title = row["title"];
                news.Timestamp = DateTime.ParseExact(row["timestamp"], "dd.MM.yyyy H:mm:ss", null);
                if (!worldNews.Contains(news))
                    worldNews.Add(news);
            }
        }

        private bool IsCurrentYear(DateTime date)
        {
            return date.Year == DateTime.Now.Year;
        }
    }
}
