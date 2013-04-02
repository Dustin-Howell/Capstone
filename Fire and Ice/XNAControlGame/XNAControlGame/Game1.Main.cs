using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreeperMessages;
using Creeper;

namespace XNAControlGame
{
    public partial class Game1
    {
        public static void Main()
        {
            using (Game1 game = new Game1())
            {
                game._eventAggregator.Publish(new MoveMessage()
                {
                    Board = game.BoardProvider.GetBoard(),
                    TurnColor = CreeperColor.Fire,
                    Type = MoveMessageType.Request,
                    PlayerType = PlayerType.Human,
                });

                game.IsMouseVisible = false;

                game.Run();
            }
        }
    }
}
