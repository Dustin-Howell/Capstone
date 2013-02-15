using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using System.ComponentModel;

namespace XNAControlGame
{
    class CreeperBoardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public CreeperBoard Board { get; private set; }

        // This should map abstract piece positions to spatial positions via array indices.
        public Position[,] GraphicalPositions { get; private set; }

        public Piece SelectedPiece
        {
            get
            {
                return _selectedPiece;
            }
            set
            {
                _selectedPiece = value;
                OnPropertyChanged("SelectedPiece");
            }
        }

        CreeperBoardViewModel(CreeperBoard board)
        {
            Board = board;
            _selectedPiece = null;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private Piece _selectedPiece;
    }
}
