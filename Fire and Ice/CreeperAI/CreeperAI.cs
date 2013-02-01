using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperAI
{
    public class CreeperAI
    {
        private Random Random = new Random();

        private const int _White = 0;
        private const int _Black = 1;

        public Move GetMove(CreeperBoard board, CreeperColor AIColor)
        {
            List<Piece> MyTeam = board.WhereTeam(AIColor);
            Piece pegToMove = MyTeam.OrderBy((x) => Random.Next()).First();
            return CreeperUtility.PossibleMoves(pegToMove, board.Pegs).OrderBy((x) => Random.Next()).First();
        }

        public int TilesToVictory()
        {
            return 0;
        }
    }
}
