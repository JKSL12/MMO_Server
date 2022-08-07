using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using MMO_Server.Data;
using MMO_Server.DB;
using MMO_Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMO_Server
{
    public partial class ClientSession : PacketSession
    {
        public int AccountDbId { get; private set; }
        public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

        public void HandleLogin(C_Login loginPacket)
        {
            Console.WriteLine($"UniqueId({loginPacket.UniqueId}");

            if (ServerState != PlayerServerState.ServerStateLogin) return;

            LobbyPlayers.Clear();

            using (AppDbContext db = new AppDbContext())
            {
                AccountDb findAccount = db.Accounts
                    .Include(a => a.Players)
                    .Where(async => async.AccountName == loginPacket.UniqueId).FirstOrDefault();

                if (findAccount != null)
                {
                    AccountDbId = findAccount.AccountDbId;

                    S_Login loginOk = new S_Login() { LoginOk = 1 };
                    foreach(PlayerDb playerDb in findAccount.Players)
                    {
                        LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
                        {
                            PlayerDbId = playerDb.PlayerDbId,
                            Name = playerDb.PlayerName,
                            StatInfo = new StatInfo()
                            {
                                Level = playerDb.Level,
                                Hp = playerDb.Hp,
                                MaxHp = playerDb.MaxHp,
                                Attack = playerDb.Attack,
                                Speed = playerDb.Speed,
                                TotalExp = playerDb.TotalExp,                                
                            }
                            
                        };

                        LobbyPlayers.Add(lobbyPlayer);

                        loginOk.Players.Add(lobbyPlayer);
                    }
                    Send(loginOk);

                    ServerState = PlayerServerState.ServerStateLobby;
                }
                else
                {
                    AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
                    db.Accounts.Add(newAccount);
                    bool success = db.SaveChangesEx();
                    if (success == false)
                        return;

                    AccountDbId = newAccount.AccountDbId;

                    S_Login loginOk = new S_Login() { LoginOk = 1 };
                    Send(loginOk);

                    ServerState = PlayerServerState.ServerStateLobby;
                }
            }
        }

        public void HandleEnterGame(C_EnterGame enterGamePacket)
        {
            if (ServerState != PlayerServerState.ServerStateLobby) return;

            LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
            if (playerInfo == null) return;

            MyPlayer = ObjectManager.Instance.Add<Player>();
            {
                MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
                MyPlayer.Info.Name = playerInfo.Name;
                MyPlayer.Info.PosInfo.State = CreatureState.Idle;
                MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
                MyPlayer.Info.PosInfo.PosX = 0;
                MyPlayer.Info.PosInfo.PosY = 0;                
                MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
                MyPlayer.Session = this;

                S_ItemList itemListPacket = new S_ItemList();

                using (AppDbContext db = new AppDbContext())
                {
                    List<ItemDb> items = db.Items
                        .Where(i => i.OwnerDbId == playerInfo.PlayerDbId)
                        .ToList();

                    PlayerDb findPlayer = db.Players
                    .Where(p => p.PlayerDbId == MyPlayer.PlayerDbId).FirstOrDefault();

                    if( findPlayer != null )
                    {
                        MyPlayer.Info.PosInfo.MapId = findPlayer.MapId;
                        MyPlayer.Stat.Str = findPlayer.Str;
                        MyPlayer.Stat.Dex = findPlayer.Dex;
                        MyPlayer.Stat.Mag = findPlayer.Mag;
                        MyPlayer.Stat.Vit = findPlayer.Vit;
                        MyPlayer.Stat.BonusStat = findPlayer.BonusPoint;
                    }

                    
                    foreach (ItemDb itemDb in items)
                    {
                        Item item = Item.MakeItem(itemDb);
                        if(item != null)
                        {
                            //MyPlayer.Inven.Add(item);
                            MyPlayer.Inven.Add(item);

                            ItemInfo info = new ItemInfo();
                            info.MergeFrom(item.Info);
                            itemListPacket.Items.Add(info);
                        }                        
                    }                    
                }

                Send(itemListPacket);
            }

            ServerState = PlayerServerState.ServerStateGame;

            GameLogic.Instance.Push(() =>
            {
                GameRoom room = GameLogic.Instance.Find(MyPlayer.Info.PosInfo.MapId);
                room.Push(room.EnterGame, MyPlayer, true, new Vector2Int( -1, -1 ));
            });
        }

        public void HandleCreatePlayer(C_CreatePlayer createPacket)
        {
            if (ServerState != PlayerServerState.ServerStateLobby) return;

            using (AppDbContext db = new AppDbContext())
            {
                PlayerDb findPlayer = db.Players
                    .Where(p => p.PlayerName == createPacket.Name).FirstOrDefault();
                
                if( findPlayer != null)
                {
                    Send(new S_CreatePlayer());
                }
                else
                {
                    StatInfo stat = null;
                    DataManager.StatDict.TryGetValue(1, out stat);

                    PlayerDb newPlayerDb = new PlayerDb()
                    {
                        PlayerName = createPacket.Name,
                        Level = stat.Level,
                        Hp = stat.Hp,
                        MaxHp = stat.MaxHp,
                        Attack = stat.Attack,
                        Speed = stat.Speed,
                        TotalExp = 0,
                        MapId = 1,
                        Str = 5,
                        Dex = 5,
                        Mag = 5,
                        Vit = 5,
                        AccountDbId = AccountDbId
                    };

                    db.Players.Add(newPlayerDb);

                    bool success = db.SaveChangesEx();
                    if (success == false)
                    {
                        Console.WriteLine($"Create Char Fail -1");
                        return;
                    }

                    for (int i = 0; i < 20; ++i)
                    {
                        ItemDb itemDb = new ItemDb()
                        {
                            TemplateId = 0,
                            Count = 0,
                            Slot = i,
                            Equipped = false,
                            OwnerDbId = newPlayerDb.PlayerDbId
                        };

                        db.Items.Add(itemDb);

                        success = db.SaveChangesEx();
                        if (success == false)
                        {
                            Console.WriteLine($"Create Char Fail {i}");
                            return;
                        }
                    }


                    //bool success = db.SaveChangesEx();
                    //if (success == false)
                    //{
                    //    Console.WriteLine($"Create Char Fail");
                    //    return;
                    //}
                        

                    LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
                    {
                        PlayerDbId = newPlayerDb.PlayerDbId,
                        Name = createPacket.Name,
                        StatInfo = new StatInfo()
                        {
                            Level = stat.Level,
                            Hp = stat.Hp,
                            MaxHp = stat.MaxHp,
                            Attack = stat.Attack,
                            Speed = stat.Speed,
                            TotalExp = 0
                        }
                    };

                    LobbyPlayers.Add(lobbyPlayer);

                    S_CreatePlayer newPlayer = new S_CreatePlayer() { Player = new LobbyPlayerInfo() };
                    newPlayer.Player.MergeFrom(lobbyPlayer);

                    Send(newPlayer);
                }
            }
        }
    }
}
