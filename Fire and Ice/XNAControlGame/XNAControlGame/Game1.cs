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
using Caliburn.Micro;
using CreeperMessages;

namespace XNAControlGame
{
    public class Game1 : XNAControl.XNAControlGame, IDisposable, IHandle<MoveRequestMessage>, IHandle<MoveResponseMessage>
    {
        private IEventAggregator _eventAggregator;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private Scene _scene;
        private Microsoft.Xna.Framework.Graphics.Model _selectedModel;
        private Microsoft.Xna.Framework.Graphics.Model _fireModel;
        private Microsoft.Xna.Framework.Graphics.Model _iceModel;
        private CreeperBoardViewModel _creeperBoardViewModel;
        private Input _input;

        private List<Nine.Graphics.Model> _firePegs;
        private List<Nine.Graphics.Model> _icePegs;
        private Nine.Graphics.Model _selectedPeg;

        private bool _humanMovePending = false;

        public Game1(IntPtr handle, int width, int height, IEventAggregator eventAggregator)
            : base(handle, "Content", width, height)
        {
            _eventAggregator = eventAggregator;

            _firePegs = new List<Nine.Graphics.Model>();
            _icePegs = new List<Nine.Graphics.Model>();

            Components.Add(new InputComponent(handle));
            _input = new Input();

            _input.MouseDown += new EventHandler<Nine.MouseEventArgs>((s,e) => DetectFullClick(e));
            _input.MouseUp += new EventHandler<Nine.MouseEventArgs>((s,e) => DetectFullClick(e));
        }

        private Nine.Graphics.Model _lastDownClickedModel;
        void DetectFullClick(Nine.MouseEventArgs e)
        {
            Nine.Graphics.Model clickedModel = GetClickedModel(new Vector2(e.MouseState.X, e.MouseState.Y));
            if (clickedModel != null)
            {
                //if downclick
                if (e.IsButtonDown(e.Button))
                {
                    _lastDownClickedModel = clickedModel;
                }
                //if upclick
                else if (_lastDownClickedModel == clickedModel)
                {
                    _selectedPeg = clickedModel;
                    _lastDownClickedModel = null;
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

        private Nine.Graphics.Model GetClickedModel(Vector2 mousePosition)
        {
            Ray selectionRay = GetSelectionRay(mousePosition);
            List<Nine.Graphics.Model> currentTeam = (GameTracker.CurrentPlayer.Color == CreeperColor.Fire) ? _firePegs : _icePegs;
            Nine.Graphics.Model clickedModel = null;

            foreach (Nine.Graphics.Model peg in currentTeam)
            {
                if (selectionRay.Intersects(peg.BoundingBox).HasValue)
                {
                    clickedModel = peg;
                    break;
                }
            }
            return clickedModel;
        }

        private void LoadViewModels()
        {
            _creeperBoardViewModel = new CreeperBoardViewModel(_scene.FindName<Surface>("boardSurface").Heightmap.Height, _scene.FindName<Surface>("boardSurface").Heightmap.Width, _scene.FindName<Surface>("boardSurface").Heightmap.Step);
        }

        protected override void Initialize()
        {
            base.Initialize();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void LoadContent()
        {
            _spriteFont = Content.Load<SpriteFont>("defaultFont");

            _scene = Content.Load<Scene>("Scene1");

            _fireModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(Resources.Models.FirePeg);
            _iceModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(Resources.Models.IcePeg);
            _selectedModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(Resources.Models.SelectedPeg);

            _firePegs = new List<Nine.Graphics.Model>();
            _icePegs = new List<Nine.Graphics.Model>();

            base.LoadContent();
            LoadViewModels();
        }

        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            
            _scene.Draw(GraphicsDevice, gameTime.ElapsedGameTime);
            _scene.DrawDiagnostics(GraphicsDevice, gameTime.ElapsedGameTime);
            
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_spriteFont, "("+ Mouse.GetState().X.ToString() + ", "+ Mouse.GetState().Y.ToString() + ")", new Vector2(0, 0), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            //dispose stuff

            base.Dispose(disposing);
        }

        public void Handle(MoveRequestMessage message)
        {
            if (message.Responder == PlayerType.Human)
            {
                _humanMovePending = true;
            }
        }

        public void Handle(MoveResponseMessage message)
        {
            //animate
            throw new NotImplementedException(message.ToString());
        }
    }
}