﻿//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Net;
//using System.Net.Sockets;
//using System.Threading;
//using ServerCore;

//namespace DummyClient
//{
//    public abstract class Packet
//    {
//        public ushort size;
//        public ushort packetId;

//        public abstract ArraySegment<byte> Write();
//        public abstract void Read(ArraySegment<byte> s);
//    }

//    class ServerSession : PacketSession
//    {
//        public override void OnConnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"OnConnected:{endPoint}");            
//        }

//        public override void OnDisconnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"OnDisconnected:{endPoint}");
//        }

//        public override void OnRecvPacket(ArraySegment<byte> buffer)
//        {
//            PacketManager.Instance.OnRecvPacket(this, buffer);
//        }

//        public override void OnSend(int numOfBytes)
//        {
//            //Console.WriteLine($"Transferred bytes:{numOfBytes}");
//        }
//    }
//}
