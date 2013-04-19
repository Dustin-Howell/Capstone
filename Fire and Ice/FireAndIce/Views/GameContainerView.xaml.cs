using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireAndIce.Views
{
    public partial class GameContainerView
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            ChatMessages.SizeChanged += new System.Windows.SizeChangedEventHandler(ChatMessages_SizeChanged);
        }

        void ChatMessages_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            ChatMessagesScrollviewer.ScrollToEnd();
        }
    }
}
