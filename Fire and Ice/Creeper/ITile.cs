using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public interface ITile
    {
        CreeperColor Color { get; set; }
        bool HasTile { get; }
    }

    public class ProtoTile : ITile
    {
        public CreeperColor Color { get; set; }
        public bool HasTile { get { return Color != CreeperColor.Empty; } }

        public ProtoTile(CreeperColor color = CreeperColor.Empty)
        {
            Color = color;
        }
    }
}
