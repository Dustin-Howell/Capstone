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

        private bool _humanMovePending = false;

        public Game1(IntPtr handle, int width, int height, IEventAggregator eventAggregator)
            : base(handle, "Content", width, height)
        {
            _eventAggregator = eventAggregator;
        }

        protected override void Initialize()
        {
            base.Initialize();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void LoadContent()
        {
            _spriteFont = Content.Load<SpriteFont>("defaultFont");
            base.LoadContent();
        }

        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_spriteFont, "Hello", new Vector2(100, 100), Color.White);
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