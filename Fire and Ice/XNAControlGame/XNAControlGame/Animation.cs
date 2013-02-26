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

namespace XNAControlGame
{
    class Animation
    {
        public Vector3 start;
        public Vector3 end;
        public Nine.Graphics.Model peg;
        public int xDirection;
        public int yDirection;

        public Animation(Vector3 start ,Vector3 end ,Nine.Graphics.Model peg, int xDirection, int yDirection)
        {
            this.start = start;
            this.end = end;
            this.peg = peg;
            this.xDirection = xDirection;
            this.yDirection = yDirection;
        }
    }
}
