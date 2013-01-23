using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Capstone
{
    public class Button : Clickable
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public String Text { get; set; }
        public Texture2D Texture { get; set; }

        public delegate void OnClick();
        //we may eventually want to make this public so that the function of a button can be changed dynamically
        protected OnClick _onClick;

        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            }
        }

        protected override bool PointIntersects(Point point)
        {
            return Rectangle.Intersects(new Rectangle(point.X, point.Y, 1, 1));
        }

        /// <summary>
        /// Button Constructor
        /// </summary>
        /// <param name="game">the game object (will be 'this' from Game1)</param>
        /// <param name="position">position of the button</param>
        /// <param name="size">x represents width, y represents height</param>
        /// <param name="onClick">function to be executed on click of the button</param>
        /// <param name="text">the text that should be on the button</param>
        public Button(Game1 game, 
            Vector2 position,
            Vector2 size,
            OnClick onClick,
            String text = "")
            : base(game)
        {
            Position = position;
            Size = size;
            _onClick = onClick;
            Text = text;
        }

        /// <summary>
        /// Executes the onclick delegate that was passed into the constructor
        /// </summary>
        protected override void OnClick()
        {
            _onClick();
        }

        protected override void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null)
            {
                spriteBatch.Draw(Texture, Rectangle, Color.White);
            }
        }
    }
}
