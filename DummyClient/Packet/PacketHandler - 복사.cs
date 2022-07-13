//using System;
//using System.Collections.Generic;
//using System.Text;
//using DummyClient;
//using ServerCore;


//class PacketHandler
//{
//    public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet)
//    {
//        S_BroadcastEnterGame chatPacket = packet as S_BroadcastEnterGame;
//        ServerSession serverSession = session as ServerSession;

//        //if (chatPacket.playerId == 1)
//        //Console.WriteLine(chatPacket.chat);
//    }

//    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet)
//    {
//        S_BroadcastLeaveGame chatPacket = packet as S_BroadcastLeaveGame;
//        ServerSession serverSession = session as ServerSession;
//    }

//    public static void S_PlayerListHandler(PacketSession session, IPacket packet)
//    {
//        S_PlayerList chatPacket = packet as S_PlayerList;
//        ServerSession serverSession = session as ServerSession;
//    }

//    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet)
//    {
//        S_BroadcastMove chatPacket = packet as S_BroadcastMove;
//        ServerSession serverSession = session as ServerSession;
//    }

//    public static void S_ChatHandler(PacketSession session, IPacket packet)
//    {
//        return;
//    }
//}