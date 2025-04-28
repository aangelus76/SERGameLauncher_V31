using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SERGamesLauncher_V31
{
    public partial class MainWindow : CustomWindow
    {
        // Collection de boutons de plateforme pour un accès facile
        private Dictionary<string, Button> platformButtons;

        // Plateforme actuellement sélectionnée
        private string currentPlatform = "Epic";

        public MainWindow()
        {
            InitializeComponent();

            // Initialiser le dictionnaire de boutons après l'initialisation des composants
            InitializePlatformButtons();

            // Appliquer la configuration de visibilité des plateformes
            ApplyPlatformVisibility();
        }

        // Initialiser le dictionnaire de boutons
        private void InitializePlatformButtons()
        {
            platformButtons = new Dictionary<string, Button>
            {
                { "Steam", btnSteam },
                { "Epic", btnEpic },
                { "Crazy", btnCrazy },
                { "Roblox", btnRoblox },
                { "BGA", btnBGA },
                { "Xbox", btnXbox }
            };
        }

        // Appliquer la configuration de visibilité des plateformes
        // Méthode publique pour pouvoir être appelée depuis CustomWindow
        public void ApplyPlatformVisibility()
        {
            try
            {
                // Charger la configuration de visibilité
                Dictionary<string, bool> platformVisibility = Panel.PlatformConfigService.LoadPlatformVisibility();

                // Appliquer la visibilité à chaque bouton
                foreach (var platform in platformVisibility)
                {
                    if (platformButtons.TryGetValue(platform.Key, out Button button))
                    {
                        button.Visibility = platform.Value ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            catch (System.Exception)
            {
                // En cas d'erreur, ne pas afficher de message
                // Tous les boutons restent visibles par défaut
            }
        }

        // Navigation entre plateformes
        private void PlatformButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string platformName)
            {
                currentPlatform = platformName;

                // À implémenter: logique pour changer le contenu en fonction de la plateforme
                // Pour l'instant, nous utilisons juste le contenu existant d'Epic Games
            }
        }

        // Lancement d'une plateforme
        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            // À implémenter: logique de lancement en fonction de currentPlatform
            CustomMessageBox.Show(this,
                $"Lancement de la plateforme {currentPlatform}.\n\nCette fonctionnalité n'est pas encore implémentée.",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}