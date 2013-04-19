using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreeperMessages
{
    public enum InitComponent {Scene, Content}

    public class ComponentInitializedMessage
    {
        public InitComponent Component { get; set; }
    }
}