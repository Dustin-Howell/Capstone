using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace FireAndIce.ViewModels
{
    public class InstructionsViewModel : PropertyChangedBase
    {
        public String Title { get; set; }
        public String HelpText
        {
            get
            {
                return @"Creeper is a 2 layered board game where you try to connect your starting corner to your end corner which is located at the opposite corner on the board. The game becomes tricky as two players are trying to connect via the same path.
Tiles are only connected when you have them both adjacent to each other. Two tiles that are diagonal to each other are not connected.  
Each player takes turns moving one peg at a time. For example, Fire will move one peg. Then Ice will move one peg, then Fire, then so on and so forth until the game reaches its conclusion.
There are 3 types of moves that can be made:
A basic jump, this is where you move your piece to an open adjacent location. This does not remove anyone’s piece. Nor does it capture any territory. 
A capture jump, this is where you move your piece to an open diagonal location. The tile slot that you jumped over now belongs to your faction and can be used as a connecting piece.
And finally a destroying jump, this is where you get the chance to remove their pieces from the board. If they have a piece adjacent to you and the slot beyond them is open you can jump over their piece and that piece is now removed from the board.  You cannot jump them diagonally, nor can you jump your own pieces.
How to Win:
    1. Connect your two corners.
How to lose:
    1. Your opponent connects their two corners.
How to tie:
    1. All of one player’s pegs have been captured.
    2. A tie is agreed on because moves are just constantly repeated.

And the most important rule of any game is to have fun, cause if you don’t what’s the point?

";
            }
        }
    }
}
