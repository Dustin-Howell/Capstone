using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Windows.Controls;
using FireAndIce.Views;
using CreeperMessages;

namespace FireAndIce.ViewModels
{
    class SplashScreenViewModel : Screen
    {
        protected override void OnViewLoaded(object view)
        {
            SplashScreenView splash = (SplashScreenView)view;
            splash.SplashScreen.MediaEnded += new System.Windows.RoutedEventHandler(SplashScreen_MediaEnded);

            splash.SplashScreen.Play();

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

            AppModel.EventAggregator.Publish(new PlayIntroScreenMessage());
        }
    }
}
