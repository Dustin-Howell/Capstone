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
    class boardController : Component
    {
        protected override void Update(float elapsedTime)
        {
            //There's probably a better way to determine whether or not there was a click. This is just placeholder.
            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                FindClickedPeg(new Vector2(mouseState.X, mouseState.Y));
            }
            base.Update(elapsedTime);
        }

        private void FindClickedPeg(Vector2 mousePosition)
        {
            Camera MainCamera = Scene.FindName<Camera>("MainCamera");
            Ray selectionRay = Parent.GetGraphicsDevice().Viewport.CreatePickRay((int)mousePosition.X, (int)mousePosition.Y, MainCamera.View, MainCamera.Projection);
            //Give all the pegs the selection ray so they can see if they were clicked. I don't know how to do this yet. That'll be you Kaleb (or Gage).
            throw new NotImplementedException();
        }

        //This logic has been copied from the Game1 UserMethods. There is probably a better way to do this now.
        //For example, load the possible models by creating instances of a group in a xaml file?
        private void CreatePossibleMoves(Position selectedPeg)
        {
            //if (selectedPeg != null)
            //{
            //    IEnumerable<Move> possibleMoves = BoardProvider.GetBoard().Pegs.At(selectedPeg).PossibleMoves(BoardProvider.GetBoard());
            //    foreach (Position position in possibleMoves.Select(x => x.EndPosition))
            //    {
            //        CreeperPeg peg = new CreeperPeg(_possibleModel)
            //        {
            //            Position = position,
            //            PegType = CreeperPegType.Possible,
            //        };

            //        _boardGroup.Add(peg);
            //    }
            //}
        }
    }
}
