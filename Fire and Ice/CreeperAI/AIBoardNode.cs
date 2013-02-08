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

        public AIBoardNode(CreeperColor color)
        {
            Color = color;
        }
    }
}
