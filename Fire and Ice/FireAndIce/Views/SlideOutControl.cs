using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace FireAndIce.Views
{
    public class SlideOutControl : UserControl
    {
        public Dock Dock { get; set; }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if (oldParent == null || oldParent as ContentPresenter != null)
            {
                DoubleAnimation animation = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500)),
                    EasingFunction = new QuadraticEase()
                };

                Point origin = new Point();

                switch (Dock)
                {
                    case Dock.Left:
                        origin.X = 0.0d;
                        origin.Y = 0.0d;
                        break;
                    case Dock.Right:
                        origin.X = 1.0d;
                        origin.Y = 0.0d;
                        break;
                    case Dock.Bottom:
                        throw new NotSupportedException();
                    case Dock.Top:
                        throw new NotSupportedException();
                    default:
                        origin.X = 0.0d;
                        origin.Y = 0.0d;
                        break;
                }

                RenderTransformOrigin = origin;

                RenderTransform = new ScaleTransform();
                RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            }
        }
    }
}
