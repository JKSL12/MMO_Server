using System;
using System.Collections.Generic;
using System.Text;

namespace DummyClient
{
    public class SessionManager
    {
        static SessionManager _session = new SessionManager();
        public static SessionManager Instance { get { return _session; } }

        //List<ServerSession> _sessions = new List<ServerSession>();
        HashSet<ServerSession> _sessions = new HashSet<ServerSession>();
        object _lock = new object();
        int _dummyId = 1;

        Random _rand = new Random();

        public void SendForEach()
        {
            lock(_lock)
            {
                foreach(ServerSession session in _sessions)
                {
                    C_Move movePacket = new C_Move();
                    movePacket.posX = _rand.Next(-50, 50);
                    movePacket.posY = 0;
                    movePacket.posZ = _rand.Next(-50, 50);

                    session.Send(movePacket.Write());
                }
            }
        }
        public ServerSession Generate()
        {
            lock(_lock)
            {
                ServerSession session = new ServerSession();
                session.DummyId = _dummyId;
                _dummyId++;

                _sessions.Add(session);
                Console.WriteLine($"Connected({_sessions.Count}) Players");
                return session;
            }
        }

        public void Remove(ServerSession session)
        {
            lock(_lock)
            {
                _sessions.Remove(session);
                Console.WriteLine($"Connected({_sessions.Count}) Players");
            }
        }
    }
}
