using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace XNAControlGame
{
    public class BouncySquare : DrawableGameComponent
    {
        public static Random Random = new Random();

        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Texture.Height, Texture.Width);
            }
        }

        public int SpeedX { get; set; }
        public int SpeedY { get; set; }
        
        public void SpeedUp()
        {
            SpeedX += (SpeedX >= 0) ? 3 : -3;
            SpeedY += (SpeedY >= 0) ? 3 : -3;
        }

        public void SlowDown()
        {
            SpeedX -= (SpeedX >= 0) ? 3 : -3;
            SpeedY -= (SpeedY >= 0) ? 3 : -3;
        }

        public BouncySquare(Game game) : base(game) { }

        public override void Initialize()
        {
            base.Initialize();

            Position = new Vector2(0, 0);
            SpeedX = Random.Next() % 10;
            SpeedY = Random.Next() % 10;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!((Game1)Game).Pause)
            {
                Position = new Vector2(Position.X + SpeedX, Position.Y + SpeedY);
            }

            if (Position.X <= 0
                || Position.X >= GraphicsDevice.Viewport.Width - Texture.Width)
            {
                SpeedX *= -1;
            }
            if (Position.Y <= 0
                || Position.Y >= GraphicsDevice.Viewport.Height - Texture.Height)
            {
                SpeedY *= -1;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch spriteBatch = ((Game1)Game).spriteBatch;

            spriteBatch.Begin();
            spriteBatch.Draw(Texture, Rectangle, Color.White);
            spriteBatch.End();

        }
    }
}
