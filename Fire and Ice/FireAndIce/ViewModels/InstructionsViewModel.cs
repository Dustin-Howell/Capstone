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
                return "\tThe goal of Creeper is to connect your team's corners which are diagonally opposite eachother.";
            }
        }

        public String RuleText
        {
            get
            {
                return @"    Moving:
        >There are 3 types of moves your characters can make.
            1: Neutral
                -A neutral move is made between the squares of the board.
                -They do not have any effect on the board and are only used to repostion your characters.
                -Note: You cannot move to the far corner positions of the board.
            2: Capture
                -A capture move is made by jumping diagonally over a square on the board.
                -This move captures the square for your team and the square will change it's appearance accordingly.
            3: Attack
                -An attack move is made much like a neutral move.
                -When one of your characters is directly adjacent to a character from the opposing team, you can jump
                    over the opposing character and the character jumped will be removed from play.
                -You cannot attack a character if the position you must jump to in order to jump over the opposing
                    character is occupied.
                -You cannot jump your own team's characters.
    ";
            }
        }
        public String EndGameText
        {
            get
            {
                return @"    Winning:
        >There is only one way to win.
        >You must connect your teams corners through adjacently capture squares.
        >Note: diagonally adjacent tiles are not considered connected!

    Tie:
        >The game is considered a draw if either team loses all thier characters.
    ";
            }
        }
    }
}
