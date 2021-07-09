using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    class Quote
    {
        public long ID { get; set; }
        public DateTime Timestamp { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Low { get; set; }
        public double High { get; set; }
        public int Count { get; set; }

        public Quote() { }

        public Quote(DateTime timestamp, double open, double close, double low, double high, int count)
        {
            Timestamp = timestamp;
            Open = open;
            Close = close;
            Low = low;
            High = high;
            Count = count;
        }

        public Quote(DateTime timestamp, double open, double close, double low, double high, int count, long companyID, long stockID):this(timestamp, open, close, low, high, count)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("Timestamp", timestamp.ToString("yyyy-MM-dd HH:mm:00"));
            data.Add("Open", open.ToString().Replace(',', '.'));
            data.Add("Close", close.ToString().Replace(',', '.'));
            data.Add("Low", low.ToString().Replace(',', '.'));
            data.Add("High", high.ToString().Replace(',', '.'));
            data.Add("Count", count);
            data.Add("Company_id", companyID);
            data.Add("Stock_exchanges_id", stockID);
            ID = Program.Adapter.InsertRow("quotes", data);
        }
    }
}
