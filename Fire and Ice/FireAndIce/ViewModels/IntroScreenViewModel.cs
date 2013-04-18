using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CreeperMessages;
using FireAndIce.Views;

namespace FireAndIce.ViewModels
{
    class IntroScreenViewModel : Screen
    {
        protected override void OnViewLoaded(object view)
        {
            IntroScreenView splash = (IntroScreenView)view;
            splash.IntroScreen.MediaEnded += new System.Windows.RoutedEventHandler(SplashScreen_MediaEnded);

            splash.IntroScreen.Play();

            base.OnViewLoaded(view);
        }

        void SplashScreen_MediaEnded(object sender, System.Windows.RoutedEventArgs e)
        {
            PlayIntroScreen();
        }

        public void Skip()
        {
            PlayIntroScreen();
        }

        void PlayIntroScreen()
        {
            this.GetHashCode();

            AppModel.EventAggregator.Publish(new ReturnToMenuMessage());
        }
    }
}
