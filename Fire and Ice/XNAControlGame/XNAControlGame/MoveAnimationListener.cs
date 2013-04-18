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
        public Queue<MoveMessage> _movesToAnimate;

        public BoardController BoardController { get; set; }

        private IEventAggregator _eventAggregator;

        public MoveAnimationListener(IEventAggregator eventAggregator)
        {
            IsAnimating = false;
            _movesToAnimate = new Queue<MoveMessage>();
            _eventAggregator = eventAggregator;
        }

        protected override void Update(float elapsedTime)
        {
            if (BoardController != null && !IsAnimating && _movesToAnimate.Any())
            {
                MoveMessage message = _movesToAnimate.Dequeue();
                if (message.Type == MoveMessageType.Response)
                {
                    IsAnimating = true;
                    BoardController.Move(message.Move, message.TurnColor, () =>
                        { 
                            IsAnimating = false;
                            _eventAggregator.Publish(
                                new MoveMessage
                                {
                                    Type = MoveMessageType.MoveMade,
                                    Move = message.Move,
                                    PlayerType = message.PlayerType,
                                    TurnColor = message.TurnColor,
                            });
                        });
                }
            }

            base.Update(elapsedTime);
        }

        protected override void OnAdded(Group parent)
        {
            BoardController = parent.Find<BoardController>();
            base.OnAdded(parent);
        }

        protected override void OnRemoved(Group parent)
        {
            BoardController = null;
            base.OnRemoved(parent);
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
