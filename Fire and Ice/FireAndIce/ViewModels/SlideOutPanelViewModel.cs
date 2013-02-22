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

namespace FireAndIce.ViewModels
{
    public class SlideOutPanelViewModel : Screen
    {
        public SolidColorBrush Background { get; set; }
        private double _width;
        public double Width
        {
            get
            {
                return
                  _width;
            }
            set
            {
                _width = value;
                NotifyOfPropertyChange(() => Width);
            }
        }

        private double _maxWidth;

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

        private void SlideOut()
        {
            double durationInMillis = 500;
            double lastElapsed = 0;
            BackgroundWorker animator = new BackgroundWorker();
            animator.DoWork += (s, e) => {
                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();

                while (Width < _maxWidth)
                {
                    double elapsed = stopwatch.Elapsed.Milliseconds - lastElapsed;
                    if (elapsed > 1)
                    {                        
                        Width += _maxWidth * (elapsed / durationInMillis);
                        lastElapsed = stopwatch.Elapsed.Milliseconds;
                    }
                }
            };

            animator.RunWorkerAsync();
        }

        public SlideOutPanelViewModel(IEnumerable<OptionButtonViewModel> buttons, 
            SolidColorBrush background,
            double maxWidth = 200)
        {
            _maxWidth = maxWidth;
            Buttons = new BindableCollection<OptionButtonViewModel>(buttons);
            Background = background;
            SlideOut();

            foreach (OptionButtonViewModel button in Buttons)
            {
                button.WasClicked += new EventHandler(_wasClicked);
            }

            OptionButtonView view = new OptionButtonView();
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
