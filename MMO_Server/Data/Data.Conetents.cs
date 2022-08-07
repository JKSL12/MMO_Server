﻿using Google.Protobuf.Protocol;
using MMO_Server.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMO_Server.Data
{
    //[Serializable]
    //public class Stat
    //{
    //    public int level;
    //    public int maxHp;
    //    public int attack;
    //    public int totalExp;
    //}

    [Serializable]
    public class StatData : ILoader<int, StatInfo>
    {
        public List<StatInfo> stats = new List<StatInfo>();

        public Dictionary<int, StatInfo> MakeDict()
        {
            Dictionary<int, StatInfo> dict = new Dictionary<int, StatInfo>();
            foreach (StatInfo stat in stats)
            {
                stat.Hp = stat.MaxHp;
                dict.Add(stat.Level, stat);
            }
            return dict;
        }
    }

    #region Skill

    [Serializable]
    public class Skill
    {
        public int id;
        public string name;
        public float cooldown;
        public int damage;
        public SkillType skillType;
        public ProjectileInfo projectile;
    }

    public class ProjectileInfo
    {
        public string name;
        public float speed;
        public int range;
        public string prefab;
    }

    [Serializable]
    public class SkillData : ILoader<int, Skill>
    {
        public List<Skill> skills = new List<Skill>();

        public Dictionary<int, Skill> MakeDict()
        {
            Dictionary<int, Skill> dict = new Dictionary<int, Skill>();
            foreach (Skill skill in skills)
                dict.Add(skill.id, skill);
            return dict;
        }
    }

    #endregion

    #region Item

    [Serializable]
    public class ItemData
    {
        public int id;
        public string name;
        public ItemType itemType;        
    }

    public class WeaponData : ItemData
    {
        public WeaponType weaponType;
        public int damage;
    }

    public class ArmorData : ItemData
    {
        public ArmorType armorType;
        public int defence;
    }

    public class ConsumableData : ItemData
    {
        public ConsumableType consumableType;
        public int maxCount;
        public int life;
    }

    [Serializable]
    public class ItemLoader : ILoader<int, ItemData>
    {
        public List<WeaponData> weapons = new List<WeaponData>();
        public List<ArmorData> armors = new List<ArmorData>();
        public List<ConsumableData> consumables = new List<ConsumableData>();

        public Dictionary<int, ItemData> MakeDict()
        {
            Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
            foreach (ItemData item in weapons)
            {
                item.itemType = ItemType.Weapon;
                dict.Add(item.id, item);
            }

            foreach (ItemData item in armors)
            {
                item.itemType = ItemType.Armor;
                dict.Add(item.id, item);
            }

            foreach (ItemData item in consumables)
            {
                item.itemType = ItemType.Consumable;
                dict.Add(item.id, item);
            }

            return dict;
        }
    }

    #endregion

    #region Monster

    [Serializable]
    public class RewardData
    {
        public int probability;
        public int itemId;
        public int count;
    }

    [Serializable]
    public class MonsterData
    {
        public int id;
        public string name;
        public StatInfo stat;
        public List<RewardData> rewards;
    }

    [Serializable]
    public class MonsterLoader : ILoader<int, MonsterData>
    {
        public List<MonsterData> monsters = new List<MonsterData>();

        public Dictionary<int, MonsterData> MakeDict()
        {
            Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
            foreach (MonsterData monster in monsters)
            {
                dict.Add(monster.id, monster);
            }
            return dict;
        }
    }
    #endregion

    #region Map
    [Serializable]
    public class MapData
    {
        public int id;
        public string name;
    }

    public class MapLoader : ILoader<int, MapData>
    {
        public List<MapData> maps = new List<MapData>();

        public Dictionary<int, MapData> MakeDict()
        {
            Dictionary<int, MapData> dict = new Dictionary<int, MapData>();
            foreach (MapData map in maps)
            {
                dict.Add(map.id, map);
            }
            return dict;
        }
    }
    #endregion

    #region Event

    [Serializable]
    public class EventData
    {
        public string name;        
        public DateTime startTime;
        public DateTime endTime;
    }

    public class EventLoader : ILoader<int, EventData>
    {
        public List<EventData> events = new List<EventData>();

        public Dictionary<int, EventData> MakeDict()
        {
            Dictionary<int, EventData> dict = new Dictionary<int, EventData>();
            int index = 0;
            foreach (EventData _event in events)
            {
                dict.Add(index++, _event);
                GameLogic.Instance.bEventDate.Add(_event.name, false);
            }
            return dict;
        }
    }

    #endregion

    #region 몬스터스폰

    [Serializable]
    public class SpawnData
    {
        public int spawnid;
        public int monsterid;
        public int count;
        public int x;
        public int y;
    }

    [Serializable]
    public class MonsterSpawnData
    {
        public int mapid;
        public List<SpawnData> infos;
    }

    [Serializable]
    public class MonsterSpawnLoader : ILoader<int, MonsterSpawnData>
    {
        public List<MonsterSpawnData> spawns = new List<MonsterSpawnData>();

        public Dictionary<int, MonsterSpawnData> MakeDict()
        {
            Dictionary<int, MonsterSpawnData> dict = new Dictionary<int, MonsterSpawnData>();
            foreach (MonsterSpawnData spawn in spawns)
            {
                dict.Add(spawn.mapid, spawn);
            }
            return dict;
        }
    }

    #endregion
}