using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using CreeperAI;
using Microsoft.Xna.Framework.Media;

namespace ProtoCreeper
{
    class Program
    {
        public static void JumpTest(CreeperBoard board)
        {

            // test's a non jumping of flipping move
           /* if (board.IsValidMove(new Move(new Position(1, 2), new Position(2, 3), CreeperColor.White)))
            {
                Console.WriteLine("SideMove Test passed!");
            }
            else
            {
                Console.WriteLine("SideMove Test failed");
            }*/

            board.Move(new Move(new Position(4,0),new Position(3,0),CreeperColor.Black));
            //board.Move(new Move(new Position(2, 0), new Position(4, 0), CreeperColor.White));
            if (board.IsValidMove(new Move(new Position(2,0), new Position(4,0), CreeperColor.White)))
            {
                Console.WriteLine("Jump Test works!");
            }
            else
            {
                Console.WriteLine("Jump Test Failed");
            }
             
        }

        public static void AIGame(CreeperBoard board)
        {
            CreeperAI.CreeperAI creeperAI = new CreeperAI.CreeperAI();
            bool pauseAfterPrint = false;

            bool gameOver = false;
            CreeperColor turn = CreeperColor.Black;

            while (!board.IsFinished(turn)
                && board.Pegs.Any(x => x.Color == CreeperColor.White)
                && board.Pegs.Any(x => x.Color == CreeperColor.Black))
            {
                turn = (turn == CreeperColor.White) ? CreeperColor.Black : CreeperColor.White;

                board.Move(creeperAI.GetMove(board, turn));
                board.PrintToConsole(pauseAfterPrint);
            }

            Console.WriteLine(String.Format("{0} won.", turn.ToString()));
        }

        public static void WhiteWin(CreeperBoard board)
        {
            Position startPosition;
            Position endPosition;
            Move move;
            bool pausePrint = false;

            for (int i = 0; i < 4; i++)
            {
                startPosition = new Position(i, i + 1);
                endPosition = new Position(i + 1, i + 2);
                move = new Move(startPosition, endPosition, CreeperColor.White);
                board.Move(move);
                board.PrintToConsole(pausePrint);
            }
            for (int i = 0; i < 3; i++)
            {
                startPosition = new Position(i, i + 2);
                endPosition = new Position(i + 1, i + 3);
                move = new Move(startPosition, endPosition, CreeperColor.White);
                board.Move(move);
                board.PrintToConsole(pausePrint);
            }

            startPosition = new Position(4, 6);
            endPosition = new Position(5, 5);
            move = new Move(startPosition, endPosition, CreeperColor.White);
            board.Move(move);
            board.PrintToConsole(pausePrint);

            startPosition = new Position(4, 5);
            endPosition = new Position(5, 4);
            move = new Move(startPosition, endPosition, CreeperColor.White);
            board.Move(move);
            board.PrintToConsole(pausePrint);


            if(board.GetGameState(CreeperColor.White) == CreeperGameState.Complete)
            {
                Console.WriteLine("White wins!");
            }
        }

        static void Main(string[] args)
        {
            CreeperBoard board = new CreeperBoard();
            //board.ReadFromFile("TestBoard.txt");
            //board.PrintToConsole();
            //WhiteWin(board);
            AIGame(board);
            //Testfunction(board);
            //using (XNAControlGame.Game1 game = new XNAControlGame.Game1())
            //{
            //    game.Run();
            //}
        }
    }
}