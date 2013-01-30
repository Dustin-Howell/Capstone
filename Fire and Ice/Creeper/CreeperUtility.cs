using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public class Position
    {
        public Position() { }

        public Position(int row, int col)
        {
            Column = col;
            Row = row;
        }

        public int Column { get; set; }
        public int Row { get; set; }

        public bool Equals(Position position)
        {
            return (Column == position.Column && Row == position.Row);
        }
    }

    public static class CreeperUtility
    {
        static List<string> letters = new List<string>() { "A", "B", "C", "D", "E", "F", "G" };

        static public string ConvertToStandardNotation(int x, int y)
        {
            x = 5 - x;

            string notation = x.ToString() + letters[y];

            return notation;
        }


        static public Position ConvertToBasic(string notation)
        {
            Position position = new Position();
            int x;
            //-1 is a bogus value to satiate the compiler
            int y = -1;
            string letter;
            x = (int)Char.GetNumericValue(notation[0]);
            letter = notation[1].ToString();

            for (int i = 0; i < letters.Count; i++)
            {
                if (letters[i] == letter)
                {
                    y = i;
                }
            }

            x = CreeperBoard.PegRows - 1 - x;
            position.Column = x;
            position.Row = y;
            return position;
        }

        static public Array PossibleMove(int location, CreeperColor[][] pegBoard, CreeperColor playerTurn)
        {
            Position position;
            int size = 7;
            int num = location; ;
            List<int> possible = new List<int>();
            int modifier = -(size + 1);

            for (int i = 0; i < 8; i++)
            {
                possible.Add(num + modifier);

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
                position = NumberToPoint(location);

                if (pegBoard[position.Column][position.Row] == CreeperColor.Empty)
                {
                    if (pegBoard[position.Column][position.Row] != playerTurn)
                    {
                        num = location - x;
                        num = x - num;
                        position = NumberToPoint(num);
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

        static public Position NumberToPoint(int number, bool isPeg = false)
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

        static public int PointToNumber(int col, int row, bool isPeg = true)
        {
            int number;
            if (isPeg)
            {
                number = (col + (row * CreeperBoard.PegRows));
            }
            else
            {
                number = (col + (row * CreeperBoard.TileRows));
            }

            return number;
        }
    }
}