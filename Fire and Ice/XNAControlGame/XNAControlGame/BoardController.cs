using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nine;
using Microsoft.Xna.Framework.Input;
using Creeper;
using Microsoft.Xna.Framework;
using Nine.Graphics;
using Caliburn.Micro;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;

namespace XNAControlGame
{
    public class BoardController : Component, ICreeperBoardLayout
    {
        private Input _input;
        private IEventAggregator _eventAggregator;
        private Group _boardGroup;
        private MoveAnimationListener _moveAnimationListener;

        private CreeperPeg _selectedPeg;
        private CreeperPeg _SelectedPeg
        {
            get
            {
                return _selectedPeg;
            }
            set
            {
                if (_selectedPeg != value)
                {
                    _selectedPeg = value;
                    UpdatePossibleMoves(value);
                }
            }
        }

        private IEnumerable<CreeperPeg> _pegs
        {
            get
            {
                return _boardGroup.Children
                    .Where(x => x.GetType() == typeof(CreeperPeg))
                    .Select(x => (CreeperPeg)x);
            }
        }
        private IEnumerable<CreeperPeg> _firePegs
        {
            get
            {
                return _pegs.Where(x => x.PegType == CreeperPegType.Fire);
            }
        }
        private IEnumerable<CreeperPeg> _icePegs
        {
            get
            {
                return _pegs.Where(x => x.PegType == CreeperPegType.Ice);
            }
        }
        private IEnumerable<CreeperPeg> _possiblePegs
        {
            get
            {
                return _pegs.Where(x => x.PegType == CreeperPegType.Possible);
            }
        }

        public IProvideBoardState BoardProvider { get; private set; }

        public BoardController(IProvideBoardState boardProvider, IEventAggregator eventAggregator)
        {
            BoardProvider = boardProvider;
            _eventAggregator = eventAggregator;
            _input = new Input();

            _input.MouseDown += new EventHandler<Nine.MouseEventArgs>((s, e) =>
            {
                if (BoardProvider.GetCurrentPlayer().Type == PlayerType.Human && !_moveAnimationListener.IsAnimating)
                {
                    DetectFullClick(e);
                }
            });
            _input.MouseUp += new EventHandler<Nine.MouseEventArgs>((s, e) =>
            {
                if (BoardProvider.GetCurrentPlayer().Type == PlayerType.Human && !_moveAnimationListener.IsAnimating)
                {
                    DetectFullClick(e);
                }
            });
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
        }

        private void UpdatePossibleMoves(CreeperPeg clickedPeg)
        {
            ClearPossiblePegs();

            if (clickedPeg != null)
            {
                IEnumerable<Move> possibleMoves = BoardProvider.GetBoard().Pegs.At(clickedPeg.Position).PossibleMoves(BoardProvider.GetBoard());
                foreach (Position position in possibleMoves.Select(x => x.EndPosition))
                {
                    Instance peg = new Instance("PossiblePeg");
                    Group group = peg.CreateInstance<Group>(Parent.ServiceProvider);
                    _boardGroup.Add(group);
                }
            }
        }

        private void ClearPossiblePegs()
        {
            foreach (CreeperPeg pegToRemove in _possiblePegs)
            {
                _boardGroup.Remove(pegToRemove);
            }
        }

        private IClickable GetClickedModel(Vector2 mousePosition)
        {
            Camera camera = Scene.FindName<Camera>("MainCamera");
            Ray selectionRay = Parent.GetGraphicsDevice().Viewport.CreatePickRay((int)mousePosition.X, (int)mousePosition.Y, camera.View, camera.Projection);

            List<IClickable> found = new List<IClickable>();
            Parent.Traverse<IClickable>(found);

            //Parent.FindAll<CreeperPeg>(ref selectionRay, (x) => x.PegType == CreeperPegType.Possible || x.PegType.ToCreeperColor() == BoardProvider.GetCurrentPlayer().Color, found);
            return found.FirstOrDefault((x) => x.IsClicked(selectionRay));
        }

        protected override void OnAdded(Group parent)
        {
            _boardGroup = parent;
            base.OnAdded(parent);
        }

        public Group BoardGroup { get { return _boardGroup; } }

        public IEnumerable<CreeperPeg> Pegs { get { return _pegs; } }

        public void FlipTile(Position position, CreeperColor color)
        {
            Texture2D maskTexture = color.IsFire() ? _fireTileMask : _iceTileMask;

            Rectangle surfaceRect = new Rectangle(0, 0, (int)_boardSurface.Size.X, (int)_boardSurface.Size.Z);

            List<Texture2D> maskTextures = MaterialPaintGroup.GetMaskTextures((MaterialGroup)_boardSurface.Material).OfType<Texture2D>().ToList();
            Texture2D oldMask = maskTextures.First();

            float scale = (oldMask.Width / 6f) / maskTexture.Width;

            Vector2 texPosition = new Vector2(position.Column * (oldMask.Width / 6f), position.Row * (oldMask.Width / 6f))
                + new Vector2(maskTexture.Width * scale / 2);

            RenderTarget2D target = new RenderTarget2D(Parent.GetGraphicsDevice(), oldMask.Width, oldMask.Height);

            Parent.GetGraphicsDevice().SetRenderTarget(target);

            SpriteBatch sb = new SpriteBatch(Parent.GetGraphicsDevice());

            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            Parent.GetGraphicsDevice().Clear(Color.Transparent);

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

            Parent.GetGraphicsDevice().SetRenderTarget(null);

            maskTextures[0].Dispose();
            maskTextures[0] = target;

            MaterialPaintGroup.SetMaskTextures((MaterialGroup)_boardSurface.Material, maskTextures);
        }
    }
}
