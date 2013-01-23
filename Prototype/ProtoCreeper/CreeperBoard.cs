using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCreeper
{
    enum PegSlot { Blank, Black, White, Corner }
    enum Tile { Blank, Black, White, Corner }

    public class CreeperBoard
    {
        public static const int TileRows = 6;
        public static const int PegRows = 7;

        protected List<List<PegSlot>> Pegs;
        protected List<List<Tile>> Tiles;

        public CreeperBoard()
        {
            Pegs = new List<List<PegSlot>>();
            Tiles = new List<List<Tile>>();
        }

        public void ResetCreeperBoard()
        {
            Tiles.Clear();
            Pegs.Clear();

            for (int i = 0; i < TileRows; i++)
            {
                Tiles[i] = new List<Tile>();
            }

            for (int i = 0; i < PegRows; i++)
            {
                Pegs[i] = new List<PegSlot>();
            }
        }

        public bool IsCorner(int row, int col, bool TileSpace)
        {
            bool isCorner = false;
            int rows = (TileSpace) ? TileRows : PegRows;

            if ((row == 0 && col == 0)
                || (row == 0 && col == rows - 1)
                || (row == rows - 1 && col == 0)
                || (row == rows - 1 && col == rows - 1)
                )
            {
                isCorner = true;
            }

            return isCorner;
        }
    }
}
