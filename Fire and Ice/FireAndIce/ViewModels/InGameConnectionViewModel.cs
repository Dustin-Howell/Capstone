using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CreeperNetwork;
using System.ComponentModel;
using Creeper;
using CreeperMessages;
using System.Windows.Forms;
using System.Windows.Media;

namespace FireAndIce.ViewModels
{
    class InGameConnectionViewModel : PropertyChangedBase
    {
        static public Timer countdown = new Timer();
        static public Timer checkTimer = new Timer();
        int stillNotConnected = 0;

        public InGameConnectionViewModel(ConnectionStatusMessage message)
        {
            AppModel.EventAggregator.Subscribe(this);
            Handle(message);
        }

        public void ExitButtonActivate()
        {
            ThisWindow = System.Windows.Visibility.Hidden;
        }

        public void Handle(ConnectionStatusMessage message)
        {
            if (AppModel.Network.isGameRunning())
            {
                if (message.ErrorType == CONNECTION_ERROR_TYPE.CABLE_UNPLUGGED)
                {
                    connectionProblem("Lost Network Connection");
                }
                else if (message.ErrorType == CONNECTION_ERROR_TYPE.CABLE_RECONNECTED)
                {
                    connectionRestored("Connection Restored");
                }
                else if (message.ErrorType == CONNECTION_ERROR_TYPE.CONNECTION_LOST)
                {
                    connectionProblem("Opponent Connection Problem");
                }
                else if (message.ErrorType == CONNECTION_ERROR_TYPE.RECONNECTED)
                {
                    connectionRestored("Connection Restored");
                }
            }
        }

        private SolidColorBrush _background;
        public SolidColorBrush Background
        {
            get { return _background; }
            set
            {
                _background = value;
                NotifyOfPropertyChange(() => Background);
            }
        }

        private string _connectionIssueMessage;
        public string ConnectionIssueMessage
        {
            get { return _connectionIssueMessage; }
            set
            {
                _connectionIssueMessage = value;
                NotifyOfPropertyChange(() => ConnectionIssueMessage);
            }
        }

        private System.Windows.Visibility _exitButton;
        public System.Windows.Visibility ExitButton
        {
            get { return _exitButton; }
            set
            {
                _exitButton = value;
                NotifyOfPropertyChange(() => ExitButton);
            }
        }

        private System.Windows.Visibility _thisWindow;
        public System.Windows.Visibility ThisWindow
        {
            get { return _thisWindow; }
            set
            {
                _thisWindow = value;
                NotifyOfPropertyChange(() => ThisWindow);
            }
        }

        private int _currentTime;
        public int CurrentTime
        {
            get { return _currentTime; }
            set
            {
                _currentTime = value;
                NotifyOfPropertyChange(() => CurrentTime);
            }
        }

        public void connectionRestored(string message)
        {
            ConnectionIssueMessage = message;
            Background = new SolidColorBrush(Colors.Green);
            ExitButton = System.Windows.Visibility.Visible;

            checkTimer.Stop();
            countdown.Stop();

            CountDownVisible = System.Windows.Visibility.Collapsed;
        }

        private System.Windows.Visibility _countdownVisible;
        public System.Windows.Visibility CountDownVisible
        {
            get { return _countdownVisible; }
            set
            {
                _countdownVisible = value;
                NotifyOfPropertyChange(() => CountDownVisible);
            }
        }

        public void connectionProblem(string message)
        {
            if (stillNotConnected < 1)
            {
                stillNotConnected++;

                ConnectionIssueMessage = message;
                Background = new SolidColorBrush(Colors.Red);
                ExitButton = System.Windows.Visibility.Collapsed;

                CurrentTime = 20;

                countdown.Tick += new EventHandler((s, e) =>
                {
                    if (CurrentTime > 0)
                    {
                        CurrentTime--;
                    }
                });
                countdown.Interval = 1000;
                countdown.Enabled = true;

                checkTimer.Tick += new EventHandler((s, e) =>
                {
                    if (CurrentTime <= 1)
                    {
                        if (message == "Lost Network Connection")
                            AppModel.EventAggregator.Publish(new NetworkErrorMessage(NetworkErrorType.DisconnectMessage));
                        else if (message == "Opponent Connection Problem")
                            AppModel.EventAggregator.Publish(new NetworkErrorMessage(NetworkErrorType.OpponentDisconnectMessage));
                    }
                });
                checkTimer.Interval = 20000;
                checkTimer.Enabled = true;
            }
        }


    }
}