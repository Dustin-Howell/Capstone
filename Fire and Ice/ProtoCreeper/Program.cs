using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using CreeperAI;

namespace ProtoCreeper
{
    class Program
    {
        public static void AIGame(CreeperBoard board)
        {
            CreeperAI.CreeperAI creeperAI = new CreeperAI.CreeperAI();

            bool gameOver = false;
            CreeperColor turn = CreeperColor.White;

            while (!gameOver)
            {
                board.Move(creeperAI.GetMove(board, turn));
                turn = (turn == CreeperColor.White) ? CreeperColor.Black : CreeperColor.White;
                board.PrintToConsole();
            }


            Console.WriteLine(String.Format("{0} lost.", turn.ToString()));
        }

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
            Position pointFrom;
            Position pointTo;
            board.PrintToConsole();
            bool gameOver = false;

            while (!gameOver)
            {
                Console.WriteLine("Make Move " + playerTurn.ToString());
                Console.WriteLine("From: ");
                moveInput = Console.ReadLine().ToUpper();

                pointFrom = CreeperUtility.ConvertToBasic(moveInput);
                Console.WriteLine(String.Format("Point from: {0},{1}", pointFrom.Column, pointFrom.Row));

                Console.WriteLine("To: ");
                moveInput = Console.ReadLine().ToUpper();

                pointTo = CreeperUtility.ConvertToBasic(moveInput);
                Console.WriteLine(String.Format("Point to: {0},{1}", pointTo.Column, pointTo.Row));

                board.Move(pointFrom.Column, pointFrom.Row, pointTo.Column, pointTo.Row, playerTurn);

                gameOver = board.GameOver(CreeperColor.White) || board.GameOver(CreeperColor.Black);

                if (!gameOver)
                {
                    if (playerTurn == CreeperColor.White)
                    {
                        playerTurn = CreeperColor.Black;
                    }
                    else
                    {
                        playerTurn = CreeperColor.White;
                    }
                }

                board.PrintToConsole();
            }
            Console.WriteLine(String.Format("{0} wins!", playerTurn.ToString()));
        }

        static void Main(string[] args)
        {
            CreeperBoard board = new CreeperBoard();
            //WhiteWin(board);
            //PlayerGame(board);
            AIGame(board);
        }
    }
}