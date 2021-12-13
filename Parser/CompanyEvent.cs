using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    class CompanyEvent
    {
        public long ID { get; set; }
        public DateTime Date { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }

        public CompanyEvent() { }

        public CompanyEvent(DateTime date, string subject, string text)
        {
            Date = date;
            Subject = subject;
            Text = text;
        }

        public CompanyEvent(DateTime date, string subject, string text, long id) : this(date, subject, text)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("date", date.ToString("yyyy-MM-dd"));
            data.Add("subject", subject.Length > 100 ? subject.Substring(0, 100) : subject);
            data.Add("text", text.Length > 500 ? text.Substring(0, 500) : text);
            data.Add("company_id", id);
            ID = Program.Adapter.InsertRow("company_events", data);
        }
    }
}
