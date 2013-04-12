using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Nine;

namespace XNAControlGame
{
    class PegController : Component, IClickable
    {
        public CreeperPegType Type { get; set; }

        public void Destroy()
        {
            Scene.Remove(Parent);
        }

        public bool IsClicked(Ray selectionRay)
        {
            return selectionRay.Intersects(Parent.ComputeBounds()) != null;
        }
    }
}
