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
            List<Peg> MyTeam = board.WhereTeam(AIColor);
            Peg pegToMove = MyTeam.OrderBy((x) => Random.Next()).First();
            
            Position endPosition = new Position(Random.Next() % CreeperBoard.PegRows, Random.Next() % CreeperBoard.PegRows);
            while (!board.IsValidMove(new Move(pegToMove.Position, endPosition, AIColor)))
            {
                endPosition = new Position(Random.Next() % CreeperBoard.PegRows, Random.Next() % CreeperBoard.PegRows);
            }

            return new Move(pegToMove.Position, endPosition, AIColor);
        }

        public int TilesToVictory()
        {
            return 0;
        }
    }
}
