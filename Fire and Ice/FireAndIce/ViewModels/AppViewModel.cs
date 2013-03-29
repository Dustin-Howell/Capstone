using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.IO;
using CreeperMessages;

namespace FireAndIce.ViewModels
{
    public class AppViewModel : Conductor<Screen>.Collection.OneActive, IHandle<StartGameMessage>
    {
        public AppViewModel()
        {
            AppModel.AppViewModel = this;
            AppModel.EventAggregator.Subscribe(this);
            ActivateItem(new MainMenuViewModel());
        }

        public void Handle(StartGameMessage message)
        {
            ActivateItem(new GameContainerViewModel(message.Settings));
        }
    }
}
