using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreeperMessages
{
    public enum CONNECTION_ERROR_TYPE
    {
        CONNECTION_LOST,
        RECONNECTED,
        CABLE_UNPLUGGED,
        CABLE_RECONNECTED
    }

    public class ConnectionStatusMessage
    {
        public CONNECTION_ERROR_TYPE ErrorType { get; private set; }

        public ConnectionStatusMessage(CONNECTION_ERROR_TYPE errorType)
        {
            ErrorType = errorType;
        }
    }
}
