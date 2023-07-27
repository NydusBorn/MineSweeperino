using System;
using System.Globalization;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

//TODO: Use cards like in windows settings app.

namespace MineSweeperAuto
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameOptionsPage : Page
    {
        private static Brush defaultTextBackground;
        public GameOptionsPage()
        {
            this.InitializeComponent();
        }

        private void GameOptionsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            defaultTextBackground = TextBoxWidth.Background;
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "SELECT * FROM GameSettings";
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                if ((long)reader["CustomDifficulty"] != 1)
                {
                    switch ((long)reader["FieldWidth"])
                    {
                        case 7:
                            ComboBoxDiffChooser.SelectedIndex = 0;
                            break;
                        case 9:
                            ComboBoxDiffChooser.SelectedIndex = 1;
                            break;
                        case 16:
                            ComboBoxDiffChooser.SelectedIndex = 2;
                            break;
                        case 30:
                            ComboBoxDiffChooser.SelectedIndex = 3;
                            break;
                        case 50:
                            ComboBoxDiffChooser.SelectedIndex = 4;
                            break;
                        case 100:
                            ComboBoxDiffChooser.SelectedIndex = 5;
                            break;
                        default:
                            ComboBoxDiffChooser.SelectedIndex = 0;
                            break;
                    }
                }
                else
                {
                    ComboBoxDiffChooser.SelectedIndex = 6;
                }
            }
        }

        private void ComboBoxDiffChooser_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "SELECT * FROM GameSettings";
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                switch (ComboBoxDiffChooser.SelectedIndex)
                {
                    case 0:
                        TextBoxWidth.Text = "7";
                        TextBoxHeight.Text = "7";
                        CheckBoxUsePercentage.IsChecked = false;
                        TextBoxMines.Text = "5";
                        CheckBoxSolutionGuaranteed.IsChecked = true;
                        break;
                    case 1:
                        TextBoxWidth.Text = "9";
                        TextBoxHeight.Text = "9";
                        CheckBoxUsePercentage.IsChecked = false;
                        TextBoxMines.Text = "10";
                        CheckBoxSolutionGuaranteed.IsChecked = true;
                        break;
                    case 2:
                        TextBoxWidth.Text = "16";
                        TextBoxHeight.Text = "16";
                        CheckBoxUsePercentage.IsChecked = false;
                        TextBoxMines.Text = "40";
                        CheckBoxSolutionGuaranteed.IsChecked = true;
                        break;
                    case 3:
                        TextBoxWidth.Text = "30";
                        TextBoxHeight.Text = "16";
                        CheckBoxUsePercentage.IsChecked = false;
                        TextBoxMines.Text = "99";
                        CheckBoxSolutionGuaranteed.IsChecked = true;
                        break;
                    case 4:
                        TextBoxWidth.Text = "50";
                        TextBoxHeight.Text = "30";
                        CheckBoxUsePercentage.IsChecked = true;
                        TextBoxMines.Text = "0.25";
                        CheckBoxSolutionGuaranteed.IsChecked = false;
                        break;
                    case 5:
                        TextBoxWidth.Text = "100";
                        TextBoxHeight.Text = "100";
                        CheckBoxUsePercentage.IsChecked = true;
                        TextBoxMines.Text = "0.5";
                        CheckBoxSolutionGuaranteed.IsChecked = false;
                        break;
                    case 6:
                        TextBoxWidth.Text = reader["FieldWidth"].ToString();
                        TextBoxHeight.Text = reader["FieldHeight"].ToString();
                        CheckBoxUsePercentage.IsChecked = reader["UsePercentage"].ToString() == "1";
                        if (CheckBoxUsePercentage.IsChecked.Value)
                        {
                            TextBoxMines.Text = reader["MinePercentage"].ToString();
                        }
                        else
                        {
                            TextBoxMines.Text = reader["MineCount"].ToString();
                        }
                        CheckBoxSolutionGuaranteed.IsChecked = reader["GuaranteeSolution"].ToString() == "1";
                        break;
                    default:
                        ComboBoxDiffChooser.SelectedIndex = 0;
                        break;
                }
                reader.Close();
                if (ComboBoxDiffChooser.SelectedIndex != 6)
                {
                    TextBoxWidth.IsEnabled = false;
                    TextBoxHeight.IsEnabled = false;
                    TextBoxMines.IsEnabled = false;
                    CheckBoxUsePercentage.IsEnabled = false;
                    CheckBoxSolutionGuaranteed.IsEnabled = false;
                    cmd.CommandText = "update GameSettings set CustomDifficulty = 0 where true;";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    TextBoxWidth.IsEnabled = true;
                    TextBoxHeight.IsEnabled = true;
                    TextBoxMines.IsEnabled = true;
                    CheckBoxUsePercentage.IsEnabled = true;
                    CheckBoxSolutionGuaranteed.IsEnabled = true;
                    cmd.CommandText = "update GameSettings set CustomDifficulty = 1 where true;";
                    cmd.ExecuteNonQuery();
                }
            }
            CheckBoxUsePercentage.Content = CheckBoxUsePercentage.IsChecked.Value ? "Use Percentage" : "Use Count";
        }

        private void TextBoxWidth_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(TextBoxWidth.Text, out int width))
            {
                if (width > 100 || width < 1)
                {
                    TextBoxWidth.Background = new SolidColorBrush(Color.FromArgb(255,255,0,0));
                    var flyout = new Flyout();
                    var text = new TextBlock();
                    text.Text = "Must be an integer in range [1,100] inclusive.";
                    flyout.Content = text;
                    flyout.ShowMode = FlyoutShowMode.Transient;
                    flyout.ShowAt(TextBoxWidth);
                    return;
                }
                TextBoxWidth.Background = defaultTextBackground;
                var cmd = MainWindow.DBConnection.CreateCommand();
                cmd.CommandText = $"update GameSettings set FieldWidth = {TextBoxWidth.Text} where true;";
                cmd.ExecuteNonQuery();
            }
            else
            {
                TextBoxWidth.Background = new SolidColorBrush(Color.FromArgb(255,255,0,0));
                var flyout = new Flyout();
                var text = new TextBlock();
                text.Text = "Must be an integer in range [1,100] inclusive.";
                flyout.Content = text;
                flyout.ShowMode = FlyoutShowMode.Transient;
                flyout.ShowAt(TextBoxWidth);
            }
        }

        private void TextBoxHeight_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(TextBoxHeight.Text, out int height))
            {
                if (height > 100 || height < 1)
                {
                    TextBoxHeight.Background = new SolidColorBrush(Color.FromArgb(255,255,0,0));
                    var flyout = new Flyout();
                    var text = new TextBlock();
                    text.Text = "Must be an integer in range [1,100] inclusive.";
                    flyout.Content = text;
                    flyout.ShowMode = FlyoutShowMode.Transient;
                    flyout.ShowAt(TextBoxHeight);
                    return;
                }
                TextBoxHeight.Background = defaultTextBackground;
                var cmd = MainWindow.DBConnection.CreateCommand();
                cmd.CommandText = $"update GameSettings set FieldHeight = {TextBoxHeight.Text} where true;";
                cmd.ExecuteNonQuery();
            }
            else{
                TextBoxHeight.Background = new SolidColorBrush(Color.FromArgb(255,255,0,0));
                var flyout = new Flyout();
                var text = new TextBlock();
                text.Text = "Must be an integer in range [1,100] inclusive.";
                flyout.Content = text;
                flyout.ShowMode = FlyoutShowMode.Transient;
                flyout.ShowAt(TextBoxHeight);
            }
        }

        private void CheckBoxUsePercentage_OnChecked(object sender, RoutedEventArgs e)
        {
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "update GameSettings set UsePercentage = 1 where true;";
            cmd.ExecuteNonQuery();
            CheckBoxUsePercentage.Content = "Use Percentage";
            if (int.TryParse(TextBoxMines.Text, out int mines))
            {
                cmd.CommandText = "select * from GameSettings";
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    double percentage = mines;
                    percentage /= (long)reader["FieldWidth"] * (long)reader["FieldHeight"];
                    TextBoxMines.Text = percentage.ToString(CultureInfo.CurrentCulture);
                }
            }
        }

        private void CheckBoxUsePercentage_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "update GameSettings set UsePercentage = 0 where true;";
            cmd.ExecuteNonQuery();
            CheckBoxUsePercentage.Content = "Use Count";
            if (double.TryParse(TextBoxMines.Text, out double percentage))
            {
                cmd.CommandText = "select * from GameSettings";
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    long count = (long)Math.Round((long)reader["FieldWidth"] * (long)reader["FieldHeight"] * percentage);
                    TextBoxMines.Text = count.ToString();
                }
            }
        }

        private void TextBoxMines_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CheckBoxUsePercentage.IsChecked.Value)
            {
                if (double.TryParse(TextBoxMines.Text, out double percentage))
                {
                    if (percentage > 1 || percentage < 0)
                    {
                        TextBoxMines.Background = new SolidColorBrush(Color.FromArgb(255,255,0,0));
                        var flyout = new Flyout();
                        var text = new TextBlock();
                        text.Text = "Must be a percentage between 0 and 1.";
                        flyout.Content = text;
                        flyout.ShowMode = FlyoutShowMode.Transient;
                        flyout.ShowAt(TextBoxMines);
                        return;
                    }
                    TextBoxMines.Background = defaultTextBackground;
                    var cmd = MainWindow.DBConnection.CreateCommand();
                    cmd.CommandText = $"update GameSettings set MinePercentage = {TextBoxMines.Text} where true;";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    TextBoxMines.Background = new SolidColorBrush(Color.FromArgb(255,255,0,0));
                    var flyout = new Flyout();
                    var text = new TextBlock();
                    text.Text = "Must be a percentage between 0 and 1.";
                    flyout.Content = text;
                    flyout.ShowMode = FlyoutShowMode.Transient;
                    flyout.ShowAt(TextBoxMines);
                }
            }
            else
            {
                if (int.TryParse(TextBoxMines.Text, out int mines))
                {
                    if (mines < 0)
                    {
                        TextBoxMines.Background = new SolidColorBrush(Color.FromArgb(255,255,0,0));
                        var flyout = new Flyout();
                        var text = new TextBlock();
                        text.Text = "Must be a positive integer.";
                        flyout.Content = text;
                        flyout.ShowMode = FlyoutShowMode.Transient;
                        flyout.ShowAt(TextBoxMines);
                        return;
                    }
                    TextBoxMines.Background = null;
                    var cmd = MainWindow.DBConnection.CreateCommand();
                    cmd.CommandText = $"update GameSettings set MineCount = {TextBoxMines.Text} where true;";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    TextBoxMines.Background = new SolidColorBrush(Color.FromArgb(255,255,0,0));
                    var flyout = new Flyout();
                    var text = new TextBlock();
                    text.Text = "Must be a positive integer.";
                    flyout.Content = text;
                    flyout.ShowMode = FlyoutShowMode.Transient;
                    flyout.ShowAt(TextBoxMines);
                }
            }
            
        }

        private void CheckBoxSolutionGuaranteed_OnChecked(object sender, RoutedEventArgs e)
        {
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "update GameSettings set GuaranteeSolution = 1 where true;";
            cmd.ExecuteNonQuery();
        }

        private void CheckBoxSolutionGuaranteed_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var cmd = MainWindow.DBConnection.CreateCommand();
            cmd.CommandText = "update GameSettings set GuaranteeSolution = 0 where true;";
            cmd.ExecuteNonQuery();
        }
    }
}
