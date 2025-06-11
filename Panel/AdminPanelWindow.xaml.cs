using System;
using System.Windows;
using System.Windows.Controls;

namespace SERGamesLauncher_V31
{
    public partial class AdminPanelWindow : Window
    {
        // Instance du contrôle utilisateur pour la visibilité des plateformes
        private Panel.PlatformVisibilityControl platformVisibilityControl;

        // Instance du contrôle utilisateur pour les chemins d'applications
        private PathConfigsControl pathConfigsControl;
        private FolderPermissionsControl folderPermissionsControl;

        // Instance du contrôle utilisateur pour les restrictions de processus
        private ProcessRestrictionsControl processRestrictionsControl;

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

            // Créer une instance du contrôle de gestion des chemins d'applications
            pathConfigsControl = new PathConfigsControl();
            folderPermissionsControl = new FolderPermissionsControl();

            // Créer une instance du contrôle de gestion des restrictions de processus
            processRestrictionsControl = new ProcessRestrictionsControl();
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
            try
            {
                // Créer une nouvelle instance du contrôle
                SteamAccountsControl steamAccountsControl = new SteamAccountsControl();

                // Afficher le contrôle
                contentPresenter.Content = steamAccountsControl;

                // Pour déboguer
                System.Diagnostics.Debug.WriteLine("SteamAccountsControl chargé avec succès");
            }
            catch (Exception ex)
            {
                // Afficher l'erreur si quelque chose ne va pas
                contentPresenter.Content = new TextBlock()
                {
                    Text = $"Erreur lors du chargement du contrôle : {ex.Message}",
                    Foreground = System.Windows.Media.Brushes.Red,
                    FontSize = 18,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Pour déboguer
                System.Diagnostics.Debug.WriteLine($"Erreur : {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void btnPlatformVisibility_Click(object sender, RoutedEventArgs e)
        {
            // Afficher le contrôle de visibilité des plateformes
            contentPresenter.Content = platformVisibilityControl;
        }

        private void btnFolderPermissions_Click(object sender, RoutedEventArgs e)
        {
            // Afficher le contrôle de gestion des permissions de dossiers
            contentPresenter.Content = folderPermissionsControl;
        }

        private void btnAppPaths_Click(object sender, RoutedEventArgs e)
        {
            // Afficher le contrôle de gestion des chemins d'applications
            contentPresenter.Content = pathConfigsControl;
        }

        private void btnProcessRestrictions_Click(object sender, RoutedEventArgs e)
        {
            // Afficher le contrôle de gestion des restrictions de processus
            contentPresenter.Content = processRestrictionsControl;
        }
    }
}