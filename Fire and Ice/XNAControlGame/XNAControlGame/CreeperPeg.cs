using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nine.Graphics;
using Creeper;
using Nine;
using Microsoft.Xna.Framework;
using Nine.Animations;

namespace XNAControlGame
{
    public enum CreeperPegType { Fire, Ice, Possible }

    public static class CreeperPegUtility
    {
        public static CreeperColor ToCreeperColor(this CreeperPegType creeperPegType)
        {
            switch (creeperPegType)
            {
                case CreeperPegType.Fire:
                    return CreeperColor.Fire;
                case CreeperPegType.Ice:
                    return CreeperColor.Ice;
                default:
                    throw new ArgumentOutOfRangeException("Can't let you do that, Star Fox.\nAndross has ordered us to take you down.");
            }
        }
    }

    public class CreeperPeg : Nine.Graphics.Model
    {
        private Position _position;
        public Position Position
        {
            get { return _position; }
            set
            {
                _position = value;
                DoTransform();
            }
        }

        private Position _destinationPosition;
        public CreeperPegType PegType { get; set; }

        private void DoTransform()
        {
            Transform = Matrix.CreateScale(Resources.Models.PegScale) * Matrix.CreateRotationY(MathHelper.ToRadians(180))
                        * Matrix.CreateTranslation(CreeperBoardViewModel.GraphicalPositions[Position.Row, Position.Column]);
        }

        public CreeperPeg(Microsoft.Xna.Framework.Graphics.Model model)
            : base(model)
        {
            _destinationPosition = Position;
        }

        public void MoveTo(Position position, Action callback)
        {
            TweenAnimation<Matrix> moveAnimation = new TweenAnimation<Matrix>()
            {
                Target = this,
                TargetProperty = "Transform",
                Duration = TimeSpan.FromSeconds(1),
                From = Transform,
                To = Matrix.CreateScale(Resources.Models.PegScale) * Matrix.CreateRotationY(MathHelper.ToRadians(180))
                        * Matrix.CreateTranslation(CreeperBoardViewModel.GraphicalPositions[position.Row, position.Column]),
                Curve = Curves.Smooth,
            };
            moveAnimation.Completed += new EventHandler((s,e) =>
                    {
                        Position = position;
                        Animations.Remove(Resources.AnimationNames.PegMove);
                        callback();
                    }
                );

            Animations.Add(Resources.AnimationNames.PegMove, moveAnimation);
            Animations.Play(Resources.AnimationNames.PegMove);
        }
    }
}
