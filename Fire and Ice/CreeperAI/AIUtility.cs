using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperAI
{
    public static class AIUtility
    {
        public static NodeType ToNodeType(this CreeperColor color)
        {
            NodeType nodeType;

            switch (color)
            {
                case CreeperColor.Black:
                    nodeType = NodeType.Black;
                    break;
                case CreeperColor.White:
                    nodeType = NodeType.White;
                    break;
                case CreeperColor.Empty:
                    nodeType = NodeType.Empty;
                    break;
                case CreeperColor.Invalid:
                    nodeType = NodeType.Invalid;
                    break;
                default:
                    nodeType = NodeType.Empty;
                    break;
            }

            return nodeType;
        }
    }
}
