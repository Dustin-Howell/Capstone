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
using System.Windows.Controls.Primitives;

namespace FireAndIce
{
    /// <summary>
    /// Interaction logic for MainMenuScreen.xaml
    /// </summary>
    public partial class MainMenuScreen : UserControl
    {
        private Border _activeBorder1;
        private Border _activeBorder2;
        private MainWindow _mainWindow;

        public MainMenuScreen(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void SlideOutBorder(Border border, double width)
        {
            border.Opacity = 1;
            border.Visibility = Visibility.Visible;

            DoubleAnimation slideAnimation = new DoubleAnimation();
            slideAnimation.From = 0;
            slideAnimation.To = width;
            slideAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(500));

            Storyboard.SetTargetName(slideAnimation, border.Name);
            Storyboard.SetTargetProperty(slideAnimation, new PropertyPath(Border.WidthProperty));

            Storyboard storyBoard = new Storyboard();
            storyBoard.Children.Add(slideAnimation);

            storyBoard.Begin(border);
        }

        private void ToggleBorder(Border border, int borderLevel, double width)
        {
            if (borderLevel == 1)
            {
                if (_activeBorder1 == border)
                {
                    _activeBorder1.Visibility = Visibility.Collapsed;
                    if (_activeBorder2 != null)
                    {
                        _activeBorder2.Visibility = Visibility.Collapsed;
                    }
                    _activeBorder1 = null;
                    _activeBorder2 = null;
                }
                else
                {
                    if (_activeBorder1 != null)
                    {
                        _activeBorder1.Visibility = Visibility.Collapsed;
                    }
                    if (_activeBorder2 != null)
                    {
                        _activeBorder2.Visibility = Visibility.Collapsed;
                    }

                    SlideOutBorder(border, width);
                    _activeBorder1 = border;
                }
            }
            if (borderLevel == 2)
            {
                if (_activeBorder2 == border)
                {
                    _activeBorder2.Visibility = Visibility.Collapsed;
                    _activeBorder2 = null;
                }
                else
                {
                    if (_activeBorder2 != null)
                    {
                        _activeBorder2.Visibility = Visibility.Collapsed;
                    }

                    SlideOutBorder(border, width);
                    _activeBorder2 = border;
                }
            }
        }

        private void ToggleButtons(ToggleButton activeButton, StackPanel buttonPanel)
        {
            IEnumerable<ToggleButton> toggles = buttonPanel.Children.OfType<ToggleButton>();
            foreach (ToggleButton toggle in toggles)
            {
                if (toggle != activeButton)
                {
                    toggle.IsChecked = false;
                }
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            ToggleBorder(NewGameMenu, 1, 200d);
            ToggleButtons(NewGameButton, MainMenuButtonPanel);
        }

        private void HighScoreButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleBorder(HighScoreBorder, 1, 400d);
            ToggleButtons(HighScoreButton, MainMenuButtonPanel);
            //Initialize high scores here
            HighScoreList.ItemsSource = new List<String> { "score 1", "score 2", "score 3" };
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleBorder(SettingsBorder, 1, 400d);
            ToggleButtons(SettingsButton, MainMenuButtonPanel);
        }

        private void CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleBorder(CreditsBorder, 1, 400d);
            ToggleButtons(CreditsButton, MainMenuButtonPanel);
        }

        private void PlayHumanGameButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleBorder(NewHumanGameMenu, 2, 200d);
            ToggleButtons(PlayHumanGameButton, ChooseOpponentButtonPanel);
        }

        private void PlayComputerGameButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleBorder(NewComputerGameMenu, 2, 200d);
            ToggleButtons(PlayComputerGameButton, ChooseOpponentButtonPanel);
        }

        private void PlayLocalHumanGameButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.Content = new GameControl();
        }

        private void PlayNetworkedHumanGameButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlayNoviceComputerButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlayExpertComputerButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleBorder(HelpBorder, 1, 200d);
            ToggleButtons(HelpButton, MainMenuButtonPanel);
        }

        private void HelpRulesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HelpTutorialButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
