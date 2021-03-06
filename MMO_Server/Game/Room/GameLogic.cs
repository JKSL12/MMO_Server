using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMO_Server.Game
{
    public class GameLogic : JobSerializer
    {
        public static GameLogic Instance { get; } = new GameLogic();

       // object _lock = new object();
        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int _roomId = 1;

        public void Update()
        {
            Flush();

            foreach(GameRoom room in _rooms.Values)
            {
                room.Update();
            }
        }

        public GameRoom Add(int mapId)
        {
            GameRoom gameRoom = new GameRoom();
            //gameRoom.Init(mapId);
            gameRoom.Push(gameRoom.Init, mapId, 10);

            //lock(_lock)
            {
                gameRoom.RoomId = _roomId;
                _rooms.Add(_roomId, gameRoom);
                _roomId++;
            }

            return gameRoom;
        }

        public GameRoom Add(string mapName)
        {
            GameRoom gameRoom = new GameRoom();
            //gameRoom.Init(mapId);
            gameRoom.Push(gameRoom.Init, mapName, 10);

            //lock(_lock)
            {
                gameRoom.RoomId = _roomId;
                _rooms.Add(_roomId, gameRoom);
                _roomId++;
            }

            return gameRoom;
        }

        public bool Remove(int roomId)
        {
            //lock(_lock)
            {
                return _rooms.Remove(roomId);
            }
        }

        public GameRoom Find(int roomId)
        {
            //lock (_lock)
            {
                GameRoom room = null;
                if(_rooms.TryGetValue(roomId, out room))
                    return room;

                return null;
            }
        }

        public void BroadcastAllMapAllPlayer(IMessage packet)
        {
            foreach(GameRoom r in _rooms.Values)
            {
                r.BroadcastRoomAllPlayer(packet);
            }
        }
    }
}
