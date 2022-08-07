﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using MMO_Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMO_Server.Game
{
    public partial class GameRoom : JobSerializer
    {       
        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null) return;


            PositionInfo movePosInfo = movePacket.PosInfo;
            ObjectInfo info = player.Info;
            //info.PosInfo = movePacket.PosInfo;

            if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
            {
                if (Map.Warp(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == true)
                {                    
                    //GameRoom leaveroom = GameLogic.Instance.Find(player.MapId);
                    player.Room.Push(player.Room.LeaveGame, player.Id);
                    //leaveroom.Push(leaveroom.LeaveGame, player.Id);

                    GameRoom enterroom = GameLogic.Instance.Find(2);
                    player.MapId = 2;
                    enterroom.Push(enterroom.EnterGame, player, true, new Vector2Int(-1, -1));

                    return;
                }

                if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                    return;                               
            }

            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.MoveDir = movePosInfo.MoveDir;
            Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

            S_Move resMovePacket = new S_Move();
            resMovePacket.ObjectId = player.Info.ObjectId;
            resMovePacket.PosInfo = movePacket.PosInfo;

            Broadcast(player.CellPos, resMovePacket);

        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null) return;
            
            ObjectInfo info = player.Info;
            if (info.PosInfo.State != CreatureState.Idle)
                return;

            info.PosInfo.State = CreatureState.Skill;

            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = info.ObjectId;
            skill.Info.SkillId = skillPacket.Info.SkillId;
            Broadcast(player.CellPos, skill);

            Data.Skill skillData = null;
            if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false) return;

            switch (skillData.skillType)
            {
                case SkillType.SkillAuto:
                    {
                        Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);

                        GameObject target = Map.Find(skillPos);
                        if (target != null)
                        {
                            Console.WriteLine($"Hit GameObject : {target.Info.ObjectId}, MyPos : {player.Info.PosInfo.PosX}, {player.Info.PosInfo.PosY}, TargetPos : {target.Info.PosInfo.PosX}, {target.Info.PosInfo.PosY}");
                        }
                    }
                    break;
                case SkillType.SkillProjectile:
                    {
                        Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                        if (arrow == null)
                            return;

                        arrow.Owner = player;
                        arrow.Data = skillData;
                        arrow.PosInfo.State = CreatureState.Moving;
                        arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
                        arrow.PosInfo.PosX = player.PosInfo.PosX;
                        arrow.PosInfo.PosY = player.PosInfo.PosY;
                        arrow.Speed = skillData.projectile.speed;
                        EnterGame(arrow, false, new Vector2Int(arrow.PosInfo.PosX, arrow.PosInfo.PosY));
                    }
                    break;
            }

            //if ( skillPacket.Info.SkillId == 1)
            //{
            //    Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);

            //    GameObject target = Map.Find(skillPos);
            //    if (target != null)
            //    {
            //        Console.WriteLine($"Hit GameObject : {target.Info.ObjectId}, MyPos : {player.Info.PosInfo.PosX}, {player.Info.PosInfo.PosY}, TargetPos : {target.Info.PosInfo.PosX}, {target.Info.PosInfo.PosY}");
            //    }
            //}
            //else if(skillPacket.Info.SkillId == 2 )
            //{
            //    Arrow arrow = ObjectManager.Instance.Add<Arrow>();
            //    if (arrow == null)
            //        return;

            //    arrow.Owner = player;
            //    arrow.PosInfo.State = CreatureState.Moving;
            //    arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
            //    arrow.PosInfo.PosX = player.PosInfo.PosX;
            //    arrow.PosInfo.PosY = player.PosInfo.PosY;
            //    EnterGame(arrow);

            //    //info.PosInfo.State = CreatureState.Idle;
            //}                                             

        }

        public void HandleKill(Player player, C_PlayerKill killPacket)
        {
            if (player == null) return;

            player.OnDamaged(player, player.Stat.MaxHp);
        }
    }
}
