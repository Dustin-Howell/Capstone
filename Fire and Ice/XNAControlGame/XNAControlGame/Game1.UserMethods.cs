﻿using System;
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
    public partial class Game1 : IDisposable, IHandle<MoveMessage>, IHandle<SychronizeBoardMessage>
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

        //void FlipTile(Move move)
        //{
        //    Piece tile = BoardProvider.GetBoard().GetFlippedTileCopy(move);

        //    int boardWidth = _boardTexture.Width;
        //    int squareWidth = (boardWidth / CreeperBoard.TileRows);

        //    Color[] texturePixels = new Color[boardWidth * boardWidth];
        //    Color color = (move.PlayerColor.IsBlack()) ? new Color(0, 0, 255) : new Color(255, 0, 0);

        //    _boardTexture.GetData(texturePixels);

        //    for (int i = tile.Position.Row * squareWidth; i < tile.Position.Row * squareWidth + squareWidth; i++)
        //    {
        //        for (int j = tile.Position.Column * squareWidth; j < tile.Position.Column * squareWidth + squareWidth; j++)
        //        {
        //            texturePixels[i * boardWidth + j] = color;
        //        }
        //    }

        //    _boardTexture.SetData(texturePixels);
        //}

        public void FlipTile(Move move)
        {
            Position position = CreeperBoard.GetFlippedPosition(move);

            string name = position.Row.ToString() + 'x' + position.Column.ToString();

            Surface jumped = _scene.FindName<Surface>(name);

            if (move.PlayerColor == CreeperColor.Fire)
            {
                jumped.Material.Texture = _fireTile;
            }
            else if (move.PlayerColor == CreeperColor.Ice)
            {
                jumped.Material.Texture = _iceTile;
            }

            jumped.Material.Alpha = 1;
        }

        /// <summary>
        /// Returns a ray fired from the click point to test for intersection with a model.
        /// </summary>
        Ray GetSelectionRay(Vector2 mouseCoor)
        {
            Camera camera = _scene.FindName<FreeCamera>("MainCamera");
            Vector3 nearsource = new Vector3(mouseCoor, 0f);
            Vector3 farsource = new Vector3(mouseCoor, 1f);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearsource, camera.Projection,
                    camera.View, world);

            Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farsource, camera.Projection,
                    camera.View, world);

            // Create a ray from the near clip plane to the far clip plane.
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            Ray pickRay = new Ray(nearPoint, direction);

            return pickRay;
        }

        private CreeperPeg GetClickedModel(Vector2 mousePosition)
        {
            Ray selectionRay = GetSelectionRay(mousePosition);
            List<CreeperPeg> currentTeam = ((BoardProvider.GetCurrentPlayer().Color == CreeperColor.Fire) ? _firePegs : _icePegs).ToList();
            currentTeam.AddRange(_possiblePegs);

            CreeperPeg clickedModel = null;

            foreach (CreeperPeg peg in currentTeam)
            {
                if (selectionRay.Intersects(peg.BoundingBox).HasValue)
                {
                    clickedModel = peg;
                    break;
                }
            }
            return clickedModel;
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

        public void Handle(MoveMessage message)
        {
        //    if (message.Type == MoveMessageType.Response)
        //    {
        //        if (CreeperBoard.IsCaptureMove(message.Move))
        //        {
        //            //capture
        //            _boardGroup.Remove(_pegs.First(x => x.Position == CreeperBoard.GetCapturedPegPosition(message.Move)));
        //            _pegs
        //                .First(x => x.Position == message.Move.StartPosition)
        //                .MoveTo(message.Move.EndPosition, () => _moveAnimationListener.IsAnimating = false);
        //        }
        //        else if (CreeperBoard.IsFlipMove(message.Move))
        //        {
        //            FlipTile(message.Move);
        //            _pegs
        //                .First(x => x.Position == message.Move.StartPosition)
        //                .MoveTo(message.Move.EndPosition, () => _moveAnimationListener.IsAnimating = false);
        //        }
        //        else
        //        {
        //            _pegs
        //                .First(x => x.Position == message.Move.StartPosition)
        //                .MoveTo(message.Move.EndPosition, () => _moveAnimationListener.IsAnimating = false);
        //        }

        //        _moveAnimationListener.IsAnimating = true;
        //    }
        }

        public void Handle(SychronizeBoardMessage message)
        {
            //throw new NotImplementedException("Undo functionality does not exist in Game1.UserMethods: Handle(SychronizedBoardMessage)");
            //remove all pegs

            //remove all tiles

            //add all pegs
            LoadPegModels();

            //add all tiles
        }
    }
}
