using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public class Move
    {
        public Position StartPosition { get; set; }
        public Position EndPosition { get; set; }
        public CreeperColor PlayerColor { get; set; }

        public Move(Position startPosition, Position endPosition, CreeperColor playerColor)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            PlayerColor = playerColor;
        }
    }
}
