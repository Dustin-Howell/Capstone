using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreeperMessages
{
    public enum SoundPlayType { Default, MenuButtonMouseOver, MenuButtonClick, MenuSlideOut, FirePegJump, IcePegJump, FireMove, IceMove, FireTileJump, IceTileJump, None }

    public class SoundPlayMessage
    {
        public SoundPlayType Type { get; set; }

        public SoundPlayMessage(SoundPlayType type)
        {
            Type = type;
        }
    }
}
