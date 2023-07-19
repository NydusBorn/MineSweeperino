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
using Game;
using WinRT;

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
        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
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
        }
    }
}
