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

        public Game1(IntPtr handle, Panel gamePanel, int width, int height) : base(handle, "Content", width, height)
        {
            Content = new ContentLoader(Services);
            GamePanel = gamePanel;


            GamePanel.MouseClick += new MouseEventHandler(Input_MouseDown);
        }

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

        private void DrawBoard()
        {
            string peglocation;
            for (int r = 0; r < CreeperBoard.PegRows; r++)
            {
                for (int c = 0; c < CreeperBoard.PegRows; c++)
                {
                    peglocation = 'p' + r.ToString() + 'x' + c.ToString();

                    if(board.Pegs.At(new Position(r,c)).Color == CreeperColor.White)
                    {
                        
                        _scene.FindName<Nine.Graphics.Model>(peglocation).Visible = true;
                    }
                    else if (board.Pegs.At(new Position(r, c)).Color == CreeperColor.Black)
                    {
                        _scene.FindName<Nine.Graphics.Model>(peglocation).Visible = true;
                    }
                    else
                    {
                        if (_scene.FindName<Nine.Graphics.Model>(peglocation) != null)
                        {
                            _scene.FindName<Nine.Graphics.Model>(peglocation).Visible = false;
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
            // Load a scene from a content file
            _scene = Content.Load<Scene>("Scene1");

            float boardHeight, boardWidth, squareWidth, squareHeight;
            boardHeight = _scene.FindName<Sprite>("boardImage").Texture.Height;
            boardWidth = _scene.FindName<Sprite>("boardImage").Texture.Width;
            squareWidth = boardWidth /6;
            squareHeight = boardHeight / 6;
            Vector3 startCoordinates = new Vector3(-boardWidth / 2, boardHeight / 2, 0);
            Position pegPosition;

            for (int pegNumber = 1; pegNumber < 46; pegNumber++)
            {
                pegPosition = NumberToPosition(pegNumber);
                String pegName = 'p' + pegPosition.Row.ToString() + 'x' + pegPosition.Column.ToString();
                Vector3 pegCoordinates = new Vector3(startCoordinates.X + squareWidth * pegPosition.Column, startCoordinates.Y - squareHeight * pegPosition.Row, 0);
                _scene.Add(new Nine.Graphics.Model(pegModel) { Transform = Matrix.CreateScale(.02f, .02f, .02f) * Matrix.CreateTranslation(pegCoordinates), Name = pegName});
            }

            base.LoadContent();
        }

        private int DistanceBetween(Position start, Position end)
        {
            return (int)Math.Sqrt((Math.Pow((start.Row - end.Row), 2) + Math.Pow((start.Column - end.Column), 2)));
        }

        private Position toBoardPosition(Position clickLocation)
        {
            double column = clickLocation.Column;
            double row = clickLocation.Row;

            column = Math.Round((-(column / (_scene.FindName<Sprite>("boardImage").Texture.Width / 6)) + 3));
            row = Math.Round((row / (_scene.FindName<Sprite>("boardImage").Texture.Height / 6)) + 3);

            //Returns the row, column of the peg selected.
            return new Position(-(clickLocation.Column / (_scene.FindName<Sprite>("boardImage").Texture.Width / 6)) + 3,
                clickLocation.Row / (_scene.FindName<Sprite>("boardImage").Texture.Height / 6) + 3);
        }

        /// <summary>
        /// Handle mouse input events
        /// </summary>
        private void Input_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Ray pickRay = GetPickRay();
            float maxDistance = float.MaxValue;
            String selectedPeg = "";
            Position pegLocation;
            for (int pegNum = 1; pegNum <= 45; pegNum++)
            {
                pegLocation = NumberToPosition(pegNum);
                String currentPeg = 'p' + pegLocation.Row.ToString() + 'x' + pegLocation.Column.ToString();
                //Need to make new boudingbox!!!
                BoundingBox modelIntersect = new BoundingBox(_scene.FindName<Nine.Graphics.Model>(currentPeg).BoundingBox.Min,
                    _scene.FindName<Nine.Graphics.Model>(currentPeg).BoundingBox.Max);
                Nullable<float> intersect = pickRay.Intersects(modelIntersect);
                if (intersect.HasValue == true)
                {
                    if (intersect.Value < maxDistance)
                    {
                        selectedPeg = currentPeg;
                    }
                }
            }
        }

        Ray GetPickRay()
        {
            MouseState mouseState = Mouse.GetState();

            int mouseX = mouseState.X;
            int mouseY = mouseState.Y;

            Vector3 nearsource = new Vector3((float)mouseX, (float)mouseY, 0f);
            Vector3 farsource = new Vector3((float)mouseX, (float)mouseY, 1f);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearsource, _cameraProj, _cameraView, world);

            Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farsource, _cameraProj, _cameraView, world);

            // Create a ray from the near clip plane to the far clip plane.
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            Ray pickRay = new Ray(nearPoint, direction);

            return pickRay;
        }

        //private Vector2 _toEngineNineMouseCoor(System.Windows.Forms.MouseEventArgs e)
        //{
        //    return new Vector2( e.X 
        //}

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
            base.Draw(gameTime);
        }
    }
}