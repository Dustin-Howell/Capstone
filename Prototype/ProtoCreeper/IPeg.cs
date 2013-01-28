using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCreeper
{
    public interface IPeg
    {
        Color Color { get; set; }
        bool HasPeg { get; }
    }

    public class ProtoPeg : IPeg
    {
        public Color Color { get; set; }

        public bool HasPeg
        {
            get { return Color != Color.Empty; }
        }

        public ProtoPeg(Color color = Color.Empty)
        {
            Color = color;
        }
    }

}
