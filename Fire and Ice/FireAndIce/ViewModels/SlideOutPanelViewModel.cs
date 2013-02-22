using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace FireAndIce.ViewModels
{
    public class SlideOutPanelViewModel : Screen
    {
        private BindableCollection<OptionButtonViewModel> _buttons;
        public BindableCollection<OptionButtonViewModel> Buttons
        {
            get
            {
                return _buttons;
            }

            private set
            {
                _buttons = value;
                NotifyOfPropertyChange(() => Buttons);
            }
        }

        public SlideOutPanelViewModel(IEnumerable<OptionButtonViewModel> buttons)
        {
            Buttons = new BindableCollection<OptionButtonViewModel>(buttons);

            foreach (OptionButtonViewModel button in Buttons)
            {
                button.WasClicked += new EventHandler(_wasClicked);
            }
        }

        private void _wasClicked(object o, EventArgs e)
        {
            foreach (OptionButtonViewModel button in Buttons)
            {
                if (button != o)
                {
                    button.IsOptionChecked = false;
                }
            }
        }
    }
}
