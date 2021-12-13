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
        public long Count { get; set; }

        public Quote() { }

        public Quote(DateTime timestamp, double open, double close, double low, double high, long count)
        {
            Timestamp = timestamp;
            Open = open;
            Close = close;
            Low = low;
            High = high;
            Count = count;
        }

        public Quote(DateTime timestamp, double open, double close, double low, double high, long count, long companyID, long stockID):this(timestamp, open, close, low, high, count)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("timestamp", timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
            data.Add("open", open.ToString().Replace(',', '.'));
            data.Add("close", close.ToString().Replace(',', '.'));
            data.Add("low", low.ToString().Replace(',', '.'));
            data.Add("high", high.ToString().Replace(',', '.'));
            data.Add("count", count);
            data.Add("company_id", companyID);
            data.Add("stock_exchanges_id", stockID);
            ID = Program.Adapter.InsertRow("quotes", data);
        }
    }
}
