using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Creeper;

namespace XNAControlGame
{
    public class InGameScreen : GameScreen
    {
        protected List<PieceButton> PegButtons { get; set; }
        public InGameScreen(Game1 game, SpriteBatch spriteBatch, CreeperBoard board) : base(game, spriteBatch) 
        {
            PegButtons = new List<PieceButton>();
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
