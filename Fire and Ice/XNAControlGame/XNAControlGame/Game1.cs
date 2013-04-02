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

        public CreeperColor GetCurrentTurn()
        {
            return CreeperColor.Fire;
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
        private Nine.Graphics.Model _possibleModel1;
        private Instance _fireModel1;
        private Instance _iceModel1;

        private Texture2D _fireTile;
        private Texture2D _iceTile;

        private Input _input;

        private CreeperPeg _selectedPeg;
        private CreeperPeg _SelectedPeg
        {
            get
            {
                return _selectedPeg;
            }
            set
            {
                if (_selectedPeg != value)
                {
                    _selectedPeg = value;
                    UpdatePossibleMoves(value);
                }
            }
        }

        private IEnumerable<CreeperPeg> _pegs
        {
            get
            {
                return _boardGroup.Children
                    .Where(x => x.GetType() == typeof(CreeperPeg))
                    .Select(x => (CreeperPeg)x);
            }
        }
        private IEnumerable<CreeperPeg> _firePegs
        {
            get
            {
                return _pegs.Where(x => x.PegType == CreeperPegType.Fire);  
            }
        }
        private IEnumerable<CreeperPeg> _icePegs
        {
            get
            {
                return _pegs.Where(x => x.PegType == CreeperPegType.Ice);                
            }
        }
        private IEnumerable<CreeperPeg> _possiblePegs
        {
            get
            {
                return _pegs.Where(x => x.PegType == CreeperPegType.Possible);
            }
        }

        private bool _humanMovePending = false;
        private bool _pegAnimating = false;
        private GraphicsDeviceManager _graphics;

        public IProvideBoardState BoardProvider { get; private set; }

        public Game1() : base()
        {
            _eventAggregator = new EventAggregator();
            _eventAggregator.Subscribe(this);
            BoardProvider = new DummyBoardProvider();

            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;

            Components.Add(new InputComponent(Window.Handle));

            IsMouseVisible = true;

            Content.RootDirectory = "Content";
        }

        public Game1(IEventAggregator eventAggregator, IProvideBoardState boardProvider) : base()
        {
            BoardProvider = boardProvider;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        private string _mouseClickCoords;
        protected override void Initialize()
        {
            _input = new Input();

            _input.MouseDown += new EventHandler<Nine.MouseEventArgs>((s, e) =>
            {
                if (_humanMovePending && !_pegAnimating)
                {
                    DetectFullClick(e);
                }
            });
            _input.MouseUp += new EventHandler<Nine.MouseEventArgs>((s, e) =>
            {
                if (_humanMovePending && !_pegAnimating)
                {
                    DetectFullClick(e);
                }
            });

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteFont = Content.Load<SpriteFont>("defaultFont");

            _fireTexture = Content.Load<Texture2D>("Textures/fire");

            _scene = Content.Load<Scene>(Resources.ElementNames.RootScene);

            _fireModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(Resources.Models.FirePeg);
            _iceModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(Resources.Models.IcePeg);
            _possibleModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(Resources.Models.PossiblePeg);

            _fireModel1 = new Instance { Template = "FirePeg" };
            _iceModel1 = new Instance { Template = "IcePeg" };
            _possibleModel1 = Content.Load<Nine.Graphics.Model>("PossiblePeg");

            _fireTile = Content.Load<Texture2D>("Assets/Fire Tile Cropped");
            _iceTile = Content.Load<Texture2D>("Assets/Ice Tile Cropped");

#if DEBUG
            _pointer = Content.Load<Texture2D>("Textures/flake");
            _sb = new SpriteBatch(GraphicsDevice);
#endif

            base.LoadContent();

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
            //_scene.DrawDiagnostics(GraphicsDevice, (float)gameTime.ElapsedGameTime.TotalSeconds);

#if DEBUG
            _sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            _sb.Draw(_pointer, _pointerPosition, Color.White);
            _sb.End();
#endif

            base.Draw(gameTime);
        }

        public static void Main()
        {
            using (Game1 game = new Game1())
            {
                game._eventAggregator.Publish(new MoveMessage()
                {
                    Board = game.BoardProvider.GetBoard(),
                    TurnColor = CreeperColor.Fire,
                    Type = MoveMessageType.Request,
                    PlayerType = PlayerType.Human,
                });

                game.IsMouseVisible = false;

                game.Run();
            }
        }
    }
}