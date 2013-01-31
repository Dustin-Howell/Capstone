using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{

    public class Peg
    {
        public CreeperColor Color { get; set; }
        public int SlotNumber { get; private set; }
        public Position Position { get { return CreeperUtility.NumberToPosition(SlotNumber, true); } }

        public bool HasPeg
        {
            get { return Color != CreeperColor.Empty; }
        }

        public Peg(CreeperColor color, int slotNumber)
        {
            Color = color;
            SlotNumber = slotNumber;
        }
    }

}
