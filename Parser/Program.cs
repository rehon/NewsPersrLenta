using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using Common;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace Parser
{
    internal class Program
    {
        public static float TimeUpdate = 60;
        public static int Chas = 1; // ? mb chas
        public static string Url = "https://lenta.ru/parts/news/";
        public static List<News> News = new List<News>();

        public static string DebugFile = "debug.txt";
        public static Stream StreamFile = File.Create(DebugFile);
        public static TextWriterTraceListener TextWriterTraceListener = new TextWriterTraceListener(StreamFile);

        static Timer Timer;

        public static async Task Main(string[] args)
        {
            Trace.Listeners.Add(TextWriterTraceListener);
            Trace.AutoFlush = true;

            Trace.WriteLine("parser starting");


            Console.WriteLine("TimeUpdate ");
            string update = Console.ReadLine(); 
            if(String.IsNullOrEmpty(update) == false)
                TimeUpdate = Convert.ToInt32(update);

            Console.WriteLine("Period ");
            string periode = Console.ReadLine();
            if (String.IsNullOrEmpty(periode) == false)
                Chas = Convert.ToInt32(periode);

            SyncGetContent();
            Timer = new Timer(TimeUpdate * 1000);
            Timer.Elapsed += TimerTic;
            Timer.Start();

            Console.ReadLine();
            Trace.Flush();
        }

        public static void StartServer()
        {
            Trace.WriteLine("start server");

            try
            {
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(iPEndPoint);
                socket.Listen(10);

                while (true)
                {
                    Socket Handler = socket.Accept();
                    byte[] buffer = new byte[10485760];
                    int byteLength = Handler.Receive(buffer);
                    string resive = JsonConvert.SerializeObject(byteLength);
                    string command = JsonConvert.DeserializeObject<string>(resive);

                    if(command == "start")
                    {
                        string json = JsonConvert.SerializeObject(buffer);
                        buffer = Encoding.UTF8.GetBytes(json);
                        Handler.Send(buffer);
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

        public static void TimerTic(object sender, ElapsedEventArgs e) => SyncGetContent();

        public static async void SyncGetContent()
        {
            string Content = await GetConten();
            PerseContent(Content);
        }

        public static async Task<string> GetConten()
        {
            Trace.WriteLine("start Get content");
            try
            {
                WebRequest request = WebRequest.Create(Url);
                using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
                {
                    string Content = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
                    Trace.WriteLine("GEt content gotovo");

                    return Content;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return null;
            }
        }

        public static void PerseContent(string Content)
        {
            try
            {
                Trace.WriteLine("start parse content");

                var html = new HtmlDocument();
                html.LoadHtml(Content);

                var doc = html.DocumentNode;
                HtmlNodeCollection htmlNodes = doc.SelectNodes("//*[@class='parts-page__item']");
                foreach (var htmlNode in htmlNodes)
                {
                    string Name = htmlNode.SelectSingleNode(".//*[@class='card-full-news__title']").InnerText;
                    string Date = htmlNode.SelectSingleNode(".//*[@class='card-full-news__info-item card-full-news__date']").InnerText;
                    string Src = htmlNode.GetAttributeValue("href", "");

                    DateTime newDate = DateTime.Parse(Date);

                    if (newDate.Hour != Chas) continue;

                    News.Add(new Common.News(Name, newDate, Src));
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }
    }
}