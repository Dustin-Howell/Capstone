using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nine;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Nine.Graphics;
using Creeper;

namespace XNAControlGame
{
    class BoardController : Component
    {
        private IProvideBoardState _boardProvider;
        private Instance _possibleModel;

        private PegController _clickedOnDown;
        private PegController _clickedOnUp;
        private PegController _moveToPeg;
        private PegController _pegToMove;

        public void ClickEvent(Nine.MouseEventArgs mouseState)
        {
            //Implement click logic.
            PegController _clickedOnDown = FindClickedPeg(new Vector2(mouseState.X, mouseState.Y));
            if (_clickedOnDown != null && _pegToMove == null)
            {
                //clear possible moves
                _pegToMove = _clickedOnDown;
                CreatePossibleMoves(_pegToMove.Position);
            }
            else if (_clickedOnDown != null && _pegToMove != null && _clickedOnDown.PegType == CreeperPegType.Possible)
            {
                _moveToPeg = _clickedOnDown;
                _pegToMove.MoveTo(new Move(_pegToMove.Position, _moveToPeg.Position, _boardProvider.GetCurrentPlayer().Color));
                ClearPossiblePegs();
                _pegToMove = null;
                _moveToPeg = null;
            }
            else
            {
                //deal with you later
            }
        }

        public BoardController(IProvideBoardState BoardProvider, Instance PossibleModel)
        {
            _possibleModel = PossibleModel;
            _boardProvider = BoardProvider;
        }

        private PegController FindClickedPeg(Vector2 mousePosition)
        {
            Camera MainCamera = Scene.FindName<Camera>("MainCamera");
            Ray selectionRay = Scene.GetGraphicsDevice().Viewport.CreatePickRay((int)mousePosition.X, (int)mousePosition.Y, MainCamera.View, MainCamera.Projection);

            List<PegController> found = new List<PegController>();
            Scene.FindAll<PegController>(new BoundingFrustum( MainCamera.View * MainCamera.Projection ), (x) => x.PegType == CreeperPegType.Possible || x.PegType.ToCreeperColor() == _boardProvider.GetCurrentPlayer().Color, found);

            foreach (PegController controller in found)
            {
                if ((controller.PegType.ToCreeperColor() == _boardProvider.GetCurrentPlayer().Color || controller.PegType == CreeperPegType.Possible) && controller.IsPegClicked(selectionRay))
                {
                    return controller;
                }
            }

            return null;
        }

        private void CreatePossibleMoves(Position selectedPeg)
        {
            CreeperColor team = _boardProvider.GetCurrentPlayer().Color;
            IEnumerable<Move> possibleMoves = _boardProvider.GetBoard().Pegs.At(selectedPeg).PossibleMoves(_boardProvider.GetBoard());

            foreach (Position position in possibleMoves.Select(x => x.EndPosition))
            {
                Group possiblePeg = _possibleModel.CreateInstance<Group>(Scene.ServiceProvider);
                possiblePeg.Transform = Matrix.CreateTranslation(CreeperBoardViewModel.GraphicalPositions[position.Row, position.Column]);
                possiblePeg.Add(new PegController(position, CreeperPegType.Possible));
                Scene.Add(possiblePeg);
            }
        }


        private void ClearPossiblePegs()
        {
            Camera MainCamera = Scene.FindName<Camera>("MainCamera");

            List<PegController> found = new List<PegController>();
            Scene.FindAll<PegController>(new BoundingFrustum(MainCamera.View * MainCamera.Projection), (x) => x.PegType == CreeperPegType.Possible || x.PegType.ToCreeperColor() == _boardProvider.GetCurrentPlayer().Color, found);

            foreach (PegController controller in found)
            {
                if (controller.PegType == CreeperPegType.Possible)
                {
                    Scene.Remove(controller.Parent);
                }
            }
        }
    }
}
