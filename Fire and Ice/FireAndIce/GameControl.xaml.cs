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
        public GameControl(PlayerType playerType, PlayerType opponentType)
        {
            InitializeComponent();
            AppModel.Core.InitializeGameGUI(xnaControl.Handle, 1280, 720);
            AppModel.Core.StartLocalGame(playerType, opponentType);
        }
    }
}
