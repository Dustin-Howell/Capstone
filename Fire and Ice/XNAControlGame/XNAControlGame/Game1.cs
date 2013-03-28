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
    /// <summary>
    /// Only class level variables and override methods go in this file
    /// </summary>
    public partial class Game1 : XNAControl.XNAControlGame
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

        public Game1(IntPtr handle, int width, int height, IEventAggregator eventAggregator)
            : base(handle, "Content", width, height)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            Components.Add(new InputComponent(handle));
            _input = new Input();

            _input.MouseDown += new EventHandler<Nine.MouseEventArgs>((s, e) => {
                if (_humanMovePending && !_pegAnimating)
                {
                    DetectFullClick(e);
                }
            });
            _input.MouseUp += new EventHandler<Nine.MouseEventArgs>((s, e) => {
                if (_humanMovePending && !_pegAnimating)
                {
                    DetectFullClick(e);
                }
            });
        }
       
        protected override void Initialize()
        {
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
            _scene.Add(_possibleModel1);

            base.LoadContent();

            OnContentLoaded();          
        }
        
        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Update(gameTime);
            _scene.Update(gameTime.ElapsedGameTime);
        }

        protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {            
            _scene.Draw(GraphicsDevice, gameTime.ElapsedGameTime);
            //_scene.DrawDiagnostics(GraphicsDevice, gameTime.ElapsedGameTime);
            
            base.Draw(gameTime);
        }

    }
}