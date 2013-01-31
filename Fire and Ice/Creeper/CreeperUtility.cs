using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{

    public static class CreeperUtility
    {
        public static List<string> Letters = new List<string>() { "A", "B", "C", "D", "E", "F", "G" };

        public static IEnumerable<Piece> GetNeighbors(this Piece tile, CreeperBoard board)
        {
            List<Piece> neighbors = new List<Piece>();
            //neighbors = board.Tiles.Where(x => 
            //    (board.IsValidPosition(x.Position.Adjacent(CardinalDirection.North), PieceType.Tile)
            //    ||)).ToList();

            return new List<Piece>();
        }

        public static Piece At(this List<Piece> pieces, Position position)
        {
            return pieces.Where(x => x.Position == position).First();
        }

    

        static public Array PossibleMove(Piece peg, CreeperColor[][] pegBoard)
        {
            Position position;
            int size = 7;
            int num = 0;
            int location = PositionToNumber(peg.Position); 
            List<int> possible = new List<int>();
            int modifier = -(size + 1);

            for (int i = 0; i < 8; i++)
            {
                possible.Add(location + modifier);

                if (modifier == -(size - 1))
                {
                    modifier = -1;
                }
                else if (modifier == -1)
                {
                    modifier = 1;
                }
                else if (modifier == 1)
                {
                    modifier = size - 1;
                }
                else
                {
                    modifier++;
                }
            }
            //First Check if out of bounds
            foreach (int x in possible)
            {
                if ((x < 1) || x > (size * size) - 2)
                {
                    possible.Remove(x);
                }
                if (x == size - 1 || x == (size * (size - 1)))
                {
                    possible.Remove(x);
                }
            }
            //now check for occupied moves have to fix errors in actual code
            foreach (int x in possible)
            {
                position = NumberToPosition(location);

                if (pegBoard[position.Column][position.Row] == CreeperColor.Empty)
                {
                    if (pegBoard[position.Column][position.Row] != peg.Color)
                    {
                        num = location - x;
                        num = x - num;
                        position = NumberToPosition(num);
                        if (position.Column > 0 && position.Column < size && position.Row > 0 && position.Row < size && pegBoard[position.Column][position.Row] == CreeperColor.Empty)
                        {
                            possible.Add(num);
                        }
                        possible.Remove(x);

                    }
                    else
                    {
                        possible.Remove(x);
                    }
                }
            }

            return possible.ToArray();
        }

        static public Position NumberToPosition(int number, bool isPeg = false)
        {
            Position position = new Position();
            if (isPeg)
            {
                position.Row = (int)number / CreeperBoard.PegRows;
                position.Column = number % CreeperBoard.PegRows;
            }
            else
            {
                position.Row = (int)number / CreeperBoard.TileRows;
                position.Column = number % CreeperBoard.TileRows;
            }

            return position;
        }

        static public int PositionToNumber(Position position, bool isPeg = true)
        {
            int number;
            if (isPeg)
            {
                number = (position.Column + (position.Row * CreeperBoard.PegRows));
            }
            else
            {
                number = (position.Column + (position.Row * CreeperBoard.TileRows));
            }

            return number;
        }
    }
}