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

        Random _randomizeTiles = new Random();
        double FULL_CIRCLE_RADS = Math.PI / 2;
        public void FlipTile(Move move)
        {
            Surface s = _scene.Find<Surface>();
            Texture2D _dynamicMaskTexture = move.PlayerColor.IsFire() ? _fireTileMask : _iceTileMask;

            Rectangle surfaceRect = new Rectangle(0, 0, (int)s.Size.X, (int)s.Size.Z);
            Position position = CreeperBoard.GetFlippedPosition(move);

            List<Texture2D> texList = MaterialPaintGroup.GetMaskTextures((MaterialGroup)s.Material).OfType<Texture2D>().ToList();

            float scale = (texList[0].Width / 6f) / _dynamicMaskTexture.Width;
            Vector2 texPosition = new Vector2(position.Column * (texList[0].Width / 6f), position.Row * (texList[0].Width / 6f)) + new Vector2(_dynamicMaskTexture.Width * scale / 2);

            RenderTarget2D target = new RenderTarget2D(GraphicsDevice, texList[0].Width, texList[0].Height);

            GraphicsDevice.SetRenderTarget(target);

            SpriteBatch sb = new SpriteBatch(GraphicsDevice);

            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            GraphicsDevice.Clear(Color.Transparent);

            sb.Draw(texList[0],
                Vector2.Zero,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                1f);

            sb.Draw(_dynamicMaskTexture,
                texPosition,
                null,
                Color.White,
                0f,
                new Vector2(_dynamicMaskTexture.Width) / 2,
                scale,
                SpriteEffects.None,
                1f);

            sb.End();

            GraphicsDevice.SetRenderTarget(null);

            texList[0] = target;

            MaterialPaintGroup.SetMaskTextures((MaterialGroup)s.Material, texList);
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

        Texture2D _boardTexture;

        private void OnContentLoaded()
        {
            _boardGroup = _scene.FindName<Group>(Resources.ElementNames.BoardGroup);
            _boardTexture = _boardGroup.Find<Surface>().Material.Texture;

            LoadViewModels();
            LoadPegModels();
            //Prototype Specific Methods
            LoadTileSurfaces();
        }
        
        //For hacked in tiles for our testing prototype. Nothing more.
        private void LoadTileSurfaces()
        {
            int boardWidth = _boardGroup.FindName<Nine.Graphics.Surface>("boardSurface").Heightmap.Width;
            int squareWidth = (boardWidth / CreeperBoard.TileRows);

            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    Surface tile = new Surface(GraphicsDevice, 2, 108 / CreeperBoard.TileRows, 108 / CreeperBoard.TileRows, 2);
                    tile.Transform = Matrix.CreateTranslation(0 + col * squareWidth * 2, 2, 0 + row * squareWidth * 2);
                    tile.Name = row.ToString() + 'x' + col.ToString();
                    tile.Material = new BasicMaterial(GraphicsDevice) { Alpha = 0, IsTransparent = true, };
                    _scene.Add(tile);
                }
            }
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

            //remove all tiles

            //add all pegs
            LoadPegModels();

            //add all tiles
        }
    }
}
