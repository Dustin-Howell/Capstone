using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControlGame
{
    public class InGameScreen : GameScreen
    {
        public InGameScreen(Game1 game, SpriteBatch spriteBatch) : base(game, spriteBatch) { }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(Game.DefaultFont, "In Game", new Vector2(100, 100), Color.White);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
