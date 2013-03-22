using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nine.Graphics;
using Creeper;
using Nine;
using Microsoft.Xna.Framework;

namespace XNAControlGame
{
    public enum CreeperPegType { Fire, Ice, Possible }

    public static class CreeperPegUtility
    {
        public static CreeperColor ToCreeperColor(this CreeperPegType creeperPegType)
        {
            switch (creeperPegType)
            {
                case CreeperPegType.Fire:
                    return CreeperColor.Fire;
                case CreeperPegType.Ice:
                    return CreeperColor.Ice;
                default:
                    throw new ArgumentOutOfRangeException("Can't let you do that, Star Fox.\nAndross has ordered us to take you down.");
            }
        }
    }

    public class CreeperPeg : Nine.Graphics.Model
    {
        private Position _position;
        public Position Position
        {
            get { return _position; }
            set
            {
                _position = value;
                DoTransform();
            }
        }
        public CreeperPegType PegType { get; set; }

        private void DoTransform()
        {
            Transform = Matrix.CreateScale(Resources.Models.PegScale)
                                       * Matrix.CreateTranslation(CreeperBoardViewModel.GraphicalPositions[Position.Row, Position.Column]);
        }

        public CreeperPeg(Microsoft.Xna.Framework.Graphics.Model model)
            : base(model)
        {
            
        }
    }
}
