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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace FireAndIce
{
    /// <summary>
    /// Interaction logic for MainMenuScreen.xaml
    /// </summary>
    public partial class MainMenuScreen : UserControl
    {
        public MainMenuScreen()
        {
            InitializeComponent();
        }

        private void ToggleNewGameMenu()
        {
            bool isVisible = NewGameMenu.Visibility == System.Windows.Visibility.Visible;

            if (!isVisible)
            {
                //TODO: Animate Here
                NewGameMenu.Opacity = 0;
                NewGameMenu.Visibility = Visibility.Visible;

                DoubleAnimation fadeIn = new DoubleAnimation();
                fadeIn.From = 0;
                fadeIn.To = 1;
                fadeIn.Duration = new Duration(TimeSpan.FromMilliseconds(500));

                Storyboard.SetTargetName(fadeIn, NewGameMenu.Name);
                Storyboard.SetTargetProperty(fadeIn, new PropertyPath(Border.OpacityProperty));

                Storyboard storyBoard = new Storyboard();
                storyBoard.Children.Add(fadeIn);

                storyBoard.Begin(NewGameMenu);
            }
            else
            {
                NewGameMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            ToggleNewGameMenu();
        }
    }
}
