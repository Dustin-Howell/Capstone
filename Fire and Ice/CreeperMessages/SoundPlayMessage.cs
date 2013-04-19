﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreeperMessages
{
    public enum SoundPlayType {Default, MenuButtonMouseOver, MenuButtonClick, MenuSlideOut, FireMove, IceMove, FireMove2, IceMove2, FireMove3, IceMove3 }

    public class SoundPlayMessage
    {
        public SoundPlayType Type { get; set; }

        public SoundPlayMessage(SoundPlayType type)
        {
            Type = type;
        }
    }
}
