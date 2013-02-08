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
        public MainWindow()
        {
            InitializeComponent();
            this.Content = new GameControl();
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
