using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public class Point
    {
        public Point() { }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }
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


        static public Point ConvertToBasic(string notation)
        {
            Point point = new Point();
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
            point.X = x;
            point.Y = y;
            return point;
        }

        static public Array PossibleMove(int location, CreeperColor[][] pegBoard, CreeperColor playerTurn)
        {
            Point point;
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
                point = NumberToPoint(location);

                if (pegBoard[point.X][point.Y] == CreeperColor.Empty)
                {
                    if (pegBoard[point.X][point.Y] != playerTurn)
                    {
                        num = location - x;
                        num = x - num;
                        point = NumberToPoint(num);
                        if (point.X > 0 && point.X < size && point.Y > 0 && point.Y < size && pegBoard[point.X][point.Y] == CreeperColor.Empty)
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
        static public Point NumberToPoint(int number, bool isPeg = false)
        {
            Point point = new Point();
            if (isPeg)
            {
                point.X = (int)number / CreeperBoard.PegRows;
                point.Y = number % CreeperBoard.PegRows;
            }
            else
            {
                point.X = (int)number / CreeperBoard.TileRows;
                point.Y = number % CreeperBoard.TileRows;
            }

            return point;
        }

        static public int PointToNumber(int x, int y, bool isPeg = true)
        {
            if (isPeg)
                return (y + (x * CreeperBoard.PegRows));
            else
                return (y + (x * CreeperBoard.TileRows));
        }
    }
}