using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Creeper
{
    public enum CreeperColor { Fire, Ice, Empty, Invalid }
    public enum CardinalDirection { North, South, East, West, Northwest, Northeast, Southwest, Southeast }
    public enum Status { ValidMove, InvalidMove, GameOver }
    public enum PieceType { Peg, Tile }
    public enum CreeperGameState { Complete, Draw, Unfinished }

    public class CreeperBoard
    {
        #region Static
        public const int TileRows = 6;
        public const int PegRows = TileRows + 1;

        private static Position _BlackStart { get { return new Position(0, TileRows - 1); } }
        private static Position _WhiteStart { get { return new Position(0, 0); } }
        private static Position _BlackEnd { get { return new Position(TileRows - 1, 0); } }
        private static Position _WhiteEnd { get { return new Position(TileRows - 1, TileRows - 1); } }
        private static Position _NorthBlackPegCorner { get { return new Position(0, PegRows - 1); } }
        private static Position _NorthWhitePegCorner { get { return new Position(0, 0); } }
        private static Position _SouthBlackPegCorner { get { return new Position(PegRows - 1, 0); } }
        private static Position _SouthWhitePegCorner { get { return new Position(PegRows - 1, PegRows - 1); } }

        public static bool IsValidPosition(Position position, PieceType pieceType)
        {
            int rows = (pieceType == PieceType.Tile) ? TileRows : PegRows;

            return (position.Column >= 0 && position.Column < rows && position.Row >= 0 && position.Row < rows);
        }

        public static bool IsFlipMove(Move move)
        {
            return (Math.Abs(move.StartPosition.Row - move.EndPosition.Row) * Math.Abs(move.StartPosition.Column - move.EndPosition.Column) == 1);
        }

        public static bool IsCaptureMove(Move move)
        {
            return (Math.Abs(move.StartPosition.Row - move.EndPosition.Row) == 2) != (Math.Abs(move.StartPosition.Column - move.EndPosition.Column) == 2);
        }

        public static Position GetFlippedPosition(Move move)
        {
            CardinalDirection direction = move.EndPosition.Row < move.StartPosition.Row ? (move.EndPosition.Column < move.StartPosition.Column ? CardinalDirection.Northwest : CardinalDirection.Northeast)
                : (move.EndPosition.Column < move.StartPosition.Column ? CardinalDirection.Southwest : CardinalDirection.Southeast);

            switch (direction)
            {
                case CardinalDirection.Northwest:
                    return new Position(move.EndPosition.Row, move.EndPosition.Column);

                case CardinalDirection.Northeast:
                    return new Position(move.EndPosition.Row, move.EndPosition.Column - 1);

                case CardinalDirection.Southwest:
                    return new Position(move.EndPosition.Row - 1, move.EndPosition.Column);

                case CardinalDirection.Southeast:
                    return new Position(move.StartPosition.Row, move.StartPosition.Column);

                default:
                    throw new ArgumentException("No flipped tile for this move.");
            }
        }

        public static Position GetCapturedPegPosition(Move move)
        {
            foreach (CardinalDirection direction in new[] { CardinalDirection.North, CardinalDirection.South, CardinalDirection.East, CardinalDirection.West })
            {
                if (move.EndPosition == move.StartPosition.AtDirection(direction).AtDirection(direction))
                {
                    return move.StartPosition.AtDirection(direction);
                }
            }

            throw new ArgumentException("No captured peg for this move.");
        }
        #endregion

        public IEnumerable<Piece> Pegs { get; private set; }
        public IEnumerable<Piece> Tiles { get; private set; }

        public CreeperBoard()
        {
            Pegs = new List<Piece>();
            Tiles = new List<Piece>();

            ResetCreeperBoard();
        }

        public CreeperBoard(CreeperBoard board)
        {
            Pegs = board.Pegs.Select(x => new Piece(x.Color, x.Position)).ToList();
            Tiles = board.Tiles.Select(x => new Piece(x.Color, x.Position)).ToList();
        }

        public IEnumerable<Piece> WhereTeam(CreeperColor color, PieceType pieceType)
        {
            IEnumerable<Piece> pieces = (pieceType == PieceType.Peg) ? Pegs : Tiles;
            return pieces.Where(x => x.Color == color);
        }

        public void ResetCreeperBoard()
        {
            Tiles = GenerateEmptyPieces(TileRows).ToList();
            Pegs = GenerateEmptyPieces(PegRows).ToList();

            foreach (Piece tile in Tiles.Where(x => IsCorner(x)))
            {
                tile.Color = CreeperColor.Invalid;
            }


            CreeperColor color = CreeperColor.Fire;
            foreach (Piece peg in Pegs.Where(x => IsCorner(x)))
            {
                peg.Color = CreeperColor.Invalid;

                foreach (CardinalDirection direction in new[] { CardinalDirection.North, CardinalDirection.South, CardinalDirection.East, CardinalDirection.West })
                {
                    if (IsValidPosition(peg.Position.AtDirection(direction), PieceType.Peg))
                    {
                        color = (peg.Position == _NorthBlackPegCorner || peg.Position == _SouthBlackPegCorner) ? CreeperColor.Ice : CreeperColor.Fire;

                        Pegs.At(peg.Position.AtDirection(direction)).Color = color;
                        Pegs.At(peg.Position.AtDirection(direction).AtDirection(direction)).Color = color;
                    }
                }
            }
        }

        public bool IsValidMove(Move move)
        {
            return Pegs.At(move.StartPosition).PossibleMoves(this).Any(x => x.EndPosition == move.EndPosition  && x.PlayerColor == move.PlayerColor);
        }

        public bool IsFinished(CreeperColor playerTurn)
        {
            return GetGameState(playerTurn) != CreeperGameState.Unfinished;
        }

        public CreeperGameState GetGameState(CreeperColor playerTurn)
        {
            IEnumerable<Piece> currentTeam = WhereTeam(playerTurn, PieceType.Peg);
            IEnumerable<Piece> opponentTeam = WhereTeam(playerTurn.Opposite(), PieceType.Peg);
            if (!currentTeam.Any()
                || !currentTeam.SelectMany(x => CreeperUtility.PossibleMoves(x, this)).Any()
                || !opponentTeam.Any()
                || !opponentTeam.SelectMany(x => CreeperUtility.PossibleMoves(x, this)).Any())
            {
                return CreeperGameState.Draw;
            }

            bool gameOver = false;
            bool stackEmpty = false;
            Stack<Piece> stack = new Stack<Piece>();
            HashSet<Piece> foundTiles = new HashSet<Piece>();
            HashSet<Piece> endTiles = new HashSet<Piece>();
            Position start = (playerTurn == CreeperColor.Fire) ? _WhiteStart : _BlackStart;
            Position end = (playerTurn == CreeperColor.Fire) ? _WhiteEnd : _BlackEnd;

            endTiles.UnionWith(Tiles.At(end).GetNeighbors(this).Where(x => x.Color == playerTurn));

            if (!endTiles.Any())
            {
                return CreeperGameState.Unfinished;
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
                }

                foundTiles.UnionWith(neighbors);
                if (foundTiles.Intersect(endTiles).Any())
                {
                    gameOver = true;
                    break;
                }

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

            return gameOver ? CreeperGameState.Complete : CreeperGameState.Unfinished;
        }

        public bool Move(Move move)
        {
            bool isValid = false;

            if (IsValidMove(move))
            {
                isValid = true;

                Pegs.First(x => x.Position.Equals(move.StartPosition)).Color = CreeperColor.Empty;
                Pegs.First(x => x.Position.Equals(move.EndPosition)).Color = move.PlayerColor;

                if (IsFlipMove(move))
                {
                    Flip(move);
                }
                else if (IsCaptureMove(move))
                {
                    Capture(move);
                }
                
            }

            return isValid;
        }


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

        #region Private
        private void Flip(Move move)
        {
            Piece tile = GetFlippedTile(move);
            if (!IsCorner(tile))
            {
                tile.Color = move.PlayerColor;
            }
        }

        private Piece GetFlippedTile(Move move)
        {
            return Tiles.At(GetFlippedPosition(move));
        }

        private void Capture(Move move)
        {
            Pegs.At(GetCapturedPegPosition(move)).Color = CreeperColor.Empty;
        }

        private IEnumerable<Piece> GenerateEmptyPieces(int size)
        {
            IEnumerable<int> range = Enumerable.Range(0, size);
            return range.Join(range,
                row => 0,
                column => 0,
                (row, column) => new Piece(CreeperColor.Empty, new Position(row, column)));
        }
        #endregion

        #region Debug Functions
        public void PrintToConsole(bool pause = false)
        {
            for (int row = 0; row < PegRows; row++)
            {
                foreach (Piece peg in Pegs.Where(x => x.Position.Row == row).OrderBy(x => x.Position.Column))
                {
                    switch (peg.Color)
                    {
                        case CreeperColor.Ice:
                            Console.Write("B");
                            break;
                        case CreeperColor.Empty:
                            Console.Write(" ");
                            break;
                        case CreeperColor.Invalid:
                            Console.Write("I");
                            break;
                        case CreeperColor.Fire:
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
                            case CreeperColor.Fire:
                                Console.Write("O");
                                break;
                            case CreeperColor.Ice:
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

            Console.WriteLine(String.Format("White Pegs: {0}", WhereTeam(CreeperColor.Fire, PieceType.Peg).Count()));
            Console.WriteLine(String.Format("Black Pegs: {0}", WhereTeam(CreeperColor.Ice, PieceType.Peg).Count()));
            Console.WriteLine();
            Console.WriteLine(String.Format("White Tiles: {0}", WhereTeam(CreeperColor.Fire, PieceType.Tile).Count()));
            Console.WriteLine(String.Format("Black Tiles: {0}", WhereTeam(CreeperColor.Ice, PieceType.Tile).Count()));
            Console.WriteLine();
            if (pause)
            {
                Console.ReadLine();
            }
        }

        private void ClearCreeperBoard()
        {
            foreach (Piece peg in Pegs)
            {
                peg.Color = CreeperColor.Empty;
            }
            foreach (Piece tile in Tiles)
            {
                tile.Color = CreeperColor.Empty;
            }
        }

        public void ReadFromFile(String path)
        {
            ClearCreeperBoard();
            List<String> fileStrings = File.ReadAllLines(path).ToList();
            List<String> pegStrings = new List<string>();
            List<String> tileStrings = new List<string>();

            for (int i = 0; i < fileStrings.Count; i++)
            {
                if (fileStrings[i][0] == '|')
                {
                    tileStrings.Add(fileStrings[i]);
                }
                else
                {
                    pegStrings.Add(fileStrings[i]);
                }
            }

            for (int row = 0; row < pegStrings.Count; row++)
            {
                int column = 0;
                for (int i = 0; i < pegStrings[row].Length; i++)
                {
                    switch (pegStrings[row][i])
                    {
                        case ' ':
                            Pegs.At(new Position(row, column)).Color = CreeperColor.Empty;
                            column++;
                            break;
                        case 'B':
                            Pegs.At(new Position(row, column)).Color = CreeperColor.Ice;
                            column++;
                            break;
                        case 'W':
                            Pegs.At(new Position(row, column)).Color = CreeperColor.Fire;
                            column++;
                            break;
                        case 'I':
                            Pegs.At(new Position(row, column)).Color = CreeperColor.Invalid;
                            column++;
                            break;
                        default:
                            break;
                    }
                }
            }

            for (int row = 0; row < tileStrings.Count; row++)
            {
                int column = 0;
                for (int i = 0; i < tileStrings[row].Length; i++)
                {
                    switch (tileStrings[row][i])
                    {
                        case ' ':
                            Tiles.At(new Position(row, column)).Color = CreeperColor.Empty;
                            column++;
                            break;
                        case 'X':
                            Tiles.At(new Position(row, column)).Color = CreeperColor.Ice;
                            column++;
                            break;
                        case 'O':
                            Tiles.At(new Position(row, column)).Color = CreeperColor.Fire;
                            column++;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        #endregion
    }
}