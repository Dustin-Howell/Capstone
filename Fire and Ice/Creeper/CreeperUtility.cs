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
                .Where(x => board.Tiles.Any(y => y.Position == tile.Position.AtDirection(x)))
                .Select(x => board.Tiles.At(tile.Position.AtDirection(x)));
        }

        public static Piece At(this List<Piece> pieces, Position position)
        {
            return pieces.Where(x => x.Position == position).First();
        }

        private static bool IsPegOnBoard(Position position)
        {
            return position.Row >= 0 && position.Row <= 6 && position.Column >= 0 && position.Column <= 6;
        }

        static public List<Move> PossibleMoves(this Piece peg, CreeperBoard board)
        {
            List<Piece> pegs = new List<Piece>(board.Pegs);
            List<Move> possibleMoves = new List<Move>();
            if (peg.Color == CreeperColor.Empty || peg.Color == CreeperColor.Invalid)
            {
                return new List<Move>();
            }
            List<CardinalDirection> neighborlyDirections = new List<CardinalDirection>
                                                                { 
                                                                    CardinalDirection.North,
                                                                    CardinalDirection.Northeast,
                                                                    CardinalDirection.Northwest,
                                                                    CardinalDirection.South, 
                                                                    CardinalDirection.Southeast,
                                                                    CardinalDirection.Southwest,
                                                                    CardinalDirection.East, 
                                                                    CardinalDirection.West 
                                                                };

            foreach (CardinalDirection direction in neighborlyDirections)
            {
                Position destinationPosition = peg.Position.AtDirection(direction);
                if (board.IsValidPosition(destinationPosition, PieceType.Peg)
                    && pegs.At(destinationPosition).Color == CreeperColor.Empty)
                {
                    Move move = new Move(peg.Position, destinationPosition, peg.Color);
                    possibleMoves.Add(move);
                }

                if (
                        (direction == CardinalDirection.North
                        || direction == CardinalDirection.South
                        || direction == CardinalDirection.East
                        || direction == CardinalDirection.West
                        )
                        && (board.IsValidPosition(destinationPosition, PieceType.Peg))
                        && (pegs.At(destinationPosition).Color == ((peg.Color == CreeperColor.White) ? CreeperColor.Black : CreeperColor.White))
                    )
                {
                    destinationPosition = destinationPosition.AtDirection(direction);
                    if (board.IsValidPosition(destinationPosition, PieceType.Peg)
                        && pegs.At(destinationPosition).Color == CreeperColor.Empty)
                    {
                        possibleMoves.Add(new Move(peg.Position, destinationPosition, peg.Color));
                    }
                }
            }            

            return possibleMoves;
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