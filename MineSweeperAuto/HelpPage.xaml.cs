using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MineSweeperAuto
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HelpPage : Page
    {
        Task UpdateTimer;

        public HelpPage()
        {
            this.InitializeComponent();
        }

        private void HelpPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateTimer = UpdateTheme();
        }

        async Task UpdateTheme()
        {
            while (true)
            {
                bool darkTheme = ActualTheme == ElementTheme.Dark;
                if (darkTheme)
                {
                    if (PlayerWin.Source is not MediaPlaybackItem || !(PlayerWin.Source as MediaPlaybackItem).Source.Uri.Equals(
                            new Uri("ms-appx:///Assets/Help/FullGameWinDark.mp4")))
                    {
                        PlayerWin.Source =
                            new MediaPlaybackItem(
                                MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Help/FullGameWinDark.mp4")));
                    }

                    if (PlayerLoss.Source is not MediaPlaybackItem || !(PlayerLoss.Source as MediaPlaybackItem).Source.Uri.Equals(
                            new Uri("ms-appx:///Assets/Help/FullGameLossDark.mp4")))
                    {
                        PlayerLoss.Source =
                            new MediaPlaybackItem(
                                MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Help/FullGameLossDark.mp4")));
                    }

                    if (PlayerNormalOpen.Source is not MediaPlaybackItem || !(PlayerNormalOpen.Source as MediaPlaybackItem).Source.Uri.Equals(
                            new Uri("ms-appx:///Assets/Help/NormalOpenCellDark.mp4")))
                    {
                        PlayerNormalOpen.Source = new MediaPlaybackItem(
                            MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Help/NormalOpenCellDark.mp4")));
                    }

                    if (PlayerMark.Source is not MediaPlaybackItem || !(PlayerMark.Source as MediaPlaybackItem).Source.Uri.Equals(
                            new Uri("ms-appx:///Assets/Help/MarkCellDark.mp4")))
                    {
                        PlayerMark.Source =
                            new MediaPlaybackItem(
                                MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Help/MarkCellDark.mp4")));
                    }

                    if (PlayerAdvancedOpen.Source is not MediaPlaybackItem || !(PlayerAdvancedOpen.Source as MediaPlaybackItem).Source.Uri.Equals(
                            new Uri("ms-appx:///Assets/Help/AdvancedOpenCellDark.mp4")))
                    {
                        PlayerAdvancedOpen.Source = new MediaPlaybackItem(
                            MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Help/AdvancedOpenCellDark.mp4")));
                    }
                }
                else
                {
                    if (PlayerWin.Source is not MediaPlaybackItem || !(PlayerWin.Source as MediaPlaybackItem).Source.Uri.Equals(
                            new Uri("ms-appx:///Assets/Help/FullGameWinLight.mp4")))
                    {
                        PlayerWin.Source =
                            new MediaPlaybackItem(
                                MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Help/FullGameWinLight.mp4")));
                    }

                    if (PlayerLoss.Source is not MediaPlaybackItem || !(PlayerLoss.Source as MediaPlaybackItem).Source.Uri.Equals(
                            new Uri("ms-appx:///Assets/Help/FullGameLossLight.mp4")
                        ))
                    {
                        PlayerLoss.Source = new MediaPlaybackItem(
                            MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Help/FullGameLossLight.mp4")));
                    }

                    if (PlayerNormalOpen.Source is not MediaPlaybackItem || !(PlayerNormalOpen.Source as MediaPlaybackItem).Source.Uri.Equals(
                            new Uri("ms-appx:///Assets/Help/NormalOpenCellLight.mp4")))
                    {
                        PlayerNormalOpen.Source = new MediaPlaybackItem(
                            MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Help/NormalOpenCellLight.mp4")));
                    }

                    if (PlayerMark.Source is not MediaPlaybackItem || !(PlayerMark.Source as MediaPlaybackItem).Source.Uri.Equals(
                            new Uri("ms-appx:///Assets/Help/MarkCellLight.mp4")))
                    {
                        PlayerMark.Source =
                            new MediaPlaybackItem(
                                MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Help/MarkCellLight.mp4")));
                    }

                    if (PlayerAdvancedOpen.Source is not MediaPlaybackItem || !(PlayerAdvancedOpen.Source as MediaPlaybackItem).Source.Uri.Equals(
                            new Uri("ms-appx:///Assets/Help/AdvancedOpenCellLight.mp4")))
                    {
                        PlayerAdvancedOpen.Source = new MediaPlaybackItem(
                            MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Help/AdvancedOpenCellLight.mp4")));
                    }
                }
                await Task.Delay(1000);
            }
        }
    }
}