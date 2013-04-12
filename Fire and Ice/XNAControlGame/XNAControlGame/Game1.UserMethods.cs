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
        private void ClearPossiblePegs()
        {
            foreach (CreeperPeg pegToRemove in _possiblePegs)
            {
                _boardGroup.Remove(pegToRemove);
            }
        }

        private CreeperPeg _lastDownClickedModel;
        void DetectFullClick(Nine.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                CreeperPeg clickedModel = GetClickedModel(new Vector2(e.MouseState.X, e.MouseState.Y));
                if (clickedModel != null)
                {
                    //if downclick
                    if (e.IsButtonDown(e.Button))
                    {
                        _lastDownClickedModel = clickedModel;
                    }
                    //if upclick
                    else if (_lastDownClickedModel == clickedModel)
                    {
                        _lastDownClickedModel = null;

                        if (clickedModel.PegType == CreeperPegType.Possible ||
                            BoardProvider.GetCurrentPlayer().Color == clickedModel.PegType.ToCreeperColor())
                        {
                            OnPegClicked(clickedModel);
                        }
                    }
                }
                else
                {
                    _SelectedPeg = null;
                }
            }
        }

        private void OnPegClicked(CreeperPeg clickedModel)
        {
            if (_SelectedPeg == clickedModel)
            {
                _selectedPeg.Attachments.Remove(_fireEffect);
                _SelectedPeg = null;
            }

            else
            {
                switch (clickedModel.PegType)
                {
                    case CreeperPegType.Fire:
                        goto case CreeperPegType.Ice;
                    case CreeperPegType.Ice:
                        if (_SelectedPeg != null )
                        {
                            _SelectedPeg.Attachments.Remove(_fireEffect);
                        }
                        _SelectedPeg = clickedModel;
                        _SelectedPeg.Attachments.Add(_fireEffect);
                        break;
                    case CreeperPegType.Possible:
                        _eventAggregator.Publish(
                            new MoveMessage()
                            {
                                PlayerType = PlayerType.Human,
                                Type = MoveMessageType.Response,
                                Move = new Move(
                                    _SelectedPeg.Position, clickedModel.Position,
                                    _SelectedPeg.PegType.ToCreeperColor()
                                )
                            }
                         );
                        _SelectedPeg.Attachments.Remove(_fireEffect);
                        _SelectedPeg = null;
                        break;
                }
            }
        }

        private void UpdatePossibleMoves(CreeperPeg clickedPeg)
        {
            ClearPossiblePegs();

            if (clickedPeg != null)
            {
                IEnumerable<Move> possibleMoves = BoardProvider.GetBoard().Pegs.At(clickedPeg.Position).PossibleMoves(BoardProvider.GetBoard());
                foreach (Position position in possibleMoves.Select(x => x.EndPosition))
                {
                    CreeperPeg peg = new CreeperPeg(_possibleModel)
                    {
                        Position = position,
                        PegType = CreeperPegType.Possible,
                    };

                    _boardGroup.Add(peg);
                }
            }
        }

        public void FlipTile(Position position, CreeperColor color)
        {
            Texture2D maskTexture = color.IsFire() ? _fireTileMask : _iceTileMask;

            Rectangle surfaceRect = new Rectangle(0, 0, (int)_boardSurface.Size.X, (int)_boardSurface.Size.Z);

            List<Texture2D> maskTextures = MaterialPaintGroup.GetMaskTextures(_boardMaterial).OfType<Texture2D>().ToList();
            Texture2D oldMask = maskTextures.First();

            float scale = (oldMask.Width / 6f) / maskTexture.Width;

            Vector2 texPosition = new Vector2(position.Column * (oldMask.Width / 6f), position.Row * (oldMask.Width / 6f))
                + new Vector2(maskTexture.Width * scale / 2);

            RenderTarget2D target = new RenderTarget2D(GraphicsDevice, oldMask.Width, oldMask.Height);

            GraphicsDevice.SetRenderTarget(target);

            SpriteBatch sb = new SpriteBatch(GraphicsDevice);

            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            GraphicsDevice.Clear(Color.Transparent);

            sb.Draw(oldMask,
                Vector2.Zero,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                1f);

            sb.Draw(maskTexture,
                texPosition,
                null,
                Color.White,
                0f,
                new Vector2(maskTexture.Width) / 2,
                scale,
                SpriteEffects.None,
                1f);

            sb.End();

            GraphicsDevice.SetRenderTarget(null);

            maskTextures[0].Dispose();
            maskTextures[0] = target;

            MaterialPaintGroup.SetMaskTextures(_boardMaterial, maskTextures);
        }

        private void SynchronizeTiles(CreeperBoard board)
        {
            Rectangle surfaceRect = new Rectangle(0, 0, (int)_boardSurface.Size.X, (int)_boardSurface.Size.Z);

            List<Texture2D> maskTextures = MaterialPaintGroup.GetMaskTextures(_boardMaterial).OfType<Texture2D>().ToList();

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

            MaterialPaintGroup.SetMaskTextures(_boardMaterial, maskTextures);
        }

        private CreeperPeg GetClickedModel(Vector2 mousePosition)
        {
            Camera camera = _scene.FindName<Camera>("MainCamera");
            Ray selectionRay = GraphicsDevice.Viewport.CreatePickRay((int)mousePosition.X, (int)mousePosition.Y, camera.View, camera.Projection);

            List<CreeperPeg> found = new List<CreeperPeg>();
            _scene.FindAll<CreeperPeg>(ref selectionRay, (x) => x.PegType == CreeperPegType.Possible || x.PegType.ToCreeperColor() == BoardProvider.GetCurrentPlayer().Color, found);
            return found.FirstOrDefault();
        }

        private void LoadViewModels()
        {
            _creeperBoardViewModel = new CreeperBoardViewModel(_scene.FindName<Surface>("boardSurface").Heightmap.Height, _scene.FindName<Surface>("boardSurface").Heightmap.Width, _scene.FindName<Surface>("boardSurface").Heightmap.Step);
        }

        MaterialGroup _boardMaterial;
        Surface _boardSurface;

        private void OnContentLoaded()
        {
            _boardGroup = _scene.FindName<Group>(Resources.ElementNames.BoardGroup);
            _boardGroup.Add(_moveAnimationListener);
            _boardSurface = _boardGroup.Find<Surface>();
            _boardMaterial = (MaterialGroup)_boardSurface.Material;

            LoadViewModels();
            LoadPegModels();
        }

        private void LoadPegModels()
        {
            foreach (Piece piece in BoardProvider.GetBoard().Pegs.Where(x => x.Color.IsTeamColor()))
            {
                if (piece.Color == CreeperColor.Fire)
                {
                    actualFireXamlFileStuff = _fireModel1.CreateInstance<Group>(_scene.ServiceProvider);
                    actualFireXamlFileStuff.Transform = Matrix.CreateTranslation(CreeperBoardViewModel.GraphicalPositions[piece.Position.Row, piece.Position.Column]);
                    _scene.Add(actualFireXamlFileStuff);
                }
                else
                {
                    actualIceXamlFileStuff = _iceModel1.CreateInstance<Group>(_scene.ServiceProvider);
                    actualIceXamlFileStuff.Transform = Matrix.CreateTranslation(CreeperBoardViewModel.GraphicalPositions[piece.Position.Row, piece.Position.Column]);
                    _scene.Add(actualIceXamlFileStuff);
                }
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
