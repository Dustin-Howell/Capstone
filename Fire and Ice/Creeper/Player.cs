﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace Creeper
{
    public enum PlayerType { AI, Human, Network, Invalid }

    public class Player
    {
        public CreeperColor Color { get; private set; }
        public PlayerType Type { get; private set; }

        public Player(PlayerType playerType, CreeperColor creeperColor)
        {
            Type = playerType;
            Color = creeperColor;
        }

        public Player(Player player)
        {
            Color = player.Color;
            Type = player.Type;
        }
    }
}
