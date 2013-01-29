using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace ProtoCreeper
{
    class Program
    {
        static void Main(string[] args)
        {
            CreeperColor playerTurn = CreeperColor.White;
            CreeperBoard board = new CreeperBoard();
            string input;
            Point pointFrom;
            Point pointTo;
            while (!board.GameOver(0,CreeperColor.White) && !board.GameOver(30,CreeperColor.Black))
            {
                board.PrintToConsole();

                Console.WriteLine("Make Move " + playerTurn.ToString());
                Console.WriteLine("From: ");
                input = Console.ReadLine();

                pointFrom = CreeperUtility.ConvertToBasic(input);

                Console.WriteLine("To: ");
                input = Console.ReadLine();

                pointTo = CreeperUtility.ConvertToBasic(input);

                board.Move(pointFrom.X, pointFrom.Y, pointTo.X, pointTo.Y, playerTurn);

                if (playerTurn == CreeperColor.White)
                {
                    playerTurn = CreeperColor.Black;
                }
                else
                {
                    playerTurn = CreeperColor.White;
                }                
            }
        }
    }
}