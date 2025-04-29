using System;
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
            FolderPermissionsControl.ResetFirstLoadFlag();

            InitializeComponent();

            // Initialiser le dictionnaire de boutons après l'initialisation des composants
            InitializePlatformButtons();

            ApplyFolderProtections();

            // Appliquer la configuration de visibilité des plateformes
            ApplyPlatformVisibility();

            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Verrouiller tous les dossiers à la fermeture de l'application
            try
            {
                // Charger la configuration des dossiers protégés
                var folders = FolderPermissionService.LoadFolderPermissions();

                // Verrouiller tous les dossiers
                foreach (var folder in folders)
                {
                    folder.IsProtectionEnabled = true;
                    FolderPermissionService.ApplyProtection(folder);
                }

                // Sauvegarder les modifications
                FolderPermissionService.SaveFolderPermissions(folders);
            }
            catch (Exception ex)
            {
                // En cas d'erreur, ne pas bloquer la fermeture de l'application
                System.Diagnostics.Debug.WriteLine($"Erreur lors du verrouillage des dossiers à la fermeture: {ex.Message}");
            }
        }

        /// <summary>
        /// Applique les protections de dossiers configurées pour le démarrage
        /// </summary>
        private void ApplyFolderProtections()
        {
            try
            {
                // Charger la configuration des dossiers protégés
                var folders = FolderPermissionService.LoadFolderPermissions();

                // Appliquer les protections pour les dossiers configurés
                FolderPermissionService.ApplyStartupProtections(folders);
            }
            catch (Exception ex)
            {
                // En cas d'erreur, ne pas afficher de message pour ne pas perturber le démarrage
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'application des protections de dossiers: {ex.Message}");
            }
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