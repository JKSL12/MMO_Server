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

        public static void IncreaseExp(Player player, Int32 exp, GameRoom room)
        {
            if (player == null || exp <= 0 || room == null)
                return;

            int level = player.Stat.Level;
            int curexp = player.Stat.TotalExp;
            int nextexp = player.GetRequiredExpNextLevel(level);

            Console.WriteLine($"{level}, {curexp}, {nextexp}, {exp}");

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
                }
                else
                {
                    curexp = curexp + exp;
                    result = 1;
                }

                Console.WriteLine($"{result}, {level}, {curexp}");

                PlayerDb playerDb = new PlayerDb();

                playerDb.PlayerDbId = player.PlayerDbId;
                playerDb.Level = level;
                playerDb.TotalExp = curexp;                                      

                Instance.Push(() =>
                {
                    using (AppDbContext db = new AppDbContext())
                    {
                        db.Entry(playerDb).State = EntityState.Unchanged;
                        db.Entry(playerDb).Property(nameof(PlayerDb.Level)).IsModified = true;
                        db.Entry(playerDb).Property(nameof(PlayerDb.TotalExp)).IsModified = true;
                        bool success = db.SaveChangesEx();
                        if (success)
                        {
                            player.Stat.Level = level;
                            player.Stat.TotalExp = curexp;

                            S_IncreaseExp increaseExpPacket = new S_IncreaseExp();
                            increaseExpPacket.ObjectId = player.Id;
                            increaseExpPacket.LevelUp = result == 2 ? true : false;
                            increaseExpPacket.Level = level;
                            increaseExpPacket.TotalExp = curexp;

                            player.Session.Send(increaseExpPacket);
                            Console.WriteLine($"{increaseExpPacket.Level}, {increaseExpPacket.TotalExp}");
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
