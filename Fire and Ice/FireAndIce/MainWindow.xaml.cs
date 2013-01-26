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

namespace FireAndIce
{
    public partial class MainWindow : Window
    {
        XNAControlGame.Game1 _game;

        public MainWindow()
        {
            InitializeComponent();

            _game = new XNAControlGame.Game1(xnaControl.Handle);
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
