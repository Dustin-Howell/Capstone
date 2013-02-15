using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace XNAControlGame
{
    class CreeperBoardViewModel
    {
        public CreeperBoard Board { get; private set; }

        // This should map abstract piece positions to spatial positions via array indices.
        public Position[,] GraphicalPositions { get; private set; }

        CreeperBoardViewModel(CreeperBoard board)
        {
            throw new NotImplementedException(this.ToString());
        }
    }
}
