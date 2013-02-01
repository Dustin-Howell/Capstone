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

        public List<Piece> Pegs { get; private set; }
        public List<Piece> Tiles { get; private set; }

        public CreeperBoard()
        {
            Pegs = new List<Piece>();
            Tiles = new List<Piece>();

            ResetCreeperBoard();
        }

        public CreeperBoard(CreeperBoard board)
        {
            Pegs = new List<Piece>(board.Pegs);
            Tiles = new List<Piece>(board.Tiles);
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

                    int slotNumber = CreeperUtility.PositionToNumber(new Position(row, col), false);
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
            return CreeperUtility.PossibleMoves(Pegs.At(move.StartPosition), Pegs)
                .Any(x => x.EndPosition == move.EndPosition);
        }

        public bool GameOver(CreeperColor playerTurn)
        {
            bool gameOver = false;
            bool stackEmpty = false;
            Stack<Piece> stack = new Stack<Piece>();
            HashSet<Piece> foundTiles = new HashSet<Piece>();
            HashSet<Piece> endTiles = new HashSet<Piece>();
            Position start = (playerTurn == CreeperColor.White) ? _WhiteStart : _BlackStart;
            Position end = (playerTurn == CreeperColor.White) ? _WhiteEnd : _BlackEnd;

            endTiles.UnionWith(Tiles.At(end).GetNeighbors(this).Where(x => x.Color == playerTurn));
            if (!endTiles.Any())
            {
                return false;
            }

            Piece currentTile = Tiles.At(start);
            IEnumerable<Piece> neighbors = currentTile.GetNeighbors(this).Where(x => x.Color == playerTurn);
            while (!stackEmpty && !currentTile.Position.Equals(end))
            {
                foreach (Piece neighbor in neighbors)
                {
                    if (!foundTiles.Contains(neighbor))
                    {
                        stack.Push(neighbor);
                    }
                    else if (foundTiles.Intersect(endTiles).Any()) //(neighbor.Position.Equals(end))
                    {
                        gameOver = true;
                    }
                }

                foundTiles.UnionWith(neighbors);

                if (stack.Any())
                {
                    currentTile = stack.Pop();
                }
                else
                {
                    stackEmpty = true;
                }

                neighbors = currentTile.GetNeighbors(this).Where(x => x.Color == playerTurn);
            }

            return gameOver;
        }

        private Piece GetFlippedTile(Move move)
        {
            CardinalDirection direction = move.EndPosition.Row < move.StartPosition.Row ? (move.EndPosition.Column < move.StartPosition.Column ? CardinalDirection.Northwest : CardinalDirection.Northeast)
                : (move.EndPosition.Column < move.StartPosition.Column ? CardinalDirection.Southwest : CardinalDirection.Southeast);

            switch (direction)
            {
                case CardinalDirection.Northwest:
                    return Tiles.At(new Position(move.EndPosition.Row, move.EndPosition.Column));

                case CardinalDirection.Northeast:
                    return Tiles.At(new Position(move.EndPosition.Row, move.EndPosition.Column - 1));

                case CardinalDirection.Southwest:
                    return Tiles.At(new Position(move.EndPosition.Row - 1, move.EndPosition.Column));

                case CardinalDirection.Southeast:
                    return Tiles.At(new Position(move.StartPosition.Row, move.StartPosition.Column));

                default:
                    throw new ArgumentException();
            }
        }

        private void Flip(Move move)
        {
            Piece tile = GetFlippedTile(move);
            if (!IsCorner(tile))
            {
                tile.Color = move.PlayerColor;
            }
        }

        private void Capture(Move move)
        {
            foreach (CardinalDirection direction in new[] { CardinalDirection.North, CardinalDirection.South, CardinalDirection.East, CardinalDirection.West })
            {
                if (move.EndPosition == move.StartPosition.Adjacent(direction).Adjacent(direction))
                {
                    Pegs.At(move.StartPosition.Adjacent(direction)).Color = CreeperColor.Empty;
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

                if (Math.Abs(move.StartPosition.Row - move.EndPosition.Row) * Math.Abs(move.StartPosition.Column - move.EndPosition.Column) == 1)
                {
                    Flip(move);
                }

                if ((Math.Abs(move.StartPosition.Row - move.EndPosition.Row) == 2) != (Math.Abs(move.StartPosition.Column - move.EndPosition.Column) == 2))
                {
                    Capture(move);
                }
            }

            return isValid;
        }

        //forgot that I wrote this function... didn't use it above. May refactor to use it
        public bool IsCorner(Piece piece)
        {
            bool isCorner = false;
            int rows = (Tiles.Contains(piece)) ? TileRows : PegRows;

            if ((piece.Position.Row == 0 && piece.Position.Column == 0)
                || (piece.Position.Row == 0 && piece.Position.Column == rows - 1)
                || (piece.Position.Row == rows - 1 && piece.Position.Column == 0)
                || (piece.Position.Row == rows - 1 && piece.Position.Column == rows - 1)
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