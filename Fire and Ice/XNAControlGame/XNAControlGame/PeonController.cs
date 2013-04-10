using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nine;
using Microsoft.Xna.Framework;
using Creeper;
using Nine.Animations;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics;

namespace XNAControlGame
{
    public class PeonController : Component 
    {
        public float Speed { get; set; }
        public enum Team { Possible, Fire, Ice };

        protected override void Update(float elapsedTime)
        {
            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (IsPegClicked(Parent.Find<Nine.Graphics.Model>(), new Vector2(mouseState.X, mouseState.Y)))
                {
                    //Load Possible Pegs
                    var Ani = Parent.Find<Nine.Graphics.Model>().Animations;
                    if (Ani["Idle"].State != Nine.Animations.AnimationState.Playing)
                    {
                        Ani.Play("Idle");
                    }
                    //follow logic from Game1
                }

            }

            base.Update(elapsedTime);
        }

        /// <summary>
        /// Will return true if the current peg has been clicked and false if it was not clicked. This function is not yet fully implemented
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        private bool IsPegClicked(Nine.Graphics.Model peg, Vector2 mousePosition)
        {
            Camera MainCamera = Scene.FindName<Camera>("MainCamera");
            Ray selectionRay = Parent.GetGraphicsDevice().Viewport.CreatePickRay((int)mousePosition.X, (int)mousePosition.Y, MainCamera.View, MainCamera.Projection);
            return selectionRay.Intersects( Parent.ComputeBounds() ).HasValue;
        }
    }
}
