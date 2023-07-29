using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
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
        private Task UpdateTimer;
        private CancellationTokenSource cts = new();

        public GamePage()
        {
            this.InitializeComponent();
        }

        private void GamePage_OnLoaded(object sender, RoutedEventArgs e)
        {
            CurrentSession = MainWindow.CurrentSession;
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "SELECT UseQuestionMarks FROM AppSettings";
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                CurrentQuestionMarkPolicy = (long)reader["UseQuestionMarks"] == 1;
            }

            if (CurrentSession == null)
            {
                InitialiseSession();
            }
            else
            {
                UpdateGridSize();
                UpdateView();
                var timer = CurrentSession.GameTimer;
                TextBlockTime.Text =
                    $"{timer.Elapsed.Hours}:{timer.Elapsed.Minutes:D2}:{timer.Elapsed.Seconds:D2}:{timer.Elapsed.Milliseconds:D3}";
            }
            
            if (CurrentSession.CurrentState == Session.GameState.Active)
            {
                UpdateTimer = UpdateTimer_Tick(cts.Token);
            }
        }

        private void InitialiseSession()
        {
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "SELECT * FROM GameSettings";
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                CurrentSession = Session.GenerateDummy(int.Parse(reader["FieldWidth"].ToString()),
                    int.Parse(reader["FieldHeight"].ToString()));
                UpdateGridSize();
                UpdateView();
                MainWindow.CurrentSession = CurrentSession;
            }
        }

        private void UpdateGridSize()
        {
            ContentGrid.RowDefinitions.Clear();
            ContentGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < CurrentSession.PlayField.Width; i++)
            {
                ContentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(50) });
            }

            for (int i = 0; i < CurrentSession.PlayField.Height; i++)
            {
                ContentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });
            }
        }

        private async Task UpdateTimer_Tick(CancellationToken ct)
        {
            Stopwatch timer = CurrentSession.GameTimer;
            int counter = 0;
            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                await Task.Delay(10);
                if (CurrentSession.CurrentState == Session.GameState.Active)
                {
                    TextBlockTime.Text =
                        $"{timer.Elapsed.Hours}:{timer.Elapsed.Minutes:D2}:{timer.Elapsed.Seconds:D2}:{timer.Elapsed.Milliseconds:D3}";
                }

                counter += 1;
                if (counter == 100)
                {
                    // call update view
                    counter = 0;
                }
            }
        }

        public const int BufferLine = 4;
        private Dictionary<(int, int), Button> DrawnButtons = new();

        private async void UpdateView()
        {
            //TODO: check for light theme on custom icons in the clock and mine counter
            if (ScrollViewerVisibleArea.ViewportHeight == 0)
            {
                await Task.Delay(50);
            }
            int markedTiles = 0;
            int mineWidth = (int)(ContentGrid.RowSpacing + ContentGrid.RowDefinitions[0].Height.Value);
            int leftViewBound = (int)ScrollViewerVisibleArea.HorizontalOffset / mineWidth;
            leftViewBound = Math.Max(0, leftViewBound - (BufferLine / 2));
            int rightViewBound =
                (int)(ScrollViewerVisibleArea.HorizontalOffset +
                      (ScrollViewerVisibleArea.ViewportWidth / ScrollViewerVisibleArea.ZoomFactor)) / mineWidth;
            rightViewBound = Math.Min(CurrentSession.PlayField.Width - 1, rightViewBound + (BufferLine / 2));
            int upperViewBound = (int)ScrollViewerVisibleArea.VerticalOffset / mineWidth;
            upperViewBound = Math.Max(0, upperViewBound - (BufferLine / 2));
            int lowerViewBound =
                (int)(ScrollViewerVisibleArea.VerticalOffset +
                      (ScrollViewerVisibleArea.ViewportHeight / ScrollViewerVisibleArea.ZoomFactor)) / mineWidth;
            lowerViewBound = Math.Min(CurrentSession.PlayField.Height - 1, lowerViewBound + (BufferLine / 2));
            bool unchekedTiles = false;
            for (int i = 0; i < CurrentSession.PlayField.Width; i++)
            {
                for (int j = 0; j < CurrentSession.PlayField.Height; j++)
                {
                    if (i >= leftViewBound && i <= rightViewBound && j >= upperViewBound && j <= lowerViewBound)
                    {
                        Button tile;
                        if (DrawnButtons.ContainsKey((i, j)))
                        {
                            tile = DrawnButtons[(i, j)];
                        }
                        else
                        {
                            DrawnButtons.Add((i, j), new Button()
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                HorizontalContentAlignment = HorizontalAlignment.Center,
                                VerticalContentAlignment = VerticalAlignment.Center
                            });
                            tile = DrawnButtons[(i, j)];
                            Grid.SetColumn(tile, i);
                            Grid.SetRow(tile, j);
                            ContentGrid.Children.Add(tile);
                        }

                        if (tile.Content is not TextBlock && !CurrentSession.PlayField.GetTileState(i, j).HasFlag)
                        {
                            tile.Content = new TextBlock()
                            {
                                Text = CurrentSession.PlayField.GetTileState(i, j).NeighboringMineCount == 0
                                    ? ""
                                    : CurrentSession.PlayField.GetTileState(i, j).NeighboringMineCount.ToString(),
                                FontSize = 25
                            };
                        }

                        if (!CurrentSession.PlayField.GetTileState(i, j).IsOpen &&
                            !CurrentSession.PlayField.GetTileState(i, j).HasFlag)
                        {
                            tile.Content = null;
                        }

                        //TODO: light theme
                        if (CurrentSession.PlayField.GetTileState(i, j).IsOpen)
                        {
                            if (CurrentSession.PlayField.GetTileState(i, j).HasMine)
                            {
                                tile.Background = new SolidColorBrush(Color.FromArgb(120, 250, 0, 0));
                                tile.Content = new Image()
                                    { Source = new BitmapImage(new Uri("ms-appx:///Assets/Indicators/Mine.png")) };
                            }
                            else
                            {
                                tile.Background = new SolidColorBrush(Color.FromArgb(60, 30, 30, 30));
                            }
                        }
                        else if (CurrentSession.PlayField.GetTileState(i, j).HasFlag &&
                                 ((tile.Background as SolidColorBrush).Color.A != 60 ||
                                  (tile.Background as SolidColorBrush).Color.R != 250 ||
                                  (tile.Background as SolidColorBrush).Color.G != 0 ||
                                  (tile.Background as SolidColorBrush).Color.B != 0))
                        {
                            //TODO: flag symbol
                            tile.Background = new SolidColorBrush(Color.FromArgb(60, 250, 0, 0));
                        }
                        else if (CurrentSession.PlayField.GetTileState(i, j).HasQuestionMark &&
                                 ((tile.Background as SolidColorBrush).Color.A != 60 ||
                                  (tile.Background as SolidColorBrush).Color.R != 250 ||
                                  (tile.Background as SolidColorBrush).Color.G != 250 ||
                                  (tile.Background as SolidColorBrush).Color.B != 250))
                        {
                            //TODO: question mark symbol
                            tile.Background = new SolidColorBrush(Color.FromArgb(60, 250, 250, 250));
                        }
                        else if (!CurrentSession.PlayField.GetTileState(i, j).HasQuestionMark && !CurrentSession.PlayField.GetTileState(i, j).HasFlag && !CurrentSession.PlayField.GetTileState(i, j).IsOpen &&
                        ((tile.Background as SolidColorBrush).Color.A != 60 ||
                         (tile.Background as SolidColorBrush).Color.R != 60 ||
                         (tile.Background as SolidColorBrush).Color.G != 60 ||
                         (tile.Background as SolidColorBrush).Color.B != 60))
                        {
                            tile.Background = new SolidColorBrush(Color.FromArgb(60, 60, 60, 60));
                        }

                        tile.Click -= StartSession;
                        tile.Click -= TileClick;
                        tile.PointerPressed -= TileMark;
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

                    var curTile = CurrentSession.PlayField.GetTileState(i, j);
                    if (curTile.HasFlag)
                    {
                        markedTiles += 1;
                    }
                    
                    if (!curTile.IsOpen && !curTile.HasFlag)
                    {
                        unchekedTiles = true;
                    }
                }
            }

            if (!unchekedTiles && markedTiles == CurrentSession.MineCount)
            {
                CurrentSession.WinGame();
                var dialog = new ContentDialog();
                dialog.XamlRoot = this.XamlRoot;
                dialog.Title = "You Win!";
                dialog.Content = $"Congratulations! You have won in {TimeFormat(CurrentSession.GameTimer.Elapsed)}!";
                dialog.IsPrimaryButtonEnabled = true;
                dialog.PrimaryButtonText = "Close";
                dialog.PrimaryButtonClick += (s, e) => s.Hide();
                dialog.ShowAsync();
            }
            
            TextBlockMines.Text = $"{CurrentSession.MineCount - markedTiles}/{CurrentSession.MineCount}";
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
                    CurrentSession = Session.GenerateFromPercentage(CurrentSession.PlayField.Width,
                        CurrentSession.PlayField.Height, (double)reader["MinePercentage"],
                        reader["GuaranteeSolution"].ToString() == "1",
                        new Point(Grid.GetColumn(target), Grid.GetRow(target)));
                }
                else
                {
                    CurrentSession = Session.GenerateFromCount(CurrentSession.PlayField.Width,
                        CurrentSession.PlayField.Height, int.Parse(reader["MineCount"].ToString()),
                        reader["GuaranteeSolution"].ToString() == "1",
                        new Point(Grid.GetColumn(target), Grid.GetRow(target)));
                }

                UpdateView();
                UpdateTimer = UpdateTimer_Tick(cts.Token);
                MainWindow.CurrentSession = CurrentSession;
            }
        }

        private string TimeFormat(TimeSpan time)
        {
            string result = "";
            if (time.Hours == 1)
            {
                result += $"{time.Hours} hour ";
            }
            else if (time.Hours > 1)
            {
                result += $"{time.Hours} hours ";
            }

            if (time.Minutes == 1)
            {
                result += $"{time.Minutes} minute ";
            }
            else if (time.Minutes > 1)
            {
                result += $"{time.Minutes} minutes ";
            }
            
            if (time.Seconds == 1)
            {
                result += $"{time.Seconds} second ";
            }
            else if (time.Seconds > 1)
            {
                result += $"{time.Seconds} seconds ";
            }
            
            if (time.Milliseconds == 1)
            {
                result += $"{time.Milliseconds} millisecond ";
            }
            else if (time.Milliseconds > 1)
            {
                result += $"{time.Milliseconds} milliseconds ";
            }
            
            return result;
        }
        
        private void TileClick(object sender, RoutedEventArgs e)
        {
            var target = sender as Button;
            if (!CurrentSession.PlayField.GetTileState(Grid.GetColumn(target), Grid.GetRow(target)).HasFlag &&
                !CurrentSession.PlayField.GetTileState(Grid.GetColumn(target), Grid.GetRow(target)).HasQuestionMark &&
                CurrentSession.CurrentState == Session.GameState.Active)
            {
                if (CurrentSession.Actor.OpenTile(Grid.GetColumn(target), Grid.GetRow(target)))
                {
                    CurrentSession.LoseGame();
                    var dialog = new ContentDialog();
                    dialog.XamlRoot = this.XamlRoot;
                    dialog.Title = "You Lost!";
                    dialog.Content = $"Unfortunately, You have lost in {TimeFormat(CurrentSession.GameTimer.Elapsed)}!";
                    dialog.IsPrimaryButtonEnabled = true;
                    dialog.PrimaryButtonText = "Close";
                    dialog.PrimaryButtonClick += (s, e) => s.Hide();
                    dialog.ShowAsync();
                }
                UpdateView();
            }
        }

        private void TileMark(object sender, PointerRoutedEventArgs e)
        {
            var target = sender as Button;
            var pressData = e.GetCurrentPoint(target);
            if (pressData.Properties.IsRightButtonPressed && !CurrentSession.PlayField
                    .GetTileState(Grid.GetColumn(target), Grid.GetRow(target)).IsOpen &&
                CurrentSession.CurrentState == Session.GameState.Active)
            {
                CurrentSession.Actor.CycleMark(Grid.GetColumn(target), Grid.GetRow(target), CurrentQuestionMarkPolicy);
                UpdateView();
            }
        }

        private void RestartSession(object sender, RoutedEventArgs e)
        {
            InitialiseSession();
            cts.Cancel();
            cts = new();
        }

        private Windows.Foundation.Point PreviousPosition = new(-999999, -999999);

        private async void ScrollViewerVisibleArea_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            ScrollViewerVisibleArea.PointerMoved -= ScrollViewerVisibleArea_OnPointerMoved;
            var pointer = e.GetCurrentPoint(ScrollViewerVisibleArea);
            if (pointer.Properties.IsMiddleButtonPressed)
            {
                if (PreviousPosition != new Windows.Foundation.Point(-999999, -999999))
                {
                    double distanceHorizontal = pointer.Position.X - PreviousPosition.X;
                    double distanceVertical = pointer.Position.Y - PreviousPosition.Y;
                    ScrollViewerVisibleArea.ChangeView(ScrollViewerVisibleArea.HorizontalOffset - distanceHorizontal,
                        ScrollViewerVisibleArea.VerticalOffset - distanceVertical,
                        ScrollViewerVisibleArea.ZoomFactor);
                    await Task.Delay(10);
                }
            }

            PreviousPosition = pointer.Position;
            ScrollViewerVisibleArea.PointerMoved += ScrollViewerVisibleArea_OnPointerMoved;
        }

        private void ScrollViewerVisibleArea_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            UpdateView();
        }
    }
}