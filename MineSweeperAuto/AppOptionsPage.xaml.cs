using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

//TODO: Use cards like in windows settings app.

namespace MineSweeperAuto
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppOptionsPage : Page
    {
        public AppOptionsPage()
        {
            this.InitializeComponent();
        }

        private void AppOptionsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "SELECT * FROM AppSettings";
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                UseQuestionMarks.IsChecked = (long)reader["UseQuestionMarks"] == 1;
                UseSound.IsChecked = (long)reader["SoundEnabled"] == 1;
            }
        }

        private void UseQuestionMarks_OnChecked(object sender, RoutedEventArgs e)
        {
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "update AppSettings set UseQuestionMarks = 1 where true;";
            cmd.ExecuteNonQuery();
        }

        private void UseQuestionMarks_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "update AppSettings set UseQuestionMarks = 0 where true;";
            cmd.ExecuteNonQuery();
        }

        private void UseSound_OnChecked(object sender, RoutedEventArgs e)
        {
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "update AppSettings set SoundEnabled = 1 where true;";
            cmd.ExecuteNonQuery();
            ElementSoundPlayer.State = ElementSoundPlayerState.On;
        }

        private void UseSound_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "update AppSettings set SoundEnabled = 0 where true;";
            cmd.ExecuteNonQuery();
            ElementSoundPlayer.State = ElementSoundPlayerState.Off;
        }
    }
}
