using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperMessages
{
    public class StartGameMessage
    {
        public PlayerType Player1Type { get; set; }
        public PlayerType Player2Type { get; set; }
        public AIDifficulty AIDifficulty { get; set; }
    }
}
