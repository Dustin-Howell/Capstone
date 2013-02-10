using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XNAControlGame;
using Creeper;
using CreeperCore;

namespace FireAndIce
{
    public partial class MainWindow : Window
    {
        private bool _debug = false;
        public MainWindow()
        {
            InitializeComponent();

            if (_debug)
            {
                this.Content = new MainMenuScreen();
            }

            else
            {
                this.Content = new SplashScreen(this);
            }
        }

        public void StartIntroScreen()
        {
            this.Content = new IntroScreen(this);
        }

        public void LoadMainMenu()
        {
            this.Content = new MainMenuScreen();
        }
    }
}
