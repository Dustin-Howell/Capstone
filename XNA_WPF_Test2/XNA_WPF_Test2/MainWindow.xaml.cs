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

namespace XNA_WPF_Test2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        XNAControlGame.Game1 _game;

        public MainWindow()
        {
            InitializeComponent();

            _game = new XNAControlGame.Game1(xnaControl.Handle);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _game.Pause = !_game.Pause;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _game.PauseWithMethodCall();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _game.SpeedUp();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _game.SlowDown();

        }
    }
}
