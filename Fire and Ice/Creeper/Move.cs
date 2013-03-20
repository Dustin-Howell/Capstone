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

        public Move()
        {
        }

        public Move(Position startPosition, Position endPosition, CreeperColor playerColor)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            PlayerColor = playerColor;
        }

        public Move(Move move)
        {
            this.StartPosition = new Position(move.StartPosition);
            this.EndPosition = new Position(move.EndPosition);
            this.PlayerColor = move.PlayerColor;
        }
    }
}
