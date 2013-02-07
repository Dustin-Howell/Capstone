using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperAI
{
    class AIBoardNode2
    {
        public Position North { get; set; }
        public Position South { get; set; }
        public Position East { get; set; }
        public Position West { get; set; }

        public NodeType NodeType {get; set;}

        public AIBoardNode2(NodeType nodeType)
        {

        }
    }
}
