using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Creeper
{
    public enum CreeperColor { White, Black, Empty, Invalid }
    public enum CardinalDirection { North, South, East, West, Northwest, Northeast, Southwest, Southeast }
    public enum Status { ValidMove, InvalidMove, GameOver }
    public enum PieceType { Peg, Tile }
    public enum CreeperGameState {Complete, Draw, Unfinished }

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
        private static Position _NorthBlackPegCorner { get { return new Position(0, 6); } }
        private static Position _NorthWhitePegCorner { get { return new Position(0, 0); } }
        private static Position _SouthBlackPegCorner { get { return new Position(6, 0); } }
        private static Position _SouthWhitePegCorner { get { return new Position(6, 6); } }

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
            Pegs = board.Pegs.Select(x => new Piece(x.Color, x.Position)).ToList();
            Tiles = board.Tiles.Select(x => new Piece(x.Color, x.Position)).ToList();
        }

        public IEnumerable<Piece> WhereTeam(CreeperColor color, PieceType pieceType)
        {
            List<Piece> pieces = (pieceType == PieceType.Peg) ? Pegs : Tiles;
            return pieces.Where(x => x.Color == color);
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

            Tiles = GenerateEmptyPieces(_TileRows).ToList();
            Pegs = GenerateEmptyPieces(_PegRows).ToList();

            foreach (Piece tile in Tiles.Where(x => x.Position == _BlackStart
                || x.Position == _WhiteStart
                || x.Position == _BlackEnd
                || x.Position == _WhiteEnd))
            {
                tile.Color = CreeperColor.Invalid;
            }


            CreeperColor color = CreeperColor.White;
            foreach (Piece peg in Pegs.Where(x => x.Position == _NorthBlackPegCorner
                || x.Position == _NorthWhitePegCorner
                || x.Position == _SouthBlackPegCorner
                || x.Position == _SouthWhitePegCorner))
            {
                peg.Color = CreeperColor.Invalid;

                foreach (CardinalDirection direction in new[] { CardinalDirection.North, CardinalDirection.South, CardinalDirection.East, CardinalDirection.West })
                {
                    if (IsValidPosition(peg.Position.AtDirection(direction), PieceType.Peg))
                    {
                        color = (peg.Position == _NorthBlackPegCorner || peg.Position == _SouthBlackPegCorner) ? CreeperColor.Black : CreeperColor.White;

                        Pegs.At(peg.Position.AtDirection(direction)).Color = color;
                        Pegs.At(peg.Position.AtDirection(direction).AtDirection(direction)).Color = color;
                    }
                }
            }
        }

        private IEnumerable<Piece> GenerateEmptyPieces(int size)
        {
            IEnumerable<int> range = Enumerable.Range(0, size);
            return range.Join(range,
                row => 0,
                column => 0,
                (row, column) => new Piece(CreeperColor.Empty, new Position(row, column)));
        }

        public bool IsValidMove(Move move)
        {
            return Pegs.At(move.StartPosition).PossibleMoves(this).Any(x => x.EndPosition == move.EndPosition  && x.PlayerColor == move.PlayerColor);
        }

        public bool IsFinished(CreeperColor playerTurn)
        {
            return BetterGetGameState(playerTurn) != CreeperGameState.Unfinished;
        }

        public CreeperGameState GetGameState(CreeperColor playerTurn)
        {
            IEnumerable<Piece> opponent = WhereTeam((playerTurn == CreeperColor.Black) ? CreeperColor.White : CreeperColor.Black, PieceType.Peg);
            if (!opponent.Any()  || !opponent.SelectMany(x => CreeperUtility.PossibleMoves(x, this)).Any())
            {
                return CreeperGameState.Draw;
            }

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
                    else if (foundTiles.Intersect(endTiles).Any())
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

            return gameOver ? CreeperGameState.Complete : CreeperGameState.Unfinished;
        }

        public List<Piece> GetValidTilesFromCurrentRow(List<Piece> knownValidTiles, int row, CreeperColor color)
        {
            //All tiles on the current row
            IEnumerable<Piece> currentRow = Pegs.Where(x => x.Position.Row == row);

            //The list of tiles we will return
            List<Piece> validTiles = new List<Piece>();

            //We want to add the ones that are already known to be valid to the valid tiles list
            validTiles.AddRange(knownValidTiles);


            foreach (Piece validTile in knownValidTiles)
            {
                int column = validTile.Position.Column;
                //Starting from our current known valid tile's column, go toward zero
                for (int i = column; i > 0; i--)
                {
                    Position adjacentPosition = new Position(validTile.Position.Row, i - 1);
                    Piece adjacentTile = Tiles.At(adjacentPosition);

                    //if the adjacent tile is our color, it's valid
                    if (adjacentTile.Color == color
                        && !validTiles.Contains(adjacentTile))
                    {
                        validTiles.Add(adjacentTile);
                    }

                    //if the adjacent tile is not valid, the one next to it won't be either, so we just break
                    else
                    {
                        break;
                    }
                }

                //Same as above, but we're going toward the higher column values now
                for (int i = column; i < TileRows - 1; i++)
                {
                    Position adjacentPosition = new Position(validTile.Position.Row, i + 1);
                    Piece adjacentTile = Tiles.At(adjacentPosition);
                    if (adjacentTile.Color == color
                        && !validTiles.Contains(adjacentTile))
                    {
                        validTiles.Add(adjacentTile);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return validTiles;
        }

        public List<Piece> GetValidTilesFromNextRow(List<Piece> knownValidTiles, CreeperColor color)
        {
            List<Piece> validTiles = new List<Piece>();

            foreach (Piece validTile in knownValidTiles)
            {
                Position southPosition = validTile.Position.AtDirection(CardinalDirection.South);
                if (IsValidPosition(southPosition, PieceType.Tile))
                {
                    Piece southTile = Tiles.At(validTile.Position.AtDirection(CardinalDirection.South));
                    if (southTile.Color == color)
                    {
                        validTiles.Add(southTile);
                    }
                }
            }

            return validTiles;
        }

        public CreeperGameState BetterGetGameState(CreeperColor playerTurn)
        {
            //Copy-Pasted from GetGameState()
            IEnumerable<Piece> opponent = WhereTeam((playerTurn == CreeperColor.Black) ? CreeperColor.White : CreeperColor.Black, PieceType.Peg);
            if (!opponent.Any() || !opponent.SelectMany(x => CreeperUtility.PossibleMoves(x, this)).Any())
            {
                return CreeperGameState.Draw;
            }

            CreeperGameState gameState = CreeperGameState.Unfinished;
            Position startPosition = (playerTurn == CreeperColor.White) ? _WhiteStart : _BlackStart;
            Position endPosition = (playerTurn == CreeperColor.White) ? _WhiteEnd : _BlackEnd;
            Piece winTile1 = Tiles.At(endPosition.AtDirection(CardinalDirection.North));
            Piece winTile2 = Tiles.At(
                (IsValidPosition(endPosition.AtDirection(CardinalDirection.East), PieceType.Tile)?
                endPosition.AtDirection(CardinalDirection.East) : endPosition.AtDirection(CardinalDirection.West) ));

            List<Piece> validPieces = new List<Piece>();
            validPieces.Add(Tiles.At(startPosition));            

            for (int i = 0; i < TileRows; i++)
            {
                validPieces = GetValidTilesFromCurrentRow(validPieces, i, playerTurn);
                if (!validPieces.Any())
                {
                    break;
                }
                if (validPieces.Any() && 
                    (validPieces.Contains(winTile1) || validPieces.Contains(winTile2)))
                {
                    gameState = CreeperGameState.Complete;
                    break;
                }
                validPieces = GetValidTilesFromNextRow(validPieces, playerTurn);
            }

            return gameState;
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
                if (move.EndPosition == move.StartPosition.AtDirection(direction).AtDirection(direction))
                {
                    Pegs.At(move.StartPosition.AtDirection(direction)).Color = CreeperColor.Empty;
                }
            }
        }

        public bool Move(Move move)
        {
            bool isValid = false;

            if (IsValidMove(move))
            {
                isValid = true;

                Pegs.First(x => x.Position.Equals(move.StartPosition)).Color = CreeperColor.Empty;
                Pegs.First(x => x.Position.Equals(move.EndPosition)).Color = move.PlayerColor;

                if (Math.Abs(move.StartPosition.Row - move.EndPosition.Row) * Math.Abs(move.StartPosition.Column - move.EndPosition.Column) == 1)
                {
                    Flip(move);
                }
                else if ((Math.Abs(move.StartPosition.Row - move.EndPosition.Row) == 2) != (Math.Abs(move.StartPosition.Column - move.EndPosition.Column) == 2))
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

        #region Debug Functions
        public void PrintToConsole(bool pause = false)
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
                            Console.Write(" ");
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

            Console.WriteLine(String.Format("White Pegs: {0}", WhereTeam(CreeperColor.White, PieceType.Peg).Count()));
            Console.WriteLine(String.Format("Black Pegs: {0}", WhereTeam(CreeperColor.Black, PieceType.Peg).Count()));
            Console.WriteLine();
            Console.WriteLine(String.Format("White Tiles: {0}", WhereTeam(CreeperColor.White, PieceType.Tile).Count()));
            Console.WriteLine(String.Format("Black Tiles: {0}", WhereTeam(CreeperColor.Black, PieceType.Tile).Count()));
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
                            Pegs.At(new Position(row, column)).Color = CreeperColor.Black;
                            column++;
                            break;
                        case 'W':
                            Pegs.At(new Position(row, column)).Color = CreeperColor.White;
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
                            Tiles.At(new Position(row, column)).Color = CreeperColor.Black;
                            column++;
                            break;
                        case 'O':
                            Tiles.At(new Position(row, column)).Color = CreeperColor.White;
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