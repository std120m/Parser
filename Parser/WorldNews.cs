using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    class WorldNews
    {
        public long ID { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }

        public WorldNews() { }

        public WorldNews(DateTime timestamp, string title)
        {
            Timestamp = timestamp;
            Title = title;

            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("timestamp", timestamp.ToString("yyyy-MM-dd HH:mm:00"));
            data.Add("title", title);
            ID = Program.Adapter.InsertRow("world_news", data);
        }
    }
}
