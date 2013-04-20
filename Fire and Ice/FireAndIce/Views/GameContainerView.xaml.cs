using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Threading;

namespace FireAndIce.Views
{
    public partial class GameContainerView
    {
        DoubleAnimation _animation;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            _animation = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = new Duration(new TimeSpan(0, 0, 0, 5, 0)),
                EasingFunction = new ExponentialEase() { Exponent = 0.1d },
            };

            _animation.Completed += new EventHandler((s, args) => InitialTurnContainer.Visibility = Visibility.Collapsed);
            ChatMessages.SizeChanged += new System.Windows.SizeChangedEventHandler(ChatMessages_SizeChanged);
            InitialTurnContainer.BeginAnimation(OpacityProperty, _animation);
        }

        void ChatMessages_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            ChatMessagesScrollviewer.ScrollToEnd();
        }
    }
}
