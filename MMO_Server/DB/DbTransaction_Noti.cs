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

            int? slot = player.Inven.GetEmptySlot();
            if (slot == null) return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Equipped = item.Equipped
            };

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
    }
}
