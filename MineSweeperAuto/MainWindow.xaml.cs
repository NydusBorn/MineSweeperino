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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Game;
using Microsoft.Data.Sqlite;
using Microsoft.Graphics.Display;
using WinRT;
using DisplayInformation = ABI.Microsoft.Graphics.Display.DisplayInformation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MineSweeperAuto
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WinUIEx.WindowEx
    {
        public static Session CurrentSession;
        public static SqliteConnection DBConnection = new SqliteConnection("Data Source=MineSweeperAuto.db");
        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            DBConnection.Open();
            bool exists = EnsureDatabase();
            if (!exists) CreateDB();
        }

        void CreateDB()
        {
            DBConnection.Close();
            DBConnection.Dispose();
            SqliteConnection.ClearAllPools();
            DBConnection.Open();
            var creator = DBConnection.CreateCommand();

            creator.CommandText = "drop table if exists GameSettings";
            creator.ExecuteNonQuery();
            creator.CommandText = "CREATE TABLE GameSettings (CustomDifficulty INTEGER, FieldWidth INTEGER, FieldHeight INTEGER, UsePercentage INTEGER, MinePercentage REAL, MineCount INTEGER, GuaranteeSolution INTEGER)";
            creator.ExecuteNonQuery();
            creator.CommandText = "insert into GameSettings values(0, 7, 7, 0, 0.1, 5, 1)";
            creator.ExecuteNonQuery();
            
            creator.CommandText = "drop table if exists AppSettings ";
            creator.ExecuteNonQuery();
            creator.CommandText = "CREATE TABLE AppSettings (UseQuestionMarks INTEGER)";
            creator.ExecuteNonQuery();
            creator.CommandText = "insert into AppSettings values(1)";
            creator.ExecuteNonQuery();
            
            creator.CommandText = "drop table if exists SolverSettings";
            creator.ExecuteNonQuery();
            creator.CommandText = "CREATE TABLE SolverSettings (MillisecondsPerMove INTEGER)";
            creator.ExecuteNonQuery();
            creator.CommandText = "insert into SolverSettings values(1000)";
            creator.ExecuteNonQuery();
        }
        
        bool EnsureDatabase()
        {
            var cmd = DBConnection.CreateCommand();
            cmd.CommandText = "select * from sqlite_master where type is 'table'";
            var reader = cmd.ExecuteReader();
            bool hasGameSettings = false, hasAppSettings = false, hasSolverSettings = false;
            while (reader.Read())
            {
                string tableName = reader["name"].ToString();
                if (tableName == "GameSettings" && !hasGameSettings) hasGameSettings = true;
                else if (tableName == "AppSettings" && !hasAppSettings) hasAppSettings = true;
                else if (tableName == "SolverSettings" && !hasSolverSettings) hasSolverSettings = true;
                else if (tableName == "GameSettings" && hasGameSettings) return false;
                else if (tableName == "AppSettings" && hasAppSettings) return false;
                else if (tableName == "SolverSettings" && hasSolverSettings) return false;
            }

            if (!hasAppSettings || !hasGameSettings || !hasSolverSettings) return false;
            reader.Close();
            
            cmd.CommandText = "PRAGMA table_info('GameSettings')";
            reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                if (reader["name"].ToString() != "CustomDifficulty") return false;
            }
            else return false;
            if (reader.Read())
            {
                if (reader["name"].ToString() != "FieldWidth") return false;
            }
            else return false;
            if (reader.Read())
            {
                if (reader["name"].ToString() != "FieldHeight") return false;
            }
            else return false;
            if (reader.Read())
            {
                if (reader["name"].ToString() != "UsePercentage") return false;
            }
            else return false;
            if (reader.Read())
            {
                if (reader["name"].ToString() != "MinePercentage") return false;
            }
            else return false;
            if (reader.Read())
            {
                if (reader["name"].ToString() != "MineCount") return false;
            }
            else return false;
            if (reader.Read())
            {
                if (reader["name"].ToString() != "GuaranteeSolution") return false;
            }
            else return false;
            if (reader.Read()) return false;
            reader.Close();
            
            cmd.CommandText = "PRAGMA table_info('AppSettings')";
            reader = cmd.ExecuteReader();
            
            if (reader.Read())
            {
                if (reader["name"].ToString() != "UseQuestionMarks") return false;
            }
            else return false;
            if (reader.Read()) return false;
            reader.Close();
            
            cmd.CommandText = "PRAGMA table_info('SolverSettings')";
            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                if (reader["name"].ToString() != "MillisecondsPerMove") return false;
            }
            else return false;
            if (reader.Read()) return false;
            reader.Close();
            
            return true;
        }
        
        private void NavigationViewPageSelector_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(AppOptionsPage));
            }
            else if ((string)args.SelectedItem.As<NavigationViewItem>().Content == "Play")
            {
                ContentFrame.Navigate(typeof(GamePage));
            }
            else if ((string)args.SelectedItem.As<NavigationViewItem>().Content == "Game Options")
            {
                ContentFrame.Navigate(typeof(GameOptionsPage));
            }
            else if ((string)args.SelectedItem.As<NavigationViewItem>().Content == "Solver Options")
            {
                ContentFrame.Navigate(typeof(SolverOptionsPage));
            }
            else if ((string)args.SelectedItem.As<NavigationViewItem>().Content == "Help")
            {
                ContentFrame.Navigate(typeof(HelpPage));
            }

            if (CurrentSession != null)
            {
                if ((string)args.SelectedItem.As<NavigationViewItem>().Content == "Play" && CurrentSession.CurrentState != Session.GameState.Initialised && CurrentSession.CurrentState != Session.GameState.Win &&
                    CurrentSession.CurrentState != Session.GameState.Loss)
                {
                    CurrentSession.ResumeGame();
                }
                else if (CurrentSession.CurrentState == Session.GameState.Active)
                {
                    CurrentSession.PauseGame();
                }
            }
        }
    }
}
