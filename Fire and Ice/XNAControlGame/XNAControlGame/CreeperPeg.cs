using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nine.Graphics;
using Creeper;
using Nine;

namespace XNAControlGame
{
    public enum CreeperPegType { Fire, Ice, Possible }

    public class CreeperPeg : Nine.Graphics.Model
    {
        public Position Position { get; set; }

        public CreeperPeg(Microsoft.Xna.Framework.Graphics.Model model, Position position)
            : base(model)
        {
            Position = position;
        }
    }
}
