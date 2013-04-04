using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreeperMessages
{
    public enum SoundPlayType {Default }

    public class SoundPlayMessage
    {
        public SoundPlayType Type { get; set; }

        public SoundPlayMessage(SoundPlayType type)
        {
            Type = type;
        }
    }
}
