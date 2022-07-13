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

        public static bool UseItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return false;

            //int? slot = player.Inven.GetEmptySlot();
            //if (slot == null) return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                TemplateId = item.TemplateId,
                Count = item.Count,
                Equipped = item.Equipped                            
            };

            bool result = true;
            

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.ItemDbId)).IsModified = true;
                    db.Entry(itemDb).Property(nameof(ItemDb.TemplateId)).IsModified = true;
                    db.Entry(itemDb).Property(nameof(ItemDb.Count)).IsModified = true;
                    db.Entry(itemDb).Property(nameof(ItemDb.Equipped)).IsModified = true;

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
