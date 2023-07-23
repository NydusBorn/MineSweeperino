using System;
using System.Collections.Generic;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using ABI.Microsoft.UI.Xaml.Input;
using Game;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
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
        private bool CurrentQuestionMarkPolicy;
        private Session CurrentSession;
        private List<List<Button>> DrawnTiles = new ();
        public GamePage()
        {
            this.InitializeComponent();
        }

        private void GamePage_OnLoaded(object sender, RoutedEventArgs e)
        {
            // CurrentSession = MainWindow.CurrentSession;
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "SELECT UseQuestionMarks FROM AppSettings";
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                CurrentQuestionMarkPolicy = (long)reader["UseQuestionMarks"] == 1;
            }
            InitialiseSession();
        }
        
        private void InitialiseSession()
        {
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "SELECT * FROM GameSettings";
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                CurrentSession = Session.GenerateDummy(int.Parse(reader["FieldWidth"].ToString()), int.Parse(reader["FieldHeight"].ToString()));
                UpdateGridSize();
                UpdateView();
            }
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
            //TODO: draw only the visible zone
            //TODO: make a Button buffer from which to source the button, initialise it at program start, use data about screen resolution to determine the amount of tiles to buffer
            //TODO: call this on window resize events
            ContentGrid.Children.Clear();
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
                        if (CurrentSession.PlayField.GetTileState(i,j).HasMine)
                        {
                            tile.Background = new SolidColorBrush(Color.FromArgb(120, 250 , 0, 0));
                            ImageIcon img = new ImageIcon();
                            img.Source = new BitmapImage(new Uri("ms-appx:///Assets/Indicators/Mine.png"));
                            tile.Content = img;
                        }
                        else
                        {
                            tile.Background = new SolidColorBrush(Color.FromArgb(60, 30 , 30, 30));
                        }
                        
                    }
                    else if (CurrentSession.PlayField.GetTileState(i,j).HasFlag)
                    {
                        tile.Background = new SolidColorBrush(Color.FromArgb(60, 250 , 0, 0));
                    }
                    else if (CurrentSession.PlayField.GetTileState(i, j).HasQuestionMark)
                    {
                        tile.Background = new SolidColorBrush(Color.FromArgb(60, 250 , 250, 250));
                    }
                    else
                    {
                        tile.Background = new SolidColorBrush(Color.FromArgb(60, 60, 60, 60));
                    }

                    text.FontSize = 25;
                    ContentGrid.Children.Add(tile);
                    Grid.SetColumn(tile, i);
                    Grid.SetRow(tile, j);
                    if (CurrentSession.IsDummySession)
                    {
                        tile.Click += StartSession;
                        tile.Click -= TileClick;
                        tile.PointerPressed -= TileMark;
                    }
                    else
                    {
                        tile.Click -= StartSession;
                        tile.Click += TileClick;
                        tile.PointerPressed += TileMark;
                    }
                    
                }
            }
        }
        
        private void StartSession(object sender, RoutedEventArgs e)
        {
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "SELECT * FROM GameSettings";
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                Button target = sender as Button;
                if (reader["UsePercentage"].ToString() == "1")
                {
                    CurrentSession = Session.GenerateFromPercentage(CurrentSession.PlayField.Width, CurrentSession.PlayField.Height, (double)reader["MinePercentage"], reader["GuaranteeSolution"].ToString() == "1", new Point(Grid.GetColumn(target), Grid.GetRow(target)));
                }
                else
                {
                    CurrentSession = Session.GenerateFromCount(CurrentSession.PlayField.Width, CurrentSession.PlayField.Height, int.Parse(reader["MineCount"].ToString()), reader["GuaranteeSolution"].ToString() == "1", new Point(Grid.GetColumn(target), Grid.GetRow(target)));
                }
                UpdateView();
            }
        }
        private void TileClick(object sender, RoutedEventArgs e)
        {
            var target = sender as Button;
            if (!CurrentSession.PlayField.GetTileState(Grid.GetColumn(target), Grid.GetRow(target)).HasFlag && !CurrentSession.PlayField.GetTileState(Grid.GetColumn(target), Grid.GetRow(target)).HasQuestionMark)
            {
                CurrentSession.Actor.OpenTile(Grid.GetColumn(target), Grid.GetRow(target));
                UpdateView();
            }
        }
        private void TileMark(object sender, PointerRoutedEventArgs e)
        {
            var target = sender as Button;
            var pressData = e.GetCurrentPoint(target);
            if (pressData.Properties.IsRightButtonPressed && !CurrentSession.PlayField.GetTileState(Grid.GetColumn(target), Grid.GetRow(target)).IsOpen)
            {
                CurrentSession.Actor.CycleMark(Grid.GetColumn(target), Grid.GetRow(target), CurrentQuestionMarkPolicy);
                UpdateView();
            }
        }

        private void RestartSession(object sender, RoutedEventArgs e)
        {
            InitialiseSession();
        }
    }
}
