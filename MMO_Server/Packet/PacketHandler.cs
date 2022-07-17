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

    public static void C_StatPlusminusHandler(PacketSession session, IMessage packet)
    {
        C_StatPlusminus statPacket = (C_StatPlusminus)packet;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        player.HandleStatPlusMinus(statPacket);
    }

    public static void C_ChatHandler(PacketSession session, IMessage packet)
    {
        C_Chat chatPacket = (C_Chat)packet;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        string type = "";
        switch(chatPacket.ChatType)
        {
            case 0:
                type = "[전체] ";
                break;
            case 1:
                type = "[지역] ";
                break;
        }
        
        string name = player.Info.Name;

        string msg = type + name + " : " + chatPacket.ChatMsg;

        S_Chat chat = new S_Chat();
        chat.ObejctId = player.Info.ObjectId;
        chat.ChatMsg = msg;

        Console.WriteLine($"chatrecv : {msg}");
        switch (chatPacket.ChatType)
        {
            case 0:
                GameLogic.Instance.BroadcastAllMapAllPlayer(chat);
                break;
            case 1:
                room.BroadcastRoomAllPlayer(chat);
                break;
        }
    }

    public static void C_DropItemHandler(PacketSession session, IMessage packet)
    {
        C_DropItem dropPacket = (C_DropItem)packet;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        room.Push(room.HandleDropItem, player, dropPacket);
    }
}
