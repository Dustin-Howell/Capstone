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

            board.Move(new Move(new Position(4,0),new Position(3,0),CreeperColor.Ice));
            //board.Move(new Move(new Position(2, 0), new Position(4, 0), CreeperColor.White));
            if (board.IsValidMove(new Move(new Position(2,0), new Position(4,0), CreeperColor.Fire)))
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
            CreeperAI.CreeperAI creeperAI = new CreeperAI.CreeperAI(2, 10, .01, 10, 11, 1000);
            bool pauseAfterPrint = false;

            bool gameOver = false;
            CreeperColor turn = CreeperColor.Ice;

            while (!board.IsFinished(turn)
                && board.Pegs.Any(x => x.Color == CreeperColor.Fire)
                && board.Pegs.Any(x => x.Color == CreeperColor.Ice))
            {
                turn = (turn == CreeperColor.Fire) ? CreeperColor.Ice : CreeperColor.Fire;

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
                move = new Move(startPosition, endPosition, CreeperColor.Fire);
                board.Move(move);
                board.PrintToConsole(pausePrint);
            }
            for (int i = 0; i < 3; i++)
            {
                startPosition = new Position(i, i + 2);
                endPosition = new Position(i + 1, i + 3);
                move = new Move(startPosition, endPosition, CreeperColor.Fire);
                board.Move(move);
                board.PrintToConsole(pausePrint);
            }

            startPosition = new Position(4, 6);
            endPosition = new Position(5, 5);
            move = new Move(startPosition, endPosition, CreeperColor.Fire);
            board.Move(move);
            board.PrintToConsole(pausePrint);

            startPosition = new Position(4, 5);
            endPosition = new Position(5, 4);
            move = new Move(startPosition, endPosition, CreeperColor.Fire);
            board.Move(move);
            board.PrintToConsole(pausePrint);


            if(board.GetGameState(CreeperColor.Fire) == CreeperGameState.Complete)
            {
                Console.WriteLine("White wins!");
            }
        }

        static void PrintAICreeperBoard(CreeperBoard board)
        {
            board.ReadFromFile("TestBoard.txt");
            AICreeperBoard AIBoard = new AICreeperBoard(board);
            AIBoard.PrintToConsole();
        }

        static void Main(string[] args)
        {
            CreeperBoard board = new CreeperBoard();
            //PrintAICreeperBoard(board);
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