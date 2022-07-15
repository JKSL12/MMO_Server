using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using MMO_Server.Data;
using MMO_Server.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMO_Server.DB
{
    public partial class DbTransaction : JobSerializer
    {
        public static DbTransaction Instance { get; } = new DbTransaction();

        public static void SavePlayerStatus_AllInOne(Player player, GameRoom room)
        {
            if (player == null || room == null) return;

            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        //room.Push(() => Console.WriteLine($"Hp Saved({playerDb.Hp})"));
                    }
                }
            });
        }

        public static void SavePlayerStatus_Step1(Player player, GameRoom room)
        {
            if (player == null || room == null) return;

            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;
            playerDb.MapId = player.PosInfo.MapId;
            Instance.Push<PlayerDb, GameRoom>(SavePlayerStatus_Step2, playerDb, room);


        }

        public static void SavePlayerStatus_Step2(PlayerDb playerDb, GameRoom room)
        {
            using (AppDbContext db = new AppDbContext())
            {
                db.Entry(playerDb).State = EntityState.Unchanged;
                db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                db.Entry(playerDb).Property(nameof(PlayerDb.MapId)).IsModified = true;
                bool success = db.SaveChangesEx();
                if (success)
                {
                    room.Push(SavePlayerStatus_Step3, playerDb.Hp);
                }
            }
        }

        public static void SavePlayerStatus_Step3(int hp)
        {
           // Console.WriteLine($"Hp Saved({hp})");
        }

        public static void IncreaseStat(Player player, Int32 type, Int32 stat)
        {
            if (player == null || type <= 0 || stat <= 0)
                return;

            if (player.Stat.BonusStat <= 0) return;

            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;

            switch (type)
            {
                case 1:
                    {
                        playerDb.Str = player.Stat.Str + stat;
                    }
                    break;
                case 2:
                    {
                        playerDb.Dex = player.Stat.Dex + stat;
                    }
                    break;
                case 3:
                    {
                        playerDb.Mag = player.Stat.Mag + stat;
                    }
                    break;
                case 4:
                    {
                        playerDb.Vit = player.Stat.Vit + stat;
                    }
                    break;
            }

            playerDb.BonusPoint = player.Stat.BonusStat - stat;

            

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    switch (type)
                    {
                        case 1:
                            {
                                db.Entry(playerDb).Property(nameof(PlayerDb.Str)).IsModified = true;
                            }
                            break;
                        case 2:
                            {
                                db.Entry(playerDb).Property(nameof(PlayerDb.Dex)).IsModified = true;
                            }
                            break;
                        case 3:
                            {
                                db.Entry(playerDb).Property(nameof(PlayerDb.Mag)).IsModified = true;
                            }
                            break;
                        case 4:
                            {
                                db.Entry(playerDb).Property(nameof(PlayerDb.Vit)).IsModified = true;
                            }
                            break;
                    }                                                                            
                    db.Entry(playerDb).Property(nameof(PlayerDb.BonusPoint)).IsModified = true;
                                      
                    bool success = db.SaveChangesEx();

                    
                    if (success)
                    {
                        int changestat = 0;
                        switch (type)
                        {
                            case 1:
                                {
                                    player.Stat.Str = player.Stat.Str + stat;
                                    changestat = player.Stat.Str;
                                }
                                break;
                            case 2:
                                {
                                    player.Stat.Dex = player.Stat.Dex + stat;
                                    changestat = player.Stat.Dex;
                                }
                                break;
                            case 3:
                                {
                                    player.Stat.Mag = player.Stat.Mag + stat;
                                    changestat = player.Stat.Mag;
                                }
                                break;
                            case 4:
                                {
                                    player.Stat.Vit = player.Stat.Vit + stat;
                                    changestat = player.Stat.Vit;
                                }
                                break;
                        }

                        player.Stat.BonusStat = player.Stat.BonusStat - stat;

                        S_StatPlusminus statPacket = new S_StatPlusminus();
                        statPacket.StatType = type;
                        statPacket.StatNum = changestat;
                        statPacket.BonusStat = player.Stat.BonusStat;

                        player.Session.Send(statPacket);
                    }
                }
            });
        }

        public static void DecreaseStat(Player player, Int32 type, Int32 stat)
        {
            if (player == null || type <= 0 || stat <= 0)
                return;

            switch (type)
            {
                case 1:
                    {
                        if (player.Stat.Str <= 5) return;
                    }
                    break;
                case 2:
                    {
                        if (player.Stat.Dex <= 5) return;
                    }
                    break;
                case 3:
                    {
                        if (player.Stat.Mag <= 5) return;
                    }
                    break;
                case 4:
                    {
                        if (player.Stat.Vit <= 5) return;
                    }
                    break;
            }

            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;

            switch (type)
            {
                case 1:
                    {
                        playerDb.Str = player.Stat.Str - stat;
                    }
                    break;
                case 2:
                    {
                        playerDb.Dex = player.Stat.Dex - stat;
                    }
                    break;
                case 3:
                    {
                        playerDb.Mag = player.Stat.Mag - stat;
                    }
                    break;
                case 4:
                    {
                        playerDb.Vit = player.Stat.Vit - stat;
                    }
                    break;
            }

            playerDb.BonusPoint = player.Stat.BonusStat + stat;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    switch (type)
                    {
                        case 1:
                            {
                                db.Entry(playerDb).Property(nameof(PlayerDb.Str)).IsModified = true;
                            }
                            break;
                        case 2:
                            {
                                db.Entry(playerDb).Property(nameof(PlayerDb.Dex)).IsModified = true;
                            }
                            break;
                        case 3:
                            {
                                db.Entry(playerDb).Property(nameof(PlayerDb.Mag)).IsModified = true;
                            }
                            break;
                        case 4:
                            {
                                db.Entry(playerDb).Property(nameof(PlayerDb.Vit)).IsModified = true;
                            }
                            break;
                    }
                    db.Entry(playerDb).Property(nameof(PlayerDb.BonusPoint)).IsModified = true;

                    bool success = db.SaveChangesEx();

                    if (success)
                    {
                        int changestat = 0;
                        switch (type)
                        {
                            case 1:
                                {
                                    player.Stat.Str = player.Stat.Str - stat;
                                    changestat = player.Stat.Str;
                                }
                                break;
                            case 2:
                                {
                                    player.Stat.Dex = player.Stat.Dex - stat;
                                    changestat = player.Stat.Dex;
                                }
                                break;
                            case 3:
                                {
                                    player.Stat.Mag = player.Stat.Mag - stat;
                                    changestat = player.Stat.Mag;
                                }
                                break;
                            case 4:
                                {
                                    player.Stat.Vit = player.Stat.Vit - stat;
                                    changestat = player.Stat.Vit;
                                }
                                break;
                        }

                        player.Stat.BonusStat = player.Stat.BonusStat + stat;

                        S_StatPlusminus statPacket = new S_StatPlusminus();
                        statPacket.StatType = type;
                        statPacket.StatNum = changestat;
                        statPacket.BonusStat = player.Stat.BonusStat;
                        player.Session.Send(statPacket);
                    }
                }
            });
        }

        public static void IncreaseExp(Player player, Int32 exp, GameRoom room)
        {
            if (player == null || exp <= 0 || room == null)
                return;

            int level = player.Stat.Level;
            int curexp = player.Stat.TotalExp;
            int nextexp = player.GetRequiredExpNextLevel(level);
            int bonusStat = player.Stat.BonusStat;

            int result = 0;

            if (nextexp == -1)
            {
                ;
            }
            else
            {
                if (curexp + exp >= nextexp)
                {
                    level = level + 1;
                    curexp = curexp + exp - nextexp;
                    result = 2;
                    bonusStat = bonusStat + 5;
                }
                else
                {
                    curexp = curexp + exp;
                    result = 1;
                }

                Console.WriteLine($"{result}, {level}, {curexp}, {bonusStat}");

                PlayerDb playerDb = new PlayerDb();

                playerDb.PlayerDbId = player.PlayerDbId;
                playerDb.Level = level;
                playerDb.TotalExp = curexp;
                playerDb.BonusPoint = bonusStat;

                Instance.Push(() =>
                {
                    using (AppDbContext db = new AppDbContext())
                    {
                        db.Entry(playerDb).State = EntityState.Unchanged;
                        db.Entry(playerDb).Property(nameof(PlayerDb.Level)).IsModified = true;
                        db.Entry(playerDb).Property(nameof(PlayerDb.TotalExp)).IsModified = true;
                        db.Entry(playerDb).Property(nameof(PlayerDb.BonusPoint)).IsModified = true;
                        bool success = db.SaveChangesEx();
                        if (success)
                        {
                            player.Stat.Level = level;
                            player.Stat.TotalExp = curexp;
                            player.Stat.BonusStat = bonusStat;

                            S_IncreaseExp increaseExpPacket = new S_IncreaseExp();
                            increaseExpPacket.ObjectId = player.Id;
                            increaseExpPacket.LevelUp = result == 2 ? true : false;
                            increaseExpPacket.Level = level;
                            increaseExpPacket.TotalExp = curexp;
                            increaseExpPacket.BonusStat = bonusStat;

                            player.Session.Send(increaseExpPacket);
                            //room.Push(() => Console.WriteLine($"Hp Saved({playerDb.Hp})"));
                        }
                    }
                });
            }
        }

        public static void RewardPlayer(Player player, RewardData rewardData, GameRoom room)
        {
            if (player == null || rewardData == null || room == null)
                return;

            int? slot = player.Inven.GetEmptySlot();
            if (slot == null) return;

            ItemDb itemDb = new ItemDb()
            {
                TemplateId = rewardData.itemId,
                Count = rewardData.count,
                Slot = slot.Value,
                OwnerDbId = player.PlayerDbId
            };

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Items.Add(itemDb);
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        room.Push(() =>
                        {
                            Item newItem = Item.MakeItem(itemDb);
                            player.Inven.Add(newItem);

                            {
                                S_AddItem itemPacket = new S_AddItem();
                                ItemInfo itemInfo = new ItemInfo();
                                itemInfo.MergeFrom(newItem.Info);
                                itemPacket.Items.Add(itemInfo);

                                player.Session.Send(itemPacket);
                            }
                        });
                    }
                }
            });
        }
    }
}
