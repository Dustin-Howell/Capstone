using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.IO;
using CreeperMessages;
using System.ComponentModel.Composition;

namespace FireAndIce.ViewModels
{
    [Export(typeof(AppViewModel))]
    public class AppViewModel : Conductor<Screen>.Collection.OneActive, IHandle<StartGameMessage>, IHandle<GameOverMessage>
    {
        private readonly IWindowManager _windowManager;

        [ImportingConstructor]
        public AppViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;

            AppModel.AppViewModel = this;
            AppModel.EventAggregator.Subscribe(this);
            ActivateItem(new MainMenuViewModel());
        }

        public void Handle(StartGameMessage message)
        {
            ActivateItem(new GameContainerViewModel(message.Settings));
        }

        public void Handle(GameOverMessage message)
        {
            _windowManager.ShowDialog(new GameOverViewModel(message.Winner));
        }
    }
}
