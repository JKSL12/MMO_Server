using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using MMO_Server.Data;
using MMO_Server.DB;
using MMO_Server.Game;
using ServerCore;
using SharedDB;

namespace MMO_Server
{    
    class Program
    {
        static Listener _listener = new Listener();


        //static void FlushRoom()
        //{         
        //    JobTimer.Instance.Push(FlushRoom, 250);
        //}

        //static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();

        //static void TickRoom(GameRoom room, int tick = 100)
        //{
        //    var timer = new System.Timers.Timer();
        //    timer.Interval = tick;
        //    timer.Elapsed += ((s, e) => { room.Update(); });
        //    timer.AutoReset = true;
        //    timer.Enabled = true;

        //    _timers.Add(timer);
        //}

        static void GameLogicTask()
        {
            while(true)
            {
                GameLogic.Instance.Update();
                Thread.Sleep(0);
            }
        }

        static void DbTask()
        {
            while(true)
            {
                DbTransaction.Instance.Flush();
                Thread.Sleep(0);
            }
        }

        static void NetworkTask()
        {
            while(true)
            {
                List<ClientSession> sessions = SessionManager.Instance.GetSessions();

                foreach(ClientSession session in sessions)
                {
                    session.FlushSend();
                }

                Thread.Sleep(0);
            }
        }

        static void StartServerInfoTask()
        {
            var t = new System.Timers.Timer();
            t.AutoReset = true;
            t.Elapsed += new System.Timers.ElapsedEventHandler((s, e) =>
            {
                using (SharedDbContext shared = new SharedDbContext())
                {
                    ServerDb serverDb = shared.Servers.Where(s => s.Name == Name).FirstOrDefault();

                    if (serverDb != null)
                    {
                        serverDb.IpAddress = IpAddress;
                        serverDb.Port = Port;
                        serverDb.BusyScore = SessionManager.Instance.GetBusyScore();
                        shared.SaveChangesEx();
                    }
                    else
                    {
                        serverDb = new ServerDb()
                        {
                            Name = Program.Name,
                            IpAddress = Program.IpAddress,
                            Port = Program.Port,
                            BusyScore = SessionManager.Instance.GetBusyScore()
                        };
                        shared.Servers.Add(serverDb);
                        shared.SaveChangesEx();
                    }                    
                }
            });
            t.Interval = 10 * 1000;
            t.Start();
        }

        public static string Name { get; } = "데포르쥬";
        public static int Port { get; } = 7777;
        public static string IpAddress { get; set; }

        static void Main(string[] args)
        {            
            ConfigManager.LoadConfig();
            DataManager.LoadData();

            //var d = DataManager.StatDict;

            foreach (MapData mapData in DataManager.MapDict.Values)
            {
                GameLogic.Instance.Push(() =>
                {
                    GameRoom room = GameLogic.Instance.Add(mapData.name);
                });
            }

            //GameLogic.Instance.Push(() =>
            //{
            //    GameRoom room = GameLogic.Instance.Add(1);
            //});

            //TickRoom(room, 50);

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, Port);

            IpAddress = ipAddr.ToString();
            
            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");

            StartServerInfoTask();
            //FlushRoom();
            // JobTimer.Instance.Push(FlushRoom);

            // dblogic
            {
                Thread t = new Thread(DbTask);
                t.Name = "DB";
                t.Start();
            }

            // networklogic
            {
                Thread t = new Thread(NetworkTask);
                t.Name = "Network";
                t.Start();
                //Task networkTask = new Task(NetworkTask, TaskCreationOptions.LongRunning);
                //networkTask.Start();
            }

            // gamelogic
            Thread.CurrentThread.Name = "GameLogic";
            GameLogicTask();

            //while (true)
            //{
            //    //    JobTimer.Instance.Flush();
            //    //Thread.Sleep(250);
            //    //  Console.WriteLine("Listening...");        

            //    //GameRoom room = RoomManager.Instance.Find(1);
            //    //room.Push(room.Update);
            //    DbTransaction.Instance.Flush();
            //}


        }
    }
}
