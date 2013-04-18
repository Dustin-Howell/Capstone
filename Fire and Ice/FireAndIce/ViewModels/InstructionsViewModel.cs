using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace FireAndIce.ViewModels
{
    public class InstructionsViewModel : PropertyChangedBase
    {
        public String GoalText
        {
            get
            {
                return "The goal of Creeper is to connect your two corners of the board.";
            }
        }

        public String RuleText
        {
            get
            {
                return @"    1) You can only move your people.
    2) They can jump over a tile space to capture that space (it does not matter if the space is already captured).
    3) They can jump over an opponent's person to destroy that piece.
    4) They can move to any adjacent, unoccupied space.
    5) They can jump over the home base (this will have no effect on the base).
    

    ";
            }
        }
        public String EndGameText
        {
            get
            {
                return @"    1) How to Win
        -Connect your two corners on the board. (A connection must have adjacent touching tiles from one corner to the other)
   
    2) How to Tie
       - All of one teams pegs are captured.
       - A team has no move available to them on their turn.
    

    ";
            }
        }
    }
}
