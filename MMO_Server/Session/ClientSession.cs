using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerCore;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using MMO_Server.Game;
using MMO_Server.Data;

namespace MMO_Server
{
    public abstract class Packet
    {
        public ushort size;
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    public partial class ClientSession : PacketSession
    {
        public PlayerServerState ServerState { get; private set; } = PlayerServerState.ServerStateLogin;

        public Player MyPlayer { get; set; }
        public int SessionId { get; set; }
        //public GameRoom Room { get; set; }
        //public float PosX { get; set; }
        //public float PosY { get; set; }
        //public float PosZ { get; set; }       

        object _lock = new object();
        List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();

        int _reservedSendBytes = 0;
        long _lastSendTick = 0;

        long _pingpongTick = 0;
        public void Ping()
        {
            if ( _pingpongTick > 0 )
            {
                long delta = (System.Environment.TickCount - _pingpongTick);
                if(delta > 30 * 1000)
                {
                    Console.WriteLine("Disconnected by pingcheck");
                    Disconnect();
                    return;
                }                
            }

            S_Ping pingPacket = new S_Ping();
            Send(pingPacket);

            GameLogic.Instance.PushAfter(5000, Ping);
        }

        public void HandlePong()
        {
            _pingpongTick = System.Environment.TickCount;
        }

        #region Network
        public void Send(IMessage packet)
        {
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
            MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);

            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));            
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            lock (_lock)
            {
                _reserveQueue.Add(sendBuffer);
                _reservedSendBytes += sendBuffer.Length;
            }
            //Send(new ArraySegment<byte>(sendBuffer));
        }

        public void FlushSend()
        {
            List<ArraySegment<byte>> sendList = null;

            lock(_lock)
            {
                long delta = (System.Environment.TickCount - _lastSendTick);
                //if (delta < 100 && _reservedSendBytes < 10000)
                //    return;

                //_reservedSendBytes = 0;
                //_lastSendTick = System.Environment.TickCount;

                if (_reserveQueue.Count == 0)
                    return;


                sendList = _reserveQueue;
                _reserveQueue = new List<ArraySegment<byte>>();
            }

            Send(sendList);
        }

        public override void OnConnected(EndPoint endPoint)
        {
            //S_Chat chat = new S_Chat()
            //{
            //    Context = "안녕하세요"
            //};            

            //Send(chat);
            //  Program.Room.Push(() => Program.Room.Enter(this));

            //  Program.Room.Enter(this);

            {
                S_Connected connectedPacket = new S_Connected();
                Send(connectedPacket);
            }

            GameLogic.Instance.PushAfter(5000, Ping);
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);


            // Console.WriteLine($"id = {id}, size = {size}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            GameLogic.Instance.Push(() =>
            {
                if (MyPlayer == null)
                    return;

                GameRoom room = GameLogic.Instance.Find(1);
                
                room.Push(room.LeaveGame, MyPlayer.Info.ObjectId);
            });

            SessionManager.Instance.Remove(this);            
        }

        //public override int OnRecv(ArraySegment<byte> buffer)
        //{
        //    string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        //    Console.WriteLine($"[From Client] {recvData}");

        //    return buffer.Count;
        //}

        public override void OnSend(int numOfBytes)
        {
           // Console.WriteLine($"Transferred bytes:{numOfBytes}");
        }

        #endregion
    }
}
