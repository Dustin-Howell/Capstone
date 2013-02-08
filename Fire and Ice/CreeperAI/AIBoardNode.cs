using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperAI
{
    public enum NodeType { Black, White, Invalid, Empty, Head }

    class AIBoardNode
    {
        public AIBoardNode TeamNorth { get; set; }
        public AIBoardNode TeamSouth { get; set; }
        public AIBoardNode TeamEast { get; set; }
        public AIBoardNode TeamWest { get; set; }

        public NodeType NodeType { get; set; }

        public AIBoardNode(NodeType color)
        {
            NodeType = color;
        }
    }
}
