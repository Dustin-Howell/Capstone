using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public class Piece
    {
        public Position Position { get; private set; }
        public CreeperColor Color { get; set; }
        public bool HasPiece { get { return Color == CreeperColor.Empty; } }

        public Piece(CreeperColor color, Position position)
        {
            Color = color;
            Position = position;
        }

        public Piece(Piece piece)
        {
            Position = new Position(piece.Position);
            Color = piece.Color;
        }
    }
}
