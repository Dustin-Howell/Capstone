﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using XNAControl;
using CreeperCore;
using FireAndIce.Views;
using CreeperNetwork;

namespace FireAndIce.ViewModels
{
    class GameContainerViewModel : Screen
    {
        private PlayerType _player1Type;
        private PlayerType _player2Type;
        private Network _network;

        public GameContainerViewModel(PlayerType player1Type, PlayerType player2Type, Network network = null) : base()
        {
            _player1Type = player1Type;
            _player2Type = player2Type;
            _network = network;
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            XNAUserControl xnaView = (view as GameContainerView).XNAControl;

            AppModel.Core.InitializeGameGUI(xnaView.Handle, (int)xnaView.Width, (int)xnaView.Height);

            if (_network == null)
            {
                AppModel.Core.StartLocalGame(_player1Type, _player2Type);
            }
            else
            {
                AppModel.Core.StartNetworkGame(_player1Type, _player2Type, _network);
            }
        }

        public void Quit()
        {
            AppModel.AppViewModel.ActivateItem(new MainMenuViewModel());
        }
    }
}
