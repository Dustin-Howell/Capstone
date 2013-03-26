using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperMessages
{
    public enum GameOverType { Win, Draw, Forfeit, Disconnect, IllegalMove }
    public class GameOverMessage
    {
        public GameOverType GameOverType { get; set; }
        public Player Sender { get; set; }

        public GameOverMessage(GameOverType gameOverType, Player sender)
        {
            GameOverType = gameOverType;
            Sender = sender;
        }
    }
}
