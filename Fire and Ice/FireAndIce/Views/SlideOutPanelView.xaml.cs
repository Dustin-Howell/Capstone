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
    /// <summary>
    /// Interaction logic for SlideOutPanelView.xaml
    /// </summary>
    public partial class SlideOutPanelView : UserControl
    {
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

                SlideOutPanel.RenderTransform = new ScaleTransform();
                SlideOutPanel.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            }
        }
    }
}
