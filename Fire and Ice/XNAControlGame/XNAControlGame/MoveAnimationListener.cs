using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nine;
using CreeperMessages;
using Creeper;
using Caliburn.Micro;
using Microsoft.Xna.Framework;

namespace XNAControlGame
{
    class MoveAnimationListener : Component, IHandle<MoveMessage>
    {
        public bool IsAnimating { get; private set; }
        private ICreeperBoardLayout _boardLayout;
        public Queue<MoveMessage> _movesToAnimate;

        public MoveAnimationListener(ICreeperBoardLayout boardLayout)
        {
            IsAnimating = false;
            _boardLayout = boardLayout;
            _movesToAnimate = new Queue<MoveMessage>();
        }

        protected override void Update(float elapsedTime)
        {
            if (!IsAnimating && _movesToAnimate.Any())
            {
                MoveMessage message = _movesToAnimate.Dequeue();
                if (message.Type == MoveMessageType.Response)
                {
                    IsAnimating = true;
                    if (CreeperBoard.IsCaptureMove(message.Move))
                    {
                        //capture
                        _boardLayout.BoardGroup.Remove(_boardLayout.Pegs.First(x => x.Position == CreeperBoard.GetCapturedPegPosition(message.Move)));
                        _boardLayout.Pegs
                            .First(x => x.Position == message.Move.StartPosition)
                            .MoveTo(message.Move.EndPosition, () => IsAnimating = false);
                    }
                    else if (CreeperBoard.IsFlipMove(message.Move))
                    {
                        _boardLayout.FlipTile(CreeperBoard.GetFlippedPosition(message.Move), message.Move.PlayerColor);
                        _boardLayout.Pegs
                            .First(x => x.Position == message.Move.StartPosition)
                            .MoveTo(message.Move.EndPosition, () => IsAnimating = false);
                    }
                    else
                    {
                        _boardLayout.Pegs
                            .First(x => x.Position == message.Move.StartPosition)
                            .MoveTo(message.Move.EndPosition, () => IsAnimating = false);
                    }
                }
            }

            base.Update(elapsedTime);
        }

        public void Handle(MoveMessage message)
        {
            if (message.Type == MoveMessageType.Response)
            {
                _movesToAnimate.Enqueue(message);
            }
        }
    }
}
