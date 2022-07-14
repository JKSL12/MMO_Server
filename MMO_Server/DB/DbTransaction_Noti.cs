using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using MMO_Server.Data;
using MMO_Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMO_Server.DB
{
    public partial class DbTransaction : JobSerializer
    {
        public static void EquipItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            //int? slot = player.Inven.GetEmptySlot();
            //if (slot == null) return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Count = item.Count
            };

            Console.WriteLine($"{item.ItemDbId} : {item.Equipped}");

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Equipped)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if( !success )
                    {

                    }
                }
            });
        }

        public static bool DeleteItemNoti(Player player, int itemDbId)
        {
            if (player == null || itemDbId <= 0)
                return false;

            bool result = true;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    ItemDb item = db.Items
                        .Where(i => i.ItemDbId == itemDbId).FirstOrDefault();

                    if (item != null)
                    {
                        item.TemplateId = 0;
                        item.Count = 0;
                        item.Equipped = false;
                        bool success = db.SaveChangesEx();

                        if (!success)
                        {
                            result = false;
                        }
                    }
                }
            });

            return result;
        }

        public static bool UseItemNoti(Player player, int itemDbId, int itemnum)
        {
            if (player == null || itemDbId <= 0 || itemnum <= 0)
                return false;

            //int? slot = player.Inven.GetEmptySlot();
            //if (slot == null) return;

            ItemDb itemDb = new ItemDb();
            itemDb.ItemDbId = itemDbId;
            itemDb.Count = itemnum;

            bool result = true;
            

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Count)).IsModified = true;

                    bool success = db.SaveChangesEx();

                    if (!success)
                    {
                        result = false;
                    }
                }
            });

            return result;
        }
    }
}
