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
        public const int TileRows = 6;
        public const int PegRows = TileRows + 1;

        public List<List<IPeg>> Pegs;
        public List<List<ITile>> Tiles;

        public CreeperBoard()
        {
            Pegs = new List<List<IPeg>>();
            Tiles = new List<List<ITile>>();

            ResetCreeperBoard();
        }

        public void ResetCreeperBoard()
        {
            Tiles.Clear();
            Pegs.Clear();

            for (int i = 0; i < TileRows; i++)
            {
                Tiles.Add(new List<ITile>());
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
                        case PegRows - 3:
                            color = CreeperColor.Black;
                            break;
                        case PegRows - 2:
                            color = CreeperColor.Black;
                            break;
                        case PegRows - 1:
                            color = CreeperColor.Invalid;
                            break;
                        case PegRows:
                            color = CreeperColor.White;
                            break;
                        case PegRows * 2 - 1:
                            color = CreeperColor.Black;
                            break;
                        case PegRows * 2:
                            color = CreeperColor.White;
                            break;
                        case PegRows * 3 - 1:
                            color = CreeperColor.Black;
                            break;
                        case PegRows * (PegRows - 3):
                            color = CreeperColor.Black;
                            break;
                        case (PegRows * (PegRows - 2)) - 1:
                            color = CreeperColor.White;
                            break;
                        case (PegRows * (PegRows - 2)):
                            color = CreeperColor.Black;
                            break;
                        case (PegRows * (PegRows - 1)) - 1:
                            color = CreeperColor.White;
                            break;
                        case PegRows * (PegRows - 1):
                            color = CreeperColor.Invalid;
                            break;
                        case (PegRows * (PegRows - 1)) + 1:
                            color = CreeperColor.Black;
                            break;
                        case (PegRows * (PegRows - 1)) + 2:
                            color = CreeperColor.Black;
                            break;
                        case (PegRows * PegRows) - 3:
                            color = CreeperColor.White;
                            break;
                        case (PegRows * PegRows) - 2:
                            color = CreeperColor.White;
                            break;
                        case (PegRows * PegRows) - 1:
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

                    Tiles[row].Add(new ProtoTile(color));
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
        protected bool GameOver()
        {
            bool gameOver = false;

            return gameOver;
        }

        //TODO: flip/add proper tiles
        public Status Move(int startRow, int startCol, int endRow, int endCol, CreeperColor playerTurn)
        {
            Status status = Status.InvalidMove;

            if (IsValidMove(startRow, startCol, endRow, endCol, playerTurn))
            {
                status = Status.ValidMove;
                Pegs[startRow][startCol].Color = CreeperColor.Empty;
                Pegs[endRow][endCol].Color = playerTurn;

                //flip/add proper tile somewhere around here

                bool gameOver = GameOver();

                if (gameOver)
                {
                    status = Status.GameOver;
                }
            }

            return status;
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
