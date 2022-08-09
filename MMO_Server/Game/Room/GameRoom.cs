using Google.Protobuf;
using Google.Protobuf.Protocol;
using MMO_Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMO_Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public const int VisionCells = 5;

        public int RoomId { get; set; }

        //List<Player> _players = new List<Player>();
        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();
        Dictionary<int, NPC> _npcs = new Dictionary<int, NPC>();

        public Zone[,] Zones { get; private set; }
        public int ZoneCells { get; private set; }
        public Map Map { get; private set; } = new Map();

        public MonsterSpawnData monsterSpawnData = null;

        long _nextCheckTick = 0;            
        
        public Zone GetZone(Vector2Int cellPos)
        {
            int x = (cellPos.x - Map.MinX) / ZoneCells;
            int y = (Map.MaxY - cellPos.y) / ZoneCells;

            return GetZone(y, x);
        }

        public Zone GetZone(int indexY, int indexX)
        {
            if (indexX < 0 || indexX >= Zones.GetLength(1))
                return null;

            if (indexY < 0 || indexY >= Zones.GetLength(0))
                return null;

            return Zones[indexY, indexX];
        }

        public void Init(int mapId, int zoneCells)
        {
            Map.LoadMap(mapId, "../../../../Common/MapData");

            ZoneCells = zoneCells;
            int countY = (Map.SizeY + zoneCells - 1) / zoneCells;
            int countX = (Map.SizeX + zoneCells - 1) / zoneCells;
            Zones = new Zone[countY, countX];
            for( int y = 0; y < countY; y++)
            {
                for(int x = 0; x < countX; x++)
                {
                    Zones[y, x] = new Zone(y, x);
                }
            }

            //for (int i = 0; i < 500; i++)
            //{
            //    Monster monster = ObjectManager.Instance.Add<Monster>();
            //    monster.Init(1);
            //    //monster.CellPos = new Vector2Int(5, 5);
            //    EnterGame(monster, randomPos: true);
            //}

            //TestTimer();
        }

        public void Init(MapData mapData, int zoneCells)
        {
            Console.WriteLine($"Room Add{mapData.name}");

            Map.LoadMap(mapData.name, "../../../../Common/MapData");

            ZoneCells = zoneCells;
            int countY = (Map.SizeY + zoneCells - 1) / zoneCells;
            int countX = (Map.SizeX + zoneCells - 1) / zoneCells;
            Zones = new Zone[countY, countX];
            for (int y = 0; y < countY; y++)
            {
                for (int x = 0; x < countX; x++)
                {
                    Zones[y, x] = new Zone(y, x);
                }
            }

            //if( string.Compare(mapName, "Beginner_1") == 0 )
            //{
            //    // 몬스터 생성
            //    for (int i = 0; i < 500; i++)
            //    {
            //        Monster monster = ObjectManager.Instance.Add<Monster>();
            //        monster.Init(1);
            //        //monster.cellpos = new vector2int(5, 5);
            //        EnterGame(monster, randomPos: true);
            //    }
            //}


            //TestTimer();
        }

        //void TestTimer()
        //{
        //    Console.WriteLine("TestTimer");
        //    PushAfter(100, TestTimer);
        //}

        public void Update()
        {
            try
            {
                Flush();

                if (_nextCheckTick > Environment.TickCount)
                    return;

                _nextCheckTick = Environment.TickCount + 3000;

                if (monsterSpawnData != null)
                {
                    int count = 0;
                                        
                    foreach (SpawnData spawnData in monsterSpawnData.infos)
                    {
                        
                        MonsterData monsterData = null;
                        DataManager.MonsterDict.TryGetValue(spawnData.monsterid, out monsterData);

                        if (monsterData == null) continue;

                        if (monsterData.npc == true)
                        {
                            count = GetNpcCount(m => m.Force == false && m.SpawnID == spawnData.spawnid);
                        }
                        else
                        {
                            count = GetMonsterCount(m => m.Force == false && m.SpawnID == spawnData.spawnid);
                        }

                        int maxCount = spawnData.count;

                        if (count < maxCount)
                        {
                            // 몬스터 생성
                            for (int i = 0; i < maxCount - count; i++)
                            {                                                             
                                if (monsterData.npc == true)
                                {                                   
                                    NPC npc = ObjectManager.Instance.Add<NPC>();
                                    npc.Init(spawnData.monsterid);
                                    npc.SpawnID = spawnData.spawnid;
                                    npc.Npc = true;
                                    npc.MapId = RoomId;

                                    npc.Info.PosInfo.State = CreatureState.Idle;
                                    npc.Info.PosInfo.MoveDir = MoveDir.Down;

                                    //monster.cellpos = new vector2int(5, 5);
                                    Vector2Int pos = new Vector2Int(spawnData.x, spawnData.y);
                                    bool randomPos = false;
                                    if (pos.x == -1 && pos.y == -1)
                                        randomPos = true;
                                    EnterGame(npc, randomPos, pos);

                                    Console.WriteLine($"spawn npc {RoomId}, {monsterData.name}, {npc.ObjectType}, {spawnData.monsterid}, {npc.Id}");
                                }
                                else
                                {
                                    Monster monster = ObjectManager.Instance.Add<Monster>();
                                    monster.Init(spawnData.monsterid);
                                    monster.SpawnID = spawnData.spawnid;
                                    monster.MapId = RoomId;

                                    monster.Info.PosInfo.State = CreatureState.Idle;
                                    monster.Info.PosInfo.MoveDir = MoveDir.Down;

                                    //monster.cellpos = new vector2int(5, 5);
                                    Vector2Int pos = new Vector2Int(spawnData.x, spawnData.y);
                                    bool randomPos = false;
                                    if (pos.x == -1 && pos.y == -1)
                                        randomPos = true;
                                    EnterGame(monster, randomPos, pos);
                                }
                            }
                        }
                    }
                }

                //foreach (Monster monster in _monsters.Values)
                //{
                //    monster.Update();
                //}
                //foreach (Projectile projectile in _projectiles.Values)
                //{
                //    projectile.Update();
                //}

                
            }
            catch (Exception e)
            {

            }
        }

        Random _rand = new Random();
        public void EnterGame(GameObject gameObject, bool randomPos, Vector2Int pos)
        {
            if (gameObject == null)
                return;

            if (randomPos)
            {
                Vector2Int respawnPos;
                while (true)
                {
                    respawnPos.x = _rand.Next(Map.MinX, Map.MaxX + 1);
                    respawnPos.y = _rand.Next(Map.MinY, Map.MaxY + 1);
                    if (Map.Find(respawnPos) == null)
                    {
                        gameObject.CellPos = respawnPos;
                        break;
                    }
                }
            }
            else
            {
                if (Map.Find(pos) == null)
                {
                    gameObject.CellPos = pos;
                }
            }

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            if (type == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                _players.Add(gameObject.Id, player);
                player.Room = this;

                player.RefreshAdditionalStat();

                Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));

                GetZone(player.CellPos).Players.Add(player);

                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.Info;
                    enterPacket.Player.PosInfo.MapId = player.MapId;
                    player.Session.Send(enterPacket);

                    player.Vision.Update();
                    //S_Spawn spawnPacket = new S_Spawn();
                    //foreach (Player p in _players.Values)
                    //{
                    //    if (player != p)
                    //        spawnPacket.Objects.Add(p.Info);
                    //}
                    //foreach (Monster m in _monsters.Values)
                    //{
                    //    spawnPacket.Objects.Add(m.Info);
                    //}
                    //foreach (Projectile p in _projectiles.Values)
                    //{
                    //    spawnPacket.Objects.Add(p.Info);
                    //}
                    //player.Session.Send(spawnPacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);
                monster.Room = this;

                Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));

                GetZone(monster.CellPos).Monsters.Add(monster);

                monster.Update();
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = gameObject as Projectile;
                _projectiles.Add(gameObject.Id, projectile);
                projectile.Room = this;

                GetZone(projectile.CellPos).Projectiles.Add(projectile);
                projectile.Update();
                //Map.ApplyMove(projectile, new Vector2Int(projectile.CellPos.x, projectile.CellPos.y));
            }
            else if (type == GameObjectType.Npc)
            {
                NPC npc = gameObject as NPC;
                _npcs.Add(gameObject.Id, npc);
                npc.Room = this;

                GetZone(npc.CellPos).Npcs.Add(npc);
                npc.Update();
            }

            {
                S_Spawn spawnPacket = new S_Spawn();

                ObjectInfo info = new ObjectInfo();
                info.MergeFrom(gameObject.Info);

                if (type == GameObjectType.Monster)
                {
                    Monster monster = gameObject as Monster;

                    info.TemplateId = monster.TemplateId;
                }
                else if(type == GameObjectType.Npc )
                {
                    NPC npc = gameObject as NPC;

                    info.TemplateId = npc.TemplateId;
                }

                spawnPacket.Objects.Add(info);

                //foreach (Player p in _players.Values)
                //{
                //    if (p.Id != gameObject.Id)
                //        p.Session.Send(spawnPacket);
                //}
                Broadcast(gameObject.CellPos, spawnPacket);
            }
        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);
            Vector2Int cellPos;
            if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;

                //_players.Remove(player);
                cellPos = player.CellPos;
                //GetZone(player.CellPos).Players.Remove(player);

                player.OnleaveGame();
                Map.ApplyLeave(player);
                player.Room = null;

                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = null;
                if (_monsters.Remove(objectId, out monster) == false)
                    return;
                               
                Map.ApplyLeave(monster);

                cellPos = monster.CellPos;
                //GetZone(monster.CellPos).Monsters.Remove(monster);

                monster.Room = null;
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = null;
                if (_projectiles.Remove(objectId, out projectile) == false)
                    return;

                cellPos = projectile.CellPos;
                Map.ApplyLeave(projectile);
                //GetZone(projectile.CellPos).Projectiles.Remove(projectile);

                projectile.Room = null;
            }
            else if (type == GameObjectType.Npc)
            {
                NPC npc = null;
                if (_npcs.Remove(objectId, out npc) == false)
                    return;

                Map.ApplyLeave(npc);

                cellPos = npc.CellPos;
                //GetZone(monster.CellPos).Monsters.Remove(monster);

                npc.Room = null;
            }
            else
            {
                return;
            }

            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
                //foreach (Player p in _players.Values)
                //{
                //    p.Session.Send(despawnPacket);
                //}
                Broadcast(cellPos, despawnPacket);
            }

        }       

        Player FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player player in _players.Values)
            {
                if (condition.Invoke(player))
                    return player;
            }

            return null;
        }

        public Player FindClosestPlayer(Vector2Int pos, int range)
        {
            List<Player> players = GetAdjacentPlayers(pos, range);

            players.Sort((left, right) =>
            {
                int leftDist = (left.CellPos - pos).cellDistFromZero;
                int rightDist = (right.CellPos - pos).cellDistFromZero;

                return leftDist - rightDist;
            });

            foreach(Player player in players)
            {
                List<Vector2Int> path = Map.FindPath(pos, player.CellPos, checkObjects: true);
                if (path.Count < 2 || path.Count > range)
                    continue;

                return player;
            }

            return null;
        }

        public void Broadcast(Vector2Int pos, IMessage packet)
        {
            List<Zone> zones = GetAdjacentZones(pos);
            foreach(Zone zone in zones)
            {
                foreach(Player p in zone.Players)
                {
                    int dx = p.CellPos.x - pos.x;
                    int dy = p.CellPos.y - pos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if (Math.Abs(dy) > GameRoom.VisionCells)
                        continue;

                    p.Session.Send(packet);
                }
            }
            //foreach (Player p in _players.Values)
            //{
            //    p.Session.Send(packet);
            //}
        }

        public void BroadcastRoomAllPlayer(IMessage packet)
        {
            foreach (Player p in _players.Values)
            {                
                p.Session.Send(packet);
            }
        }

        public List<Player> GetAdjacentPlayers(Vector2Int pos, int range)
        {
            List<Zone> zones = GetAdjacentZones(pos, range);
            return zones.SelectMany(z => z.Players).ToList();
        }

        public List<Zone> GetAdjacentZones(Vector2Int cellPos, int range = VisionCells)
        {
            HashSet<Zone> zones = new HashSet<Zone>();

            int maxY = cellPos.y + range;
            int minY = cellPos.y - range;
            int maxX = cellPos.x + range;
            int minX = cellPos.x - range;

            Vector2Int leftTop = new Vector2Int(minX, maxY);

            int minIndexX = (leftTop.x - Map.MinX) / ZoneCells;
            int minIndexY = (Map.MaxY - leftTop.y) / ZoneCells;

            Vector2Int rightBot = new Vector2Int(maxX, minY);

            int maxIndexX = (rightBot.x - Map.MinX) / ZoneCells;
            int maxIndexY = (Map.MaxY - rightBot.y) / ZoneCells;

            for( int x = minIndexX; x <= maxIndexX; x++)
            {
                for(int y = minIndexY; y <= maxIndexY; y++ )
                {
                    Zone zone = GetZone(y, x);
                    if (zone == null)
                        continue;

                    zones.Add(zone);
                }
            }

            int[] delta = new int[2] { -range, +range };

            foreach(int dy in delta)
            {
                foreach(int dx in delta)
                {
                    int y = cellPos.y + dy;
                    int x = cellPos.x + dx;
                    Zone zone = GetZone(new Vector2Int(x, y));
                    if (zone == null)
                        continue;

                    zones.Add(zone);
                }
            }

            return zones.ToList();
        }

        public int GetMonsterCount(Func<Monster, bool> condition)
        {
            int count = 0;

            foreach (Monster monster in _monsters.Values)
            {
                if (condition.Invoke(monster))
                    count++;
            }

            return count;
        }

        public int GetNpcCount(Func<NPC, bool> condition)
        {
            int count = 0;

            foreach (NPC npc in _npcs.Values)
            {
                if (condition.Invoke(npc))
                    count++;
            }

            return count;
        }
    }
}
