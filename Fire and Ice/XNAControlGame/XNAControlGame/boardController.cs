using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nine;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Nine.Graphics;
using Creeper;
using Nine.Graphics.Primitives;
using Caliburn.Micro;
using CreeperMessages;

namespace XNAControlGame
{
    public static class BoardHelper
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

        public static bool IsTeamOrPossible(this CreeperPegType type, CreeperColor color)
        {
            return type == CreeperPegType.Possible || type.ToCreeperColor() == color;
        }
    }

    class BoardController : Component, IHandle<MoveMessage>, IHandle<GameOverMessage>
    {
        public IProvideBoardState BoardProvider { get; set; }
        public Action<Move> PublishMove { get; set; }
        public Action<Position, CreeperColor> FlipTile { get; set; }
        public CreeperBoardViewModel ViewModel { get; set; }

        public Random random = new Random();

        private Instance _firePossibleModel;
        private Instance _icePossibleModel;

        private PegController _selectedPeg;
        private PegController _SelectedPeg
        {
            get
            {
                return _selectedPeg;
            }
            set
            {

                if (_selectedPeg != value)
                {
                    
                    if (value != null)
                    {
                        value.SelectPeg();
                    }
                    
                    if (_selectedPeg != null)
                    {
                        _selectedPeg.DeselectPeg();
                    }
                    
                    _selectedPeg = value;
                    UpdatePossibleMoves(value);
                   if (value != null)
                    {
                        value.SelectPeg();
                    }
                }
            }
        }

        public BoardController(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _firePossibleModel = new Instance { Template = "FirePossiblePeg" };
            _icePossibleModel = new Instance { Template = "IcePossiblePeg" };
        }

        protected override void OnAdded(Group parent)
        {
            base.OnAdded(parent);

            _eventAggregator.Publish(new ComponentInitializedMessage() { Component = InitComponent.Scene, });
        }

        public void Move(Move move, CreeperColor turnColor, System.Action callback)
        {
            List<PegController> pegs = new List<PegController>();
            Scene.Traverse(pegs);
            PegController peg = pegs.First(x => x.Position == move.StartPosition);

            System.Action wrapCallback = callback;
            MoveType moveType = MoveType.Normal;
            if (CreeperBoard.IsFlipMove(move))
            {
                moveType = MoveType.TileJump;
                wrapCallback = () =>
                {
                    FlipTile(CreeperBoard.GetFlippedPosition(move), move.PlayerColor);
                    callback();
                };
            }
            else if (CreeperBoard.IsCaptureMove(move))
            {
                moveType = MoveType.PegJump;
            }

            PlaySoundEffect(moveType, turnColor);

            MoveInfo info = new MoveInfo
            {
                Position = move.EndPosition,
                EndPoint = ViewModel.GraphicalPositions[move.EndPosition.Row, move.EndPosition.Column],
                Type = moveType,
                JumpedPeg = (moveType == MoveType.PegJump) ? pegs.First(x => x.Position == CreeperBoard.GetCapturedPegPosition(move)) : null,
            };

            peg.MoveTo(info, wrapCallback);
        }

        private void PlaySoundEffect(MoveType type, CreeperColor color)
        {
            SoundPlayType sound = SoundPlayType.None;
            if (color.IsTeamColor())
            {
                switch (type)
                {
                    case MoveType.Normal:
                        sound = color.IsFire() ? SoundPlayType.FireMove : SoundPlayType.IceMove;
                        break;
                    case MoveType.PegJump:
                        sound = color.IsFire() ? SoundPlayType.FirePegJump : SoundPlayType.IcePegJump;
                        break;
                    case MoveType.TileJump:
                        sound = color.IsFire() ? SoundPlayType.FireTileJump : SoundPlayType.IceTileJump;
                        break;
                }
            }
            _eventAggregator.Publish(new SoundPlayMessage(sound));
        }

        private void ClearPossiblePegs()
        {
            Camera MainCamera = Scene.FindName<Camera>("MainCamera");

            List<PegController> found = new List<PegController>();
            Scene.FindAll<PegController>(new BoundingFrustum(MainCamera.View * MainCamera.Projection), (x) => x.PegType == CreeperPegType.Possible, found);

            foreach (PegController controller in found)
            {
                if (controller.PegType == CreeperPegType.Possible)
                {
                    Scene.Remove(controller.Parent);
                }
            }
        }

        public void DetectFullClick(Nine.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PegController clickedModel = GetClickedModel(new Vector2(e.MouseState.X, e.MouseState.Y));
                if (clickedModel != null && clickedModel.PegType.IsTeamOrPossible(BoardProvider.GetCurrentPlayer().Color))
                {
                    //if downclick
                    if (e.IsButtonDown(e.Button))
                    {
                        OnPegClicked(clickedModel);
                    }
                }
                else
                {
                    
                    _SelectedPeg = null;
                }
            }
        }

        private void OnPegClicked(PegController clickedModel)
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
                        PublishMove(new Move(new Position(_SelectedPeg.Position), new Position(clickedModel.Position), _SelectedPeg.PegType.ToCreeperColor()));
                        
                        _SelectedPeg = null;
                        break;
                }
            }
        }

        private PegController GetClickedModel(Vector2 mousePosition)
        {
                Camera camera = Scene.FindName<Camera>("MainCamera");
                Ray selectionRay = Scene.GetGraphicsDevice().Viewport.CreatePickRay((int)mousePosition.X, (int)mousePosition.Y, camera.View, camera.Projection);

                List<PegController> found = new List<PegController>();
                if (Scene.ComputeBounds().Intersects(selectionRay).HasValue)
                {
                    Scene.Traverse<PegController>(found);
                }

                return found.FirstOrDefault(x => x.IsPegClicked(selectionRay));
        }

        private void UpdatePossibleMoves(PegController clickedPeg)
        {
            ClearPossiblePegs();

            if (clickedPeg != null)
            {
                IEnumerable<Move> possibleMoves = BoardProvider.GetBoard().Pegs.At(clickedPeg.Position).PossibleMoves(BoardProvider.GetBoard());
                foreach (Position position in possibleMoves.Select(x => x.EndPosition))
                {
                    PegController peg = new PegController()
                    {
                        Position = position,
                        PegType = CreeperPegType.Possible,
                    };

                    Group group;
                    if (BoardProvider.GetCurrentPlayer().Color == CreeperColor.Fire)
                    {
                        group = _firePossibleModel.CreateInstance<Group>(Game1.ServiceProvider);
                    }
                    else
                    {
                        group = _icePossibleModel.CreateInstance<Group>(Game1.ServiceProvider);
                    }

                    group.Add(peg);
                    group.Transform = Matrix.CreateTranslation(ViewModel.GraphicalPositions[position.Row, position.Column]);
                    Scene.Add(group);
                    //RefreshBoardGroup();
                }
            }
        }

        private void RefreshBoardGroup()
        {
            Scene.Remove(Parent);
            Scene.Add(Parent);
        }

        public void SynchronizePegs(CreeperBoard creeperBoard)
        {
            List<PegController> found = new List<PegController>();
            Scene.Traverse<PegController>(found);
            found.Apply((x) => Scene.Children.Remove(x.Parent));
        }

        public void Handle(MoveMessage message)
        {
            List<PegController> pegs = new List<PegController>();
            Scene.Traverse(pegs);

            if (message.Type == MoveMessageType.Request)
            {                
                foreach (PegController peg in pegs.Where(x => x.PegType != CreeperPegType.Possible && x.PegType.ToCreeperColor().Opposite() == message.TurnColor))
                {
                    peg.EndIdle();
                }

                foreach (PegController peg in pegs.Where(x => x.PegType != CreeperPegType.Possible && x.PegType.ToCreeperColor() == message.TurnColor))
                {
                    peg.StartIdle();
                }
            }
        }

        public void Handle(GameOverMessage message)
        {
            List<PegController> pegs = new List<PegController>();
            Scene.Traverse(pegs);

                if (message.Winner == CreeperColor.Fire || message.Winner == CreeperColor.Ice)
                {
                
                    foreach (PegController peg in pegs.Where(x => x.PegType != CreeperPegType.Possible && x.PegType.ToCreeperColor() == message.Winner))
                    {

                        peg.Victory(random.Next(4));
                        
                    }

                    foreach (PegController peg in pegs.Where(x => x.PegType != CreeperPegType.Possible && x.PegType.ToCreeperColor() != message.Winner))
                    {
                        peg.DieEndGame();
                    }
                }
                else
                {
                    foreach (PegController peg in pegs)
                    {
                        peg.DieEndGame();
                    }
                }
            
        }

        private IEventAggregator _eventAggregator;
    }
}
