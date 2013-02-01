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
            List<Move> possible = new List<Move>();
            List<Move> jump = new List<Move>();

            Position position = peg.Position;

            int size = (int)Math.Sqrt(pegs.Count);
            int num = 0;

            int location = PositionToNumber(peg.Position);
            int modifier = -(size + 1);

            Move possibleMove = new Move();
            
            possibleMove.StartPosition = position;
            possibleMove.PlayerColor = peg.Color;
            
            for (int i = 0; i < 8; i++)
            {
                possibleMove.EndPosition = NumberToPosition(location + modifier , true);
                if (location % size != 0 || (location + modifier) % size != size - 1)
                {
                    if (location % size != size - 1 || (location + modifier) % size != 0)
                    {
                        possible.Add(new Move(possibleMove));
                    }
                }
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
            possible = possible.Where((x) => !(x.EndPosition.Row < 1
                || x.EndPosition.Row > (size * size) - 2
                || (PositionToNumber(x.EndPosition) == size - 1)
                || PositionToNumber(x.EndPosition) == (size * (size - 1)))).ToList();


            //now check for occupied moves have to fix errors in actual code
            foreach (Move move in possible)
            {
                
                //move.endposition
                if (pegs.At(move.EndPosition).Color != CreeperColor.Empty)
                {
                    //Find jumps change to move .endposition
                    if (pegs.At(move.EndPosition).Color != peg.Color)
                    {
                        num = location - PositionToNumber(move.EndPosition);
                        num = PositionToNumber(move.EndPosition) - num;


                        position = NumberToPosition(num, true);
                        if (position.Row > 0 && position.Row < size && pegs.At(position).Color == CreeperColor.Empty)
                        {
                            if (location % size != 1 || num % size != size - 1)
                            {
                                if (num % size != 0 || location % size != size - 2)
                                {
                                    if (Math.Abs(location - num) != size * 2 + 2 || Math.Abs(location - num) != size * 2 + 2)
                                    {
                                        jump.Add(new Move(peg.Position, position, peg.Color));
                                    }
                                }
                            }
                        }
                    }
                    
                    deleteable.Add(move);
                        
                    
                }
            }

            foreach (Move move in deleteable)
            {
                possible.Remove(move);
            }

            foreach (Move move in jump)
            {
                possible.Add(move);
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