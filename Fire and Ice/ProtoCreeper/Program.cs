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

            while (!board.GameOver(turn)
                && board.Pegs.Any(x => x.Color == CreeperColor.White)
                && board.Pegs.Any(x => x.Color == CreeperColor.Black))
            {
                board.Move(creeperAI.GetMove(board, turn));
                turn = (turn == CreeperColor.White) ? CreeperColor.Black : CreeperColor.White;
                board.PrintToConsole();
                Console.ReadLine();
            }

            Console.WriteLine(String.Format("{0} lost.", turn.ToString()));
        }

        public static void WhiteWin(CreeperBoard board)
        {
            Position startPosition;
            Position endPosition;
            Move move;

            for (int i = 0; i < 4; i++)
            {
                startPosition = new Position(i, i + 1);
                endPosition = new Position(i + 1, i + 2);
                move = new Move(startPosition, endPosition, CreeperColor.White);
                board.Move(move);
                board.PrintToConsole();
            }
            for (int i = 0; i < 3; i++)
            {
                startPosition = new Position(i, i + 2);
                endPosition = new Position(i + 1, i + 3);
                move = new Move(startPosition, endPosition, CreeperColor.White);
                board.Move(move);
                board.PrintToConsole();
            }

            startPosition = new Position(4, 6);
            endPosition = new Position(5, 5);
            move = new Move(startPosition, endPosition, CreeperColor.White);
            board.Move(move);
            board.PrintToConsole();

            startPosition = new Position(4, 5);
            endPosition = new Position(5, 4);
            move = new Move(startPosition, endPosition, CreeperColor.White);
            board.Move(move);
            board.PrintToConsole();


            if(board.GameOver(CreeperColor.White))
            {
                Console.WriteLine("White wins!");
            }
        }

        //public static void PlayerGame(CreeperBoard board)
        //{
        //    CreeperColor playerTurn = CreeperColor.White;
        //    string moveInput;
        //    Position pointFrom;
        //    Position pointTo;
        //    board.PrintToConsole();
        //    bool gameOver = false;

        //    while (!gameOver)
        //    {
        //        Console.WriteLine("Make Move " + playerTurn.ToString());
        //        Console.WriteLine("From: ");
        //        moveInput = Console.ReadLine().ToUpper();

        //        pointFrom = CreeperUtility.ConvertToBasic(moveInput);
        //        Console.WriteLine(String.Format("Point from: {0},{1}", pointFrom.Column, pointFrom.Row));

        //        Console.WriteLine("To: ");
        //        moveInput = Console.ReadLine().ToUpper();

        //        pointTo = CreeperUtility.ConvertToBasic(moveInput);
        //        Console.WriteLine(String.Format("Point to: {0},{1}", pointTo.Column, pointTo.Row));

        //        board.Move(pointFrom.Column, pointFrom.Row, pointTo.Column, pointTo.Row, playerTurn);

        //        gameOver = board.GameOver(CreeperColor.White) || board.GameOver(CreeperColor.Black);

        //        if (!gameOver)
        //        {
        //            if (playerTurn == CreeperColor.White)
        //            {
        //                playerTurn = CreeperColor.Black;
        //            }
        //            else
        //            {
        //                playerTurn = CreeperColor.White;
        //            }
        //        }

        //        board.PrintToConsole();
        //    }
        //    Console.WriteLine(String.Format("{0} wins!", playerTurn.ToString()));
        //}

        /*public static void PlayerGame(CreeperBoard board)
        {
            CreeperColor playerTurn = CreeperColor.White;
            string moveInput;
                Position positionFrom;
                Position positionTo;
                board.PrintToConsole();
                bool gameOver = false;
                Move move;
                while (!gameOver)
                {
                    Console.WriteLine("Make Move " + playerTurn.ToString());
                    Console.WriteLine("From: ");
                    moveInput = Console.ReadLine().ToUpper();

                    positionFrom = CreeperUtility.ConvertToBasic(moveInput);
                    Console.WriteLine(String.Format("Point from: {0},{1}", positionFrom.Column, positionFrom.Row));

                    Console.WriteLine("To: ");
                    moveInput = Console.ReadLine().ToUpper();

                    positionTo = CreeperUtility.ConvertToBasic(moveInput);
                    Console.WriteLine(String.Format("Point to: {0},{1}", positionTo.Column, positionTo.Row));
                    move = new Move(positionFrom, positionTo, playerTurn);
                    board.Move(move);

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
        }*/

        static void Main(string[] args)
        {
            CreeperBoard board = new CreeperBoard();
            //board.PrintToConsole();
            //WhiteWin(board);
            //PlayerGame(board);
            while (true)
            {
                AIGame(board);
                board.ResetCreeperBoard();
            }
            
        }
    }
}