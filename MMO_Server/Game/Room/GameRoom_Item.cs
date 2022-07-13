﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using MMO_Server.Data;
using MMO_Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMO_Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleEquipItem(Player player, C_EquipItem equipPacket)
        {
            if (player == null) return;

            player.HandleEquipItem(equipPacket);
        }
    }
}
