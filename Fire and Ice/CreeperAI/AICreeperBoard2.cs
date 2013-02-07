using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperAI
{
    //Only adds nodes where there are tiles
    //Nodes point to positions, not nodes
    class AICreeperBoard2
    {
        AIBoardNode2[ , ] Board { get; set; }
        // This will be tileRows + 1 to account for heads
        // Which makes me realize, the pieces in our AI board are going to be 1 indexed, aren't they?
        private int _boardRows;

        public AICreeperBoard2(CreeperBoard board)
        {
            _boardRows = CreeperBoard.TileRows + 1;
            //the plus one is for the head node
            Board = new AIBoardNode2[_boardRows, _boardRows];

            foreach (Piece tile in board.Tiles)
            {
                Board[tile.Position.Row + 1, tile.Position.Column + 1] = new AIBoardNode2(tile.Color.ToNodeType());
            }

            for (int row = 0; row < _boardRows; row++)
            {
                for (int column = 0; column < _boardRows; column++)
                {
                    if (row == 0 || column == 0)
                    {
                        Board[row, column].NodeType = NodeType.Head;
                    }
                    
                    // For the moment, this will crash, this just conveys the idea
                    Board[row, column].North = new Position(row - 1, column);
                    Board[row, column].South = new Position(row + 1, column);
                    Board[row, column].East = new Position(row, column + 1);
                    Board[row, column].West = new Position(row, column - 1);
                }
            }
        }

    }
}
