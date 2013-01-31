using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public class Position
    {
        public Position() { }

        public Position(int row, int col)
        {
            Column = col;
            Row = row;
        }

        public int Column { get; set; }
        public int Row { get; set; }

        public bool Equals(Position position)
        {
            return (Column == position.Column && Row == position.Row);
        }

        public Position Adjacent(CardinalDirection direction)
        {
            Position position = new Position();
            if (direction == CardinalDirection.North)
            {
                position.Row = this.Row;
                position.Column = this.Column - 1;
            }
            else if (direction == CardinalDirection.South)
            {
                position.Row = this.Row;
                position.Column = this.Column + 1;
            }
            else if (direction == CardinalDirection.East)
            {
                position.Row = this.Row + 1;
                position.Column = this.Column;
            }
            else if (direction == CardinalDirection.West)
            {
                position.Row = this.Row - 1;
                position.Column = this.Column;
            }
            else if (direction == CardinalDirection.Northwest)
            {
                position.Row = this.Row - 1;
                position.Column = this.Column - 1;
            }
            else if (direction == CardinalDirection.Northeast)
            {
                position.Row = this.Row + 1;
                position.Column = this.Column - 1;
            }
            else if (direction == CardinalDirection.Southeast)
            {
                position.Row = this.Row + 1;
                position.Column = this.Column + 1;
            }
            else if (direction == CardinalDirection.Southwest)
            {
                position.Row = this.Row - 1;
                position.Column = this.Column + 1;
            }
            return position;
        }

    }
}
