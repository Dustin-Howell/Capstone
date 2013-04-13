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
    class pegController : Component
    {
        //Placement of this variable and the GetMoveDirection method is temporary (probably).
        //In the final version all directions should equal some angle relative the board (North would probably be 45 degrees.
        enum Direction { N, S, E, W, NW, SW, NE, SE };

        public float Speed { get; set; }
        public enum Team { Possible, Fire, Ice };

        protected override void Update(float elapsedTime)
        {
            //We might need update logic to determine when to activate certain animations in the move sequence.
            //For example, during a peg jump move, we could test for collision with another bounding box, and if
            //it was detected we could pause the tween animation moving peg's position, switch to the kill animation (chop?)
            //and then resume the run and position tween animations until the move is completed. On completion of the positional
            //tween animation, switch from run animation to idle animation. Sheesh.

            base.Update(elapsedTime);
        }

        /// <summary>
        /// Will return true if the current peg has been clicked and false if it was not clicked. This function is not yet fully implemented
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        public bool IsPegClicked( Ray selectionRay )
        {
            return selectionRay.Intersects( Parent.ComputeBounds() ).HasValue;
        }

        private void MoveTo(Move moveToAnimate, Action callback)
        {
            Direction direction = GetDirection( moveToAnimate );
            //Set the peg's rotation to the appropriate cardinal direction's angle as set in the enum.

            //Create the positional tween animation.
            TweenAnimation<Matrix> moveAnimation = new TweenAnimation<Matrix>()
            {
                Target = Parent,
                TargetProperty = "Transform",
                Duration = TimeSpan.FromSeconds(1),
                From = Parent.Transform,
                To = Matrix.CreateScale(Resources.Models.PegScale) * Matrix.CreateRotationY(MathHelper.ToRadians(135))
                        * Matrix.CreateTranslation(CreeperBoardViewModel.GraphicalPositions[moveToAnimate.EndPosition.Row, moveToAnimate.EndPosition.Column]),
                Curve = Curves.Smooth,
            };
            moveAnimation.Completed += new EventHandler((s, e) =>
            {
                //Position = position;
                Parent.Animations["Run"].Stop();
                Parent.Animations.Play("Idle");
                Parent.Animations.Remove(Resources.AnimationNames.PegMove);
                callback();
            }
                );

            Parent.Animations.Add(Resources.AnimationNames.PegMove, moveAnimation);
            Parent.Animations.Play(Resources.AnimationNames.PegMove);
            Parent.Animations.Play("Run");

            //Have different methods based on the type of animation it is for easier scripting? (Move, Capture, Jump)
        }

        private Direction GetDirection(Move moveToAnimate)
        {
            return Direction.S;
        }
    }
    
}
