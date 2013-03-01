using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using XNAControl;
using CreeperCore;
using FireAndIce.Views;
using CreeperNetwork;
using System.Windows.Media;
using Creeper;

namespace FireAndIce.ViewModels
{
    class GameContainerViewModel : Screen
    {
        private PlayerType _player1Type;
        private PlayerType _player2Type;
        private Network _network;
        private AIDifficulty _aiDifficulty = AIDifficulty.Hard;

        //private SlideOutPanelViewModel _gameMenu;
        public SlideOutPanelViewModel GameMenu
        {
            get
            {
                return new SlideOutPanelViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel>()
                        {
                            new OptionButtonViewModel() { ClickAction = ReturnToMainMenu, Title = "Quit" },
                        },
                    Background = AppModel.Resources["Primary1"] as SolidColorBrush,
                    Title = "Game Menu",
                };
            }
        }

        public GameContainerViewModel(PlayerType player1Type, PlayerType player2Type, Network network = null) : base()
        {
            AppModel.ResetCreeperCore();

            _player1Type = player1Type;
            _player2Type = player2Type;
            _network = network;
        }

        public GameContainerViewModel(PlayerType player1Type, PlayerType player2Type, AIDifficulty difficulty)
            : base()
        {
            AppModel.ResetCreeperCore();

            _player1Type = player1Type;
            _player2Type = player2Type;
            _aiDifficulty = difficulty;
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            XNAUserControl xnaView = (view as GameContainerView).XNAControl;

            AppModel.Core.InitializeGameGUI(xnaView.Handle, (int)xnaView.Width, (int)xnaView.Height);

            if (_network == null)
            {
                AppModel.Core.StartLocalGame(_player1Type, _player2Type, _aiDifficulty);
            }
            else
            {
                AppModel.Core.StartNetworkGame(_player1Type, _player2Type, _network);
            }
        }

        public void ReturnToMainMenu()
        {
            AppModel.AppViewModel.ActivateItem(new MainMenuViewModel());
        }
    }
}
