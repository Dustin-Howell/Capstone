using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Nine;
using Nine.Components;
using Nine.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.ParticleEffects;
using Nine.Graphics.PostEffects;
using Nine.Graphics.Primitives;
using Nine.Physics;
using Creeper;
using Nine.Animations;

namespace XNAControlGame
{
    public class PegAnimation : Animation
    {
        private CreeperPeg _peg;
        private Position _startPostion;
        private Position _endPosition;
        private float _duration;

        public PegAnimation(CreeperPeg peg, Position startPostion, Position endPosition, float duration)
        {
            _peg = peg;
            _startPostion = startPostion;
            _endPosition = endPosition;
            _duration = duration;
        }

        public override void Update(float elapsedTime)
        {
            _duration -= elapsedTime;

        }
    }
}
