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
        public AIBoardNode North { get; set; }
        public AIBoardNode South { get; set; }
        public AIBoardNode East { get; set; }
        public AIBoardNode West { get; set; }

        public NodeType NodeType { get; set; }

        public AIBoardNode(NodeType color)
        {
            NodeType = color;
        }
    }
}
