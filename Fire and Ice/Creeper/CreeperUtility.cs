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

        static public List<Move> PossibleMoves(this Piece peg, List<Piece> pegs)
        {
            List<Move> deleteable = new List<Move>();
            Position position = peg.Position;
            int size = 7;//sqrt of pegs?
            int num = 0;
            int location = PositionToNumber(peg.Position); 
            List<Move> possible = new List<Move>();
            Move possibleMove = new Move();
            int modifier = -(size + 1);
            possibleMove.StartPosition = position;
            possibleMove.PlayerColor = peg.Color;

            foreach (CardinalDirection direction in directions)
            {
                if (IsPegOnBoard(peg.Position.Adjacent(direction)) && pegs.At(peg.Position.Adjacent(direction)).Color == CreeperColor.Empty)
                {
                    moves.Add(new Move(peg.Position, peg.Position.Adjacent(direction), peg.Color));
                }
            }

            //First Check if out of bounds
            possible = possible.Where((x) => !(PositionToNumber(x.EndPosition) < 1) 
                || (PositionToNumber(x.EndPosition) > (size * size) - 2)
                || (PositionToNumber(x.EndPosition) == size - 1)
                || PositionToNumber(x.EndPosition) == (size * (size - 1))).ToList();


            //now check for occupied moves have to fix errors in actual code
            foreach (Move move in possible)
            {
                

                if (pegs.At(position).Color == CreeperColor.Empty)
                {
                    if (pegs.At(position).Color != peg.Color)
                    {
                        num = location - PositionToNumber(move.EndPosition);
                        num = PositionToNumber(move.EndPosition) - num;
                        position = NumberToPosition(num);
                        if (position.Column > 0 && position.Column < size && position.Row > 0 && position.Row < size && pegs.At(position).Color == CreeperColor.Empty)
                        {
                            possible.Add(move);
                        }
                        deleteable.Add(move);
                        //possible.Remove(move);

                    }
                    else
                    {
                        deleteable.Add(move);
                        //possible.Remove(move);
                    }
                }
            }

            foreach (Move move in deleteable)
            {
                possible.Remove(move);
            }

            return possible;
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
                number = (position.Row + (position.Column * CreeperBoard.PegRows));
            }
            else
            {
                number = (position.Row + (position.Column * CreeperBoard.TileRows));
            }

            return number;
        }
    }
}