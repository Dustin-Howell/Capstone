using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FireAndIce.Views;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace FireAndIce.ViewModels
{
    public class ToggleButtonMenuViewModel : Screen
    {
        public ToggleButtonMenuViewModel MenuParent { get; set; }

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

        private SolidColorBrush _background;
        public SolidColorBrush Background
        {
            get
            {
                return _background = _background ?? new SolidColorBrush();
            }
            set
            {
                _background = value;
                NotifyOfPropertyChange(() => Background);
            }
        }

        public bool _controlIsVisible;
        public bool ControlIsVisible { get { return _controlIsVisible; } set { _controlIsVisible = value; NotifyOfPropertyChange(() => ControlIsVisible); } }

        private BindableCollection<OptionButtonViewModel> _buttons;
        public BindableCollection<OptionButtonViewModel> Buttons
        {
            get
            {
                return _buttons = _buttons ?? new BindableCollection<OptionButtonViewModel>();
            }

            set
            {
                if (_buttons != null)
                {
                    foreach (OptionButtonViewModel button in _buttons)
                    {
                        button.WasClicked -= new EventHandler(_wasClicked);
                    }
                }

                    _buttons = value;

                if (_buttons != null)
                {
                    foreach (OptionButtonViewModel button in _buttons)
                    {
                        button.WasClicked += new EventHandler(_wasClicked);
                    }
                }

                NotifyOfPropertyChange(() => Buttons);
            }
        }

        public ToggleButtonMenuViewModel() : base()
        {
            ControlIsVisible = false;
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
