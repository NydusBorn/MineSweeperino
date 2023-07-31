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
        /// <summary>
        /// Indicates whether to use question marks
        /// </summary>
        private bool CurrentQuestionMarkPolicy;
        /// <summary>
        /// Current session
        /// </summary>
        private Session CurrentSession;
        /// <summary>
        /// Timer for updating the clock and indicate regular ui refresh
        /// </summary>
        private Task UpdateTimer;
        /// <summary>
        /// Required for cancellation of the timer when the new one is requested 
        /// </summary>
        private CancellationTokenSource cts = new();

        public GamePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Loads settings and determines whether to reinitialise the session
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            if (CurrentSession == null || CurrentSession.CurrentState == Session.GameState.Initialised)
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

        /// <summary>
        /// Makes a dummy session and makes it currentsession
        /// </summary>
        private void InitialiseSession()
        {
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "SELECT * FROM GameSettings";
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                DrawnButtons.Clear();
                CurrentSession = Session.GenerateDummy(int.Parse(reader["FieldWidth"].ToString()),
                    int.Parse(reader["FieldHeight"].ToString()));
                UpdateGridSize();
                Task.Delay(50);
                UpdateView();
                MainWindow.CurrentSession = CurrentSession;
            }
        }

        /// <summary>
        /// Sets the grid to be the same size as the playfield
        /// </summary>
        private void UpdateGridSize()
        {
            ContentGrid.Children.Clear();
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

        /// <summary>
        /// Updates the clock and once a second causes a ui refresh
        /// </summary>
        /// <param name="ct"></param>
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
                    UpdateView();
                    counter = 0;
                }
            }
        }

        /// <summary>
        /// How far out of viewport to draw tiles
        /// </summary>
        public const int BufferLine = 2;
        /// <summary>
        /// Buttons that are already drawn are stored in this variable
        /// </summary>
        private Dictionary<(int, int), Button> DrawnButtons = new();

        /// <summary>
        /// Draws mines and updates the themes when required, checks for win condition
        /// </summary>
        private async void UpdateView()
        {
            if (ScrollViewerVisibleArea.ViewportHeight == 0)
            {
                await Task.Delay(50);
            }

            int markedTiles = 0;
            int mineWidth = (int)(ContentGrid.RowSpacing + ContentGrid.RowDefinitions[0].Height.Value);
            int leftViewBound = (int)ScrollViewerVisibleArea.HorizontalOffset / mineWidth;
            leftViewBound = Math.Max(0, leftViewBound - BufferLine);
            int rightViewBound =
                (int)(ScrollViewerVisibleArea.HorizontalOffset +
                      (ScrollViewerVisibleArea.ViewportWidth / ScrollViewerVisibleArea.ZoomFactor)) / mineWidth;
            rightViewBound = Math.Min(CurrentSession.PlayField.Width - 1, rightViewBound + BufferLine);
            int upperViewBound = (int)ScrollViewerVisibleArea.VerticalOffset / mineWidth;
            upperViewBound = Math.Max(0, upperViewBound - BufferLine);
            int lowerViewBound =
                (int)(ScrollViewerVisibleArea.VerticalOffset +
                      (ScrollViewerVisibleArea.ViewportHeight / ScrollViewerVisibleArea.ZoomFactor)) / mineWidth;
            lowerViewBound = Math.Min(CurrentSession.PlayField.Height - 1, lowerViewBound + BufferLine);
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

                        ThemeButton(tile, CurrentSession.PlayField.GetTileState(i, j));

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

            if (!unchekedTiles && markedTiles == CurrentSession.MineCount &&
                CurrentSession.CurrentState != Session.GameState.Win)
            {
                CurrentSession.WinGame();
                var dialog = new ContentDialog();
                dialog.XamlRoot = this.XamlRoot;
                dialog.RequestedTheme = ActualTheme;
                dialog.Title = "You Win!";
                dialog.Content = $"Congratulations! You have won in {TimeFormat(CurrentSession.GameTimer.Elapsed)}!";
                dialog.IsPrimaryButtonEnabled = true;
                dialog.PrimaryButtonText = "Close";
                dialog.PrimaryButtonClick += (s, e) => s.Hide();
                dialog.ShowAsync();
                UpdateView();
            }

            TextBlockMines.Text = $"{CurrentSession.MineCount - markedTiles}/{CurrentSession.MineCount}";
            
            ThemeStaticElements();
        }

        /// <summary>
        /// Theming for static elements, e.g. clock, mine counter
        /// </summary>
        private void ThemeStaticElements()
        {
            bool darkTheme = ActualTheme == ElementTheme.Dark;
            //clock
            if (darkTheme)
            {
                if (!(ImageClock.Source as BitmapImage).UriSource.Equals(new Uri("ms-appx:///Assets/Indicators/darkClock.png")))
                {
                    ImageClock.Source = new BitmapImage(new Uri("ms-appx:///Assets/Indicators/darkClock.png"));
                }
            }
            else
            {
                if (!(ImageClock.Source as BitmapImage).UriSource.Equals(new Uri("ms-appx:///Assets/Indicators/lightClock.png")))
                {
                    ImageClock.Source = new BitmapImage(new Uri("ms-appx:///Assets/Indicators/lightClock.png"));
                }
            }
            
            //mine counter
            if (darkTheme)
            {
                if (!(ImageMine.Source as BitmapImage).UriSource.Equals(new Uri("ms-appx:///Assets/Indicators/darkMine.png")))
                {
                    ImageMine.Source = new BitmapImage(new Uri("ms-appx:///Assets/Indicators/darkMine.png"));
                }
            }
            else
            {
                if (!(ImageMine.Source as BitmapImage).UriSource.Equals(new Uri("ms-appx:///Assets/Indicators/lightMine.png")))
                {
                    ImageMine.Source = new BitmapImage(new Uri("ms-appx:///Assets/Indicators/lightMine.png"));
                }
            }
        }
        // Brushes used for tiles
        private SolidColorBrush darkThemeOpenTile = new(Color.FromArgb(60, 30, 30, 30));
        private SolidColorBrush darkThemeFlagTile = new(Color.FromArgb(60, 250, 0, 0));
        private SolidColorBrush darkThemeQuestionMarkTile = new(Color.FromArgb(60, 250, 250, 250));
        private SolidColorBrush darkThemeClosedTile = new(Color.FromArgb(60, 60, 60, 60));
        private SolidColorBrush darkThemeOpenMineTile = new(Color.FromArgb(120, 250, 0, 0));
        private SolidColorBrush lightThemeOpenTile = new(Color.FromArgb(120, 100, 100, 100));
        private SolidColorBrush lightThemeFlagTile = new(Color.FromArgb(60, 250, 0, 0));
        private SolidColorBrush lightThemeQuestionMarkTile = new(Color.FromArgb(60, 250, 250, 250));
        private SolidColorBrush lightThemeClosedTile = new(Color.FromArgb(120, 160, 160, 160));
        private SolidColorBrush lightThemeOpenMineTile = new(Color.FromArgb(120, 250, 0, 0));
        /// <summary>
        /// Sets background and content for a tile
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="state"></param>
        private void ThemeButton(Button tile, TileState state)
        {
            bool darkTheme = ActualTheme == ElementTheme.Dark;
            // background determination
            if (state.IsOpen && state.HasMine)
            {
                if (darkTheme)
                {
                    if (!tile.Background.Equals(darkThemeOpenMineTile))
                    {
                        tile.Background = darkThemeOpenMineTile;
                    }
                }
                else
                {
                    if (!tile.Background.Equals(lightThemeOpenMineTile))
                    {
                        tile.Background = lightThemeOpenMineTile;
                    }
                }
            }
            else if (state.IsOpen && !state.HasMine)
            {
                if (darkTheme)
                {
                    if (!tile.Background.Equals(darkThemeOpenTile))
                    {
                        tile.Background = darkThemeOpenTile;
                    }
                }
                else
                {
                    if (!tile.Background.Equals(lightThemeOpenTile))
                    {
                        tile.Background = lightThemeOpenTile;
                    }
                }
            }
            else if (state.HasFlag)
            {
                if (darkTheme)
                {
                    if (!tile.Background.Equals(darkThemeFlagTile))
                    {
                        tile.Background = darkThemeFlagTile;
                    }
                }
                else
                {
                    if (!tile.Background.Equals(lightThemeFlagTile))
                    {
                        tile.Background = lightThemeFlagTile;
                    }
                }
            }
            else if (state.HasQuestionMark)
            {
                if (darkTheme)
                {
                    if (!tile.Background.Equals(darkThemeQuestionMarkTile))
                    {
                        tile.Background = darkThemeQuestionMarkTile;
                    }
                }
                else
                {
                    if (!tile.Background.Equals(lightThemeQuestionMarkTile))
                    {
                        tile.Background = lightThemeQuestionMarkTile;
                    }
                }
            }
            else
            {
                if (darkTheme)
                {
                    if (!tile.Background.Equals(darkThemeClosedTile))
                    {
                        tile.Background = darkThemeClosedTile;
                    }
                }
                else
                {
                    if (!tile.Background.Equals(lightThemeClosedTile))
                    {
                        tile.Background = lightThemeClosedTile;
                    }
                }
            }

            // content determination
            if (CurrentSession.CurrentState == Session.GameState.Loss && state.HasMine)
            {
                if (darkTheme)
                {
                    if (tile.Content is not Image || !((tile.Content as Image).Source as BitmapImage).UriSource.Equals(new Uri("ms-appx:///Assets/Indicators/darkMine.png")))
                    {
                        tile.Content = new Image(){Source = new BitmapImage(new Uri("ms-appx:///Assets/Indicators/darkMine.png"))};
                    }
                }
                else
                {
                    if (tile.Content is not Image || !((tile.Content as Image).Source as BitmapImage).UriSource.Equals(new Uri("ms-appx:///Assets/Indicators/lightMine.png")))
                    {
                        tile.Content = new Image(){Source = new BitmapImage(new Uri("ms-appx:///Assets/Indicators/lightMine.png"))};
                    }
                }
            }
            else if (!state.IsOpen && state.HasQuestionMark)
            {
                if (tile.Content is not TextBlock || (tile.Content as TextBlock).Text != "?")
                {
                    tile.Content = new TextBlock()
                    {
                        Text = "?",
                        FontSize = 25
                    };
                }
            }
            else if (!state.IsOpen && state.HasFlag)
            {
                if (darkTheme)
                {
                    if (tile.Content is not Image || !((tile.Content as Image).Source as BitmapImage).UriSource.Equals(new Uri("ms-appx:///Assets/Indicators/darkFlag.png")))
                    {
                        tile.Content = new Image(){Source = new BitmapImage(new Uri("ms-appx:///Assets/Indicators/darkFlag.png"))};
                    }
                }
                else
                {
                    if (tile.Content is not Image || !((tile.Content as Image).Source as BitmapImage).UriSource.Equals(new Uri("ms-appx:///Assets/Indicators/lightFlag.png")))
                    {
                        tile.Content = new Image(){Source = new BitmapImage(new Uri("ms-appx:///Assets/Indicators/lightFlag.png"))};
                    }
                }
            }
            else if (state.IsOpen && state.NeighboringMineCount > 0)
            {
                if (tile.Content is not TextBlock || (tile.Content as TextBlock).Text == "?")
                {
                    tile.Content = new TextBlock()
                    {
                        Text = state.NeighboringMineCount.ToString(),
                        FontSize = 25
                    };
                }
            }
            else
            {
                tile.Content = null;
            }
        }
        /// <summary>
        /// Begins the game at a clicked tile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        
        /// <summary>
        /// Gives time in more verbal form
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Open action for tile, can end the game in loss if it was a mine.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    dialog.RequestedTheme = ActualTheme;
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

        /// <summary>
        /// Cycles the mark on the tile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Replaces the current session with an empty one 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestartSession(object sender, RoutedEventArgs e)
        {
            InitialiseSession();
            cts.Cancel();
            cts = new();
        }
        /// <summary>
        /// Previous position of the pointer device
        /// </summary>
        private Windows.Foundation.Point PreviousPosition = new(-999999, -999999);

        /// <summary>
        /// Allows for middle click and drag functionality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// When the viewport is moved we need to update the ui
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewerVisibleArea_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            UpdateView();
        }
    }
}