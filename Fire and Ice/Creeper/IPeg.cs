using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{

    public class Peg
    {
        public CreeperColor Color { get; set; }
        public List<Peg> Neighbors { get; set; }

        public bool HasPeg
        {
            get { return Color != CreeperColor.Empty; }
        }

        public Peg(CreeperColor color = CreeperColor.Empty)
        {
            Color = color;
        }
    }

}
