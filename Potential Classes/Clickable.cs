using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Capstone
{
    public abstract class Clickable : GameComponent
    {
        protected abstract void OnClick();

        public abstract bool PointIntersects(Vector2 point);

        public Clickable(Game game) : base(game) { }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Game1.PreviousMouseState.LeftButton == ButtonState.Pressed
                && PointIntersects(new Vector2(Game1.PreviousMouseState.X, Game1.PreviousMouseState.Y))
                && Game1.CurrentMouseState.LeftButton == ButtonState.Released
                && PointIntersects(new Vector2(Game1.CurrentMouseState.X, Game1.CurrentMouseState.Y)))
            {
                OnClick();
            }
        }
    }
}
