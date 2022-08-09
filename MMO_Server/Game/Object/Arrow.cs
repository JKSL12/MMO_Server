using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMO_Server.Game
{
    class Arrow : Projectile
    {
        public GameObject Owner { get; set; }

       // long _nextMoveTick = 0;

        public override void Update()
        {
            if (Data == null || Data.projectile == null || Owner == null || Room == null)
                return;

            //if (_nextMoveTick >= Environment.TickCount)
            //    return;

            int tick = (int)(1000 / Data.projectile.speed);
            Room.PushAfter(tick, Update);

           // _nextMoveTick = Environment.TickCount + tick;

            Vector2Int destPos = GetFrontCellPos();
            if( Room.Map.ApplyMove(this, destPos, collision: false))
            {
                //CellPos = destPos;

                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.PosInfo = PosInfo;
                Room.Broadcast(CellPos, movePacket);              
            }
            else
            {
                GameObject target = Room.Map.Find(destPos);
                if(target != null)
                {
                    if (target as NPC == null)
                    {
                        target.OnDamaged(this, Data.damage + Owner.TotalAttack);
                    }
                }
                
                //Room.LeaveGame(Id);
                Room.Push(Room.LeaveGame, Id);
            }
        }

        public override GameObject GetOwner()
        {
            return Owner;
        }
    }
}
