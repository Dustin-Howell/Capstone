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
        public int SlotNumber { get; private set; }
        public Position Position { get { return CreeperUtility.NumberToPosition(SlotNumber, false); } }
        public List<Tile> Neighbors { get; private set; }

        public void SetNeighbors(List<Tile> neighbors)
        {
            Neighbors = neighbors;
        }

        public Tile(CreeperColor color, int slotNumber)
        {
            Color = color;
            SlotNumber = slotNumber;
        }
    }
}
