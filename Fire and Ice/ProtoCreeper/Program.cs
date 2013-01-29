using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace ProtoCreeper
{
    class Program
    {
        public static void WhiteWin(CreeperBoard board)
        {
            for (int i = 0; i < 4; i++)
            {
                board.Move(i, i + 1, i + 1, i + 2, CreeperColor.White);
            }
            for (int i = 0; i < 3; i++)
            {
                board.Move(i, i + 2, i + 1, i + 3, CreeperColor.White);
            }

            board.Move(4, 6, 5, 5, CreeperColor.White);
            board.Move(4, 5, 5, 4, CreeperColor.White);

            board.PrintToConsole();

            if(board.GameOver(CreeperColor.White))
            {
                Console.WriteLine("White wins!");
            }
        }

        public static void PlayerGame(CreeperBoard board)
        {
            CreeperColor playerTurn = CreeperColor.White;
            string moveInput;
            Point pointFrom;
            Point pointTo;
            board.PrintToConsole();

            while (!board.GameOver(CreeperColor.White) && !board.GameOver(CreeperColor.Black))
            {
                Console.WriteLine("Make Move " + playerTurn.ToString());
                Console.WriteLine("From: ");
                moveInput = Console.ReadLine().ToUpper();

                pointFrom = CreeperUtility.ConvertToBasic(moveInput);
                Console.WriteLine(String.Format("Point from: {0},{1}", pointFrom.X, pointFrom.Y));

                Console.WriteLine("To: ");
                moveInput = Console.ReadLine().ToUpper();

                pointTo = CreeperUtility.ConvertToBasic(moveInput);
                Console.WriteLine(String.Format("Point to: {0},{1}", pointTo.X, pointTo.Y));

                board.Move(pointFrom.X, pointFrom.Y, pointTo.X, pointTo.Y, playerTurn);


                if (playerTurn == CreeperColor.White)
                {
                    playerTurn = CreeperColor.Black;
                }
                else
                {
                    playerTurn = CreeperColor.White;
                }

                board.PrintToConsole();
            }
            Console.WriteLine("WE HAVE A WINNER!");
        }

        static void Main(string[] args)
        {
            CreeperBoard board = new CreeperBoard();
            //WhiteWin(board);
            PlayerGame(board);
        }
    }
}