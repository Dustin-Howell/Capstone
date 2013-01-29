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
        protected List<Button> PegButtons { get; set; }
        public InGameScreen(Game1 game, SpriteBatch spriteBatch) : base(game, spriteBatch) 
        {
            PegButtons = new List<Button>();

            foreach (List<Peg> row in Game.Board.Pegs)
            {
                foreach (Peg peg in row)
                {
                }
            }
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            _spriteBatch.Begin();
            foreach (List<Tile> row in Game.Board.Tiles)
            {
                foreach (Tile tile in row)
                {
                }
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
