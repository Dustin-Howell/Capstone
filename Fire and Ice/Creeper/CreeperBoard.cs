using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public enum CreeperColor { White, Black, Empty, Invalid }
    public enum CardinalDirection { North, South, East, West, Northwest, Northeast, Southwest, Southeast }
    public enum Status { ValidMove, InvalidMove, GameOver }
    public enum PieceType { Peg, Tile }

    public class CreeperBoard
    {
        protected const int _TileRows = 6;
        protected const int _PegRows = _TileRows + 1;
        public static int TileRows { get { return _TileRows; } }
        public static int PegRows { get { return _PegRows; } }

        private static Position _BlackStart { get { return new Position(0, 5); } }
        private static Position _WhiteStart { get { return new Position(0, 0); } }
        private static Position _BlackEnd { get { return new Position(5, 0); } }
        private static Position _WhiteEnd { get { return new Position(5, 5); } }

        public List<Piece> Pegs;
        public List<Piece> Tiles;

        public CreeperBoard()
        {
            Pegs = new List<Piece>();
            Tiles = new List<Piece>();

            ResetCreeperBoard();
        }

        public List<Piece> WhereTeam(CreeperColor color)
        {
            return Pegs.Where(x => x.Color == color).ToList();
        }

        public bool IsValidPosition(Position position, PieceType pieceType)
        {
            int rows = (pieceType == PieceType.Tile) ? TileRows : PegRows;

            return (position.Column >= 0 && position.Column < rows && position.Row >= 0 && position.Row < rows);
        }

        //TODO: delete this
        //protected void AssignNeighbors()
        //{
        //    foreach (Tile tile in Tiles)
        //    {
        //        List<Piece> neighbors = new List<Piece>();

        //        int col = tile.Position.Column;
        //        int row = tile.Position.Row;

        //        List<Position> possibleNeighbors = new List<Position>();

        //        possibleNeighbors.Add(new Position(row - 1, col));
        //        possibleNeighbors.Add(new Position(row, col + 1));
        //        possibleNeighbors.Add(new Position(row + 1, col));
        //        possibleNeighbors.Add(new Position(row, col - 1));

        //        foreach (Position position in possibleNeighbors)
        //        {
        //            if (IsValidPosition(position, PieceType.Tile))
        //            {
        //                neighbors.Add(Tiles.Where(x => x.Position.Equals(position)).First());
        //            }
        //        }


        //        tile.SetNeighbors(neighbors);
        //    }
        //}

        public void ResetCreeperBoard()
        {
            Tiles.Clear();
            Pegs.Clear();

            for (int row = 0; row < PegRows; row++)
            {
                for (int col = 0; col < PegRows; col++)
                {
                    //TODO: remove slotnumber stuff
                    int slotNumber = (row * PegRows) + col;
                    CreeperColor color;

                    switch (slotNumber)
                    {
                        case 0:
                            color = CreeperColor.Invalid;
                            break;
                        case 1:
                            color = CreeperColor.White;
                            break;
                        case 2:
                            color = CreeperColor.White;
                            break;
                        case _PegRows - 3:
                            color = CreeperColor.Black;
                            break;
                        case _PegRows - 2:
                            color = CreeperColor.Black;
                            break;
                        case _PegRows - 1:
                            color = CreeperColor.Invalid;
                            break;
                        case _PegRows:
                            color = CreeperColor.White;
                            break;
                        case _PegRows * 2 - 1:
                            color = CreeperColor.Black;
                            break;
                        case _PegRows * 2:
                            color = CreeperColor.White;
                            break;
                        case _PegRows * 3 - 1:
                            color = CreeperColor.Black;
                            break;
                        case _PegRows * (_PegRows - 3):
                            color = CreeperColor.Black;
                            break;
                        case (_PegRows * (_PegRows - 2)) - 1:
                            color = CreeperColor.White;
                            break;
                        case (_PegRows * (_PegRows - 2)):
                            color = CreeperColor.Black;
                            break;
                        case (_PegRows * (_PegRows - 1)) - 1:
                            color = CreeperColor.White;
                            break;
                        case _PegRows * (_PegRows - 1):
                            color = CreeperColor.Invalid;
                            break;
                        case (_PegRows * (_PegRows - 1)) + 1:
                            color = CreeperColor.Black;
                            break;
                        case (_PegRows * (_PegRows - 1)) + 2:
                            color = CreeperColor.Black;
                            break;
                        case (_PegRows * _PegRows) - 3:
                            color = CreeperColor.White;
                            break;
                        case (_PegRows * _PegRows) - 2:
                            color = CreeperColor.White;
                            break;
                        case (_PegRows * _PegRows) - 1:
                            color = CreeperColor.Invalid;
                            break;
                        default:
                            color = CreeperColor.Empty;
                            break;
                    }
                    Piece peg = new Piece(color, new Position(row, col));
                    Pegs.Add(peg);
                }
            }

            for (int row = 0; row < TileRows; row++)
            {
                for (int col = 0; col < TileRows; col++)
                {
                    CreeperColor color = CreeperColor.Empty;

                    //TODO: remove slotnumber stuff

                    int slotNumber = 0;//CreeperUtility.PositionToNumber(row, col, false);
                    if (
                        (slotNumber == 0)
                        || (slotNumber == TileRows - 1)
                        || (slotNumber == TileRows * (TileRows - 1))
                        || (slotNumber == (TileRows * TileRows) - 1)
                        )
                    {
                        color = CreeperColor.Invalid;
                    }

                    Tiles.Add(new Piece(color, new Position(row,col)));
                }
            }


        }

        public bool IsValidMove(Move move)
        {
            List<Move> validMoves = new List<Move>();

            validMoves = CreeperUtility.PossibleMove(Pegs.At(move.StartPosition), Pegs);

            foreach (Move possible in validMoves)
            {
                if (possible.EndPosition == move.EndPosition)
                {
                    return true;
                }
            }

            return false;
           }

        public bool GameOver(CreeperColor playerTurn)
        {
            bool gameOver = false;
            bool stackEmpty = false;
            Stack<Piece> tiles = new Stack<Piece>();
            Position start = (playerTurn == CreeperColor.White) ? _WhiteStart : _BlackStart;
            Position end = (playerTurn == CreeperColor.White) ? _WhiteEnd : _BlackEnd;

            Piece currentTile = Tiles.Where(x => x.Position.Equals(start)).First();
            while (!stackEmpty && !currentTile.Position.Equals(end))
            {
                foreach (Piece neighbor in currentTile.GetNeighbors(this))
                {
                    if (neighbor.Color == playerTurn && !tiles.Contains(neighbor))
                    {
                        tiles.Push(neighbor);
                    }
                    else if (neighbor.Position.Equals(end))
                    {
                        gameOver = true;
                    }
                }

                if (tiles.Any())
                {
                    currentTile = tiles.Pop();
                }
                else
                {
                    stackEmpty = true;
                }
            }

            return gameOver;
        }

        private void Flip(Move move)
        {
            int startRow = move.StartPosition.Row;
            int startCol = move.StartPosition.Column;
            int endRow = move.EndPosition.Row;
            int endCol = move.EndPosition.Column;
            CreeperColor playerTurn = move.PlayerColor;

            if (startRow > endRow)
            {
                if (startCol > endCol)
                {
                    //same as destination
                    Tiles.Where(x => x.Position.Equals(move.EndPosition)).First().Color = move.PlayerColor;
                }
                else
                {
                    //same row as destination
                    //col - 1
                    Tiles.Where(x => x.Position.Equals(new Position(move.EndPosition.Row, move.EndPosition.Column - 1))).First().Color = move.PlayerColor;
                }
            }
            else
            {
                if (startCol > endCol)
                {
                    //same col as destination
                    //row - 1
                    Tiles.Where(x => x.Position.Equals(new Position(move.EndPosition.Row - 1, move.EndPosition.Column))).First().Color = move.PlayerColor;
                }
                else
                {
                    //same as start
                    Tiles.Where(x => x.Position.Equals(move.StartPosition)).First().Color = move.PlayerColor;
                }
            }
        }

        public bool Move(Move move)
        {
            bool isValid = false;

            if (IsValidMove(move))
            {
                isValid = true;

                Pegs.Where(x => x.Position.Equals(move.StartPosition)).First().Color = CreeperColor.Empty;
                Pegs.Where(x => x.Position.Equals(move.EndPosition)).First().Color = move.PlayerColor;

                if (Math.Abs(move.StartPosition.Row - move.EndPosition.Row) + Math.Abs(move.StartPosition.Column - move.EndPosition.Column) == 2)
                {
                    Flip(move);
                }
                
            }

            return isValid;
        }

        //forgot that I wrote this function... didn't use it above. May refactor to use it
        public bool IsCorner(int row, int col, bool TileSpace)
        {
            bool isCorner = false;
            int rows = (TileSpace) ? TileRows : PegRows;

            if ((row == 0 && col == 0)
                || (row == 0 && col == rows - 1)
                || (row == rows - 1 && col == 0)
                || (row == rows - 1 && col == rows - 1)
                )
            {
                isCorner = true;
            }

            return isCorner;
        }

        public void PrintToConsole()
        {
            for (int row = 0; row < PegRows; row++)
            {
                foreach (Piece peg in Pegs.Where(x => x.Position.Row == row).OrderBy(x => x.Position.Column))
                {
                    switch (peg.Color)
                    {
                        case CreeperColor.Black:
                            Console.Write("B");
                            break;
                        case CreeperColor.Empty:
                            Console.Write("E");
                            break;
                        case CreeperColor.Invalid:
                            Console.Write("I");
                            break;
                        case CreeperColor.White:
                            Console.Write("W");
                            break;
                    }

                    if (peg.Position.Column < PegRows - 1)
                    {
                        Console.Write("-");
                    }
                }
                Console.Write("\n");

                if (row < TileRows)
                {
                    foreach (Piece tile in Tiles.Where(x => x.Position.Row == row).OrderBy(x => x.Position.Column))
                    {
                        Console.Write("|");
                        switch (tile.Color)
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
                Console.Write("\n");
            }

            //Console.Read();
        }
    }
}