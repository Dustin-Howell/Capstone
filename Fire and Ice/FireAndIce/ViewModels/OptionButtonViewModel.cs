using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CreeperSound;
using CreeperMessages;
using System.Windows;
using System.Windows.Forms;

namespace FireAndIce.ViewModels
{
    public class OptionButtonViewModel : PropertyChangedBase
    {
        public OptionButtonViewModel(bool isOptionChecked)
        {
            IsOptionChecked = isOptionChecked;
        }

        public OptionButtonViewModel() : this(false) {}

        public System.Action ClickAction { get; set; }
        public event EventHandler WasClicked;

        private bool _isOptionChecked;
        public bool IsOptionChecked
        {
            get
            {
                return _isOptionChecked;
            }

            set
            {
                if (_isOptionChecked != value)
                {
                    _isOptionChecked = value;
                    NotifyOfPropertyChange(() => IsOptionChecked);

                    // TODO: Fix this nastiness...
                    NotifyOfPropertyChange(() => CanDoSomething);
                }
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        public bool CanDoSomething { get { return !IsOptionChecked; } }

        public void DoSomething()
        {
            AppModel.EventAggregator.Publish(new SoundPlayMessage(SoundPlayType.MenuButtonClick));

            ClickAction();
            if (WasClicked != null)
            {
                WasClicked(this, null);
            }
        }

        public void MouseEnter()
        {
            AppModel.EventAggregator.Publish(new SoundPlayMessage(SoundPlayType.MenuButtonMouseOver));
        }
    }
}
