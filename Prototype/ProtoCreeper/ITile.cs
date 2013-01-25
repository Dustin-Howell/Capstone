using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCreeper
{
    public interface ITile
    {
        Color Color { get; set; }
        bool HasTile { get; }
    }

    public class ProtoTile : ITile
    {
        public Color Color { get; set; }
        public bool HasTile { get { return Color != Color.Empty; } }

        public ProtoTile(Color color = Color.Empty)
        {
            Color = color;
        }
    }
}
