using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperAI
{
    public class AIBoardNode
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

        public AIBoardNode(Piece piece)
        {
            Color = piece.Color;
            Row = piece.Position.Row;
            Column = piece.Position.Column;
        }

        public AIBoardNode(CreeperColor color)
        {
            Color = color;
            Row = -1;
            Column = -1;
        }

        public AIBoardNode(AIBoardNode node)
        {
            Color = node.Color;
            Row = node.Row;
            Column = node.Column;
        }
    }
}
