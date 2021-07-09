using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    class StockExchange
    {
        public long ID { get; set; }
        public string Title { get; set; }
        public List<Company> Companies { get; set; }
        public Dictionary<Company, List<Quote>> Quotes { get; set; }

        public StockExchange(string title)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("Title", title);
            ID = Program.Adapter.InsertRow("stock_exchanges", data);
            Title = title;
            Companies = new List<Company>();
            Quotes = new Dictionary<Company, List<Quote>>();
        }
    }
}
