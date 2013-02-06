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
        protected XNAControlGame.Game1 _xnaGame;
        protected CreeperCore.CreeperCore _gameCore;
        protected CreeperBoard _board;

        public MainWindow()
        {
            InitializeComponent();
            _xnaGame = new XNAControlGame.Game1(/*xnaControl.Handle*/);
            _gameCore = new CreeperCore.CreeperCore(_xnaGame);
            _gameCore.Run();
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            _gameCore.StartGame();
        }
    }
}
