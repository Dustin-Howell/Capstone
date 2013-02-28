using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreeperNetwork
{
    public enum CONNECTION_ERROR_TYPE
    {
        CONNECTION_LOST,
        RECONNECTED
    }

    public class ConnectionEventArgs : EventArgs
    {
        public CONNECTION_ERROR_TYPE ERROR_TYPE { get; set; }

        public ConnectionEventArgs(CONNECTION_ERROR_TYPE errorTypeIn)
        {
            ERROR_TYPE = errorTypeIn;
        }
    }
}
