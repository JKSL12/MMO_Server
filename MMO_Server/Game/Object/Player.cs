using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using MMO_Server.Data;
using MMO_Server.DB;
using MMO_Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMO_Server.Game
{
    public class Player : GameObject
    {
        public int PlayerDbId { get; set; }
        public ClientSession Session { get; set; }
        public VisionCube Vision { get; private set; }
        public Inventory Inven { get; private set; } = new Inventory();

        public int WeaponDamage { get; private set; }
        public int ArmorDefence { get; private set; }

        public override int TotalAttack {  get { return Stat.Attack + WeaponDamage; } }
        public override int TotalDefence {  get { return ArmorDefence; } }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Vision = new VisionCube(this);
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);           
        }

        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
        }

        public void OnleaveGame()
        {
            //DbTransaction.SavePlayerStatus_AllInOne(this, Room);
            DbTransaction.SavePlayerStatus_Step1(this, Room);
        }

        public void HandleEquipItem(C_EquipItem equipPacket)
        {
            Item item = Inven.Get(equipPacket.ItemDbId);
            if (item == null) return;

            if (item.ItemType == ItemType.Consumable)
                return;

            if (equipPacket.Equipped)
            {
                Item unequipItem = null;

                if (item.ItemType == ItemType.Weapon)
                {
                    unequipItem = Inven.Find(i => i.Equipped && i.ItemType == ItemType.Weapon);
                }
                else if (item.ItemType == ItemType.Armor)
                {
                    ArmorType armorType = ((Armor)item).ArmorType;
                    unequipItem = Inven.Find(i => i.Equipped && i.ItemType == ItemType.Armor &&
                   ((Armor)i).ArmorType == armorType);
                }

                if (unequipItem != null)
                {
                    unequipItem.Equipped = false;

                    DbTransaction.EquipItemNoti(this, unequipItem);

                    S_EquipItem equipOkItem = new S_EquipItem();
                    equipOkItem.ItemDbId = unequipItem.ItemDbId;
                    equipOkItem.Equipped = unequipItem.Equipped;
                    Session.Send(equipOkItem);
                }
            }

            {
                item.Equipped = equipPacket.Equipped;

                DbTransaction.EquipItemNoti(this, item);

                S_EquipItem equipOkItem = new S_EquipItem();
                equipOkItem.ItemDbId = equipPacket.ItemDbId;
                equipOkItem.Equipped = equipPacket.Equipped;
                Session.Send(equipOkItem);
            }

            RefreshAdditionalStat();            
        }

        public void HandleUseItem(C_UseItem usePacket)
        {
            Item item = Inven.Get(usePacket.ItemDbId);
            if (item == null) return;

            if (item.ItemType != ItemType.Consumable)
                return;
                        
            Consumable consumeItem = (Consumable)item;

            {
                if (item.Count - usePacket.UseNum < 0) return;
                
                //item.Count = item.Count - usePacket.UseNum;

                bool result = false;

                if (item.Count - usePacket.UseNum <= 0 )
                {
                    result = DbTransaction.DeleteItemNoti(this, item.ItemDbId);

                    if (result)
                    {
                        item.TemplateId = 0;
                        item.Count = 0;
                        item.Equipped = false;
                    }
                }
                else
                {
                    result = DbTransaction.UseItemNoti(this, item.ItemDbId, (item.Count - usePacket.UseNum));

                    if(result)
                    {
                        item.Count = (item.Count - usePacket.UseNum);
                    }
                }

                if (result)
                {
                    S_UseItem useItem = new S_UseItem();
                    useItem.ItemSlot = item.Info.Slot;
                    useItem.ItemNum = item.Count;
                    Session.Send(useItem);

                    if( consumeItem.ConsumableType == ConsumableType.Potion )
                    {
                        Stat.Hp = Math.Min(Stat.Hp += consumeItem.Life, Stat.MaxHp);

                        S_ChangeHp changePacket = new S_ChangeHp();
                        changePacket.ObjectId = Id;
                        changePacket.Hp = Stat.Hp;
                        Room.Broadcast(CellPos, changePacket);
                    }
                }
            }
        }

        public void RefreshAdditionalStat()
        {
            WeaponDamage = 0;
            ArmorDefence = 0;

            foreach(Item item in Inven.Items.Values)
            {
                if (item.Equipped == false)
                    continue;

                switch(item.ItemType)
                {
                    case ItemType.Weapon:
                        WeaponDamage += ((Weapon)item).Damage;
                        break;
                    case ItemType.Armor:
                        ArmorDefence += ((Armor)item).Defence;
                        break;
                }
            }
        }

        public int GetRequiredExpNextLevel( int level )
        {
            StatInfo statData = null;
            DataManager.StatDict.TryGetValue(level + 1, out statData);

            if (statData == null) return -1;

            return statData.TotalExp;
        }
    }
}