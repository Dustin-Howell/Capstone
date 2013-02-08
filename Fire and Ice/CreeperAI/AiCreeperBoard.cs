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
        private int _headIndex;

        public AICreeperBoard(CreeperBoard board)
        {
            _boardRows = CreeperBoard.TileRows;

            Board = new AIBoardNode[_boardRows, _boardRows];

            foreach (Piece tile in board.Tiles)
            {
                Board[tile.Position.Row, tile.Position.Column] = new AIBoardNode(tile.Color.ToNodeType());
            }

            for (int row = 0; row < _boardRows; row++)
            {
                for (int column = 0; column < _boardRows; column++)
                {
                    if (Board[row, column].NodeType == NodeType.Black || Board[row, column].NodeType == NodeType.White)
                    {
                        Board[row, column].TeamNorth = GetNextNode(row, column, CardinalDirection.North);
                        Board[row, column].TeamSouth = GetNextNode(row, column, CardinalDirection.South);
                        Board[row, column].TeamEast = GetNextNode(row, column, CardinalDirection.East);
                        Board[row, column].TeamWest = GetNextNode(row, column, CardinalDirection.West);
                    }                    
                }
            }


        }

        private AIBoardNode GetNextNode(int row, int column, CardinalDirection direction)
        {
            int rowIncrement = 0;
            int columnIncrement = 0;

            int currentRow = row;
            int currentColumn = column;

            AIBoardNode nextNode = Board[row, column];

            switch (direction)
            {
                case CardinalDirection.North:
                    rowIncrement = -1;
                    break;
                case CardinalDirection.South:
                    rowIncrement = 1;
                    break;
                case CardinalDirection.East:
                    columnIncrement = 1;
                    break;
                case CardinalDirection.West:
                    columnIncrement = -1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Only pass North, South, East or West.");
            }

            do
            {
                currentRow = (currentRow + rowIncrement) % _boardRows;
                currentColumn = (currentColumn + columnIncrement) % _boardRows;

                nextNode = (Board[currentRow, currentColumn].NodeType == nextNode.NodeType) ? Board[currentRow, currentColumn] : nextNode;                
            }
            while (nextNode != Board[currentRow, currentColumn]);

            return nextNode;
        }


    }
}