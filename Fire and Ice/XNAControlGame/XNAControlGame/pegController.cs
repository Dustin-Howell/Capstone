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
    static class DirectionToDegrees
    {
        public static float North { get { return MathHelper.ToRadians(315); } }
        public static float South { get { return MathHelper.ToRadians(135); } }
        public static float East { get { return MathHelper.ToRadians(225); } }
        public static float West { get { return MathHelper.ToRadians(45); } }
        public static float NorthEast { get { return MathHelper.ToRadians(270); } }
        public static float SouthEast { get { return MathHelper.ToRadians(180); } }
        public static float NorthWest { get { return MathHelper.ToRadians(0); } }
        public static float SouthWest { get { return MathHelper.ToRadians(90); } }
    }

    class PegController : Component
    {
        public Position Position { get; private set; }
        public CreeperPegType PegType { get; private set; }
        public Nine.Graphics.Model PegControlled { get { return Parent.Find<Nine.Graphics.Model>(); } }

        public PegController(Position initPosition, CreeperPegType team)
        {
            Position = initPosition;
            PegType = team;
        }

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

        //public void MoveTo(Move moveToAnimate, Action callback)
        public void MoveTo(Move moveToAnimate)
        {
            //Set the peg's rotation to the appropriate cardinal direction of the move.
            Parent.Transform *= Matrix.CreateRotationY(GetDirection(moveToAnimate));

            //Create the positional tween animation.
            TweenAnimation<Matrix> moveAnimation = new TweenAnimation<Matrix>()
            {
                Target = Parent,
                TargetProperty = "Transform",
                Duration = TimeSpan.FromSeconds(1),
                From = Parent.Transform,
                To = Matrix.CreateScale(Resources.Models.PegScale) * Matrix.CreateRotationY(DirectionToDegrees.South)
                        * Matrix.CreateTranslation(CreeperBoardViewModel.GraphicalPositions[moveToAnimate.EndPosition.Row, moveToAnimate.EndPosition.Column]),
                Curve = Curves.Smooth,
            };
            //moveAnimation.Completed += new EventHandler((s, e) =>
            //{
            //    //Position = position;
            //    Parent.Animations["Run"].Stop();
            //    Parent.Animations.Play("Idle");
            //    Parent.Animations.Remove(Resources.AnimationNames.PegMove);
            //    callback();
            //}
            //    );

            Parent.Animations.Add(Resources.AnimationNames.PegMove, moveAnimation);
            Parent.Animations.Play(Resources.AnimationNames.PegMove);
            Parent.Animations.Play("Run");

            Position = moveToAnimate.EndPosition;

            //Have different methods based on the type of animation it is for easier scripting? (Move, Capture, Jump)
        }

        private float GetDirection(Move moveToAnimate)
        {
            return DirectionToDegrees.South;
        }
    }
    
}
