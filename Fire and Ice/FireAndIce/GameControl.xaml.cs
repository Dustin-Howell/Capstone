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
using CreeperCore;

namespace FireAndIce
{
    /// <summary>
    /// Interaction logic for GameControl.xaml
    /// </summary>
    public partial class GameControl : UserControl
    {
        //protected CreeperCore.CreeperCore _gameCore;
        protected CreeperGameCore _gameCore;
        protected CreeperBoard _board;
        protected XNAControlGame.Game1 _xnaGame;

        public GameControl(PlayerType playerType, PlayerType opponentType)
        {
            InitializeComponent();

            _xnaGame = new XNAControlGame.Game1(xnaControl.Handle, 1280, 720);
            //_gameCore = new CreeperCore.CreeperCore(_xnaGame);
            _gameCore = new CreeperGameCore(_xnaGame);
            _gameCore.StartLocalGame(playerType, opponentType);
        }
    }
}
