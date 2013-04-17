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
    public enum MoveType { TileJump, PegJump, Normal, }
    public enum CreeperPegType { Fire, Ice, Possible, }

    class PegController : Component
    {
        private Nine.Graphics.Model _pegModel;

        private Position _position;
        private Vector3 _graphicalPosition;
        public Position Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        private void DoTransform()
        {
            _pegModel.Transform = Matrix.CreateScale(Resources.Models.PegScale) * Matrix.CreateTranslation(_graphicalPosition);
        }

        public CreeperPegType PegType { get; set; }

        public bool IsPegClicked( Ray selectionRay )
        {
            return selectionRay.Intersects( Parent.ComputeBounds() ).HasValue;
        }

        public void MoveTo(Position position, Vector3 endPoint, MoveType type, System.Action callback)
        {
            //TweenAnimation<Matrix> rotateAnimation = new TweenAnimation<Matrix>
            //{
            //    Target = Parent,
            //    TargetProperty = "Transform",
            //    Duration = TimeSpan.FromMilliseconds(500),
            //    From = Parent.Transform,
            //    To = Matrix.CreateRotationY((float)Math.PI),
            //    Curve = Curves.Smooth,
            //};

            Parent.Transform *=
                Matrix.CreateTranslation(-Parent.Transform.Translation)
                * Matrix.CreateRotationY(MathHelper.Pi)
                * Matrix.CreateTranslation(Parent.Transform.Translation);

            TweenAnimation<Matrix> moveAnimation = new TweenAnimation<Matrix>
            {
                Target = Parent,
                TargetProperty = "Transform",
                Duration = TimeSpan.FromSeconds(1),
                From = Parent.Transform,
                To = Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(endPoint),
                Curve = Curves.Smooth,
            };

            //rotateAnimation.Completed += new EventHandler((s, e) =>
            //{
            //    Parent.Animations.Add(Resources.AnimationNames.PegMove, moveAnimation);
            //    Parent.Animations.Play(Resources.AnimationNames.PegMove);
            //});

            moveAnimation.Completed += new EventHandler((s, e) =>
            {
                _pegModel.Animations["Run"].Stop();
                _pegModel.Animations.Play("Idle");
                Parent.Animations.Remove(Resources.AnimationNames.PegMove);
                _graphicalPosition = endPoint;
                Position = position;
                //DoTransform();
                callback();
            });

            Parent.Animations.Add("move", moveAnimation);
            Parent.Animations.Play("move");

            //Parent.Animations.Add("Rotate", rotateAnimation);
            //Parent.Animations.Play("Rotate");
            _pegModel.Animations.Play("Run");
        }

        protected override void OnAdded(Group parent)
        {
            _pegModel = parent.Find<Nine.Graphics.Model>();
            if (PegType != CreeperPegType.Possible)
                _pegModel.Animations.Play("Idle");
            base.OnAdded(parent);
        }

        protected override void OnRemoved(Group parent)
        {
            _pegModel = null;
            base.OnRemoved(parent);
        }

        internal void Destroy()
        {
            Scene.Remove(Parent);
        }
    }
    
}
