using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Capstone
{
    public abstract class Clickable : GameComponent
    {
        protected Game _game;

        protected MouseState _previousMouseState { get; set; }
        protected MouseState _currentMouseState { get; set; }

        /// <summary>
        /// Whoever extends this class must define what to do when the clickable is clicked
        /// </summary>
        protected abstract void OnClick();

        /// <summary>
        /// Extender of class is responsible for detecting when a point is within the bounds of the clickable
        /// Clickable will take care of checking for clicks using this information
        /// </summary>
        /// <param name="point">the point to be checked against the clickable</param>
        /// <returns></returns>
        protected abstract bool PointIntersects(Point point);

        /// <summary>
        /// User is responsible for supplying a draw method
        /// </summary>
        /// <param name="spriteBatch">the spritebatch that does the drawing</param>
        protected abstract void Draw(SpriteBatch spriteBatch);

        public Clickable(Game game)
            : base(game)
        {
            _game = game;
        }

        public override void Initialize()
        {
            base.Initialize();

            _currentMouseState = Mouse.GetState();
            _previousMouseState = _currentMouseState;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();

            if (_previousMouseState.LeftButton == ButtonState.Pressed
                && PointIntersects(new Point(_previousMouseState.X, _previousMouseState.Y))
                && _currentMouseState.LeftButton == ButtonState.Released
                && PointIntersects(new Point(_currentMouseState.X, _currentMouseState.Y)))
            {
                OnClick();
            }
        }
    }
}
