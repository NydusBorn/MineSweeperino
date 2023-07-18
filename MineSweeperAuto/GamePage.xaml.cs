using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using ABI.Microsoft.UI.Xaml.Input;
using Game;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Point = System.Drawing.Point;
using PointerRoutedEventArgs = Microsoft.UI.Xaml.Input.PointerRoutedEventArgs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MineSweeperAuto
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        private Session CurrentSession;
        public GamePage()
        {
            this.InitializeComponent();
        }

        private void GamePage_OnLoaded(object sender, RoutedEventArgs e)
        {
            // CurrentSession = MainWindow.CurrentSession;
            CurrentSession = Session.GenerateFromPercentage(10, 10, 0.1, true, new Point(5, 5));
            UpdateGridSize();
            UpdateView();
        }

        private void UpdateGridSize()
        {
            ContentGrid.RowDefinitions.Clear();
            ContentGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < CurrentSession.PlayField.Width; i++)
            {
                ContentGrid.ColumnDefinitions.Add(new ColumnDefinition(){ Width = new GridLength(50)});
            }

            for (int i = 0; i < CurrentSession.PlayField.Height; i++)
            {
                ContentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });
            }
        }

        private void UpdateView()
        {
            for (int i = 0; i < CurrentSession.PlayField.Width; i++)
            {
                for (int j = 0; j < CurrentSession.PlayField.Height; j++)
                {
                    Button tile = new Button();
                    tile.HorizontalAlignment = HorizontalAlignment.Stretch;
                    tile.VerticalAlignment = VerticalAlignment.Stretch;
                    tile.HorizontalContentAlignment = HorizontalAlignment.Center;
                    tile.VerticalContentAlignment = VerticalAlignment.Center;
                    TextBlock text = new TextBlock();
                    tile.Content = text;
                    text.Text = CurrentSession.PlayField.GetTileState(i,j).NeighboringMineCount.ToString();
                    if (text.Text == "0" || !CurrentSession.PlayField.GetTileState(i,j).IsOpen)
                    {
                        text.Text = "";
                    }
                    
                    if (CurrentSession.PlayField.GetTileState(i,j).IsOpen)
                    {
                        tile.Background = new SolidColorBrush(Color.FromArgb(60, 30 , 30, 30));
                    }
                    else
                    {
                        tile.Background = new SolidColorBrush(Color.FromArgb(60, 60, 60, 60));
                    }

                    text.FontSize = 25;
                    ContentGrid.Children.Add(tile);
                    Grid.SetColumn(tile, i);
                    Grid.SetRow(tile, j);
                    tile.PointerPressed += TileClick;
                }
            }
        }

        private void TileClick(object sender, PointerRoutedEventArgs e)
        {
            //TODO: make this work
            var target = sender as Button;
            var pressData = e.GetCurrentPoint(target);
            if (pressData.Properties.IsLeftButtonPressed)
            {
                CurrentSession.Actor.OpenTile(Grid.GetColumn(target), Grid.GetRow(target));
            }
        }

        private void RestartSession(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
