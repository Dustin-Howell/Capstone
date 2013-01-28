using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public interface IPeg
    {
        CreeperColor Color { get; set; }
        bool HasPeg { get; }
    }

    public class ProtoPeg : IPeg
    {
        public CreeperColor Color { get; set; }

        public bool HasPeg
        {
            get { return Color != CreeperColor.Empty; }
        }

        public ProtoPeg(CreeperColor color = CreeperColor.Empty)
        {
            Color = color;
        }
    }

}
