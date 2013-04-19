using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperMessages
{
    public class SychronizeBoardMessage
    {
        public Action Callback { get; set; }
        private CreeperBoard _board;
        public CreeperBoard Board
        {
            get
            {
                return _board ?? new CreeperBoard();
            }
            set
            {
                _board = value;
            }
        }
    }
}
