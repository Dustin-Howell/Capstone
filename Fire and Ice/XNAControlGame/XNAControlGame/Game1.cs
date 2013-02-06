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
using Creeper;

namespace XNAControlGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        double row = 0;
        double column = 0;
        MouseState previousMouseState = new MouseState();
        MouseState mouseState = new MouseState();
        Texture2D boardImage;
        Texture2D blackPeg;
        Texture2D whitePeg;
        Position start = new Position(-1,-1);
        Position end = new Position(-1,-1);
        CreeperBoard board = new CreeperBoard();
        CreeperColor turn;

        private void drawBoard()
        {
            for (int r = 0; r < CreeperBoard.PegRows; r++)
            {
                for (int c = 0; c < CreeperBoard.PegRows; c++)
                {
                    if (board.Pegs.At(new Position(r, c)).Color == CreeperColor.White)
                    {
                        spriteBatch.Draw(whitePeg,
                            new Rectangle(r * (boardImage.Width / CreeperBoard.PegRows), c * (boardImage.Height / CreeperBoard.PegRows), whitePeg.Width, whitePeg.Height),
                            Color.White);
                    }
                    if (board.Pegs.At(new Position(r, c)).Color == CreeperColor.Black)
                    {
                        spriteBatch.Draw(blackPeg,
                            new Rectangle(r * (boardImage.Width / CreeperBoard.PegRows), c * (boardImage.Height / CreeperBoard.PegRows), blackPeg.Width, blackPeg.Height),
                            Color.Black);
                    }
                }
            }
        }

        Move move;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        
        
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            turn = CreeperColor.White;
            IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            boardImage = this.Content.Load<Texture2D>("board");
            blackPeg = this.Content.Load<Texture2D>("smallBlackPeg");
            whitePeg = this.Content.Load<Texture2D>("smallWhitePeg");
            font = Content.Load<SpriteFont>("defaultFont");
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

            KeyboardState keystate = Keyboard.GetState();

            if (keystate.IsKeyDown(Keys.Escape))
            {
                graphics.PreferredBackBufferWidth = 640;
                graphics.PreferredBackBufferHeight = 360;
                graphics.ToggleFullScreen();
                graphics.ApplyChanges();
            }

            mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed &&
                previousMouseState.LeftButton == ButtonState.Released)
            {
                row = mouseState.X / (boardImage.Width / 7);
                column = mouseState.Y / (boardImage.Height / 7);
                row = Math.Round(row);
                column = Math.Round(column);
                
                if (board.IsValidPosition(new Position((int)row,(int)column),PieceType.Peg))
                {
                    if (start.Row == -1)
                    {
                        start.Row = (int)row;
                        start.Column = (int)column;
                    }
                    else if (end.Row == -1)
                    {
                        end.Row = (int)row;
                        end.Column = (int)column;

                    }
                }
            }

            previousMouseState = mouseState;

            if (start.Row != -1 && end.Row != -1)
            {
                //last paramiter needs to be changed for sake of change
                move = new Move(start, end, turn); 
                board.Move(move);
                if (turn == CreeperColor.White)
                {
                    turn = CreeperColor.Black;
                }
                else
                {
                    turn = CreeperColor.White;
                }
                Console.WriteLine("Start: " + start.Row + "," + start.Column);
                Console.WriteLine("End: " + end.Row + "," + end.Column);
                start = new Position(-1, -1);
                end = new Position(-1, -1);
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

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.DrawString(font, "Start: " + start.Row + "," + start.Column, new Vector2(400, 100), Color.Black);
            spriteBatch.DrawString(font, "End: " + end.Row + "," + end.Column, new Vector2(400, 300), Color.Black);
            spriteBatch.Draw(boardImage, new Rectangle(-10, 0, boardImage.Width, boardImage.Height), Color.White);
            drawBoard();
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
