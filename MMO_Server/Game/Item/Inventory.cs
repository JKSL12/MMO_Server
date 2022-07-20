using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMO_Server.Game
{
    public class Inventory
    {
        public Dictionary<int, Item> Items = new Dictionary<int, Item>();

        public void Add(Item item)
        {
            Items.Add(item.Slot, item);
        }

        public Item Get(int Slot)
        {
            Item item = null;
            Items.TryGetValue(Slot, out item);
            return item;
        }

        public void Set(Item newitem)
        {
            Item item = null;
            Items.TryGetValue(newitem.Slot, out item);

            if (item != null)
                item = newitem;
        }

        public Item Find(Func<Item, bool> condition)
        {
            foreach(Item item in Items.Values)
            {
                if (condition.Invoke(item))
                    return item;
            }

            return null;
        }

        public int? GetEmptySlot(int templateId, int itemNum)
        {            
            for (int slot = 0; slot < 20; slot++)
            {
                Item item = Items.Values.FirstOrDefault(i => i.Slot == slot);
                if (item != null)
                {
                    
                    if (item.TemplateId == 0) return slot;

                    Console.WriteLine($"empty {slot}, {item.TemplateId}, {item.Stackable}");

                    if (Item.CanStack(templateId) == true)
                    {                    
                        if (templateId == item.TemplateId && item.Count + itemNum <= item.MaxCount)
                        {
                            return slot;
                        }
                    }                    
                }
            }

            return null;
        }
    }
}
