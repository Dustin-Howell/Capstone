using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CreeperMessages;
using System.Diagnostics;

namespace CreeperCore
{
    public class CreeperTeacher : IHandle<MoveMessage>
    {
        public void Handle(MoveMessage message)
        {
            Debug.Assert(false, "NotImplemented");
        }
    }
}
