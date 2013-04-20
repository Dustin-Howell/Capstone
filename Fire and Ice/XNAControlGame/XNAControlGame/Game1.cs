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
    internal class DummyBoardProvider : IProvideBoardState
    {
        CreeperBoard _board;

        public CreeperBoard GetBoard()
        {
            return _board = _board ?? new CreeperBoard();
        }

        public Player GetCurrentPlayer()
        {
            return new Player(PlayerType.Human, CreeperColor.Fire);
        }

        public Stack<CreeperBoard> BoardHistory
        {
            get { throw new NotImplementedException(); }
        }
    }

    /// <summary>
    /// Only class level variables and override methods go in this file
    /// </summary>
    public partial class Game1 : Game
    {
        private IEventAggregator _eventAggregator;
        private SpriteFont _spriteFont;
        private Texture2D _fireTexture;

        private Scene _scene;
        private Group _boardGroup;
        private CreeperBoardViewModel _creeperBoardViewModel;

        private Microsoft.Xna.Framework.Graphics.Model _possibleModel;
        private Microsoft.Xna.Framework.Graphics.Model _fireModel;
        private Microsoft.Xna.Framework.Graphics.Model _iceModel;
        Group _fireGroup;
        Group _iceGroup;
        private Instance _fireModel1;
        private Instance _iceModel1;
        private Texture2D _fireTileMask;
        private Texture2D _iceTileMask;

        static public ParticleEffect _fireEffect;

        private Input _input;

        private MoveAnimationListener _moveAnimationListener;
        private BoardController _boardController;

        private GraphicsDeviceManager _graphics;

        public IProvideBoardState BoardProvider { get; private set; }

        public Game1() : this(new EventAggregator(), new DummyBoardProvider())
        {
            Components.Add(new InputComponent(Window.Handle));
        }

        public Game1(IEventAggregator eventAggregator, IProvideBoardState boardProvider) : base()
        {
            BoardProvider = boardProvider;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            _eventAggregator.Subscribe(_moveAnimationListener = new MoveAnimationListener(eventAggregator));

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Content = new ContentManager(Services, "Content");
        }

        protected override void Initialize()
        {
            _input = new Input();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            _fireTileMask = Content.Load<Texture2D>("Assets/greenOctoMask");
            _iceTileMask = Content.Load<Texture2D>("Assets/blueOctoMask");

            _spriteFont = Content.Load<SpriteFont>("defaultFont");

            _fireTexture = Content.Load<Texture2D>("Textures/fire");

            _scene = Content.Load<Scene>(Resources.ElementNames.RootScene);

            // Workaround for wierdly static default service provider in Group:
            if (Content.ServiceProvider.GetService(typeof(ContentManager)) == null)
                ((GameServiceContainer)Content.ServiceProvider).AddService(typeof(ContentManager), Content);

            _fireModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(Resources.Models.FirePeg);
            _iceModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(Resources.Models.IcePeg);

            _possibleModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(Resources.Models.PossiblePeg);

            _fireModel1 = new Instance { Template = "FirePeg" };
            _iceModel1 = new Instance { Template = "IcePeg" };

            //Loads in the fire particle effect
            _fireEffect = Content.Load<Nine.Graphics.ParticleEffects.ParticleEffect>("FireEffect");

#if DEBUG
            _pointer = Content.Load<Texture2D>("Textures/flake");
            _sb = new SpriteBatch(GraphicsDevice);
#endif
            OnContentLoaded();
        }
        
        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            _scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

#if DEBUG
            _pointerPosition = new Vector2(Mouse.GetState().X - 16, Mouse.GetState().Y - 16);
#endif

            base.Update(gameTime);
        }

#if DEBUG
        SpriteBatch _sb;
        Texture2D _pointer;
        Vector2 _pointerPosition = new Vector2();
#endif
        protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            _scene.Draw(GraphicsDevice, (float)gameTime.ElapsedGameTime.TotalSeconds);

#if DEBUG
            _sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            _sb.Draw(_pointer, _pointerPosition, Color.White);
            _sb.End();
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
                _scene.DrawDiagnostics(GraphicsDevice, (float)gameTime.ElapsedGameTime.TotalSeconds);
#endif

            base.Draw(gameTime);
        }

        bool _disposing = false;
        new public void Dispose()
        {
            if (!_disposing)
            {
                _disposing = true;

                Content.Unload();
                Content.Dispose();
                base.Dispose(true);
            }
        }
    }
}