using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Creeper;

namespace XNAControlGame
{
    /// <summary>
    /// This will essentially exist only to manage game states
    /// </summary>
    public class Game1 : XNAControl.XNAControlGame
    {
        protected SpriteBatch _spriteBatch;
        public SpriteFont DefaultFont { get; private set; }
        public CreeperBoard Board { get; set; }
        public Texture2D Square { get; set; }
        private List<GameScreen> GameScreens { get; set; }
        public GameScreen InGameScreen { get; set; }

        public void StateChange(GameState previousState, GameState newState)
        {
            foreach (GameScreen screen in GameScreens)
            {
                screen.Deactivate();
            }

            switch (newState)
            {
                case GameState.InGame:
                    InGameScreen.Activate();
                    break;
            }
        }

        public void MakeMove(Move move)
        {

        }

        public Game1(IntPtr handle) : base(handle, "Content")
        {
            GameScreens.Add(InGameScreen);
        }

        protected override void Initialize()
        {
            base.Initialize();
            InGameScreen = new InGameScreen(this, _spriteBatch, Board);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            DefaultFont = Content.Load<SpriteFont>("defaultFont");
            Square = Content.Load<Texture2D>("square");
            base.LoadContent();
        }
    }
}
