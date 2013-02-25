using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.IO;

namespace FireAndIce.ViewModels
{
    public class AppViewModel : Conductor<Screen>.Collection.OneActive
    {
        public AppViewModel()
        {
            AppModel.AppViewModel = this;
            ActivateItem(new MainMenuViewModel());
        }
    }
}
