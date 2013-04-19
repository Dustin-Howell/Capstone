using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public interface IProvideBoardState
    {
        CreeperBoard GetBoard();
        Player GetCurrentPlayer();
        Stack<CreeperBoard> BoardHistory { get; }
    }
}
