using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public class Tile
    {
        public CreeperColor Color { get; set; }
        public bool Marked { get;  set; }
        public bool HasTile { get { return Color != CreeperColor.Empty; } }

        public Tile(CreeperColor color = CreeperColor.Empty)
        {
            Color = color;
        }
    }
}
