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
        AIBoardNode[ , ] TileBoard { get; set; }
        AIBoardNode[ , ] PegBoard { get; set; }
        AIBoardNode[] RowHeadBlack { get; set; }
        AIBoardNode[] RowHeadWhite { get; set; }
        AIBoardNode[] ColumnHeadBlack { get; set; }
        AIBoardNode[] ColumnHeadWhite { get; set; }
        // This will be tileRows + 1 to account for heads
        // Which makes me realize, the pieces in our AI board are going to be 1 indexed, aren't they?
        private int _tileRows;
        private int _pegRows;
        private int _headIndex;

        public AICreeperBoard(CreeperBoard board)
        {
            _tileRows = CreeperBoard.TileRows;
            _pegRows = CreeperBoard.PegRows;

            TileBoard = new AIBoardNode[_tileRows, _tileRows];
            PegBoard = new AIBoardNode[_pegRows, _pegRows];
            RowHeadBlack = new AIBoardNode[_tileRows];
            RowHeadWhite = new AIBoardNode[_tileRows];
            ColumnHeadBlack = new AIBoardNode[_tileRows];
            ColumnHeadWhite = new AIBoardNode[_tileRows];

            foreach (Piece tile in board.Tiles)
            {
                TileBoard[tile.Position.Row, tile.Position.Column] = new AIBoardNode(tile.Color);
            }

            foreach (Piece peg in board.Pegs)
            {
                PegBoard[peg.Position.Row, peg.Position.Column] = new AIBoardNode(peg.Color);
            }

            for (int row = 0; row < _tileRows; row++)
            {
                for (int column = 0; column < _tileRows; column++)
                {
                    if (TileBoard[row, column].Color == CreeperColor.Black || TileBoard[row, column].Color == CreeperColor.White)
                    {
                        UpdateListHeads(row, column, TileBoard[row, column].Color);
                        TileBoard[row, column].TeamNorth = GetNextNode(row, column, CardinalDirection.North);
                        TileBoard[row, column].TeamSouth = GetNextNode(row, column, CardinalDirection.South);
                        TileBoard[row, column].TeamEast = GetNextNode(row, column, CardinalDirection.East);
                        TileBoard[row, column].TeamWest = GetNextNode(row, column, CardinalDirection.West);
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

            AIBoardNode nextNode = TileBoard[row, column];

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
                if ((currentRow + rowIncrement) == -1)
                {
                    currentRow = _tileRows - 1;
                }
                else
                {
                    currentRow = (currentRow + rowIncrement) % _tileRows;
                }

                if ((currentColumn + columnIncrement) == -1)
                {
                    currentColumn = _tileRows - 1;
                }
                else
                {
                    currentColumn = (currentColumn + columnIncrement) % _tileRows;
                }

                nextNode = (TileBoard[currentRow, currentColumn].Color == nextNode.Color) ? TileBoard[currentRow, currentColumn] : nextNode;                
            }
            while (nextNode != TileBoard[currentRow, currentColumn]);

            return nextNode;
        }

        private void UpdateListHeads(int row, int column, CreeperColor type)
        {
            // This gives us direct access to the first node added to a given row, column, and color.
            if (type == CreeperColor.Black)
            {
                RowHeadBlack[row] = RowHeadBlack[row] ?? TileBoard[row, column];
                ColumnHeadBlack[column] = ColumnHeadBlack[column] ?? TileBoard[row, column];
            }
            else if (type == CreeperColor.White)
            {
                RowHeadWhite[row] = RowHeadWhite[row] ?? TileBoard[row, column];
                ColumnHeadWhite[column] = ColumnHeadWhite[column] ?? TileBoard[row, column];
            }
            else
            {
                // We don't want this method called with anything but tiles.
                throw new ArgumentOutOfRangeException(type.ToString());
            }
        }

        public bool IsValidPosition(Position position, PieceType pieceType)
        {
            int rows = (pieceType == PieceType.Tile) ? _tileRows : _pegRows;

            return (position.Column >= 0 && position.Column < rows && position.Row >= 0 && position.Row < rows);
        }

        public bool IsValidMove(Move move)
        {
            bool valid = true;

            //is the move in bounds?
            if (!IsValidPosition(move.StartPosition, PieceType.Peg)
                || !IsValidPosition(move.EndPosition, PieceType.Peg))
            {
                valid = false;
            }

            //Does the start space have the player's piece?
            else if (PegBoard[move.StartPosition.Row, move.StartPosition.Column].Color != move.PlayerColor)
            {
                valid = false;
            }

            //Is the end space empty?
            else if (PegBoard[move.EndPosition.Row, move.EndPosition.Column].Color != CreeperColor.Empty)
            {
                valid = false;
            }

            //is the end space one away from the start space?
            else if ((Math.Abs(move.StartPosition.Row - move.EndPosition.Row) > 1)
                || (Math.Abs(move.StartPosition.Column - move.EndPosition.Column) > 1)
                || (move.StartPosition.Equals(move.EndPosition)))
            {
                valid = false;
            }

            return valid;
        }

        private AIBoardNode GetFlippedTile(Move move)
        {
            CardinalDirection direction = move.EndPosition.Row < move.StartPosition.Row ? (move.EndPosition.Column < move.StartPosition.Column ? CardinalDirection.Northwest : CardinalDirection.Northeast)
                : (move.EndPosition.Column < move.StartPosition.Column ? CardinalDirection.Southwest : CardinalDirection.Southeast);

            switch (direction)
            {
                case CardinalDirection.Northwest:
                    return TileBoard[move.EndPosition.Row, move.EndPosition.Column];

                case CardinalDirection.Northeast:
                    return TileBoard[move.EndPosition.Row, move.EndPosition.Column - 1];

                case CardinalDirection.Southwest:
                    return TileBoard[move.EndPosition.Row - 1, move.EndPosition.Column];

                case CardinalDirection.Southeast:
                    return TileBoard[move.StartPosition.Row, move.StartPosition.Column];

                default:
                    throw new ArgumentException();
            }
        }

        private void Flip(Move move)
        {
            AIBoardNode tile = GetFlippedTile(move);
            if (tile.Color != CreeperColor.Invalid)
            {
                tile.Color = move.PlayerColor;
            }
        }

        private void Capture(Move move)
        {
            if (move.StartPosition.Row + 2 == move.EndPosition.Row && move.StartPosition.Column == move.EndPosition.Column)
            {
                PegBoard[move.StartPosition.Row + 1, move.StartPosition.Column].Color = CreeperColor.Empty;
            }
            else if (move.StartPosition.Row - 2 == move.EndPosition.Row && move.StartPosition.Column == move.EndPosition.Column)
            {
                PegBoard[move.StartPosition.Row - 1, move.StartPosition.Column].Color = CreeperColor.Empty;
            }
            else if (move.StartPosition.Row == move.EndPosition.Row && move.StartPosition.Column + 2 == move.EndPosition.Column)
            {
                PegBoard[move.StartPosition.Row, move.StartPosition.Column + 1].Color = CreeperColor.Empty;
            }
            else if (move.StartPosition.Row == move.EndPosition.Row && move.StartPosition.Column - 2 == move.EndPosition.Column)
            {
                PegBoard[move.StartPosition.Row, move.StartPosition.Column - 1].Color = CreeperColor.Empty;
            }
        }

        public void Move(Move move)
        {
            PegBoard[move.StartPosition.Row, move.StartPosition.Column].Color = CreeperColor.Empty;
            PegBoard[move.EndPosition.Row, move.EndPosition.Column].Color = move.PlayerColor;

            if (Math.Abs(move.StartPosition.Row - move.EndPosition.Row) * Math.Abs(move.StartPosition.Column - move.EndPosition.Column) == 1)
            {
                Flip(move);
            }
            else if ((Math.Abs(move.StartPosition.Row - move.EndPosition.Row) == 2) != (Math.Abs(move.StartPosition.Column - move.EndPosition.Column) == 2))
            {
                Capture(move);
            }
        }

        public void PrintToConsole()
        {
            for (int row = 0; row < _pegRows; row++)
            {
                for (int column = 0; column < _pegRows; column++)
                {
                    switch (PegBoard[row, column].Color)
                    {
                        case CreeperColor.Black:
                            Console.Write("B");
                            break;
                        case CreeperColor.Empty:
                            Console.Write(" ");
                            break;
                        case CreeperColor.Invalid:
                            Console.Write("I");
                            break;
                        case CreeperColor.White:
                            Console.Write("W");
                            break;
                    }

                    if (column < _pegRows - 1)
                    {
                        Console.Write("-");
                    }                   
                }

                System.Console.WriteLine();

                if (row < _tileRows)
                {
                    for (int column = 0; column < _tileRows; column++)
                    {
                        Console.Write("|");
                        switch (TileBoard[row, column].Color)
                        {
                            case CreeperColor.White:
                                Console.Write("O");
                                break;
                            case CreeperColor.Black:
                                Console.Write("X");
                                break;
                            case CreeperColor.Invalid:
                                Console.Write("*");
                                break;
                            default:
                                Console.Write(" ");
                                break;
                        }
                    }
                    Console.Write("|");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
