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

        public StockExchange() 
        {
            Companies = new List<Company>();
            Quotes = new Dictionary<Company, List<Quote>>();
        }
        public StockExchange(string title) : this()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("title", title);
            ID = Program.Adapter.InsertRow("stock_exchanges", data);
            Title = title;
        }
    }
}
