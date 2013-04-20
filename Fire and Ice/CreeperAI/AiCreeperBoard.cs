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
        //public AIHash Hash {get; private set;}

        public AIBoardNode[ , ] TileBoard { get; private set; }
        public AIBoardNode[ , ] PegBoard { get; private set; }
        public AIBoardNode[] RowHeadBlack { get; private set; }
        public AIBoardNode[] RowHeadWhite { get; private set; }
        public AIBoardNode[] ColumnHeadBlack { get; private set; }
        public AIBoardNode[] ColumnHeadWhite { get; private set; }

        Stack<CreeperColor> TileHistory { get; set; }
        Stack<Move> MoveHistory { get; set; }
        //also, eliminate gameStateHistory in favor of singleton-esque gameState evaluation
        Stack<CreeperGameState> GameStateHistory { get; set; }

        public HashSet<AIBoardNode> BlackPegs { get; private set; }
        public HashSet<AIBoardNode> WhitePegs { get; private set; }
        public Move CurrentMove { get { return new Move(MoveHistory.Peek()); } }

        public int BlackTileCount { get; private set; }
        public int WhiteTileCount { get; private set; }

        public CreeperGameState GameState
        {
            get
            {
                return IsFinished ? GameStateHistory.Peek() : CreeperGameState.Unfinished;
            }
        }

        public bool IsFinished
        {
            get
            {
                return GameStateHistory.Any() && GameStateHistory.Peek() != CreeperGameState.Unfinished;
            }
        }

        private int _tileRows;
        private int _pegRows;
        private const int _MinimumToWin = 9;

        public static Position _BlackStart { get { return new Position(0, 5); } }
        public static Position _WhiteStart { get { return new Position(0, 0); } }
        public static Position _BlackEnd { get { return new Position(5, 0); } }
        public static Position _WhiteEnd { get { return new Position(5, 5); } }

        public AICreeperBoard(CreeperBoard board)
        {
            _tileRows = CreeperBoard.TileRows;
            _pegRows = CreeperBoard.PegRows;

            BlackTileCount = 0;
            WhiteTileCount = 0;
            BlackPegs = new HashSet<AIBoardNode>();
            WhitePegs = new HashSet<AIBoardNode>();

            TileHistory = new Stack<CreeperColor>();
            MoveHistory = new Stack<Move>();
            GameStateHistory = new Stack<CreeperGameState>();

            TileBoard = new AIBoardNode[_tileRows, _tileRows];
            PegBoard = new AIBoardNode[_pegRows, _pegRows];
            RowHeadBlack = new AIBoardNode[_tileRows];
            RowHeadWhite = new AIBoardNode[_tileRows];
            ColumnHeadBlack = new AIBoardNode[_tileRows];
            ColumnHeadWhite = new AIBoardNode[_tileRows];

            foreach (Piece tile in board.Tiles)
            {
                TileBoard[tile.Position.Row, tile.Position.Column] = new AIBoardNode(tile.Position.Row, tile.Position.Column, tile.Color);
            }

            foreach (Piece peg in board.Pegs)
            {
                PegBoard[peg.Position.Row, peg.Position.Column] = new AIBoardNode(peg.Position.Row, peg.Position.Column, peg.Color);
                if (peg.Color == CreeperColor.Ice)
                {
                    BlackPegs.Add(PegBoard[peg.Position.Row, peg.Position.Column]);
                }
                else if (peg.Color == CreeperColor.Fire)
                {
                    WhitePegs.Add(PegBoard[peg.Position.Row, peg.Position.Column]);
                }
            }

            for (int row = 0; row < _tileRows; row++)
            {
                for (int column = 0; column < _tileRows; column++)
                {
                    if (TileBoard[row, column].Color == CreeperColor.Ice || TileBoard[row, column].Color == CreeperColor.Fire)
                    {
                        UpdateListHeads(row, column, TileBoard[row, column].Color);
                        AddTileToTeam(TileBoard[row, column]);
                    }
                }
            }

            //Hash = new AIHash(this);
        }

        public int TeamCount(CreeperColor turn, PieceType type)
        {
            if (turn == CreeperColor.Invalid || turn == CreeperColor.Empty)
                throw new ArgumentOutOfRangeException(turn.ToString());

            return ((turn == CreeperColor.Ice) ? ((type == PieceType.Peg) ? BlackPegs.Count : BlackTileCount) : ((type == PieceType.Peg) ? WhitePegs.Count : WhiteTileCount));
        }

        private void AddTileToTeam(AIBoardNode tile)
        {
            tile.TeamNorth = GetNextNode(tile.Row, tile.Column, CardinalDirection.North);
            tile.TeamSouth = GetNextNode(tile.Row, tile.Column, CardinalDirection.South);
            tile.TeamEast = GetNextNode(tile.Row, tile.Column, CardinalDirection.East);
            tile.TeamWest = GetNextNode(tile.Row, tile.Column, CardinalDirection.West);

            if ((tile.Color == CreeperColor.Ice))
            {
                if (++BlackTileCount > 32) throw new InvalidOperationException(BlackTileCount.ToString() + " is too big!");
            }

            else if (tile.Color == CreeperColor.Fire)
            {
                if (++WhiteTileCount > 32) throw new InvalidOperationException(WhiteTileCount.ToString() + " is too big!");
            }

            if (WhiteTileCount + BlackTileCount > 32)
            {
                throw new InvalidOperationException(WhiteTileCount.ToString() + " is too big!");
            }
        }

        private void RemoveTileFromTeam(AIBoardNode tile)
        {
            tile.TeamNorth.TeamSouth = tile.TeamSouth;
            tile.TeamSouth.TeamNorth = tile.TeamNorth;
            tile.TeamEast.TeamWest = tile.TeamWest;
            tile.TeamWest.TeamEast = tile.TeamEast;

            if (tile.Color == CreeperColor.Ice)
            {
                if (--BlackTileCount < 0) throw new InvalidOperationException(BlackTileCount.ToString());
            }
            else if (tile.Color == CreeperColor.Fire)
            {
                if (--WhiteTileCount < 0) throw new InvalidOperationException(WhiteTileCount.ToString());
            }
        }

        private AIBoardNode GetNextNode(int row, int column, CardinalDirection direction)
        {
            if (row > 5) throw new ArgumentOutOfRangeException(row.ToString());
            if (column > 5) throw new ArgumentOutOfRangeException(column.ToString());

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
            if (row > 5) throw new ArgumentOutOfRangeException(row.ToString());
            if (column > 5) throw new ArgumentOutOfRangeException(column.ToString());

            // This gives us direct access to the first node added to a given row, column, and color.
            // Also, we now remove the tile from the opposite team's head array, when appropriate.
            if (type == CreeperColor.Ice)
            {
                if (RowHeadWhite[row] == TileBoard[row, column])
                {
                    RowHeadWhite[row] = (RowHeadWhite[row] == RowHeadWhite[row].TeamNorth) ? null : RowHeadWhite[row].TeamNorth;
                }

                if (ColumnHeadWhite[column] == TileBoard[row, column])
                {
                    ColumnHeadWhite[column] = (ColumnHeadWhite[column] == ColumnHeadWhite[column].TeamEast) ? null : ColumnHeadWhite[column].TeamEast;
                }

                if (TileBoard[row, column].Color != CreeperColor.Empty)
                {
                    RowHeadBlack[row] = RowHeadBlack[row] ?? TileBoard[row, column];
                    ColumnHeadBlack[column] = ColumnHeadBlack[column] ?? TileBoard[row, column];
                }
            }
            else if (type == CreeperColor.Fire)
            {
                if (RowHeadBlack[row] == TileBoard[row, column])
                {
                    RowHeadBlack[row] = (RowHeadBlack[row] == RowHeadBlack[row].TeamNorth) ? null : RowHeadBlack[row].TeamNorth;
                }

                if (ColumnHeadBlack[column] == TileBoard[row, column])
                {
                    ColumnHeadBlack[column] = (ColumnHeadBlack[column] == ColumnHeadBlack[column].TeamEast) ? null : ColumnHeadBlack[column].TeamEast;
                }

                if (TileBoard[row, column].Color != CreeperColor.Empty)
                {
                    RowHeadWhite[row] = RowHeadWhite[row] ?? TileBoard[row, column];
                    ColumnHeadWhite[column] = ColumnHeadWhite[column] ?? TileBoard[row, column];
                }
            }
            else
            {
                // We don't want this method called with anything but tiles.
                throw new ArgumentOutOfRangeException(type.ToString());
            }
        }

        public bool IsValidPosition(int row, int column, PieceType pieceType)
        {
            int rows = (pieceType == PieceType.Tile) ? _tileRows : _pegRows;

            return (column >= 0 && column < rows && row >= 0 && row < rows)

                // Not a corner piece
                && !(row == 0 && column == 0)
                && !(row == 0 && column == rows - 1)
                && !(row == rows - 1 && column == 0)
                && !(row == rows - 1 && column == rows - 1);
        }

        public bool IsValidMove(Move move)
        {
            bool valid = true;

            //is the move in bounds?
            if (!IsValidPosition(move.StartPosition.Row, move.StartPosition.Column, PieceType.Peg)
                || !IsValidPosition(move.EndPosition.Row, move.EndPosition.Column, PieceType.Peg))
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

            else
            {
                if ((Math.Abs(move.StartPosition.Row - move.EndPosition.Row) > 1)
                || (Math.Abs(move.StartPosition.Column - move.EndPosition.Column) > 1)
                || (move.StartPosition.Equals(move.EndPosition)))
                {
                    valid = false;
                }

                if (Math.Abs(Math.Abs(move.StartPosition.Row - move.EndPosition.Row) - Math.Abs(move.StartPosition.Column - move.EndPosition.Column)) == 2
                    && PegBoard[move.StartPosition.Row + ((move.EndPosition.Row - move.StartPosition.Row) / 2), move.StartPosition.Column + ((move.EndPosition.Column - move.StartPosition.Column) / 2)].Color == move.PlayerColor.Opposite())
                {
                    valid = true;
                }

                else if (move.PlayerColor == CreeperColor.Empty || move.PlayerColor == CreeperColor.Invalid)
                {
                    throw new ArgumentOutOfRangeException(move.ToString());
                }

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
                    throw new ArgumentOutOfRangeException(move.ToString() + " must be a flipping move!");
            }
        }

        public AIBoardNode GetFlippedTileCopy(Move move)
        {
            return new AIBoardNode(GetFlippedTile(move));
        }

        private void Flip(Move move)
        {
            AIBoardNode tile = GetFlippedTile(move);
            if (tile.Color != CreeperColor.Invalid)
            {
                TileHistory.Push(tile.Color);
                if (tile.Color == CreeperColor.Empty)
                {
                    tile.Color = move.PlayerColor;
                    AddTileToTeam(tile);
                }
                else if (tile.Color != move.PlayerColor)
                {
                    RemoveTileFromTeam(tile);
                    tile.Color = move.PlayerColor;
                    AddTileToTeam(tile);
                }

                UpdateListHeads(tile.Row, tile.Column, tile.Color);
            }
        }

        private void UnFlip(Move move)
        {
            AIBoardNode tile = GetFlippedTile(move);
            if (tile.Color != CreeperColor.Invalid)
            {
                CreeperColor color = TileHistory.Pop();
                if (color == CreeperColor.Empty)
                {
                    RemoveTileFromTeam(tile);
                    tile.Color = color;
                }
                // Flipping opposite team's tile.
                else if (color == move.PlayerColor.Opposite())
                {
                    RemoveTileFromTeam(tile);
                    tile.Color = color;
                    AddTileToTeam(tile);
                }

                UpdateListHeads(tile.Row, tile.Column, move.PlayerColor.Opposite());
            }
        }

        private void Capture(Move move)
        {
            if (move.StartPosition.Row + 2 == move.EndPosition.Row && move.StartPosition.Column == move.EndPosition.Column)
            {
                PegBoard[move.StartPosition.Row + 1, move.StartPosition.Column].Color = CreeperColor.Empty;
                ((move.PlayerColor == CreeperColor.Ice) ? WhitePegs : BlackPegs).Remove(PegBoard[move.StartPosition.Row + 1, move.StartPosition.Column]);
            }
            else if (move.StartPosition.Row - 2 == move.EndPosition.Row && move.StartPosition.Column == move.EndPosition.Column)
            {
                PegBoard[move.StartPosition.Row - 1, move.StartPosition.Column].Color = CreeperColor.Empty;
                ((move.PlayerColor == CreeperColor.Ice) ? WhitePegs : BlackPegs).Remove(PegBoard[move.StartPosition.Row - 1, move.StartPosition.Column]);
            }
            else if (move.StartPosition.Row == move.EndPosition.Row && move.StartPosition.Column + 2 == move.EndPosition.Column)
            {
                PegBoard[move.StartPosition.Row, move.StartPosition.Column + 1].Color = CreeperColor.Empty;
                ((move.PlayerColor == CreeperColor.Ice) ? WhitePegs : BlackPegs).Remove(PegBoard[move.StartPosition.Row, move.StartPosition.Column + 1]);
            }
            else if (move.StartPosition.Row == move.EndPosition.Row && move.StartPosition.Column - 2 == move.EndPosition.Column)
            {
                PegBoard[move.StartPosition.Row, move.StartPosition.Column - 1].Color = CreeperColor.Empty;
                ((move.PlayerColor == CreeperColor.Ice) ? WhitePegs : BlackPegs).Remove(PegBoard[move.StartPosition.Row, move.StartPosition.Column - 1]);
            }
        }

        private void UnCapture(Move move)
        {
            if (move.StartPosition.Row + 2 == move.EndPosition.Row && move.StartPosition.Column == move.EndPosition.Column)
            {
                PegBoard[move.StartPosition.Row + 1, move.StartPosition.Column].Color = move.PlayerColor.Opposite();
                ((move.PlayerColor == CreeperColor.Ice) ? WhitePegs : BlackPegs).Add(PegBoard[move.StartPosition.Row + 1, move.StartPosition.Column]);
            }
            else if (move.StartPosition.Row - 2 == move.EndPosition.Row && move.StartPosition.Column == move.EndPosition.Column)
            {
                PegBoard[move.StartPosition.Row - 1, move.StartPosition.Column].Color = move.PlayerColor.Opposite();
                ((move.PlayerColor == CreeperColor.Ice) ? WhitePegs : BlackPegs).Add(PegBoard[move.StartPosition.Row - 1, move.StartPosition.Column]);
            }
            else if (move.StartPosition.Row == move.EndPosition.Row && move.StartPosition.Column + 2 == move.EndPosition.Column)
            {
                PegBoard[move.StartPosition.Row, move.StartPosition.Column + 1].Color = move.PlayerColor.Opposite();
                ((move.PlayerColor == CreeperColor.Ice) ? WhitePegs : BlackPegs).Add(PegBoard[move.StartPosition.Row, move.StartPosition.Column + 1]);
            }
            else if (move.StartPosition.Row == move.EndPosition.Row && move.StartPosition.Column - 2 == move.EndPosition.Column)
            {
                PegBoard[move.StartPosition.Row, move.StartPosition.Column - 1].Color = move.PlayerColor.Opposite();
                ((move.PlayerColor == CreeperColor.Ice) ? WhitePegs : BlackPegs).Add(PegBoard[move.StartPosition.Row, move.StartPosition.Column - 1]);
            }
        }

        public bool IsFlipMove(Move move)
        {
            return Math.Abs(move.StartPosition.Row - move.EndPosition.Row) * Math.Abs(move.StartPosition.Column - move.EndPosition.Column) == 1;
        }

        public bool IsCaptureMove(Move move)
        {
            return (Math.Abs(move.StartPosition.Row - move.EndPosition.Row) == 2) != (Math.Abs(move.StartPosition.Column - move.EndPosition.Column) == 2);
        }

        public void PushMove(Move move)
        {
            MoveHistory.Push(move);

            ((move.PlayerColor == CreeperColor.Ice) ? BlackPegs : WhitePegs).Remove(PegBoard[move.StartPosition.Row, move.StartPosition.Column]);
            PegBoard[move.StartPosition.Row, move.StartPosition.Column].Color = CreeperColor.Empty;
            PegBoard[move.EndPosition.Row, move.EndPosition.Column].Color = move.PlayerColor;
            ((move.PlayerColor == CreeperColor.Ice) ?  BlackPegs : WhitePegs ).Add(PegBoard[move.EndPosition.Row, move.EndPosition.Column]);

            if (IsFlipMove(move))
            {
                Flip(move);
            }
            else if (IsCaptureMove(move))
            {
                Capture(move);
            }

            GameStateHistory.Push(CouldBeFinished(move.PlayerColor) ? GetGameState(move.PlayerColor) : CreeperGameState.Unfinished);
        }

        public Move PopMove()
        {
            Move move = MoveHistory.Pop();

            PegBoard[move.StartPosition.Row, move.StartPosition.Column].Color = move.PlayerColor;
            ((move.PlayerColor == CreeperColor.Fire) ? WhitePegs : BlackPegs).Add(PegBoard[move.StartPosition.Row, move.StartPosition.Column]);
            ((move.PlayerColor == CreeperColor.Fire) ? WhitePegs : BlackPegs).Remove(PegBoard[move.EndPosition.Row, move.EndPosition.Column]);
            PegBoard[move.EndPosition.Row, move.EndPosition.Column].Color = CreeperColor.Empty;

            if (Math.Abs(move.StartPosition.Row - move.EndPosition.Row) * Math.Abs(move.StartPosition.Column - move.EndPosition.Column) == 1)
            {
                // Undoing tile flips will be tricky, since there will be no way of distinguishing flipping and originating a tile.
                UnFlip(move);
            }
            else if ((Math.Abs(move.StartPosition.Row - move.EndPosition.Row) == 2) != (Math.Abs(move.StartPosition.Column - move.EndPosition.Column) == 2))
            {
                UnCapture(move);
            }

            GameStateHistory.Pop();
            return move;
        }

        public Move[] AllPossibleMoves(CreeperColor color)
        {
            if (color == CreeperColor.Invalid || color == CreeperColor.Empty) throw new ArgumentOutOfRangeException(color.ToString());

            Position startPosition = new Position();
            Move[] possibleMoves = new Move[100];
            int index = 0;
            foreach (AIBoardNode peg in (color == CreeperColor.Ice) ? BlackPegs : WhitePegs)
            {
                startPosition = new Position(peg.Row, peg.Column);

                Move[] movesForPeg = new Move[]
                    {
                        new Move(startPosition, new Position(peg.Row + 1, peg.Column), color),
                        new Move(startPosition, new Position(peg.Row - 1, peg.Column), color),
                        new Move(startPosition, new Position(peg.Row, peg.Column + 1), color),
                        new Move(startPosition, new Position(peg.Row, peg.Column - 1), color),
                        new Move(startPosition, new Position(peg.Row + 1, peg.Column + 1), color),
                        new Move(startPosition, new Position(peg.Row + 1, peg.Column - 1), color),
                        new Move(startPosition, new Position(peg.Row - 1, peg.Column + 1), color),
                        new Move(startPosition, new Position(peg.Row - 1, peg.Column - 1), color),
                        new Move(startPosition, new Position(peg.Row + 2, peg.Column), color),
                        new Move(startPosition, new Position(peg.Row - 2, peg.Column), color),
                        new Move(startPosition, new Position(peg.Row, peg.Column + 2), color),
                        new Move(startPosition, new Position(peg.Row, peg.Column - 2), color),
                    };

                for (int i = 0; i < movesForPeg.Length; i++)
                {
                    if (IsValidMove(movesForPeg[i]))
                    {
                        possibleMoves[index++] = movesForPeg[i];
                    }
                }
            }

            Array.Resize(ref possibleMoves, index);
            return possibleMoves;
        }

        public Move[] AllPossibleCaptures(CreeperColor color)
        {
            if (color == CreeperColor.Invalid || color == CreeperColor.Empty) throw new ArgumentOutOfRangeException(color.ToString());

            Position startPosition = new Position();
            Move[] possibleMoves = new Move[100];
            int index = 0;
            foreach (AIBoardNode peg in (color == CreeperColor.Ice) ? BlackPegs : WhitePegs)
            {
                startPosition = new Position(peg.Row, peg.Column);

                Move[] movesForPeg = new Move[]
                    {
                        new Move(startPosition, new Position(peg.Row + 2, peg.Column), color),
                        new Move(startPosition, new Position(peg.Row - 2, peg.Column), color),
                        new Move(startPosition, new Position(peg.Row, peg.Column + 2), color),
                        new Move(startPosition, new Position(peg.Row, peg.Column - 2), color),
                    };

                for (int i = 0; i < movesForPeg.Length; i++)
                {
                    if (IsValidMove(movesForPeg[i]))
                    {
                        possibleMoves[index++] = movesForPeg[i];
                    }
                }
            }

            Array.Resize(ref possibleMoves, index);
            return possibleMoves;
        }

        public IEnumerable<AIBoardNode> GetNeighbors(AIBoardNode node, CreeperColor color)
        {
            foreach (Position neighborPosition in new[]{
                new Position(node.Row - 1, node.Column),
                new Position(node.Row + 1, node.Column),
                new Position(node.Row, node.Column - 1),
                new Position(node.Row, node.Column + 1),
            })
            {
                if (IsValidPosition(neighborPosition.Row, neighborPosition.Column, PieceType.Tile)
                    && TileBoard[neighborPosition.Row, neighborPosition.Column].Color == color)
                {
                    yield return TileBoard[neighborPosition.Row, neighborPosition.Column];
                }
            }
        }

        public CreeperGameState GetGameState(CreeperColor playerTurn)
        {
            if (TeamCount(playerTurn, PieceType.Peg) == 0 || TeamCount(playerTurn.Opposite(), PieceType.Peg) == 0)
            {
                return CreeperGameState.Draw;
            }

            bool gameOver = false;
            bool stackEmpty = false;
            Stack<AIBoardNode> stack = new Stack<AIBoardNode>();
            HashSet<AIBoardNode> foundTiles = new HashSet<AIBoardNode>();
            HashSet<AIBoardNode> endTiles = new HashSet<AIBoardNode>();
            Position start = (playerTurn == CreeperColor.Fire) ? _WhiteStart : _BlackStart;
            Position end = (playerTurn == CreeperColor.Fire) ? _WhiteEnd : _BlackEnd;
            AIBoardNode winTile1 = TileBoard[end.Row - 1, end.Column];
            AIBoardNode winTile2 = TileBoard[end.Row, IsValidPosition(end.Row, end.Column - 1, PieceType.Tile)? end.Column - 1: end.Column + 1];
            if (winTile1.Color == playerTurn) endTiles.Add(winTile1);
            if (winTile2.Color == playerTurn) endTiles.Add(winTile2);

            if (!endTiles.Any())
            {
                return CreeperGameState.Unfinished;
            }

            AIBoardNode currentTile = TileBoard[start.Row, start.Column];
            IEnumerable<AIBoardNode> neighbors = GetNeighbors(currentTile, playerTurn);
            while (!stackEmpty && !(currentTile.Row == end.Row && currentTile.Column == end.Column))
            {
                foreach (AIBoardNode neighbor in neighbors)
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

                neighbors = GetNeighbors(currentTile, playerTurn);
            }

            return gameOver ? CreeperGameState.Complete : CreeperGameState.Unfinished;
        }

        public void PrintToConsole()
        {
            for (int row = 0; row < _pegRows; row++)
            {
                for (int column = 0; column < _pegRows; column++)
                {
                    switch (PegBoard[row, column].Color)
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
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public bool CouldBeFinished(CreeperColor turnColor)
        {
            if (BlackPegs.Count == 0 || WhitePegs.Count == 0)
            {
                return true;
            }
            if (TeamCount(turnColor, PieceType.Tile) < _MinimumToWin)
            {
                return false;
            }
            
            AIBoardNode[] rowHeadArray = (turnColor == CreeperColor.Fire)? RowHeadWhite : RowHeadBlack;
            AIBoardNode[] columnHeadArray = (turnColor == CreeperColor.Fire)? ColumnHeadWhite : ColumnHeadBlack;

            if (rowHeadArray.Count(x => x != null) < _tileRows - 2
                || columnHeadArray.Count(x => x != null) < _tileRows - 2)
            {
                return false;
            }

            return true;
        }
    }
}
