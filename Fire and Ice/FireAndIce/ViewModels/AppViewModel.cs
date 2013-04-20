using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.IO;
using CreeperMessages;
using System.ComponentModel.Composition;
using FireAndIce.Views;

namespace FireAndIce.ViewModels
{
    [Export(typeof(AppViewModel))]
    public class AppViewModel : Conductor<Screen>.Collection.OneActive, IHandle<InitializeGameMessage>, IHandle<GameOverMessage>, IHandle<ResetMessage>, IHandle<ReturnToMenuMessage>, IHandle<PlayIntroScreenMessage>, IHandle<MainMenuMusicMessage>
    {
        private readonly IWindowManager _windowManager;

        [ImportingConstructor]
        public AppViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;

            AppModel.AppViewModel = this;
            AppModel.EventAggregator.Subscribe(this);

            //Yeah...not what best way to do this. Temporary, or permanent, depending on time. 
            String path = "Music";
            String soundFile = "\\";
            String actualFile = path + soundFile + "MenuMusic.mp3";
            PlayMusic = new Uri(actualFile, UriKind.Relative);
        }

        private Uri _playMusic;
        public Uri PlayMusic
        {
            get { return _playMusic; }
            set
            {
                _playMusic = value;
                NotifyOfPropertyChange(() => PlayMusic);
            }
        }

        AppView _appView;
        protected override void OnViewLoaded(object view)
        {
            _appView = view as AppView;

            base.OnViewLoaded(view);

            AppModel.EventAggregator.Publish(new PlayIntroScreenMessage());
        }

        public void Handle(InitializeGameMessage message)
        {
            ActivateItem(new GameContainerViewModel(message.Settings, AppModel.SlimCore));
            AppModel.EventAggregator.Publish(new MainMenuMusicMessage() { MusicState = MusicState.Stop });
        }         

        public void Handle(ResetMessage message)
        {
            message.EventAggregator.Subscribe(this);
        }

        public void Handle(GameOverMessage message)
        {
            _windowManager.ShowWindow(new GameOverViewModel(message.Winner));
        }

        public void Handle(ReturnToMenuMessage message)
        {
            ActivateItem(new MainMenuViewModel());

            AppModel.EventAggregator.Publish(new MainMenuMusicMessage() { MusicState = MusicState.Play });
        }

        public void Handle(PlayIntroScreenMessage message)
        {
            ActivateItem(new IntroScreenViewModel());
            AppModel.EventAggregator.Publish(new MainMenuMusicMessage() { MusicState = MusicState.Play });
        }

        public void Handle(MainMenuMusicMessage message)
        {
            if (message.MusicState == MusicState.Play)
            {
                _appView.GameMusic.Play();
            }
            else if (message.MusicState == MusicState.Stop)
            {
                _appView.GameMusic.Stop();
            }
            else if (message.MusicState == MusicState.Pause)
            {
                _appView.GameMusic.Pause();
            }
        }
    }
}
