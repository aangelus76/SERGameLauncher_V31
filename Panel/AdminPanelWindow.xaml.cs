using System.Windows;
using System.Windows.Controls;

namespace SERGamesLauncher_V31
{
    public partial class AdminPanelWindow : Window
    {
        // Instance du contrôle utilisateur pour la visibilité des plateformes
        private Panel.PlatformVisibilityControl platformVisibilityControl;

        public AdminPanelWindow()
        {
            InitializeComponent();

            // Permettre le déplacement de la fenêtre sans bordure
            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            // Initialiser les contrôles utilisateur
            InitializeUserControls();

            // Afficher le message par défaut
            contentPresenter.Content = new TextBlock()
            {
                Text = "Sélectionnez une option dans le menu",
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        // Initialiser les contrôles utilisateur
        private void InitializeUserControls()
        {
            // Créer une instance du contrôle de visibilité des plateformes
            platformVisibilityControl = new Panel.PlatformVisibilityControl();

            // Ici vous pourrez initialiser d'autres contrôles à l'avenir
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Gestion des boutons du menu
        private void btnSteamAccounts_Click(object sender, RoutedEventArgs e)
        {
            // Pour l'instant, afficher juste un message (à remplacer par un contrôle plus tard)
            contentPresenter.Content = new TextBlock()
            {
                Text = "Gestion des comptes Steam",
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void btnPlatformVisibility_Click(object sender, RoutedEventArgs e)
        {
            // Afficher le contrôle de visibilité des plateformes
            contentPresenter.Content = platformVisibilityControl;
        }

        private void btnFolderPermissions_Click(object sender, RoutedEventArgs e)
        {
            contentPresenter.Content = new TextBlock()
            {
                Text = "Gestion des permissions dossiers",
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void btnAppPaths_Click(object sender, RoutedEventArgs e)
        {
            contentPresenter.Content = new TextBlock()
            {
                Text = "Configuration des chemins d'applications",
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }
    }
}