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
            foreach (Nine.Graphics.Model pegToRemove in _possiblePegs)
            {
                _boardGroup.Remove(pegToRemove);
            }
        }

        //private CreeperPeg _lastDownClickedModel;

        //CUT OUT???
        /*void DetectFullClick(Nine.MouseEventArgs e)
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

        //CUT OUT???
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
                        _SelectedPeg = clickedModel;
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
                        _SelectedPeg = null;
                        break;
                }
            }
        }*/


        public void UpdatePossibleMoves(CreeperPeg clickedPeg)
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

        //CUT OUT???
        /*private CreeperPeg GetClickedModel(Vector2 mousePosition)
        {
            Camera camera = _scene.FindName<Camera>("MainCamera");
            Ray selectionRay = GraphicsDevice.Viewport.CreatePickRay((int)mousePosition.X, (int)mousePosition.Y, camera.View, camera.Projection);

            List<CreeperPeg> found = new List<CreeperPeg>();
            _scene.FindAll<CreeperPeg>(ref selectionRay, (x) => x.PegType == CreeperPegType.Possible || x.PegType.ToCreeperColor() == BoardProvider.GetCurrentPlayer().Color, found);
            return found.FirstOrDefault();
        }*/

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
                if (piece.Color == CreeperColor.Fire)
                {
                    actualFireXamlFileStuff = _fireModel1.CreateInstance<Group>(_scene.ServiceProvider);
                    actualFireXamlFileStuff.Add(new PeonController());
                    actualFireXamlFileStuff.Transform = TransfomationMatrix(piece);
                    //_scene.Add(actualFireXamlFileStuff);
                }
                else
                {
                    actualIceXamlFileStuff = _iceModel1.CreateInstance<Group>(_scene.ServiceProvider);
                    actualIceXamlFileStuff.Add(new PeonController());
                    actualIceXamlFileStuff.Transform = TransfomationMatrix(piece);
                    //_scene.Add(actualIceXamlFileStuff);
                }

            } 
        }


        private Matrix TransfomationMatrix(Piece piece)
        {
            return Matrix.CreateRotationY(MathHelper.ToRadians(135))
                        * Matrix.CreateTranslation(CreeperBoardViewModel.GraphicalPositions[piece.Position.Row, piece.Position.Column]);
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
            ClearBoardTiles();

            //add all pegs
            LoadPegModels();

            //add all tiles
            LoadBoardTiles();
        }

        private void LoadBoardTiles()
        {
            foreach (Piece piece in BoardProvider.GetBoard().Tiles)
            {
                Surface check =  _scene.FindName<Surface>(piece.Position.Row.ToString() + 'x' + piece.Position.Column.ToString());
                if (piece.Color == CreeperColor.Fire)
                {
                    check.Material.Texture = _fireTile;
                    check.Material.Alpha = 1;
                }
                else if (piece.Color == CreeperColor.Ice)
                {
                    check.Material.Texture = _iceTile;
                    check.Material.Alpha = 1;
                }
            }
        }

        private void ClearBoardTiles()
        {
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    _scene.FindName<Surface>(row.ToString() + 'x' + col.ToString()).Material.Alpha = 0;
                }
            }
        }
    }
}
