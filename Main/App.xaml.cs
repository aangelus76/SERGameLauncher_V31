using System.Windows;
using System.Windows.Controls;

namespace SERGamesLauncher_V31
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Méthodes helper pour les boutons de la barre de titre
        public static void WindowMinimizeClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var window = Window.GetWindow(button);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        public static void WindowCloseClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var window = Window.GetWindow(button);
            if (window != null)
            {
                window.Close();
            }
        }
    }
}