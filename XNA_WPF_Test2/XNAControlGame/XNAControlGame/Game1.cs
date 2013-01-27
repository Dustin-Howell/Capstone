//This is a test for git.
//Jon Scott Smith

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
        public SpriteBatch spriteBatch;
        List<BouncySquare> squares;

        const int SquareCount = 1000;

        public bool Pause { get; set; }
        public void PauseWithMethodCall()
        {
            Pause = !Pause;
        }

        public void SpeedUp()
        {
            foreach (BouncySquare square in squares)
            {
                square.SpeedUp();
            }
        }

        public void SlowDown()
        {
            foreach (BouncySquare square in squares)
            {
                square.SlowDown();
            }
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
            Pause = false;

            squares = new List<BouncySquare>();

            for (int i = 0; i < SquareCount; i++)
            {
                squares.Add(new BouncySquare(this));
                Components.Add(squares[i]);
            }

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
            Texture2D squareTexture = Content.Load<Texture2D>("square");
            foreach (BouncySquare square in squares)
            {
                square.Texture = squareTexture;
            }
            
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
            //spriteBatch.Draw(square, squareRectangle, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
