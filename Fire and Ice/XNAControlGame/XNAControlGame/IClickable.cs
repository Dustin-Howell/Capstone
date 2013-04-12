using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace XNAControlGame
{
    interface IClickable
    {
        bool IsClicked(Ray selectionRay);
    }
}
