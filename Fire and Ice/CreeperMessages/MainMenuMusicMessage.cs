using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreeperMessages
{
    public enum MusicState { Stop, Play, Pause }
    public class MainMenuMusicMessage
    {
        public MusicState MusicState { get; set; }
    }
}
