using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControlGame
{
    public enum GameState { InGame, SplashScreen, StartMenu, PostGame, GameOver }

    public class GameScreen : DrawableGameComponent
    {
        protected bool _activated = false;
        protected SpriteBatch _spriteBatch;
        protected new Game1 Game { get; set; }

        public GameScreen(Game1 game, SpriteBatch spriteBatch)
            : base(game)
        {
            _spriteBatch = spriteBatch;
            Game = game;
        }

        public void Activate()
        {
            _activated = true;
            if (!Game.Components.Contains(this))
            {
                Game.Components.Add(this);
            }
        }

        public void Deactivate()
        {
            _activated = false;
            if (Game.Components.Contains(this))
            {
                Game.Components.Remove(this);
            }
        }
    }
}
