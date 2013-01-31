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

        protected Piece(Position position)
        {
            Position = position;
        }
    }
}
