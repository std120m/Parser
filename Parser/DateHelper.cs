using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    static class DateHelper
    {
        public static Dictionary<string, string> MonthToNumber = new Dictionary<string, string>()
        {
            { "января", "01" },
            { "февраля", "02" },
            { "марта", "03" },
            { "апреля", "04" },
            { "мая", "05" },
            { "июня", "06" },
            { "июля", "07" },
            { "августа", "08" },
            { "сентября", "09" },
            { "октября", "10" },
            { "ноября", "11" },
            { "декабря", "12" }
        };

        public static Dictionary<string, string> MonthNumberToShortName = new Dictionary<string, string>()
        {
            { "01", "Янв" },
            { "02", "Фев" },
            { "03", "Мар" },
            { "04", "Апр" },
            { "05", "Май" },
            { "06", "Июн" },
            { "07", "Июл" },
            { "08", "Авг" },
            { "09", "Сен" },
            { "10", "Окт" },
            { "11", "Ноя" },
            { "12", "Дек" }
        };
    }
}
