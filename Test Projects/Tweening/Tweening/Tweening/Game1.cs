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

namespace Tweening
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        Scene scene;
        Input input;
        SpriteFont font;
        GraphicsDeviceManager graphics;
        private Nine.Graphics.Model _thePeg;
        private Nine.Animations.TweenAnimation<Matrix> anim;
        bool mouseDown = false;
        //Nine.Animations.TweenAnimation<Vector3> moveModel;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("Font");

            Components.Add(new FrameRate(GraphicsDevice, font));
            Components.Add(new InputComponent(Window.Handle));


            // Create an event based input handler.
            // Note that you have to explictly keep a strong reference to the Input instance.
            input = new Input();
            input.MouseDown += new EventHandler<MouseEventArgs>(Input_MouseDown);


            // Load a scene from a content file
            scene = Content.Load<Scene>("Scene1");
            _thePeg = scene.Find<Nine.Graphics.Model>();

            base.LoadContent();
        }

        /// <summary>
        /// Handle mouse input events
        /// </summary>
        private void Input_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            anim = new Nine.Animations.TweenAnimation<Matrix>()
            {
                Target = _thePeg,
                TargetProperty = "Transform",
                Duration = TimeSpan.FromSeconds(1),
                From = _thePeg.Transform,
                To = Matrix.CreateScale(.05f, .05f, .05f) * Matrix.CreateTranslation(10, 0, 10),
                Curve = Curves.Smooth,
                Repeat = 1,
                AutoReverse = false
            };

            var anim2 = new Nine.Animations.TweenAnimation<Matrix>()
            {
                Target = scene.FindName<Nine.Graphics.Model>("Peg2"),
                TargetProperty = "Transform",
                Duration = TimeSpan.FromSeconds(1),
                From = scene.FindName<Nine.Graphics.Model>("Peg2").Transform,
                To = Matrix.CreateScale(.05f, .05f, .05f) * Matrix.CreateTranslation(0, 0, 10),
                Curve = Curves.Smooth,
                Repeat = 10000,
                AutoReverse = true
            };

            _thePeg.Animations.Add("MovePeg", anim);
            _thePeg.Animations.Play("MovePeg");
            scene.FindName<Nine.Graphics.Model>("Peg2").Animations.Add("RepeatMovePeg", anim2);
            scene.FindName<Nine.Graphics.Model>("Peg2").Animations.Play("RepeatMovePeg");
            //scene.Find<Group>().Animations.Current.Play();

            //_thePeg.Animations.Add("MovePeg", anim);
            //_thePeg.Animations["MovePeg"].Play();
            //moveModel = new Nine.Animations.TweenAnimation<Vector3>();

            //moveModel.From = _thePeg.Transform.Translation;
            //moveModel.To = new Vector3(10, 0, 10);
            //moveModel.Easing = Nine.Animations.Easing.InOut;
            //moveModel.Duration = System.TimeSpan.FromSeconds(5);
            //moveModel.a
            //moveModel.AutoReverse = true;

            //moveModel.Play();
        }

        /// <summary>
        /// This is called when the game should update itself.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            scene.Update(gameTime.ElapsedGameTime);
            scene.UpdatePhysicsAsync(gameTime.ElapsedGameTime);
            //_thePeg.Transform *= Matrix.CreateTranslation(moveModel.Value);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            scene.Draw(GraphicsDevice, gameTime.ElapsedGameTime);
#if DEBUG
            // Draw scene diagnostics information
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                scene.DrawDiagnostics(GraphicsDevice, gameTime.ElapsedGameTime);
#endif
            base.Draw(gameTime);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }
}
