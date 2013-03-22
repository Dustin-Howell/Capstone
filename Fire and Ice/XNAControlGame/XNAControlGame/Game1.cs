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
        private Group _boardGroup;
        private Microsoft.Xna.Framework.Graphics.Model _selectedModel;
        private Microsoft.Xna.Framework.Graphics.Model _fireModel;
        private Microsoft.Xna.Framework.Graphics.Model _iceModel;
        private CreeperBoardViewModel _creeperBoardViewModel;
        private Input _input;

        private IEnumerable<CreeperPeg> _firePegs
        {
            get
            {
                return _boardGroup.Children
                    .Where(x => x.GetType() == typeof(CreeperPeg) 
                        && ((CreeperPeg)x).PegType == CreeperPegType.Fire)
                    .Select(x => (CreeperPeg)x);
            }
        }
        private IEnumerable<CreeperPeg> _icePegs
        {
            get
            {
                return _boardGroup.Children
                    .Where(x => x.GetType() == typeof(CreeperPeg) 
                        && ((CreeperPeg)x).PegType == CreeperPegType.Ice)
                    .Select(x => (CreeperPeg)x);                
            }
        }
        private IEnumerable<CreeperPeg> _possiblePegs
        {
            get
            {
                return _boardGroup.Children
                    .Where(x => x.GetType() == typeof(CreeperPeg)
                        && ((CreeperPeg)x).PegType == CreeperPegType.Possible)
                    .Select(x => (CreeperPeg)x);
            }
        }

        private CreeperPeg _selectedPeg;

        private bool _humanMovePending = false;

        public Game1(IntPtr handle, int width, int height, IEventAggregator eventAggregator)
            : base(handle, "Content", width, height)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            Components.Add(new InputComponent(handle));
            _input = new Input();

            _input.MouseDown += new EventHandler<Nine.MouseEventArgs>((s, e) => { ClearPossiblePegs(); DetectFullClick(e); });
            _input.MouseUp += new EventHandler<Nine.MouseEventArgs>((s,e) => DetectFullClick(e));
        }

        private void ClearPossiblePegs()
        {
            foreach (CreeperPeg pegToRemove in _possiblePegs)
            {
                _boardGroup.Remove(pegToRemove);
            }
        }

        private CreeperPeg _lastDownClickedModel;
        void DetectFullClick(Nine.MouseEventArgs e)
        {
            CreeperPeg clickedModel = GetClickedModel(new Vector2(e.MouseState.X, e.MouseState.Y));
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
                    _lastDownClickedModel = null;

                    if (GameTracker.CurrentPlayer.Color == clickedModel.PegType.ToCreeperColor())
                    {
                        OnPegClicked(clickedModel);
                    }
                }
            }
        }

        private void OnPegClicked(CreeperPeg clickedModel)
        {
            switch (clickedModel.PegType)
            {
                case CreeperPegType.Fire:
                    _selectedPeg = clickedModel;
                    UpdatePossibleMoves(clickedModel);
                    break;
                case CreeperPegType.Ice:
                    _selectedPeg = clickedModel;
                    UpdatePossibleMoves(clickedModel);
                    break;
                case CreeperPegType.Possible:
                    _eventAggregator.Publish(
                        new MoveResponseMessage(
                            new Move(_selectedPeg.Position, clickedModel.Position, 
                                _selectedPeg.PegType.ToCreeperColor()), 
                                PlayerType.Human)
                            );
                    break;
            }
        }

        private void UpdatePossibleMoves(CreeperPeg clickedPeg)
        {
            IEnumerable<Move> possibleMoves = GameTracker.Board.Pegs.At(clickedPeg.Position).PossibleMoves(GameTracker.Board);
            foreach (Position position in possibleMoves.Select(x => x.EndPosition))
            {
                CreeperPeg peg = new CreeperPeg(_iceModel) { Position = position, PegType = CreeperPegType.Possible, };
                _boardGroup.Add(peg);
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

        private CreeperPeg GetClickedModel(Vector2 mousePosition)
        {
            Ray selectionRay = GetSelectionRay(mousePosition);
            IEnumerable<CreeperPeg> currentTeam = (GameTracker.CurrentPlayer.Color == CreeperColor.Fire) ? _firePegs : _icePegs;
            CreeperPeg clickedModel = null;

            foreach (CreeperPeg peg in currentTeam)
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

            _scene = Content.Load<Scene>(Resources.ElementNames.RootScene);            

            _fireModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(Resources.Models.FirePeg);
            _iceModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(Resources.Models.IcePeg);
            _selectedModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(Resources.Models.SelectedPeg);

            base.LoadContent();

            OnContentLoaded();          
        }

        private void OnContentLoaded()
        {
            _boardGroup = _scene.FindName<Group>(Resources.ElementNames.BoardGroup);

            LoadViewModels();
            LoadPegModels();  
        }

        private void LoadPegModels()
        {
            foreach (Piece piece in GameTracker.Board.Pegs.Where(x => x.Color.IsTeamColor()))
            {
                CreeperPeg peg;
                if (piece.Color == CreeperColor.Fire)
                {
                    peg = new CreeperPeg(_fireModel) 
                    { 
                        PegType = CreeperPegType.Fire, 
                        Position = piece.Position, 
                    };
                }
                else
                {
                    peg = new CreeperPeg(_iceModel) 
                    { 
                        PegType = CreeperPegType.Ice, 
                        Position = piece.Position,
                    };
                }
                
                _boardGroup.Add(peg);
            }
        }

        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Update(gameTime);

            _scene.Update(gameTime.ElapsedGameTime);
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

        }
    }
}