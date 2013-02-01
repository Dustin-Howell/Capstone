using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{

    public static class CreeperUtility
    {
        public static List<string> Letters = new List<string>() { "A", "B", "C", "D", "E", "F", "G" };

        public static IEnumerable<Piece> GetNeighbors(this Piece tile, CreeperBoard board)
        {
            List<CardinalDirection> neighborlyDirections = new List<CardinalDirection> { CardinalDirection.North, CardinalDirection.South, CardinalDirection.East, CardinalDirection.West };
            return neighborlyDirections
            .Where(x => board.Tiles.Any(y => y.Position == tile.Position.Adjacent(x)))
            .Select(x => board.Tiles.At(tile.Position.Adjacent(x)));
        }

        public static Piece At(this List<Piece> pieces, Position position)
        {
            return pieces.Where(x => x.Position == position).First();
        }

        private static bool IsPegOnBoard(Position position)
        {
            return position.Row >= 0 && position.Row <= 6 && position.Column >= 0 && position.Column <= 6;
        }

        static public List<Move> PossibleMoves(Piece peg, List<Piece> pegs)
        {
            List<Move> moves = new List<Move>();
            Array directions = Enum.GetValues(typeof(CardinalDirection));

            foreach (CardinalDirection direction in directions)
            {
                if (IsPegOnBoard(peg.Position.Adjacent(direction)) && pegs.At(peg.Position.Adjacent(direction)).Color == CreeperColor.Empty)
                {
                    moves.Add(new Move(peg.Position, peg.Position.Adjacent(direction), peg.Color));
                }
            }

            foreach (CardinalDirection direction in new[] { CardinalDirection.North, CardinalDirection.South, CardinalDirection.East, CardinalDirection.West })
            {
                if (IsPegOnBoard(peg.Position.Adjacent(direction)) && IsPegOnBoard(peg.Position.Adjacent(direction).Adjacent(direction))
                    && pegs.At(peg.Position.Adjacent(direction).Adjacent(direction)).Color == CreeperColor.Empty
                    && pegs.At(peg.Position.Adjacent(direction)).Color == (peg.Color == CreeperColor.White ? CreeperColor.Black : CreeperColor.White))
                {
                    moves.Add(new Move(peg.Position, peg.Position.Adjacent(direction).Adjacent(direction), peg.Color));
                }
            }

            return moves;
        }

        static public Position NumberToPosition(int number, bool isPeg = false)
        {
            Position position = new Position();
            if (isPeg)
            {
                position.Row = (int)number / CreeperBoard.PegRows;
                position.Column = number % CreeperBoard.PegRows;
            }
            else
            {
                position.Row = (int)number / CreeperBoard.TileRows;
                position.Column = number % CreeperBoard.TileRows;
            }

            return position;
        }

        static public int PositionToNumber(Position position, bool isPeg = true)
        {
            int number;
            if (isPeg)
            {
                number = (position.Column + (position.Row * CreeperBoard.PegRows));
            }
            else
            {
                number = (position.Column + (position.Row * CreeperBoard.TileRows));
            }

            return number;
        }
    }
}