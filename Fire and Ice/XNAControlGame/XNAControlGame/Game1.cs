using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nine;
using Nine.Components;
using Nine.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.ParticleEffects;
using Nine.Graphics.PostEffects;
using Nine.Graphics.Primitives;
using Nine.Physics;
using System.Windows.Forms;
using Creeper;

namespace XNAControlGame
{
    public class Game1 : XNAControl.XNAControlGame
    {
        Position _startPosition = new Position(-1, -1);
        Position _endPostion = new Position(-1, -1);
        Scene _scene;
        Input _input;
        SpriteFont _font;
        Matrix _cameraView;
        Matrix _cameraProj;
        Panel GamePanel { get; set; }
        CreeperBoard board = new CreeperBoard();
        bool SecondClick = false;
        CreeperColor PlayerTurn = CreeperColor.White;
        Texture2D _blankTile;
        Texture2D _whiteTile;
        Texture2D _blackTile;


        public Game1(IntPtr handle, Panel gamePanel, int width, int height) : base(handle, "Content", width, height)
        {
            Content = new ContentLoader(Services);
            GamePanel = gamePanel;


            GamePanel.MouseClick += new MouseEventHandler(Input_MouseDown);
        }

        /// <summary>
        /// Convert a peg number a board position (row and column).
        /// </summary>
        static public Position NumberToPosition(int number)
        {
            Position position = new Position();

            if (number >= CreeperBoard.PegRows - 1)
            {
                number++;
            }

            if (number >= (CreeperBoard.PegRows - 1) * CreeperBoard.PegRows)
            {
                number++;
            }

            position.Row = (int)number / CreeperBoard.PegRows;
            position.Column = number % CreeperBoard.PegRows;
           
            return position;
        }

        /// <summary>
        /// Determine what the display properties of each board location should and set them.
        /// </summary>
        private void DrawBoard()
        {
            string location;
            
            for (int r = 0; r < CreeperBoard.PegRows; r++)
            {
                for (int c = 0; c < CreeperBoard.PegRows; c++)
                {
                    location = 'p' + r.ToString() + 'x' + c.ToString();

                    if(board.Pegs.At(new Position(r,c)).Color == CreeperColor.White)
                    {
                        _scene.FindName<Nine.Graphics.Model>(location).Visible = true;
                    }
                    else if (board.Pegs.At(new Position(r, c)).Color == CreeperColor.Black)
                    {
                        _scene.FindName<Nine.Graphics.Model>(location).Visible = true;
                    }
                    else
                    {
                        if (_scene.FindName<Nine.Graphics.Model>(location) != null)
                        {
                            _scene.FindName<Nine.Graphics.Model>(location).Visible = false;
                        }
                    }
                }
            }

            for (int r = 0; r < CreeperBoard.TileRows; r++)
            {
                for (int c = 0; c < CreeperBoard.TileRows; c++)
                {
                    location = 't' + r.ToString() + 'x' + c.ToString();

                    if (board.Tiles.At(new Position(r, c)).Color == CreeperColor.White)
                    {
                        _scene.FindName<Sprite>(location).Texture = _whiteTile;
                    }
                    else if (board.Tiles.At(new Position(r, c)).Color == CreeperColor.Black)
                    {
                        _scene.FindName<Sprite>(location).Texture = _blackTile;
                    }
                    else
                    {
                        if (_scene.FindName<Sprite>(location) != null)
                        {
                            _scene.FindName<Sprite>(location).Texture = _blankTile;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load the peg model
            Microsoft.Xna.Framework.Graphics.Model pegModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>("Model/tank");
            // Load the Tile sprite.
            Sprite tile = new Sprite(GraphicsDevice);
            
            _blankTile = Content.Load<Texture2D>("square");
            _whiteTile = Content.Load<Texture2D>("whiteTile");
            _blackTile = Content.Load<Texture2D>("blackTile");

            // Load a scene from a content file
            _scene = Content.Load<Scene>("Scene1");

            //Find all of the dimensions of the board to determine where the peg models need to be placed in relation to the middle of the board.
            float boardHeight, boardWidth, squareWidth, squareHeight;
            boardHeight = _scene.FindName<Sprite>("boardImage").Texture.Height;
            boardWidth = _scene.FindName<Sprite>("boardImage").Texture.Width;
            squareWidth = boardWidth /6;
            squareHeight = boardHeight / 6;
            Vector3 startCoordinates = new Vector3(-boardWidth / 2, boardHeight / 2, 0);
            Position pegPosition;

            _font = Content.Load<SpriteFont>("defaultFont");
            //Create a Nine Model peg with a XNA model, set its properties, and place it in its correct location relative to the middle of the board.
            //Do this for ever possible peg location on the board.
            for (int pegNumber = 1; pegNumber < 46; pegNumber++)
            {
                pegPosition = NumberToPosition(pegNumber);
                String pegName = 'p' + pegPosition.Row.ToString() + 'x' + pegPosition.Column.ToString();
                Vector3 pegCoordinates = new Vector3(startCoordinates.X + squareWidth * pegPosition.Column, startCoordinates.Y - squareHeight * pegPosition.Row, 0);
                _scene.Add(new Nine.Graphics.Model(pegModel) { Transform = Matrix.CreateScale(.02f, .02f, .02f) * Matrix.CreateTranslation(pegCoordinates), Name = pegName});
            }
            //Place a transparent sprite for tiles in every possible tile position.
            startCoordinates += new Vector3(squareWidth / 2, -(squareHeight / 2), 0);
            for (int tileNumber = 0; tileNumber < 35; tileNumber++)
            {
                Position tilePosition = CreeperUtility.NumberToPosition(tileNumber);
                String tileName = 't' + tilePosition.Row.ToString() + 'x' + tilePosition.Column.ToString();
                tile.Position = new Vector2(startCoordinates.X + squareWidth * tilePosition.Column, startCoordinates.Y - squareHeight * tilePosition.Row);
                tile.ZOrder = 1;
                tile.Name = tileName;
                tile.Texture = _blankTile;
                _scene.Add(tile);
                tile = new Sprite(GraphicsDevice);
            }


            
            base.LoadContent();
        }

        /// <summary>
        /// Handle mouse input events
        /// </summary>
        private void Input_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            
            //Create a ray fired from the point of click point.
            Ray pickRay = GetSelectionRay( new Vector2( e.X, e.Y ) );
            float maxDistance = float.MaxValue;

            //Test all 45 locations to see if one was click. If one was, determine which one was clicked.
            String selectedPeg = "";
            Position pegLocation;
            for (int pegNum = 1; pegNum <= 45; pegNum++)
            {
                pegLocation = NumberToPosition(pegNum);
                String currentPeg = 'p' + pegLocation.Row.ToString() + 'x' + pegLocation.Column.ToString();
                BoundingBox modelIntersect = new BoundingBox(_scene.FindName<Nine.Graphics.Model>(currentPeg).BoundingBox.Min,
                    _scene.FindName<Nine.Graphics.Model>(currentPeg).BoundingBox.Max);
                Nullable<float> intersect = pickRay.Intersects(modelIntersect);

                //Calculates a move
                if (!SecondClick)
                {
                    if (intersect.HasValue == true)
                    {
                        if (intersect.Value < maxDistance)
                        {
                            selectedPeg = currentPeg;
                            _startPosition = new Position(Convert.ToInt32(currentPeg[1] -'0'), Convert.ToInt32(currentPeg[3]-'0'));
                            SecondClick = true;
                        }
                    }
                }
                else
                {
                    if (intersect.HasValue == true)
                    {
                        if (intersect.Value < maxDistance)
                        {
                            //selectedPeg = currentPeg;
                            _endPostion = new Position(Convert.ToInt32(currentPeg[1] -'0'), Convert.ToInt32(currentPeg[3] -'0'));
                        }
                        else
                        {
                            _startPosition = new Position(-1,-1);
                            
                        }
                        SecondClick = false;
                    }
                }

                selectedPeg = 'p' + _startPosition.Row.ToString() + 'x' + _startPosition.Column.ToString();
                if (_endPostion.Row != -1)
                {

                    Move move = new Move (_startPosition,_endPostion, PlayerTurn);

                    if (_scene.FindName<Nine.Graphics.Model>(selectedPeg).Visible == true)
                    {
                        if (board.Move(move))
                        {
                            if (PlayerTurn == CreeperColor.White)
                            {
                                PlayerTurn = CreeperColor.Black;
                            }
                            else
                            {
                                PlayerTurn = CreeperColor.White;
                            }
                        }
                    }

                    _startPosition = new Position(-1, -1);
                    _endPostion = new Position(-1, -1);

                }
            }
        }

        /// <summary>
        /// Returns a ray fired from the click point to test for intersection with a model.
        /// </summary>
        Ray GetSelectionRay( Vector2 mouseCoor )
        {
            Vector3 nearsource = new Vector3(mouseCoor, 0f);
            Vector3 farsource = new Vector3(mouseCoor, 1f);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearsource, _cameraProj, _cameraView, world);

            Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farsource, _cameraProj, _cameraView, world);

            // Create a ray from the near clip plane to the far clip plane.
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            Ray pickRay = new Ray(nearPoint, direction);

            return pickRay;
        }

        /// <summary>
        /// This is called when the game should update itself.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            _scene.Update(gameTime.ElapsedGameTime);
            _scene.UpdatePhysicsAsync(gameTime.ElapsedGameTime);
            _cameraView = _scene.Find<FreeCamera>().View;
            _cameraProj = _scene.Find<FreeCamera>().Projection;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            _scene.Draw(GraphicsDevice, gameTime.ElapsedGameTime);
            DrawBoard();



            //Test String drawing
            SpriteBatch spritebatch = new SpriteBatch(GraphicsDevice);
            spritebatch.Begin();
            spritebatch.DrawString(_font, "Player Turn = " + PlayerTurn.ToString(), new Vector2(0, 0), Color.Black);

            spritebatch.End();
                
            base.Draw(gameTime);
        }
    }
}