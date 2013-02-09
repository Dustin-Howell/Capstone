using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperAI
{
    class AIBoardNode
    {
        public AIBoardNode TeamNorth { get; set; }
        public AIBoardNode TeamSouth { get; set; }
        public AIBoardNode TeamEast { get; set; }
        public AIBoardNode TeamWest { get; set; }

        public CreeperColor Color { get; set; }

        public int Row { get; private set; }
        public int Column { get; private set; }

        public AIBoardNode(int row, int column, CreeperColor color)
        {
            Color = color;
            Row = row;
            Column = column;
        }

        public AIBoardNode(CreeperColor color)
        {
            Color = color;
            Row = -1;
            Column = -1;
        }
    }
}
