using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using System.ComponentModel;
using Nine.Graphics;
using Microsoft.Xna.Framework;
using Nine.Graphics.Materials;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControlGame
{
    public class CreeperBoardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static string BoardProperty = CreeperUtility.GetPropertyName((CreeperBoardViewModel x) => x.Board);
        public CreeperBoard Board { get; private set; }

        // This should map abstract piece positions to spatial positions via array indices.
        public static Vector3[,] GraphicalPositions { get; private set; }

        public static string SelectedPieceProperty = CreeperUtility.GetPropertyName((CreeperBoardViewModel x) => x.SelectedPiece);
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

        public CreeperBoardViewModel(int surfaceHeight, int surfaceWidth, float surfaceStep)
        {
            GraphicalPositions = new Vector3[7,7];
            float boardHeight, boardWidth, squareWidth, squareHeight;
            boardHeight = surfaceHeight;
            boardWidth = surfaceWidth;
            squareHeight = (boardHeight / CreeperBoard.TileRows) * surfaceStep;
            squareWidth = (boardWidth / CreeperBoard.TileRows) * surfaceStep;
            Vector3 startCoordinates = new Vector3(0, 0, 0);

            for (int row = 0; row < 7; row++)
            {
                for (int column = 0; column < 7; column++)
                {
                    Vector3 pegCoordinates = new Vector3(startCoordinates.X + squareWidth * column, 0, startCoordinates.Y + squareHeight * row);
                    GraphicalPositions[row, column] = pegCoordinates;
                }
            }

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
