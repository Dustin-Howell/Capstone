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

            float newDirectionRadian = (float)Math.Atan2(-(endPoint.X - Parent.Transform.Translation.X), -(endPoint.Z - Parent.Transform.Translation.Z));

                Parent.Transform = Matrix.CreateRotationY(newDirectionRadian)
                    * Matrix.CreateTranslation(Parent.Transform.Translation);

            TweenAnimation<Matrix> moveAnimation = new TweenAnimation<Matrix>
                {
                    Target = Parent,
                    TargetProperty = "Transform",
                    Duration = TimeSpan.FromSeconds(1),
                    From = Parent.Transform,
                    To = Matrix.CreateRotationY(newDirectionRadian) * Matrix.CreateTranslation(endPoint),
                    Curve = Curves.Smooth,
                };

            moveAnimation.Completed += new EventHandler((s, e) =>
                {
                    _pegModel.Animations["Run"].Stop();
                    _pegModel.Animations.Play("Idle");
                    Parent.Animations.Remove(Resources.AnimationNames.PegMove);
                    _graphicalPosition = endPoint;
                    Position = position;
                    callback();
                });
            if (type != MoveType.PegJump)
            {

                Parent.Animations.Add("move", moveAnimation);
                Parent.Animations.Play("move");

                _pegModel.Animations.Play("Run");
            }
            else
            {
                

                Vector3 distance = endPoint - Parent.Transform.Translation;
                distance /= 1.4f;

                Parent.Transform = Matrix.CreateRotationY(newDirectionRadian)
                    * Matrix.CreateTranslation(Parent.Transform.Translation);

                TweenAnimation<Matrix> killAnimation = new TweenAnimation<Matrix>
                {
                    Target = Parent,
                    TargetProperty = "Transform",
                    Duration = TimeSpan.FromSeconds(1),
                    From = Parent.Transform,
                    To = Matrix.CreateRotationY(newDirectionRadian) * Matrix.CreateTranslation(endPoint - distance),
                    Curve = Curves.Smooth,
                };


                killAnimation.Completed += new EventHandler((s, e) =>
                {
                    _pegModel.Animations["Chop"].Stop();
                     Parent.Animations.Remove("kill");
                     moveAnimation.From = Parent.Transform;
                     _pegModel.Animations.Play("Run");
                    Parent.Animations.Play("move");
                    _graphicalPosition = endPoint;
                    Position = position;
                    callback();
                });

                Parent.Animations.Add("move", moveAnimation);
                Parent.Animations.Add("kill", killAnimation);
                Parent.Animations.Play("kill");
                _pegModel.Animations.Play("Chop");
            }
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
