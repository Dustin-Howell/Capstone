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
                NewGameMenu.Opacity = 1;
                NewGameMenu.Visibility = Visibility.Visible;

                DoubleAnimation slideIn = new DoubleAnimation();
                slideIn.From = 0;
                slideIn.To = 200;
                slideIn.Duration = new Duration(TimeSpan.FromMilliseconds(500));

                Storyboard.SetTargetName(slideIn, NewGameMenu.Name);
                Storyboard.SetTargetProperty(slideIn, new PropertyPath(Border.WidthProperty));

                Storyboard storyBoard = new Storyboard();
                storyBoard.Children.Add(slideIn);

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
