using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using System.Diagnostics;
using System.IO;

namespace CreeperAI
{
    public class CreeperAI
    {
        //debug variables\\
        private bool _reportTime = true;

        private AICreeperBoard _board;
        private CreeperColor _turnColor;

        public Move GetMove(CreeperBoard board, CreeperColor turnColor)
        {
            _board = new AICreeperBoard(board);
            _turnColor = turnColor;

            Stopwatch stopwatch = new Stopwatch();
            if (_reportTime) stopwatch.Start();

            Move bestMove = new Move();
            bestMove = GetAlphaBetaMiniMaxMove(_board);

            if (_reportTime)
            {
                stopwatch.Stop();
                System.Console.WriteLine("Seconds elapsed: {0}", ((double)stopwatch.ElapsedMilliseconds) / 1000);
                using (StreamWriter file = new StreamWriter("Times.log", true))
                {
                    file.WriteLine("Seconds elapsed: {0}", ((double)stopwatch.ElapsedMilliseconds) / 1000);
                }
            }

            return bestMove;
        }

        private Move GetAlphaBetaMiniMaxMove(AICreeperBoard _board)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<Move> GetPossibleMoves()
        {
            throw new NotImplementedException();
        }


    }
}
