using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace XNAControlGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : XNAControl.XNAControlGame
    {
        SpriteBatch spriteBatch;

        public bool Pause { get; set; }
        public void PauseWithMethodCall()
        {
            Pause = !Pause;
        }

        Texture2D square;
        Vector2 squarePosition;
        Vector2 squareSize;
        Rectangle squareRectangle
        {
            get
            {
                return new Rectangle((int)squarePosition.X, (int)squarePosition.Y, square.Height, square.Width);
            }
        }


        public int SpeedX { get; set; }
        public int SpeedY { get; set; }

        public void SpeedUp()
        {
            SpeedX += (SpeedX >= 0)? 3: -3;
            SpeedY += (SpeedY >= 0)? 3: -3;
        }

        public void SlowDown()
        {
            SpeedX -= (SpeedX >= 0) ? 3 : -3;
            SpeedY -= (SpeedY >= 0) ? 3 : -3;
        }

        public Game1(IntPtr handle) : base(handle, "Content")
        {
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            squarePosition = new Vector2(0, 0);
            Pause = false;
            SpeedX = 1;
            SpeedY = 1;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            square = Content.Load<Texture2D>("square");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (!Pause)
            {
                squarePosition.X += SpeedX;
                squarePosition.Y += SpeedY;
            }

            if (squarePosition.X <= 0
                || squarePosition.X >= GraphicsDevice.Viewport.Width - square.Width)
            {
                SpeedX *= -1;
            }
            if (squarePosition.Y <= 0
                || squarePosition.Y >= GraphicsDevice.Viewport.Height - square.Height)
            {
                SpeedY *= -1;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(square, squareRectangle, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
