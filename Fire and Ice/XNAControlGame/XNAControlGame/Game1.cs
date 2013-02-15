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

namespace XNAControlGame
{
    public class Game1 : XNAControl.XNAControlGame
    {
        Position startPosition = new Position(-1, -1);
        Position endPostion = new Position(-1, -1);
        Scene scene;
        Input input;
        SpriteFont font;
        GraphicsDeviceManager graphics;
        Matrix cameraView;
        Matrix cameraProj;

        public Game1(IntPtr handle, int width, int height) : base(handle, "Content", width, height) { }



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
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
           
            Components.Add(new InputComponent(Window.Handle));

            input = new Input();
            input.MouseDown += new EventHandler<MouseEventArgs>(Input_MouseDown);

            // Load models from the models folder
            Microsoft.Xna.Framework.Graphics.Model pegModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>("Model/tank");
            // Load a scene from a content file
            scene = Content.Load<Scene>("Scene1");

            float boardHeight, boardWidth, squareWidth, squareHeight;
            boardHeight = scene.FindName<Sprite>("boardImage").Texture.Height;
            boardWidth = scene.FindName<Sprite>("boardImage").Texture.Width;
            squareWidth = boardWidth / 7;
            squareHeight = boardHeight / 7;
            Vector3 startCoordinates = new Vector3(-boardWidth / 2, boardHeight / 2, 0);

            for (int pegNumber = 1; pegNumber < 46; pegNumber++)
            {
                Position pegPosition = NumberToPosition(pegNumber);
                String pegName = 'p' + pegPosition.Row.ToString() + 'x' + pegPosition.Column.ToString();
                Vector3 pegCoordinates = new Vector3(startCoordinates.X + squareWidth * pegPosition.Column, startCoordinates.Y - squareHeight * pegPosition.Row, 0);
                scene.Add( new Nine.Graphics.Model(pegModel) { Transform = Matrix.CreateTranslation(pegCoordinates) * Matrix.CreateScale(.02f, .02f, .02f), Name = pegName } );
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

            column = Math.Round((-(column / (scene.FindName<Sprite>("boardImage").Texture.Width / 6)) + 3));
            row = Math.Round((row / (scene.FindName<Sprite>("boardImage").Texture.Height / 6)) + 3);

            //Returns the row, column of the peg selected.
            return new Position(-(clickLocation.Column / (scene.FindName<Sprite>("boardImage").Texture.Width / 6)) + 3,
                clickLocation.Row / (scene.FindName<Sprite>("boardImage").Texture.Height / 6) + 3);
        }

        /// <summary>
        /// Handle mouse input events
        /// </summary>
        private void Input_MouseDown(object sender, MouseEventArgs e)
        {
            Ray pickRay = GetPickRay();
            float maxDistance = float.MaxValue;
            String selectedPeg = "";

            for (int pegNum = 1; pegNum <= scene.FindName<Group>("Pegs").Count; pegNum++)
            {
                String currentPeg = 'p' + pegNum.ToString();
                //Need to make new boudingbox!!!
                BoundingBox modelIntersect = new BoundingBox(scene.FindName<Nine.Graphics.Model>(currentPeg).BoundingBox.Min,
                    scene.FindName<Nine.Graphics.Model>(currentPeg).BoundingBox.Max);
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

            Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearsource, cameraProj, cameraView, world);

            Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farsource, cameraProj, cameraView, world);

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
            scene.Update(gameTime.ElapsedGameTime);
            scene.UpdatePhysicsAsync(gameTime.ElapsedGameTime);
            cameraView = scene.Find<FreeCamera>().View;
            cameraProj = scene.Find<FreeCamera>().Projection;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            scene.Draw(GraphicsDevice, gameTime.ElapsedGameTime);
            base.Draw(gameTime);
        }
    }
}