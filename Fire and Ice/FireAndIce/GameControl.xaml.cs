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
using Creeper;

namespace FireAndIce
{
    /// <summary>
    /// Interaction logic for GameControl.xaml
    /// </summary>
    public partial class GameControl : UserControl
    {
        protected CreeperCore.CreeperCore _gameCore;
        protected CreeperBoard _board;
        protected XNAControlGame.Game1 _xnaGame;

        public GameControl()
        {
            InitializeComponent();
            _xnaGame = new XNAControlGame.Game1(xnaControl.Handle, 800, 600);//(int)xnaControl.ActualWidth, (int)xnaControl.ActualHeight);
            _gameCore = new CreeperCore.CreeperCore(_xnaGame);
            _gameCore.Run();
        }
    }
}
