﻿using System;
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
using Nine.Graphics.ParticleEffects;

namespace XNAControlGame
{
    public enum MoveType { TileJump, PegJump, Normal, }
    public enum CreeperPegType { Fire, Ice, Possible, }
    
    public struct MoveInfo
    {
        public MoveType Type;
        public PegController JumpedPeg;
        public Position Position;
        public Vector3 EndPoint;
    };

    

    public class PegController : Component
    {
        private Nine.Graphics.Model _pegModel;
        Random r = new Random();

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


        public void SelectPeg()
        {
            ParticleEmitter emitter = (ParticleEmitter)Parent.Find<ParticleEffect>().Emitter;
            emitter.Transform = Matrix.CreateTranslation(Parent.Transform.Translation);
            emitter.Duration = 3f;
           
            
        }

        public void DeselectPeg()
        {
            ParticleEmitter emitter = (ParticleEmitter)Parent.Find<ParticleEffect>().Emitter;
            emitter.Duration = 0f;
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

        public void MoveTo(MoveInfo info, System.Action callback)
        {

            float newDirectionRadian = (float)Math.Atan2(-(info.EndPoint.X - Parent.Transform.Translation.X), -(info.EndPoint.Z - Parent.Transform.Translation.Z));

                Parent.Transform = Matrix.CreateRotationY(newDirectionRadian)
                    * Matrix.CreateTranslation(Parent.Transform.Translation);

            TweenAnimation<Matrix> moveAnimation = new TweenAnimation<Matrix>
                {
                    Target = Parent,
                    TargetProperty = "Transform",
                    Duration = TimeSpan.FromSeconds(1),
                    From = Parent.Transform,
                    To = Matrix.CreateRotationY(newDirectionRadian) * Matrix.CreateTranslation(info.EndPoint),
                    Curve = Curves.Smooth,
                };

            moveAnimation.Completed += new EventHandler((s, e) =>
                {
                    _pegModel.Animations["Run"].Stop();
                    _pegModel.Animations.Play("Idle");
                    Parent.Animations.Remove(Resources.AnimationNames.PegMove);
                    _graphicalPosition = info.EndPoint;
                    Position = info.Position;
                    callback();
                });
            if (info.Type != MoveType.PegJump)
            {

                Parent.Animations.Add("move", moveAnimation);
                Parent.Animations.Play("move");

                _pegModel.Animations.Play("Run");
            }
            else
            {
                

                Vector3 distance = info.EndPoint - Parent.Transform.Translation;
                distance /= 1.4f;

                Parent.Transform = Matrix.CreateRotationY(newDirectionRadian)
                    * Matrix.CreateTranslation(Parent.Transform.Translation);

                TweenAnimation<Matrix> killAnimation = new TweenAnimation<Matrix>
                {
                    Target = Parent,
                    TargetProperty = "Transform",
                    Duration = TimeSpan.FromSeconds(1),
                    From = Parent.Transform,
                    To = Matrix.CreateRotationY(newDirectionRadian) * Matrix.CreateTranslation(info.EndPoint - distance),
                    Curve = Curves.Smooth,
                };

                killAnimation.Completed += new EventHandler((s, e) =>
                {
                    
                    _pegModel.Animations["Chop"].Stop();
                    _pegModel.Animations["Attack"].Stop();
                     Parent.Animations.Remove("kill");
                     moveAnimation.From = Parent.Transform;
                     info.JumpedPeg.Die();
                     _pegModel.Animations.Play("Run");
                    Parent.Animations.Play("move");
                    _graphicalPosition = info.EndPoint;
                    Position = info.Position;
                });

                Parent.Animations.Add("move", moveAnimation);
                Parent.Animations.Add("kill", killAnimation);
                Parent.Animations.Play("kill");

                int nextValue = r.Next(2);
                if (nextValue == 0)
                {
                    _pegModel.Animations.Play("Attack");
                }
                else
                {
                    _pegModel.Animations.Play("Chop");
                }
            }
        }

        public void Die()
        {
            AnimationPlayer animationPlayer = _pegModel.Animations;

            ((animationPlayer.Play("Die") as BoneAnimation).Controllers.First() as BoneAnimationController).Repeat = 1;
            animationPlayer.Play("Die").Completed += new EventHandler((s, e) =>
            {
                Scene.FindName<Group>("GameBoard").Remove(Parent);
            });

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
