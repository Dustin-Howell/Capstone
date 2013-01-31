using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public class Tile : Piece
    {
        public bool HasTile { get { return Color != CreeperColor.Empty; } }
        public List<Tile> Neighbors { get; private set; }

        public void SetNeighbors(List<Tile> neighbors)
        {
            Neighbors = neighbors;
        }

        public Tile(CreeperColor color, Position position) : base(position)
        {
            Color = color;
        }
    }
}
