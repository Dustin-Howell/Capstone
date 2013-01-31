using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{

    public class Peg : Piece
    {
        public bool HasPeg
        {
            get { return Color != CreeperColor.Empty; }
        }

        public Peg(CreeperColor color, Position position) : base(position)
        {
            Color = color;
        }
    }

}
