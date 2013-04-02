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
    class MoveAnimationListener : GameComponent, IHandle<MoveMessage>
    {
        public bool IsAnimating { get; private set; }
        private ICreeperBoardLayout _boardLayout;
        public Queue<MoveMessage> _movesToAnimate;

        public MoveAnimationListener(ICreeperBoardLayout boardLayout) : base((Game)boardLayout)
        {
            IsAnimating = false;
            _boardLayout = boardLayout;
            _movesToAnimate = new Queue<MoveMessage>();
        }

        public override void Update(GameTime elapsedTime)
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
                        _boardLayout.FlipTile(message.Move);
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
            _movesToAnimate.Enqueue(message);
        }
    }
}
