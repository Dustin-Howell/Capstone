using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCreeper
{

    class Program
    {
        //TODO: Write a real main that lets us play a game of creeper on the computer
        static void Main(string[] args)
        {
            CreeperBoard board = new CreeperBoard();
            board.PrintToConsole();

            int startRow, startCol, endRow, endCol;

            Console.Write("Start Row: ");
            startRow = Int32.Parse(Console.ReadLine());

            Console.Write("Start Col: ");
            startCol = Int32.Parse(Console.ReadLine());

            Console.Write("End Row: ");
            endRow = Int32.Parse(Console.ReadLine());

            Console.Write("End Col: ");
            endCol = Int32.Parse(Console.ReadLine());

            board.Move(startRow, startCol, endRow, endCol);

            board.PrintToConsole();
        }
    }
}
