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
            Position startPosition = new Position();
            Position endPosition = new Position();

            List<Peg> MyTeam = board.WhereTeam(AIColor);
            Peg pegToMove = MyTeam.OrderBy((x) => Random.Next()).First();


            return new Move(startPosition, endPosition, AIColor);
        }
    }
}
