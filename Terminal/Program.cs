using System;
using System.Net.Sockets;
using System.Net;
using Common;
using Newtonsoft.Json;
using System.Text;

namespace Terminal
{
    internal class Programm
    {
        public static async Task Main(string[] args)
        {
            try
            {
                List<News> lNews = new List<News>();
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5194);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(iPEndPoint);
                if (socket.Connected)
                {
                    lNews.Clear();
                    byte[] buffer = new byte[10485760];
                    string command = JsonConvert.SerializeObject("start");
                    buffer = Encoding.UTF8.GetBytes(command);
                    socket.Send(buffer);

                    int byteLength = socket.Receive(buffer);
                    string resive = Encoding.UTF8.GetString(buffer, 0, byteLength);
                    lNews = JsonConvert.DeserializeObject<List<News>>(resive);
                    foreach (Common.News New in lNews)
                    {
                        Console.WriteLine(New.Name);
                        Console.WriteLine(New.Date);
                        Console.WriteLine(New.Src);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}