using System;

namespace Common
{
    public class News
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Src { get; set; }
        public News(string name, DateTime date, string src)
        {
            Name = name;
            Date = date;
            Src = src;
        }
    }
}
