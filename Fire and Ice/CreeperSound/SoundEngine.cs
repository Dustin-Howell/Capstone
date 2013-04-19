using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Caliburn.Micro;
using CreeperMessages;
using System.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.ComponentModel;
using System.Threading;

namespace CreeperSound
{
    
    public class SoundEngine : IHandle<SoundPlayMessage>, IHandle<ResetMessage>
    {
        private BackgroundWorker _soundWorker;

        private static bool _muted = false;
        public static bool IsMuted
        {
            get
            {
                return _muted;
            }
            private set
            {
                _muted = value;
            }
        }

        public static void ToggleSound()
        {
            IsMuted = !IsMuted;
        }

        public static void ToggleSound(bool muted)
        {
            IsMuted = muted;
        }

        public SoundEngine(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }

        public void Handle(SoundPlayMessage message)
        {
            Thread t = new Thread(() =>
            {
                if (!_muted)
                {
                    String path = Path.GetFullPath("SoundAssets");
                    String soundFile = "\\";
                    SoundPlayer player;
                    bool sync = false;


                    switch (message.Type)
                    {
                        case SoundPlayType.Default:
                            soundFile += "default.wav";
                            break;
                        case SoundPlayType.MenuSlideOut:
                            soundFile += "MenuSlideOut.wav";
                            break;
                        case SoundPlayType.MenuButtonMouseOver:
                            soundFile += "MenuButtonMouseOver.wav";
                            break;
                        case SoundPlayType.MenuButtonClick:
                            soundFile += "MenuButtonClick.wav";
                            break;
                        case SoundPlayType.FirePegJump:
                            soundFile += "FireEffect.wav";
                            break;
                        case SoundPlayType.IcePegJump:
                            soundFile += "IceEffect.wav";
                            break;
                        case SoundPlayType.FireMove:
                            soundFile += "FireEffect2.wav";
                            break;
                        case SoundPlayType.IceMove:
                            soundFile += "IceEffect2.wav";
                            break;
                        case SoundPlayType.FireTileJump:
                            soundFile += "FireEffect3.wav";
                            break;
                        case SoundPlayType.IceTileJump:
                            soundFile += "IceEffect3.wav";
                            break;
                        default:
                            soundFile = null;
                            break;
                    }

                    if (soundFile != null)
                    {
                        player = new SoundPlayer(path + soundFile);

                        if (sync)
                            player.PlaySync();
                        else
                            player.Play();
                    }
                }
            });

           t.SetApartmentState(ApartmentState.STA);

            t.Start();
        }

        public void Handle(ResetMessage message)
        {
            message.EventAggregator.Subscribe(this);
        }
    }
}
