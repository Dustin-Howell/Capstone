﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public class CreeperCore
    {
        public CreeperBoard Board { get; private set; }

        public CreeperCore()
        {
            Board = new CreeperBoard();
        }
    }
}
