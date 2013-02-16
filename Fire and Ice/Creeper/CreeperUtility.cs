using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

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

        public static Piece At(this IEnumerable<Piece> pieces, Position position)
        {
            return pieces.First(x => x.Position == position);
        }

        private static bool IsPegOnBoard(Position position)
        {
            return position.Row >= 0 && position.Row <= 6 && position.Column >= 0 && position.Column <= 6;
        }

        public static CreeperColor Opposite(this CreeperColor color)
        {
            if (color != CreeperColor.Black && color != CreeperColor.White)
                throw new ArgumentOutOfRangeException("Argument must be either Black or White to have an opposite color.");

            return (color == CreeperColor.White) ? CreeperColor.Black : CreeperColor.White;
        }

        static public IEnumerable<Move> PossibleMoves(this Piece peg, CreeperBoard board)
        {
            List<Piece> pegs = new List<Piece>(board.Pegs);

            if (peg.Color == CreeperColor.Empty || peg.Color == CreeperColor.Invalid)
            {
                yield return new Move();
                yield break;
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
                    yield return new Move(peg.Position, destinationPosition, peg.Color);
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
                        yield return new Move(peg.Position, destinationPosition, peg.Color);
                    }
                }
            }

            yield break;
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

        // Get the name of a static or instance property from a property access lambda.
        public static string GetPropertyName<T1, T2>(Expression<Func<T1, T2>> propertyLambda)
        {
            var me = propertyLambda.Body as MemberExpression;

            if (me == null)
            {
                throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
            }

            return me.Member.Name;
        }
    }
}