using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
{
    class Program
    {
        static int DummyClientCount { get; } = 500;

        static void Main(string[] args)
        {
            Thread.Sleep(3000);

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connecter = new Connector();

            connecter.Connect(endPoint, () => { return SessionManager.Instance.Generate(); }, DummyClientCount);

            while (true)
            {
                //try
                //{
                //    //    Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //    //    socket.Connect(endPoint);
                //    //    Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");                                                                           

                //    //socket.Shutdown(SocketShutdown.Both);
                //    //socket.Close();
                //    SessionManager.Instance.SendForEach();
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine($"{e.ToString()}");
                //}

                Thread.Sleep(10000);
            }

        }
    }
}
