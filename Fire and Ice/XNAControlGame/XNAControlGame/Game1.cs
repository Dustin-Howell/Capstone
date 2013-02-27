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
using Creeper;
using System.Collections.Generic;
using System.IO;

namespace XNAControlGame
{
    public class Game1 : XNAControl.XNAControlGame
    {

        //may be used in move and animation
        private Position _startPosition = new Position(-1, -1);
        private Position _endPostion = new Position(-1, -1);
        private String _selectedPeg;
        Scene _scene;
        Input _input;

        Nine.Graphics.Primitives.Plane _ground;
        

        //Used For Testing, Can Delete for the final project.
        SpriteFont _font;

        //Holds the Status of the board.
        public CreeperBoard Board { get; set; }

        //Keeps track of which click is happening.
        bool _secondClick = false;

        //Keeps track of whose turn it is
        public CreeperColor PlayerTurn { get { return TurnTracker.CurrentPlayer.Color; } }
        //Tile Textures
        Texture2D _blankTile;
        Texture2D _whiteTile;
        Texture2D _blackTile;
        Texture2D _board;

        //Allows access to clicking on the board only when it's supposed to be accessed.
        private bool _iStillNeedToMakeAMove = false;

        //The Move to be returned to the Creeper Core
        public Move LastMoveMade { get; private set; }

        //List of possible moves so that highlighting is possible
        List<Move> possible = new List<Move>();


        List<Animation> animation = new List<Animation>();
        List<Animation> finishedAnimation = new List<Animation>();

        public event EventHandler<MoveEventArgs> UserMadeMove;

        public void OnMoveMade(Move move)
        {
            //Sets the start and end value
            var first = _scene.FindName<Nine.Graphics.Model>("p" + move.StartPosition.Row.ToString() + "x" + move.StartPosition.Column.ToString());
            var second = _scene.FindName<Nine.Graphics.Model>("i" + move.EndPosition.Row.ToString() + "x" + move.EndPosition.Column.ToString());

            Animation animate = new Animation(first.Transform.Translation, second.Transform.Translation, first, (move.EndPosition.Column - move.StartPosition.Column), -(move.EndPosition.Row - move.StartPosition.Row), move.StartPosition, move.EndPosition);

            animation.Add(animate);

            _startPosition = new Position(-1, -1);
            _endPostion = new Position(-1, -1);

        }

        /// <summary>
        /// Convert a peg number a board position (row and column). Specialized for the GUI.
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
        /// Constructor for Game1.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Game1(IntPtr handle, int width, int height)
            : base(handle, "Content", width, height)
        {
            Content = new ContentLoader(Services);

            Components.Add(new InputComponent(handle));
            _input = new Input();

            _input.MouseDown += new EventHandler<Nine.MouseEventArgs>(Input_MouseDown);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load the peg model
            Microsoft.Xna.Framework.Graphics.Model pegModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(Resources.Models.PegModel);

            // Load the Tile sprite.
            Sprite tile = new Sprite(GraphicsDevice);

            //Loads in the Textures
            _board = Content.Load<Texture2D>(Resources.Textures.GameBoard);
            _blankTile = Content.Load<Texture2D>(Resources.Textures.UncapturedTile);
            _whiteTile = Content.Load<Texture2D>(Resources.Textures.WhiteTile);
            _blackTile = Content.Load<Texture2D>(Resources.Textures.BlackTile);

            // Load a scene from a content file
            _scene = Content.Load<Scene>(Resources.Scenes.MainPlayScene);

            
            //Find all of the dimensions of the board to determine where the peg models need to be placed in relation to the middle of the board.
            float boardHeight, boardWidth, squareWidth, squareHeight;
            boardHeight = _scene.Find<Surface>().Heightmap.Height;
            boardWidth = _scene.Find<Surface>().Heightmap.Width;
            squareHeight = (boardHeight / CreeperBoard.TileRows) * _scene.Find<Surface>().Heightmap.Step;
            squareWidth = (boardWidth / CreeperBoard.TileRows) * _scene.Find<Surface>().Heightmap.Step;
            Vector3 startCoordinates = new Vector3(0, 0, 0);

            Position pegPosition;

            _font = Content.Load<SpriteFont>("defaultFont");
            //Create a Nine Model peg with a XNA model, set its properties, and place it in its correct location relative to the middle of the board.
            //Do this for ever possible peg location on the board.
            for (int pegNumber = 1; pegNumber < 46; pegNumber++)
            {
                BasicMaterial defaultMaterial = new BasicMaterial(GraphicsDevice);
                defaultMaterial.Texture = Content.Load<Texture2D>(Resources.Textures.Default);
                pegPosition = NumberToPosition(pegNumber);
                String pegName = 'p' + pegPosition.Row.ToString() + 'x' + pegPosition.Column.ToString();
                String iPegName = 'i' + pegPosition.Row.ToString() + 'x' + pegPosition.Column.ToString();
                Vector3 pegCoordinates = new Vector3(startCoordinates.X + squareWidth * pegPosition.Column, 0, startCoordinates.Y + squareHeight * pegPosition.Row);
                _scene.Add(new Nine.Graphics.Model(pegModel) { Transform = Matrix.CreateScale(Resources.Models.PegScale) * Matrix.CreateTranslation(pegCoordinates), Name = pegName, Material = defaultMaterial });
                _scene.Add(new Nine.Graphics.Model(pegModel) { Transform = Matrix.CreateScale(Resources.Models.PegScale) * Matrix.CreateTranslation(pegCoordinates), Name = iPegName, Visible = false });
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
        private void Input_MouseDown(object sender, Nine.MouseEventArgs e)
        {
            //Create a ray fired from the point of click point.
            Ray pickRay = GetSelectionRay(new Vector2(e.X, e.Y));
            float maxDistance = float.MaxValue;

            //Test all 45 locations to see if one was click. If one was, determine which one was clicked.
            Position pegLocation;
            bool intersectionNotFound = true;
            for (int pegNum = 1; pegNum <= 45 && intersectionNotFound; pegNum++)
            {
                pegLocation = NumberToPosition(pegNum);
                String currentPeg = 'p' + pegLocation.Row.ToString() + 'x' + pegLocation.Column.ToString();
                BoundingBox modelIntersect = new BoundingBox(_scene.FindName<Nine.Graphics.Model>(currentPeg).BoundingBox.Min,
                    _scene.FindName<Nine.Graphics.Model>(currentPeg).BoundingBox.Max);
                Nullable<float> intersect = pickRay.Intersects(modelIntersect);

                //Selection Logic

                //If a model was selected
                if (intersect.HasValue == true)
                {
                    intersectionNotFound = false;

                    if (intersect.Value < maxDistance)
                    {
                        //And if the peg to move has not been selected or the peg clicked matches the current turn
                        if (!_secondClick || Board.Pegs.At(pegLocation).Color == PlayerTurn)
                        {
                            _selectedPeg = currentPeg;
                            _startPosition = new Position(Convert.ToInt32(currentPeg[1] - '0'), Convert.ToInt32(currentPeg[3] - '0'));
                            _secondClick = true;
                            if (Board.Pegs.At(pegLocation).Color == PlayerTurn)
                            {
                                possible = CreeperUtility.PossibleMoves(Board.Pegs.At(_startPosition), Board).ToList();
                            }
                        }
                        //Otherwise the end point of the move is being selected
                        else
                        {
                            //Check to see if the location being selected is an empty peg location. It must be so to be moved to.
                            if (Board.Pegs.At(pegLocation).Color == CreeperColor.Empty && _secondClick)
                            {
                                _endPostion = new Position(Convert.ToInt32(currentPeg[1] - '0'), Convert.ToInt32(currentPeg[3] - '0'));
                            }
                            //If it isn't, deselect the peg
                            else
                            {
                                _startPosition = _endPostion = new Position(-1, -1);
                            }
                            _secondClick = false;
                            possible.Clear();
                            
                        }
                    }
                }

                //If both the start and end position have been determined, send the move to the board and reset start and end.
                if (_endPostion.Row != -1 && _selectedPeg != null)
                {
                    Move move = new Move(_startPosition, _endPostion, PlayerTurn);

                    if (_scene.FindName<Nine.Graphics.Model>(_selectedPeg).Visible == true)
                    {
                        if (Board.IsValidMove(move))
                        {
                            LastMoveMade = move;
                            _iStillNeedToMakeAMove = false;

                            if (UserMadeMove != null)
                            {
                                UserMadeMove(this, new MoveEventArgs(move));
                            }
                        }
                        else
                        {
                            _startPosition = new Position(-1, -1);
                            _endPostion = new Position(-1, -1);
                        }
                    }

                    _selectedPeg = null;
                    _secondClick = false;
                    possible.Clear();
                }
            }
        }

        /// <summary>
        /// Returns a ray fired from the click point to test for intersection with a model.
        /// </summary>
        Ray GetSelectionRay(Vector2 mouseCoor)
        {
            Vector3 nearsource = new Vector3(mouseCoor, 0f);
            Vector3 farsource = new Vector3(mouseCoor, 1f);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearsource, _scene.FindName<FreeCamera>("MainCamera").Projection,
                    _scene.FindName<FreeCamera>("MainCamera").View, world);

            Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farsource, _scene.FindName<FreeCamera>("MainCamera").Projection,
                    _scene.FindName<FreeCamera>("MainCamera").View, world);

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
            if (animation.Count != 0 && animation != null)
            {
                foreach (Animation animate in animation)
                {
                    animate.peg.Transform *= Matrix.CreateTranslation((animate.xDirection * (_board.Height / CreeperBoard.TileRows) / 50),
                            0, -(animate.yDirection * (_board.Width / CreeperBoard.TileRows) / 50));

                    if (animate.peg.Contains(animate.endLocation))
                    {
                        finishedAnimation.Add(animate);
                    }

                }

                if (finishedAnimation.Count != 0)
                {
                    foreach (Animation animate in finishedAnimation)
                    {
                        animate.peg.Visible = false;
                        animate.peg.Transform = Matrix.CreateScale(Resources.Models.PegScale) * Matrix.CreateTranslation(animate.startLocation);
                        if (animation.Contains(animate))
                        {
                            animation.Remove(animate);
                        }
                    }
                }
                finishedAnimation.Clear();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            DrawBoard();

            _scene.Draw(GraphicsDevice, gameTime.ElapsedGameTime);
            base.Draw(gameTime);
        }

        /// <summary>
        /// Determine what the display properties of each board location should and set them.
        /// </summary>
        private void DrawBoard()
        {
            string location;

            BasicMaterial black = new BasicMaterial(GraphicsDevice);
            black.DiffuseColor = new Vector3(0, 0, 0);
            BasicMaterial white = new BasicMaterial(GraphicsDevice);
            white.DiffuseColor = new Vector3(255, 255, 255);
            BasicMaterial yellow = new BasicMaterial(GraphicsDevice);
            yellow.DiffuseColor = new Vector3(255, 255, 0);
            BasicMaterial blue = new BasicMaterial(GraphicsDevice);
            blue.DiffuseColor = new Vector3(0, 0, 255);

            if (animation.Count == 0)
            {
                for (int r = 0; r < CreeperBoard.PegRows; r++)
                {
                    for (int c = 0; c < CreeperBoard.PegRows; c++)
                    {
                        location = 'p' + r.ToString() + 'x' + c.ToString();

                        if (Board.Pegs.At(new Position(r, c)).Color == CreeperColor.White)
                        {
                            _scene.FindName<Nine.Graphics.Model>(location).Visible = true;
                            _scene.FindName<Nine.Graphics.Model>(location).Material = white;
                        }
                        else if (Board.Pegs.At(new Position(r, c)).Color == CreeperColor.Black)
                        {
                            _scene.FindName<Nine.Graphics.Model>(location).Visible = true;
                            _scene.FindName<Nine.Graphics.Model>(location).Material = black;
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


                foreach (Move move in possible)
                {
                    if (move.EndPosition.Row != null)
                    {
                        location = 'p' + move.EndPosition.Row.ToString() + 'x' + move.EndPosition.Column.ToString();
                        _scene.FindName<Nine.Graphics.Model>(location).Visible = true;
                        _scene.FindName<Nine.Graphics.Model>(location).Material = yellow;
                    }
                }

                for (int r = 0; r < CreeperBoard.TileRows; r++)
                {
                    for (int c = 0; c < CreeperBoard.TileRows; c++)
                    {
                        location = 't' + r.ToString() + 'x' + c.ToString();

                        if (Board.Tiles.At(new Position(r, c)).Color == CreeperColor.White)
                        {
                            _scene.FindName<Sprite>(location).Texture = _whiteTile;
                        }
                        else if (Board.Tiles.At(new Position(r, c)).Color == CreeperColor.Black)
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
            if (_selectedPeg != null && !_iStillNeedToMakeAMove)
            {
                _scene.FindName<Nine.Graphics.Model>(_selectedPeg).Material = blue;
            }
        }

    }
}