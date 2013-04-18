using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace CreeperMessages
{
    public class EventDebugger : 
        IHandle<ChatMessage>, 
        IHandle<ConnectionStatusMessage>,
        IHandle<GameOverMessage>,
        IHandle<MoveMessage>,
        IHandle<NetworkErrorMessage>,
        IHandle<PlayIntroScreenMessage>,
        IHandle<ResetMessage>,
        IHandle<ReturnToMenuMessage>,
        IHandle<SoundPlayMessage>,
        IHandle<StartGameMessage>,
        IHandle<SychronizeBoardMessage>
    {
        public EventDebugger(EventAggregator agg)
        {
            agg.Subscribe(this);
        }

        public void Handle(ChatMessage message)
        {
            Console.WriteLine(message.GetType().ToString());
        }

        public void Handle(ConnectionStatusMessage message)
        {
            Console.WriteLine(message.GetType().ToString());
            
        }

        public void Handle(GameOverMessage message)
        {
            Console.WriteLine(message.GetType().ToString());
            
        }

        public void Handle(MoveMessage message)
        {
            Console.WriteLine(message.GetType().ToString());

            Console.WriteLine("-type: {0}", message.Type.ToString());
            Console.WriteLine("-player type: {0}", message.PlayerType.ToString());
            Console.WriteLine("-turn color: {0}", message.TurnColor.ToString());
        }

        public void Handle(NetworkErrorMessage message)
        {
            Console.WriteLine(message.GetType().ToString());
            
        }

        public void Handle(PlayIntroScreenMessage message)
        {
            Console.WriteLine(message.GetType().ToString());
            
        }

        public void Handle(ResetMessage message)
        {
            Console.WriteLine(message.GetType().ToString());
            message.EventAggregator.Subscribe(this);
        }

        public void Handle(ReturnToMenuMessage message)
        {
            Console.WriteLine(message.GetType().ToString());
            
        }

        public void Handle(SoundPlayMessage message)
        {
            Console.WriteLine(message.GetType().ToString());
            
        }

        public void Handle(StartGameMessage message)
        {
            Console.WriteLine(message.GetType().ToString());
            
        }

        public void Handle(SychronizeBoardMessage message)
        {
            Console.WriteLine(message.GetType().ToString());
            
        }
    }
}
