using Google.Protobuf;
using Google.Protobuf.Protocol;
using MMO_Server;
using MMO_Server.DB;
using MMO_Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PacketHandler
{
    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;

        //Console.WriteLine($"C_Move ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY})");

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        //room.HandleMove(player, movePacket);
        room.Push(room.HandleMove, player, movePacket);
    }

    public static void C_SkillHandler(PacketSession session, IMessage packet)
    {
        C_Skill skillPacket = packet as C_Skill;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        //room.HandleSkill(player, skillPacket);
        room.Push(room.HandleSkill, player, skillPacket);
    }

    public static void C_LoginHandler(PacketSession session, IMessage packet)
    {
        C_Login loginPacket = packet as C_Login;
        ClientSession clientSession = session as ClientSession;

        clientSession.HandleLogin(loginPacket);
    }

    public static void C_EnterGameHandler(PacketSession session, IMessage packet)
    {
        C_EnterGame enterGamePacket = (C_EnterGame)packet;
        ClientSession clientSession = session as ClientSession;

        clientSession.HandleEnterGame(enterGamePacket);
    }

    public static void C_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        C_CreatePlayer createPacket = (C_CreatePlayer)packet;
        ClientSession clientSession = session as ClientSession;

        clientSession.HandleCreatePlayer(createPacket);
    }

    public static void C_EquipItemHandler(PacketSession session, IMessage packet)
    {
        C_EquipItem equipPacket = (C_EquipItem)packet;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        room.Push(room.HandleEquipItem, player, equipPacket);
    }

    public static void C_PlayerKillHandler(PacketSession session, IMessage packet)
    {
        C_PlayerKill killPacket = (C_PlayerKill)packet;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        room.Push(room.HandleKill, player, killPacket);
    }

    public static void C_PongHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = session as ClientSession;
        clientSession.HandlePong();
    }

    public static void C_UseItemHandler(PacketSession session, IMessage packet)
    {
        C_UseItem usePacket = (C_UseItem)packet;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        room.Push(room.HandleUseItem, player, usePacket);
    }
}
