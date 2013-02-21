using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace FireAndIce.ViewModels
{
    public class MainMenuViewModel : Conductor<Screen>.Collection.OneActive
    {
        private bool _newGameChecked;
        public bool NewGameChecked 
        {
            get
            {
                return _newGameChecked;
            }
            set
            {
                NotifyOfPropertyChange(() => NewGameChecked);
                _newGameChecked = value;
            }
        }

        public void NewGame()
        {
            ActivateItem(new SlideOutPanelViewModel(new List<OptionButtonViewModel> {
                new OptionButtonViewModel {ClickAction = () => StartLocalGame(), Title = "Local" },
            }));
        }

        private void StartLocalGame()
        {
            ActivateItem(new SlideOutPanelViewModel(new List<OptionButtonViewModel> {
                new OptionButtonViewModel {ClickAction = () => StartAIGame(), Title = "AI" },
            }));
        }

        private void StartAIGame()
        {
            throw new NotImplementedException();
        }

    }
}
