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
    public class Game1 : XNAControl.XNAControlGame
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
        Texture2D whiteTile;
        Texture2D blackTile;
        Position start = new Position(-1,-1);
        Position end = new Position(-1,-1);
        CreeperBoard board = new CreeperBoard();
        CreeperColor turn;

        public Game1(IntPtr handle, int width, int height) : base( handle, "Content", width, height ) {}

        private void drawBoard()
        {
            //Print Pegs
            for (int r = 0; r < CreeperBoard.PegRows; r++)
            {
                for (int c = 0; c < CreeperBoard.PegRows; c++)
                {
                    if (board.Pegs.At(new Position(r, c)).Color == CreeperColor.White)
                    {
                        spriteBatch.Draw(whitePeg,
                            new Rectangle((c * (boardImage.Width / (CreeperBoard.PegRows - 1)))-whitePeg.Width/2, (r * (boardImage.Height / (CreeperBoard.PegRows - 1))) - whitePeg.Height/2, whitePeg.Width, whitePeg.Height),
                            Color.White);
                    }
                    if (board.Pegs.At(new Position(r, c)).Color == CreeperColor.Black)
                    {
                        spriteBatch.Draw(blackPeg,
                            new Rectangle((c * (boardImage.Width / (CreeperBoard.PegRows - 1))) - blackPeg.Width/2, (r * (boardImage.Height / (CreeperBoard.PegRows -1))) -blackPeg.Height/2, blackPeg.Width, blackPeg.Height),
                            Color.Black);
                    }
                }
            }
            //Print Tiles
            for (int r = 0; r < CreeperBoard.TileRows; r++)
            {
                for (int c = 0; c < CreeperBoard.TileRows; c++)
                {
                    if (board.Tiles.At(new Position(r, c)).Color == CreeperColor.White)
                    {
                        spriteBatch.Draw(whiteTile,
                            new Rectangle((((boardImage.Width / CreeperBoard.TileRows) * c) + ((boardImage.Width / CreeperBoard.TileRows)/2) - whiteTile.Width / 2),
                                (((boardImage.Height / CreeperBoard.TileRows) * r) + ((boardImage.Height / CreeperBoard.TileRows) / 2) - whiteTile.Width / 2),
                                whiteTile.Width, whiteTile.Height),
                                Color.White);
                    }
                    if (board.Tiles.At(new Position(r, c)).Color == CreeperColor.Black)
                    {
                        spriteBatch.Draw(blackTile,
                            new Rectangle((((boardImage.Width / CreeperBoard.TileRows) * c) + ((boardImage.Width / CreeperBoard.TileRows) / 2) - blackTile.Width / 2),
                                (((boardImage.Height / CreeperBoard.TileRows) * r) + ((boardImage.Height / CreeperBoard.TileRows) / 2) - blackTile.Width / 2),
                                blackTile.Width, blackTile.Height),
                                Color.Black);
                    }
                }
            }
        }

        Move move;
        
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
            whiteTile = this.Content.Load<Texture2D>("whiteTile");
            blackTile = this.Content.Load<Texture2D>("blackTile");
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


            mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed &&
                previousMouseState.LeftButton == ButtonState.Released)
            {
                row = mouseState.Y / (boardImage.Width / CreeperBoard.PegRows - 1);
                column = mouseState.X / (boardImage.Height / CreeperBoard.PegRows - 1);
                row = Math.Round(row);
                column = Math.Round(column);

                //Peg Selection Logic (Start Position is Selection, End Position is Selection's Move)
                //A peg (start position) is selected when the user in a turn selects a peg the same color as his turn. This peg is then set as the start position.
                //The move (end position) is selected when a user selects a valid position that is empty.
                //If a user selects an invalid position (off board or corner), an invalid end position (invalid move), or the opposite color's peg, the selected peg (start
                //  position) is deselected (set back to (-1, -1)).
                //If a user selects a different peg the same color as his turn after already selecting a peg, the selection (start position) is changed to the new peg selection.
                if (board.IsValidPosition(new Position((int)row, (int)column), PieceType.Peg))
                {
                    if (board.Pegs.At(new Position((int)row, (int)column)).Color == turn)
                    {
                        start.Row = (int)row;
                        start.Column = (int)column;
                    }
                    else
                    {
                        if (start.Row != -1)
                        {
                            end.Row = (int)row;
                            end.Column = (int)column;
                        }
                    }
                }
                else
                {
                    start = new Position(-1, -1);
                    end = new Position(-1, -1);
                }
            }

            previousMouseState = mouseState;

            //Peg Selection Logic (Start Position is Selection, End Position is Selection's Move)
            //A peg (start position) is selected when the user in a turn selects a peg the same color as his turn. This peg is then set as the start position.
            //The move (end position) is selected when a user selects a valid position that is empty.
            //If a user selects an invalid position (off board or corner), an invalid end position (invalid move), or the opposite color's peg, the selected peg (start
            //  position) is deselected (set back to (-1, -1)).
            //If a user selects a different peg the same color as his turn after already selecting a peg, the selection (start position) is changed to the new peg selection.
            if (start.Row != -1 && end.Row != -1)
            {
                //last paramiter needs to be changed for sake of change
                move = new Move(start, end, turn); 
                if(board.Move(move))
                {
                    if (turn == CreeperColor.White)
                    {
                        turn = CreeperColor.Black;
                    }
                    else
                    {
                        turn = CreeperColor.White;
                    }
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
            spriteBatch.DrawString(font, "Board: Width = " + boardImage.Width + " Height = " + boardImage.Height, new Vector2(400, 50), Color.Black);
            spriteBatch.DrawString(font, "Start: " + start.Row + "," + start.Column, new Vector2(400, 100), Color.Black);
            spriteBatch.DrawString(font, "End: " + end.Row + "," + end.Column, new Vector2(400, 200), Color.Black);
            spriteBatch.DrawString(font, "Mouse At: " + Mouse.GetState().Y + "," + Mouse.GetState().X, new Vector2(400, 300), Color.Black);
            spriteBatch.DrawString(font, "Mouse At(r,c): " + Mouse.GetState().Y / (boardImage.Width / CreeperBoard.PegRows - 1) + "," + Mouse.GetState().X / (boardImage.Width / CreeperBoard.PegRows - 1), new Vector2(400, 350), Color.Black);
            spriteBatch.DrawString(font, "Players turn: " + turn.ToString(), new Vector2(400, 400), Color.Black);
            spriteBatch.Draw(boardImage, new Rectangle(0, 0, boardImage.Width, boardImage.Height), Color.White);
            drawBoard();
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
