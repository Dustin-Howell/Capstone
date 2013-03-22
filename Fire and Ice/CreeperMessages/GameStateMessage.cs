using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreeperMessages
{
    enum GameState { End, Quit }
    public class GameStateMessage
    {
        public GameState GameState { get; set; }

        public GameStateMessage(GameState newState)
        {
            GameState = newState;
        }
    }
}
