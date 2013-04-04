using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Caliburn.Micro;
using CreeperMessages;
using System.Media;

namespace CreeperSound
{
    
    public class SoundEngine : IHandle<SoundPlayMessage>
    {
        public SoundEngine(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }


        public void Handle(SoundPlayMessage message)
        {
            String path = Path.GetFullPath("..\\..\\..\\CreeperSound\\SoundAssets");
            SoundPlayer player = new SoundPlayer(path + "\\default.wav");

            player.Play();
        }
    }
}
