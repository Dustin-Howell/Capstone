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
using System.Timers;

namespace FireAndIce
{
    /// <summary>
    /// Interaction logic for IntroScreen.xaml
    /// </summary>
    public partial class IntroScreen : UserControl
    {
        private MainWindow _mainWindow;

        public IntroScreen(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            IntroVideo.Play();
            IntroVideo.MediaEnded += new RoutedEventHandler(IntroVideo_MediaEnded);
        }

        private void SkipToMainMenu()
        {
            _mainWindow.LoadMainMenu();
        }

        void IntroVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            SkipToMainMenu();
        }

        private void IntroVideo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SkipToMainMenu();
        }
    }
}
