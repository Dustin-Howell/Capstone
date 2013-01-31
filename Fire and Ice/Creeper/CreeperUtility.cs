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
            List<Piece> neighbors = new List<Piece>();
            //neighbors = board.Tiles.Where(x => 
            //    (board.IsValidPosition(x.Position.Adjacent(CardinalDirection.North), PieceType.Tile)
            //    ||)).ToList();

            return new List<Piece>();
        }

        public static Piece At(this List<Piece> pieces, Position position)
        {
            return pieces.Where(x => x.Position == position).First();
        }

    

        static public List<Move> PossibleMove(Piece peg, List<Piece> pegs)
        {
            Position position = peg.Position;
            int size = 7;//sqrt of pegs?
            int num = 0;
            int location = PositionToNumber(peg.Position); 
            List<Move> possible = new List<Move>();
            Move possibleMove = new Move();
            int modifier = -(size + 1);
            possibleMove.StartPosition = position;
            possibleMove.PlayerColor = peg.Color;

            //gets all the spots around the current peg
            for (int i = 0; i < 8; i++)
            {
                possibleMove.EndPosition = NumberToPosition(location + modifier);
                possible.Add(possibleMove);

                if (modifier == -(size - 1))
                {
                    modifier = -1;
                }
                else if (modifier == -1)
                {
                    modifier = 1;
                }
                else if (modifier == 1)
                {
                    modifier = size - 1;
                }
                else
                {
                    modifier++;
                }
            }

            //First Check if out of bounds
            foreach (Move move in possible)
            {
                if ((PositionToNumber(move.EndPosition) < 1) || PositionToNumber(move.EndPosition) > (size * size) - 2)
                {
                    possible.Remove(move);
                }
                if ((PositionToNumber(move.EndPosition) == size - 1 || PositionToNumber(move.EndPosition) == (size * (size - 1))))
                {
                    possible.Remove(move);
                }
            }

            //now check for occupied moves have to fix errors in actual code
            foreach (Move x in possible)
            {
                

                if (pegs.At(position).Color == CreeperColor.Empty)
                {
                    if (pegs.At(position).Color != peg.Color)
                    {
                        num = location - PositionToNumber(x.EndPosition);
                        num = PositionToNumber(x.EndPosition) - num;
                        position = NumberToPosition(num);
                        if (position.Column > 0 && position.Column < size && position.Row > 0 && position.Row < size && pegs.At(position).Color == CreeperColor.Empty)
                        {
                            possible.Add(x);
                        }

                        possible.Remove(x);

                    }
                    else
                    {
                        possible.Remove(x);
                    }
                }
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