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
    }
}
