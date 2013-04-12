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

        public override void Update(float elapsedTime)
        {
            if (!IsAnimating && _movesToAnimate.Any())
            {
                MoveMessage message = _movesToAnimate.Dequeue();
                _boardLayout.AnimateMove(message.Move, () => IsAnimating = false);
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
