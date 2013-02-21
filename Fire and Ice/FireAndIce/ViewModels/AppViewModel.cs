using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace FireAndIce.ViewModels
{
    public class AppViewModel : Conductor<Screen>.Collection.OneActive
    {
        public AppViewModel()
        {
            ActivateItem(new MainMenuViewModel());
        }
    }
}
