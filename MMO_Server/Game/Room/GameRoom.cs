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

        public Zone[,] Zones { get; private set; }
        public int ZoneCells { get; private set; }
        public Map Map { get; private set; } = new Map();

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

        public void Init(string mapName, int zoneCells)
        {
            Map.LoadMap(mapName, "../../../../Common/MapData");

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

            if( string.Compare(mapName, "Beginner_1") == 0 )
            {
                // 몬스터 생성
                for (int i = 0; i < 500; i++)
                {
                    Monster monster = ObjectManager.Instance.Add<Monster>();
                    monster.Init(1);
                    //monster.cellpos = new vector2int(5, 5);
                    EnterGame(monster, randomPos: true);
                }
            }


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
                //foreach (Monster monster in _monsters.Values)
                //{
                //    monster.Update();
                //}
                //foreach (Projectile projectile in _projectiles.Values)
                //{
                //    projectile.Update();
                //}

                Flush();
            }
            catch (Exception e)
            {

            }
        }

        Random _rand = new Random();
        public void EnterGame(GameObject gameObject, bool randomPos)
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

            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.Info);
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
    }
}
