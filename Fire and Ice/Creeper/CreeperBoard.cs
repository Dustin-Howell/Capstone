using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public enum CreeperColor { White, Black, Empty, Invalid }
    public enum Status { ValidMove, InvalidMove, GameOver }


    public class CreeperBoard
    {
        protected const int _TileRows = 6;
        protected const int _PegRows = _TileRows + 1;
        public static int TileRows { get { return _TileRows; } }
        public static int PegRows { get { return _PegRows; } }

        private static int _BlackStart { get { return TileRows * (TileRows - 1); } }
        private static int _WhiteStart { get { return 0; } }
        private static int _BlackEnd { get { return TileRows - 1; } }
        private static int _WhiteEnd { get { return (TileRows * TileRows) - 1; } }

        public List<List<Peg>> Pegs;
        public List<List<Tile>> Tiles;

        public CreeperBoard()
        {
            Pegs = new List<List<Peg>>();
            Tiles = new List<List<Tile>>();

            ResetCreeperBoard();
            AssignNeighbors();
        }

        public List<Peg> WhereTeam(CreeperColor color)
        {
            List<Peg> teamList = new List<Peg>();

            foreach (List<Peg> pegRow in Pegs)
            {
                List<Peg> list = pegRow.Where(x => x.Color == color).ToList();
                teamList.AddRange(list);
            }

            return teamList;
        }

        public bool IsValidPoint(Position point, bool tilePoint = true)
        {
            int rows = (tilePoint)? TileRows : PegRows;

            return (point.Column >= 0 && point.Column < rows && point.Row >= 0 && point.Row < rows);
        }

        protected void AssignNeighbors()
        {
            foreach (List<Tile> tileRow in Tiles)
            {
                foreach (Tile tile in tileRow)
                {
                    List<Tile> neighbors = new List<Tile>();

                    int col = tile.Point.Column;
                    int row = tile.Point.Row;

                    List<Position> possibleNeighbors = new List<Position>();
                    
                    possibleNeighbors.Add(new Position(col, row - 1));
                    possibleNeighbors.Add(new Position(col + 1, row));
                    possibleNeighbors.Add(new Position(col, row + 1));
                    possibleNeighbors.Add(new Position(col - 1, row));

                    foreach (Position point in possibleNeighbors)
                    {
                        if (IsValidPoint(point))
                        {
                            neighbors.Add(Tiles[point.Row][point.Column]);
                        }
                    }

                    tile.SetNeighbors(neighbors);
                }
            }
        }

        public void ResetCreeperBoard()
        {
            Tiles.Clear();
            Pegs.Clear();

            for (int i = 0; i < TileRows; i++)
            {
                Tiles.Add(new List<Tile>());
            }

            for (int i = 0; i < PegRows; i++)
            {
                Pegs.Add(new List<Peg>());
            }

            for (int row = 0; row < PegRows; row++)
            {
                for (int col = 0; col < PegRows; col++)
                {
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
                    Peg peg = new Peg(color, slotNumber);
                    Pegs[row].Add(peg);
                }
            }

            for (int row = 0; row < TileRows; row++)
            {
                for (int col = 0; col < TileRows; col++)
                {
                    CreeperColor color = CreeperColor.Empty;

                    int slotNumber = CreeperUtility.PointToNumber(row, col, false);
                    if (
                        (slotNumber == 0)
                        || (slotNumber == TileRows - 1)
                        || (slotNumber == TileRows * (TileRows - 1))
                        || (slotNumber == (TileRows * TileRows) - 1)
                        )
                    {
                        color = CreeperColor.Invalid;
                    }

                    Tiles[row].Add(new Tile(color, slotNumber));
                }
            }


        }

        public bool IsValidMove(int startRow, int startCol, int endRow, int endCol, CreeperColor playerTurn)
        {
            bool valid = true;

            //is the move in bounds?
            if (startCol < 0 || startCol >= PegRows
                || endCol < 0 || endCol >= PegRows
                || startRow < 0 || startRow >= PegRows
                || endRow < 0 || endRow >= PegRows)
            {
                valid = false;
            }

            //Does the start space have the player's piece?
            else if (Pegs[startRow][startCol].Color != playerTurn)
            {
                valid = false;
            }

            //Is the end space empty?
            else if (Pegs[endRow][endCol].Color != CreeperColor.Empty)
            {
                valid = false;
            }

            //is the end space one away from the start space?
            else if ((Math.Abs(startRow - endRow) > 1)
                || (Math.Abs(startCol - endCol) > 1)
                || (startCol == endCol && startRow == endRow))
            {
                valid = false;
            }

            return valid;
        }

        public bool GameOver(CreeperColor playerTurn)
        {
            bool gameOver = false;
            Stack<Tile> tiles = new Stack<Tile>();
            Position start = CreeperUtility.NumberToPoint((playerTurn == CreeperColor.White) ? _WhiteStart : _BlackStart);
            int end = (playerTurn == CreeperColor.White) ? _WhiteEnd : _BlackEnd;

            tiles.Push(Tiles[start.Row][start.Column]);

            while (!gameOver && tiles.Any())
            {
                Tile currentTile = tiles.Pop();
                currentTile.Marked = true;
                foreach (Tile neighbor in currentTile.Neighbors)
                {
                    if (!neighbor.Marked && neighbor.Color == playerTurn)
                    {
                        tiles.Push(neighbor);
                    }
                    else if (neighbor.Color == CreeperColor.Invalid && neighbor.SlotNumber == end)
                    {
                        gameOver = true;
                    }
                }
            }

            return gameOver;
        }

        private void Flip(int startRow, int startCol, int endRow, int endCol, CreeperColor playerTurn)
        {
            Position point;
            int start = CreeperUtility.PointToNumber(startRow, startCol);
            int end = CreeperUtility.PointToNumber(endRow, endCol);
            int number = Math.Abs(start - (start / PegRows));

            if ((Math.Abs(start - end)) == PegRows + 1)
            {
                point = CreeperUtility.NumberToPoint(number);
                Tiles[point.Row][point.Column].Color = playerTurn;
            }
            else if ((Math.Abs(start - end)) == PegRows - 1)
            {
                point = CreeperUtility.NumberToPoint(number - 1);
                Tiles[point.Row][point.Column].Color = playerTurn;
            }
        }

        public bool Move(Move move)
        {
            return Move(move.StartPoint.Row, move.StartPoint.Column, move.EndPoint.Row, move.EndPoint.Column, move.PlayerColor);
        }

        public bool Move(int startRow, int startCol, int endRow, int endCol, CreeperColor playerTurn)
        {
            bool isValid = false;

            if (IsValidMove(startRow, startCol, endRow, endCol, playerTurn))
            {
                isValid = true;
                Pegs[startRow][startCol].Color = CreeperColor.Empty;
                Pegs[endRow][endCol].Color = playerTurn;

                if (Math.Abs(startRow - endRow) + Math.Abs(startCol - endCol) == 2)
                {
                    Flip(startRow, startCol, endRow, endCol, playerTurn);
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
            foreach (List<Peg> row in Pegs)
            {
                foreach (Peg peg in row)
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
                    if (row.IndexOf(peg) < PegRows - 1)
                    {
                        Console.Write("-");
                    }
                }
                Console.Write("\n");

                if (Pegs.IndexOf(row) < TileRows)
                {
                    Console.Write("|");
                    for (int i = 0; i < TileRows; i++)
                    {
                        switch (Tiles[Pegs.IndexOf(row)][i].Color)
                        {
                            case CreeperColor.Black:
                                Console.Write("X");
                                break;
                            case CreeperColor.Empty:
                                Console.Write(" ");
                                break;
                            case CreeperColor.Invalid:
                                Console.Write("*");
                                break;
                            case CreeperColor.White:
                                Console.Write("O");
                                break;
                        }
                        Console.Write("|");
                    }
                    Console.Write("\n");
                }                
            }
        }
    }
}