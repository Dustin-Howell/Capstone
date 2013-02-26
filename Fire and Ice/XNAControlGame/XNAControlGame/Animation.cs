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

namespace XNAControlGame
{
    class Animation
    {
        public Vector3 startLocation;
        public Vector3 endLocation;
        public Nine.Graphics.Model peg;
        public int xDirection;
        public int yDirection;
        public Position startCoord;
        public Position endCoord;

        public Animation(Vector3 startLocation ,Vector3 endLocation ,Nine.Graphics.Model peg, int xDirection, int yDirection, Position startCoord, Position endCoord)
        {
            this.startLocation = startLocation;
            this.endLocation = endLocation;
            this.peg = peg;
            this.xDirection = xDirection;
            this.yDirection = yDirection;
            this.startCoord = startCoord;
            this.endCoord = endCoord;

        }
    }
}
