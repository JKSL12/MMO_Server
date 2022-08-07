using Google.Protobuf.Protocol;
using MMO_Server.Data;
using MMO_Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMO_Server.Game
{
    public class Monster : GameObject
    {
        public int TemplateId { get; private set; }
        public bool Force { get; set; }
        public int SpawnID { get; set; }

        public Monster()
        {
            ObjectType = GameObjectType.Monster;

           
        }

        public void Init(int templateId)
        {
            TemplateId = templateId;
            Force = false;

            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);
            Stat.MergeFrom(monsterData.stat);            
            Stat.Hp = monsterData.stat.MaxHp;
            
            State = CreatureState.Idle;
        }

        IJob _job;
        public override void Update()
        {
            switch(State)
            {
                case CreatureState.Idle:
                    UpdateIdle();
                    break;
                case CreatureState.Moving:
                    UpdateMoving();
                    break;
                case CreatureState.Skill:
                    UpdateSkill();
                    break;
                case CreatureState.Dead:
                    UpdateDead();
                    break;
            }

            if (Room != null)
                _job = Room.PushAfter(200, Update);
        }

        Player _target;
        int _searchCellDist = 10;
        int _chaseCellDist = 20;

        long _nextSearchTick = 0;
        protected virtual void UpdateIdle()
        {
            if (_nextSearchTick > Environment.TickCount)
                return;

            _nextSearchTick = Environment.TickCount + 1000;

            //Player target = Room.FindPlayer(p =>
            //{
            //    Vector2Int dir = p.CellPos - CellPos;
            //    return dir.cellDistFromZero <= _searchCellDist;
            //});

            Player target = Room.FindClosestPlayer(CellPos, _searchCellDist);

            if (target == null) return;

            _target = target;
            State = CreatureState.Moving;
        }

        int _skillRange = 1;
        long _nextMoveTick = 0;
        protected virtual void UpdateMoving()
        {
            if (_nextMoveTick > Environment.TickCount)
                return;
            int moveTick = (int)(1000 / Speed);
            _nextMoveTick = Environment.TickCount + moveTick;

            if( _target == null || _target.Room != Room )
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            Vector2Int dir = _target.CellPos - CellPos;
            int dist = dir.cellDistFromZero;
            if( dist == 0 || dist > _chaseCellDist )
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObjects: true);
            if(path.Count < 2 || path.Count > _chaseCellDist)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            if( dist <= _skillRange && (dir.x == 0 || dir.y == 0) )
            {
                _coolTick = 0;
                State = CreatureState.Skill;
                return;
            }

            Dir = GetDirFromVec(path[1] - CellPos);
            Room.Map.ApplyMove(this, path[1]);
            BroadcastMove();
        }

        void BroadcastMove()
        {
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(CellPos, movePacket);
        }

        long _coolTick = 0;
        protected virtual void UpdateSkill()
        {
            if( _coolTick == 0 )
            {
                if(_target == null || _target.Room != Room || _target.Hp < 0 )
                {
                    _target = null;
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }

                Vector2Int dir = (_target.CellPos - CellPos);
                int dist = dir.cellDistFromZero;
                bool canUseSkill = (dist <= _skillRange && (dir.x == 0 || dir.y == 0));
                if( canUseSkill == false)
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }

                MoveDir lookDir = GetDirFromVec(dir);
                if(Dir != lookDir)
                {
                    Dir = lookDir;
                    BroadcastMove();
                }

                Skill skillData = null;
                DataManager.SkillDict.TryGetValue(1, out skillData);

                _target.OnDamaged(this, skillData.damage + TotalAttack);

                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = Id;
                skill.Info.SkillId = skillData.id;
                Room.Broadcast(CellPos, skill);

                int coolTick = (int)(1000 * skillData.cooldown);
                _coolTick = Environment.TickCount + coolTick;
            }

            if (_coolTick > Environment.TickCount)
                return;

            _coolTick = 0;
        }

        protected virtual void UpdateDead()
        {

        }

        public override void OnDead(GameObject attacker)
        {
            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }

            base.OnDead(attacker);

            GameObject owner = attacker.GetOwner();

            if(owner.ObjectType == GameObjectType.Player)
            {
                Player player = (Player)owner;

                Int32 exp = Stat.TotalExp;

                DbTransaction.IncreaseExp(player, exp, Room);

                RewardData rewardData = GetRandomReward();
                if( rewardData != null)
                {                    
                    DbTransaction.RewardPlayer(player, rewardData, Room);
                }
            }            
        }

        

        RewardData GetRandomReward()
        {
            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);

            int rand = new Random().Next(0, 101);
            int sum = 0;
            foreach(RewardData rewardData in monsterData.rewards)
            {
                sum += rewardData.probability;
                if( rand <= sum)
                {
                    return rewardData;
                }
            }

            return null;
        }
    }
}
