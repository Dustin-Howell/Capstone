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
using System.Windows.Forms;

namespace XNAControlGame
{
    public class Game1 : XNAControl.XNAControlGame
    {
        //may be used in move and animation
        private Position _startPosition;
        private Position _endPostion;
        private String _selectedPeg;
        Scene _scene;
        Input _input;

        Nine.Graphics.ParticleEffects.ParticleEffect particleEffect;

        Nine.Graphics.ParticleEffects.ParticleEffect fireOne;
        Nine.Graphics.ParticleEffects.ParticleEffect fireTwo;
        Nine.Graphics.ParticleEffects.ParticleEffect iceOne;
        Nine.Graphics.ParticleEffects.ParticleEffect iceTwo;

        bool _surroundObjects = false;

        //Used For Testing, Can Delete for the final project.
        SpriteFont _font;

        //Keeps track of which click is happening.
        bool _secondClick;

        //Keeps track of whose turn it is
        public CreeperColor PlayerTurn { get { return GameTracker.CurrentPlayer.Color; } }
        //Tile Textures
        Texture2D _blankTile;
        Texture2D _whiteTile;
        Texture2D _blackTile;
        Texture2D _board;
        Texture2D _iceCorner;
        Texture2D _fireCorner;

        Texture2D _firePeg;
        Texture2D _icePeg;

        Texture2D _fireHighlight;
        Texture2D _iceHighlight;

        Position fireStart = new Position(0, 0);
        Position iceStart = new Position(CreeperBoard.TileRows - 1, 0);
        Position fireEnd = new Position(CreeperBoard.TileRows - 1, CreeperBoard.TileRows - 1);
        Position iceEnd = new Position(0, CreeperBoard.TileRows - 1);

        //Allows access to clicking on the board only when it's supposed to be accessed.
        private bool _iStillNeedToMakeAMove = false;

        //The Move to be returned to the Creeper Core
        public Move LastMoveMade { get; private set; }

        //List of possible moves so that highlighting is possible
        List<Move> possible = new List<Move>();


        List<Animation> animation = new List<Animation>();
        List<Animation> finishedAnimation = new List<Animation>();

        public event EventHandler<MoveEventArgs> UserMadeMove;

        private static Game1 _instance;

        private PointEmitter makePointEmitter()
        {

            PointEmitter pointEmiter = new PointEmitter();
            pointEmiter.Emission = 50;

            Nine.Range<float> range = new Nine.Range<float>();
            CylinderEmitter cylinderEmitter = new CylinderEmitter();

            range.Min = 0.5f;
            range.Max = 3f;
            pointEmiter.Duration = range;

            range.Min = 1f;
            range.Max = 2f;
            pointEmiter.Speed = range;

            range.Min = 2f;
            range.Max = 5f;
            pointEmiter.Size = range;
            return pointEmiter;
        }
        private CylinderEmitter makeCylinderEmitter()
        {
            CylinderEmitter cylinderEmitter = new CylinderEmitter();
            Nine.Range<float> range = new Nine.Range<float>();

            cylinderEmitter.Emission = 50;
            cylinderEmitter.Radiate = true;
            cylinderEmitter.Shell = true;
            cylinderEmitter.Radius = 5;
            cylinderEmitter.Height = 0;

            range.Min = 0.5f;
            range.Max = 1.5f;
            cylinderEmitter.Duration = range;

            range.Min = 1f;
            range.Max = 2f;
            cylinderEmitter.Speed = range;

            range.Min = 2f;
            range.Max = 3f;
            cylinderEmitter.Size = range;

            return cylinderEmitter;
        }
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

        public static Game1 GetInstance(IntPtr handle, int width, int height)
        {
            if (_instance == null)
            {
                _instance = new Game1(handle, width, height);
            }
            else
            {
                _instance.UpdateWindowHandle(handle);
                _instance._input.MouseDown -= new EventHandler<Nine.MouseEventArgs>(_instance.Input_MouseDown);
            }

            
            _instance._startPosition = new Position(-1, -1);
            _instance._endPostion = new Position(-1, -1);
            _instance._secondClick = false;
            _instance._iStillNeedToMakeAMove = false;
            _instance.animation.Clear();
            _instance.finishedAnimation.Clear();
            _instance.possible.Clear();
            _instance.Components.Add(new InputComponent(handle));
            _instance._input = new Input();
            _instance._input.MouseDown += new EventHandler<Nine.MouseEventArgs>(_instance.Input_MouseDown);


            return _instance;
        }

        /// <summary>
        /// Constructor for Game1.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private Game1(IntPtr handle, int width, int height)
            : base(handle, "Content", width, height)
        {
            Content = new ContentLoader(Services);
        }
        private void RemoveParticleEffects()
        {
            particleEffect.Transform = Matrix.CreateTranslation(200, 200, 200);
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load the peg model
            Microsoft.Xna.Framework.Graphics.Model pegModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(Resources.Models.PegModel);
            fireOne = new Nine.Graphics.ParticleEffects.ParticleEffect(GraphicsDevice);
            fireTwo = new Nine.Graphics.ParticleEffects.ParticleEffect(GraphicsDevice);
            iceOne = new Nine.Graphics.ParticleEffects.ParticleEffect(GraphicsDevice);
            iceTwo = new Nine.Graphics.ParticleEffects.ParticleEffect(GraphicsDevice);

            particleEffect = new Nine.Graphics.ParticleEffects.ParticleEffect(GraphicsDevice);



            particleEffect.Emitter = makeCylinderEmitter();
            
            _fireHighlight = Content.Load<Texture2D>("Textures/fire");
            _iceHighlight = Content.Load<Texture2D>("Textures/flake");
            fireOne.Texture = _fireHighlight;
            fireTwo.Texture = _fireHighlight;
            iceOne.Texture = _iceHighlight;
            iceTwo.Texture = _iceHighlight;

            fireOne.Emitter = makePointEmitter();
            fireTwo.Emitter = makePointEmitter();
            iceOne.Emitter = makeCylinderEmitter();
            iceTwo.Emitter = makeCylinderEmitter();


            // Load the Tile surface.
            Surface tile = new Surface(GraphicsDevice, 2, 108 / CreeperBoard.TileRows, 108 / CreeperBoard.TileRows, 2);

            //Loads in the Textures
            _board = Content.Load<Texture2D>(Resources.Textures.GameBoard);
            _iceCorner = Content.Load<Texture2D>(Resources.Textures.IceCorner);
            _fireCorner = Content.Load<Texture2D>(Resources.Textures.FireCorner);

            _blankTile = Content.Load<Texture2D>(Resources.Textures.UncapturedTile);
            _whiteTile = Content.Load<Texture2D>(Resources.Textures.WhiteTile);
            _blackTile = Content.Load<Texture2D>(Resources.Textures.BlackTile);

            _firePeg = Content.Load<Texture2D>(Resources.Textures.FirePeg);
            _icePeg = Content.Load<Texture2D>(Resources.Textures.IcePeg);

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
            startCoordinates = new Vector3(0, 0, 0);
            for (int tileNumber = 0; tileNumber < 36; tileNumber++)
            {
                Position tilePosition = CreeperUtility.NumberToPosition(tileNumber);
                String tileName = 't' + tilePosition.Row.ToString() + 'x' + tilePosition.Column.ToString();
                tile.Position = new Vector3(startCoordinates.X + squareWidth * tilePosition.Column, 2, startCoordinates.Y + squareHeight * tilePosition.Row);
                tile.Name = tileName;
                BasicMaterial transparentTile = new BasicMaterial(GraphicsDevice);
                transparentTile.Texture = _blankTile;
                transparentTile.IsTransparent = true;
                tile.Material = transparentTile;
                tile.LevelOfDetailEnabled = false;
                _scene.Add(tile);
                tile = new Surface(GraphicsDevice, 2, 108 / CreeperBoard.TileRows, 108 / CreeperBoard.TileRows, 2);

                if (tileNumber == 0)
                {
                    fireOne.Transform = Matrix.CreateTranslation((startCoordinates.X + squareWidth * tilePosition.Column) + squareWidth/2, 5, (startCoordinates.Y + squareHeight * tilePosition.Row) + squareHeight/2);
                    _scene.Add(fireOne);
                }
                if (tileNumber == 35)
                {
                    fireTwo.Transform = Matrix.CreateTranslation((startCoordinates.X + squareWidth * tilePosition.Column) + squareWidth / 2, 5, (startCoordinates.Y + squareHeight * tilePosition.Row) + squareHeight / 2);
                    _scene.Add(fireTwo);
                }

                if (tileNumber == CreeperBoard.TileRows - 1)
                {
                    iceOne.Transform = Matrix.CreateTranslation((startCoordinates.X + squareWidth * tilePosition.Column) + squareWidth / 2, 5, (startCoordinates.Y + squareHeight * tilePosition.Row) + squareHeight / 2);
                    _scene.Add(iceOne);
                }
                if (tileNumber == 30)
                {
                    iceTwo.Transform = Matrix.CreateTranslation((startCoordinates.X + squareWidth * tilePosition.Column) + squareWidth / 2, 5, (startCoordinates.Y + squareHeight * tilePosition.Row) + squareHeight / 2);
                    _scene.Add(iceTwo);
                }
            }
            
            base.LoadContent();
        }

        /// <summary>
        /// Handle mouse input events
        /// </summary>
        private void Input_MouseDown(object sender, Nine.MouseEventArgs e)
        {
            
            _surroundObjects = !_surroundObjects;
            

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
                        //and the current player is a local human
                        if ((!_secondClick || GameTracker.Board.Pegs.At(pegLocation).Color == PlayerTurn)
                            && GameTracker.CurrentPlayer.PlayerType == PlayerType.Human)
                        {
                            _selectedPeg = currentPeg;
                            particleEffect.Transform = Matrix.CreateTranslation( _scene.FindName<Nine.Graphics.Model>(_selectedPeg).Transform.Translation.X,
                                _scene.FindName<Nine.Graphics.Model>(_selectedPeg).Transform.Translation.Y,
                                _scene.FindName<Nine.Graphics.Model>(_selectedPeg).Transform.Translation.Z);

                            if (GameTracker.CurrentPlayer.Color == CreeperColor.Ice)
                            {
                                particleEffect.Texture = _iceHighlight;
                            }
                            else
                            {
                                particleEffect.Texture = _fireHighlight;
                            }
                            if (!_scene.Contains(particleEffect))
                            { 
                                _scene.Add(particleEffect);
                            }
                            _startPosition = new Position(Convert.ToInt32(currentPeg[1] - '0'), Convert.ToInt32(currentPeg[3] - '0'));
                            _secondClick = true;
                            if (GameTracker.Board.Pegs.At(pegLocation).Color == PlayerTurn)
                            {
                                possible = CreeperUtility.PossibleMoves(GameTracker.Board.Pegs.At(_startPosition), GameTracker.Board).ToList();
                            }
                        }
                        //Otherwise the end point of the move is being selected
                        else
                        {
                            //Check to see if the location being selected is an empty peg location. It must be so to be moved to.
                            if (GameTracker.Board.Pegs.At(pegLocation).Color == CreeperColor.Empty && _secondClick)
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
                        if (GameTracker.Board.IsValidMove(move))
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
                    if (_scene.Contains(particleEffect))
                    {
                        particleEffect.Emitter = makeCylinderEmitter();
                        //_scene.Remove(particleEffect);
                        RemoveParticleEffects();
                        
                    }
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

                BasicMaterial blue = new BasicMaterial(GraphicsDevice);
                blue.DiffuseColor = new Vector3(.4f, .576f, .604f);

                BasicMaterial red = new BasicMaterial(GraphicsDevice);
                red.DiffuseColor = new Vector3(1, .46f, 0);

                foreach (Animation animate in animation)
                {
                    Position movingPeg = new Position(animate.endCoord.Row, animate.endCoord.Column);
                    if (GameTracker.Board.Pegs.At(movingPeg).Color == CreeperColor.Fire)
                    {
                        animate.peg.Material = red;
                    }
                    else if (GameTracker.Board.Pegs.At(movingPeg).Color == CreeperColor.Ice)
                    {
                        animate.peg.Material = blue;
                    }

                    animate.peg.Transform *= Matrix.CreateTranslation((animate.xDirection * (_board.Height / CreeperBoard.TileRows) / 200),
                            0, -(animate.yDirection * (_board.Width / CreeperBoard.TileRows) / 200));

                    if (animate.peg.Contains(animate.endLocation))
                    {
                        finishedAnimation.Add(animate);
                    }

                }
                foreach (Animation animate in finishedAnimation)
                {
                    animate.peg.Visible = false;
                    animate.peg.Transform = Matrix.CreateScale(Resources.Models.PegScale) * Matrix.CreateTranslation(animate.startLocation);
                    if (animation.Contains(animate))
                    {
                        animation.Remove(animate);
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

            
            //_scene.DrawDiagnostics(GraphicsDevice, gameTime.ElapsedGameTime);
            

            base.Draw(gameTime);
        }

        /// <summary>
        /// Determine what the display properties of each board location should and set them.
        /// </summary>
        private void DrawBoard()
        {
            string location;

          
            BasicMaterial yellow = new BasicMaterial(GraphicsDevice);
            yellow.DiffuseColor = new Vector3(255, 255, 0);

            BasicMaterial blue = new BasicMaterial(GraphicsDevice);
            blue.DiffuseColor = new Vector3(.4f, .576f, .604f);

            BasicMaterial red = new BasicMaterial(GraphicsDevice);
            red.DiffuseColor = new Vector3(1, .46f, 0);
            if (animation.Count == 0)
            {
                for (int r = 0; r < CreeperBoard.PegRows; r++)
                {
                    for (int c = 0; c < CreeperBoard.PegRows; c++)
                    {
                        location = 'p' + r.ToString() + 'x' + c.ToString();

                        if (GameTracker.Board.Pegs.At(new Position(r, c)).Color == CreeperColor.Fire)
                        {
                            _scene.FindName<Nine.Graphics.Model>(location).Visible = true;
                            _scene.FindName<Nine.Graphics.Model>(location).Material = red;
                        }
                        else if (GameTracker.Board.Pegs.At(new Position(r, c)).Color == CreeperColor.Ice)
                        {
                            _scene.FindName<Nine.Graphics.Model>(location).Visible = true;
                            _scene.FindName<Nine.Graphics.Model>(location).Material = blue;
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

                        if (GameTracker.Board.Tiles.At(new Position(r, c)).Color == CreeperColor.Fire)
                        {
                            _scene.FindName<Surface>(location).Material.Texture = _whiteTile;
                        }
                        else if (GameTracker.Board.Tiles.At(new Position(r, c)).Color == CreeperColor.Ice)
                        {
                            _scene.FindName<Surface>(location).Material.Texture = _blackTile;
                        }
                        else
                        {
                            if (_scene.FindName<Surface>(location) != null)
                            {
                                _scene.FindName<Surface>(location).Material.Texture = _blankTile;
                            }
                        }
                    }
                }
            }

            if (_selectedPeg != null && !_iStillNeedToMakeAMove)
            {
                //_scene.FindName<Nine.Graphics.Model>(_selectedPeg).Material = blue;
            }

            if (animation.Count > 0)
            {
                bool notAnimating;
                for (int r = 0; r < CreeperBoard.PegRows; r++)
                {
                    for (int c = 0; c < CreeperBoard.PegRows; c++)
                    {
                        location = 'p' + r.ToString() + 'x' + c.ToString();
                        notAnimating = true;

                        foreach (Animation animate in animation)
                        {
                            if (animate.startCoord == new Position(r, c))
                            {
                                notAnimating = false;
                            }
                        }
                        if (notAnimating && GameTracker.Board.Pegs.At(new Position(r, c)).Color == CreeperColor.Empty)
                        {
                            _scene.FindName<Nine.Graphics.Model>(location).Visible = false;
                        }
                    }
                }
            }
            _scene.FindName<Surface>('t' + fireStart.Row.ToString() + 'x' + fireStart.Column.ToString()).Material.Texture = _fireCorner;
            _scene.FindName<Surface>('t' + fireEnd.Row.ToString() + 'x' + fireEnd.Column.ToString()).Material.Texture = _fireCorner;
            _scene.FindName<Surface>('t' + iceStart.Row.ToString() + 'x' + iceStart.Column.ToString()).Material.Texture = _iceCorner;
            _scene.FindName<Surface>('t' + iceEnd.Row.ToString() + 'x' + iceEnd.Column.ToString()).Material.Texture = _iceCorner;
        }
    }
}