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
                //((UIElement)VisualParent).UpdateLayout();
                Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                DoubleAnimation animation = new DoubleAnimation()
                {
                    //From = -SlideOutPanel.DesiredSize.Width,
                    //To = 0,
                    From = 0,
                    To = 1,
                    Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500)),
                    EasingFunction = new BounceEase() { Bounciness = 6, Bounces = 1, }
                };

                //Storyboard.SetTargetProperty(animation, new PropertyPath(SlideOutPanelView.WidthProperty));
                //Storyboard widthStoryboard = new Storyboard() { Children = new TimelineCollection { animation } };
                //SlideOutPanel.BeginStoryboard(widthStoryboard);

                //SlideOutPanel.RenderTransform = new TranslateTransform();
                //SlideOutPanel.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);

                SlideOutPanel.RenderTransform = new ScaleTransform();
                SlideOutPanel.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            }
        }
    }
}
