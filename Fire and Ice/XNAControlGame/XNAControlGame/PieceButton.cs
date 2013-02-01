using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using Microsoft.Xna.Framework;

namespace XNAControlGame
{
    public class PieceButton : Button
    {
        public CreeperColor Color { get; set; }

        public PieceButton(Game1 game, Vector2 position, Vector2 size, OnClickDelegate onClick) : base(game, position, size, onClick, "")
        {

        }
    }
}
