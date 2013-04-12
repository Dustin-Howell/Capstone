using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Creeper;
using Nine.Graphics;
using Caliburn.Micro;
using CreeperMessages;
using Nine;
using Nine.Graphics.ParticleEffects;
using Nine.Animations;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;

namespace XNAControlGame
{
    /// <summary>
    /// Any methods that are not overrides go here
    /// </summary>
    public partial class Game1 : IDisposable, IHandle<SychronizeBoardMessage>
    {


 

 



        private void SynchronizeTiles(CreeperBoard board)
        {
            Rectangle surfaceRect = new Rectangle(0, 0, (int)_boardSurface.Size.X, (int)_boardSurface.Size.Z);

            List<Texture2D> maskTextures = MaterialPaintGroup.GetMaskTextures((MaterialGroup)_boardSurface.Material).OfType<Texture2D>().ToList();

            RenderTarget2D target = new RenderTarget2D(GraphicsDevice, maskTextures[0].Width, maskTextures[0].Height);

            GraphicsDevice.SetRenderTarget(target);

            SpriteBatch sb = new SpriteBatch(GraphicsDevice);

            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            GraphicsDevice.Clear(new Color(1f, 0f, 0f, 0f));

            foreach (Piece piece in board.Tiles.Where((x) => x.Color.IsTeamColor()))
            {
                Texture2D maskTexture = piece.Color.IsFire() ? _fireTileMask : _iceTileMask;
                float scale = (target.Width / 6f) / maskTexture.Width;
                Vector2 position = new Vector2(piece.Position.Column * (target.Width / 6f), piece.Position.Row * (target.Width / 6f))
                    + new Vector2(maskTexture.Width * scale / 2);

                sb.Draw(maskTexture,
                    position,
                    null,
                    Color.White,
                    0f,
                    new Vector2(maskTexture.Width) / 2,
                    scale,
                    SpriteEffects.None,
                    1f);
            }

            sb.End();

            GraphicsDevice.SetRenderTarget(null);

            maskTextures[0].Dispose();
            maskTextures[0] = target;

            MaterialPaintGroup.SetMaskTextures((MaterialGroup)_boardSurface.Material, maskTextures);
        }



        private void LoadViewModels()
        {
            _creeperBoardViewModel = new CreeperBoardViewModel(_scene.FindName<Surface>("boardSurface").Heightmap.Height, _scene.FindName<Surface>("boardSurface").Heightmap.Width, _scene.FindName<Surface>("boardSurface").Heightmap.Step);
        }

        Texture2D _boardTexture;
        Surface _boardSurface;

        private void OnContentLoaded()
        {
            _boardGroup = _scene.FindName<Group>(Resources.ElementNames.BoardGroup);
            _boardSurface = _boardGroup.Find<Surface>();
            _boardTexture = _boardSurface.Material.Texture;

            LoadViewModels();
            LoadPegModels();
        }

        private void LoadPegModels()
        {
            foreach (Piece piece in BoardProvider.GetBoard().Pegs.Where(x => x.Color.IsTeamColor()))
            {
                CreeperPeg peg;
                if (piece.Color == CreeperColor.Fire)
                {
                    peg = new CreeperPeg(_fireModel)
                    {
                        PegType = CreeperPegType.Fire,
                        Position = piece.Position,
                    };

                }
                else
                {
                    peg = new CreeperPeg(_iceModel)
                    {
                        PegType = CreeperPegType.Ice,
                        Position = piece.Position,
                    };
                }
                
                _boardGroup.Add(peg);
            }
        }

        protected override void Dispose(bool disposing)
        {
            //dispose stuff

            base.Dispose(disposing);
        }

        public void Handle(SychronizeBoardMessage message)
        {
            //throw new NotImplementedException("Undo functionality does not exist in Game1.UserMethods: Handle(SychronizedBoardMessage)");
            //remove all pegs
            _pegs.Apply(x => _boardGroup.Children.Remove(x));

            //add all pegs
            LoadPegModels();
            SynchronizeTiles(message.Board);
        }
    }
}
