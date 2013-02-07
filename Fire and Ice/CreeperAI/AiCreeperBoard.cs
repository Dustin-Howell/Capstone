using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace CreeperAI
{
    // Every slot in the array has a node, some are just defined as empty
    // Nodes have reference to neighbor nodes
    public class AICreeperBoard
    {
        AIBoardNode[ , ] Board { get; set; }
        // This will be tileRows + 1 to account for heads
        // Which makes me realize, the pieces in our AI board are going to be 1 indexed, aren't they?
        private int _boardRows;

        public AICreeperBoard(CreeperBoard board)
        {
            _boardRows = CreeperBoard.TileRows + 1;
            //the plus one is for the head node
            Board = new AIBoardNode[_boardRows, _boardRows];

            foreach (Piece tile in board.Tiles)
            {
                Board[tile.Position.Row + 1, tile.Position.Column + 1] = new AIBoardNode(tile.Color.ToNodeType());
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
                    Board[row, column].North = Board[row - 1, column];
                    Board[row, column].South = Board[row + 1, column];
                    Board[row, column].East = Board[row, column + 1];
                    Board[row, column].West = Board[row, column - 1];
                }
            }
        }


    }
}