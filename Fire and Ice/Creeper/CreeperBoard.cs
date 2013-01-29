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
        private static int BLACKEND { get { return TileRows - 1; } }
        private static int WHITEEND { get { return (TileRows * TileRows) - 1; } }
        public List<List<IPeg>> Pegs;
        public List<List<Tile>> Tiles;

        public CreeperBoard()
        {
            Pegs = new List<List<IPeg>>();
            Tiles = new List<List<Tile>>();

            ResetCreeperBoard();
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
                Pegs.Add(new List<IPeg>());
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
                            color = CreeperColor.White;
                            break;
                        default:
                            color = CreeperColor.Empty;
                            break;
                    }
                    IPeg peg = new ProtoPeg(color);
                    Pegs[row].Add(peg);
                }
            }

            for (int row = 0; row < TileRows; row++)
            {
                for (int col = 0; col < TileRows; col++)
                {
                    CreeperColor color = CreeperColor.Empty;

                    int slotNumber = (row * PegRows) + col;
                    if (slotNumber == 0
                        || slotNumber == TileRows - 1
                        || slotNumber == TileRows * (TileRows - 1)
                        || slotNumber == (TileRows * TileRows) - 1)
                    {
                        color = CreeperColor.Invalid;
                    }

                    Tiles[row].Add(new Tile(color));
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

        //TODO: Write this function
        protected bool GameOver(int x, int y, CreeperColor playerTurn, int endX, int endY)
        {

            if (x == endX && y == endY)
            {
                return true;
            }

            if (Tiles[x][y].Marked)
            {
                return false;
            }

            Tiles[x][y].Marked = true;

            if ((y - 1 > 0) && Tiles[x][y - 1].Color == playerTurn)
            {
                GameOver(x, y - 1, playerTurn, endX, endY);
            }

            if ((x - 1 > 0) && Tiles[x - 1][y].Color == playerTurn)
            {
                GameOver(x - 1, y, playerTurn, endX, endY);
            }

            if ((y + 1 < TileRows) && Tiles[x][y + 1].Color == playerTurn)
            {
                GameOver(x, y + 1, playerTurn, endX, endY);
            }

            if ((x + 1 < TileRows) && Tiles[x + 1][y].Color == playerTurn)
            {
                GameOver(x + 1, y, playerTurn, endX, endY);
            }
            Tiles[x][y].Marked = false;
            return false;
        }

        public bool GameOver(int location, CreeperColor playerTurn)
        {
            Point point;
            Point endPoint;
            point = CreeperUtility.NumberToPoint(location);

            int end;
            if (playerTurn == CreeperColor.Black)
            {
                end = BLACKEND;
            }
            else
            {
                end = WHITEEND;
            }

            endPoint = CreeperUtility.NumberToPoint(end);

            Tiles[point.X][point.Y].Marked = true;

            if (CreeperUtility.PointToNumber(point.X, point.Y, false) == end)
            {
                return true;
            }


            if ((point.Y - 1 > 0) && Tiles[point.X][point.Y - 1].Color == playerTurn)
            {
                GameOver(point.X, point.Y - 1, playerTurn, endPoint.X, endPoint.Y);
            }

            if ((point.X - 1 > 0) && Tiles[point.X - 1][point.Y].Color == playerTurn)
            {
                GameOver(point.X - 1, point.Y, playerTurn, endPoint.X, endPoint.Y);
            }

            if ((point.Y + 1 < TileRows) && Tiles[point.X][point.Y + 1].Color == playerTurn)
            {
                GameOver(point.X, point.Y + 1, playerTurn, endPoint.X, endPoint.Y);
            }

            if ((point.X + 1 < TileRows) && Tiles[point.X + 1][point.Y].Color == playerTurn)
            {
                GameOver(point.X + 1, point.Y, playerTurn, endPoint.X, endPoint.Y);
            }

            Tiles[point.X][point.Y].Marked = false;
            return false;
        }



        private void Flip(int startRow, int startCol, int endRow, int endCol, CreeperColor playerTurn)
        {
            Point point;
            int start = CreeperUtility.PointToNumber(startRow, startCol);
            int end = CreeperUtility.PointToNumber(endRow, endCol);
            int number = Math.Abs(start - (start / PegRows));

            if ((Math.Abs(start - end)) == PegRows + 1)
            {
                point = CreeperUtility.NumberToPoint(number);
                Tiles[point.X][point.Y].Color = playerTurn;
            }
            else if ((Math.Abs(start - end)) == PegRows - 1)
            {
                point = CreeperUtility.NumberToPoint(number - 1);
                Tiles[point.X][point.Y].Color = playerTurn;
            }
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

        //TODO: Make this print the tile spaces as well
        public void PrintToConsole()
        {
            foreach (List<IPeg> row in Pegs)
            {
                foreach (IPeg peg in row)
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
                }
                Console.Write("\n");
            }
        }
    }
}