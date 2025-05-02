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
        private string currentPlatform = null;

        // Contrôle de contenu de plateforme
        private PlatformContentControl platformContentControl;

        // Moniteur Steam
        private SteamActivityMonitor steamMonitor;

        private ProcessMonitor processMonitor;

        public MainWindow()
        {
            FolderPermissionsControl.ResetFirstLoadFlag();

            InitializeComponent();

            // Initialiser le moniteur Steam
            steamMonitor = new SteamActivityMonitor();

            // Démarrer le moniteur Steam dès le lancement de l'application
            steamMonitor.Start();

            // Vérifier immédiatement si Steam est déjà en cours d'exécution
            steamMonitor.CheckImmediately();

            // Initialiser le contrôle de contenu de plateforme
            platformContentControl = new PlatformContentControl(steamMonitor);
            contentContainer.Child = platformContentControl;

            // Initialiser le dictionnaire de boutons après l'initialisation des composants
            InitializePlatformButtons();

            // Appliquer les protections de dossiers
            ApplyFolderProtections();

            // Appliquer la configuration de visibilité des plateformes
            ApplyPlatformVisibility();

            // Initialiser et démarrer le moniteur de processus
            processMonitor = new ProcessMonitor();
            processMonitor.Start();
            processMonitor.UserInfoUpdated += ProcessMonitor_UserInfoUpdated;

            // Récupérer les informations utilisateur
            UpdateUserInfo();

            // S'abonner aux événements de lancement
            platformContentControl.LaunchRequested += PlatformContentControl_LaunchRequested;

            // S'abonner à l'événement de fermeture
            this.Closing += MainWindow_Closing;
        }

        // Gestionnaire d'événement pour les mises à jour des informations utilisateur
        private void ProcessMonitor_UserInfoUpdated(string displayName, int age)
        {
            // Mettre à jour l'affichage sur le thread UI
            Dispatcher.Invoke(() =>
            {
                txtUserName.Text = $"Utilisateur : {displayName}";
                txtUserAge.Text = $"Âge : {age} ans";
            });
        }

        // Méthode pour mettre à jour les informations utilisateur
        private void UpdateUserInfo()
        {
            try
            {
                // Récupérer le nom d'utilisateur actuel
                string username = Environment.UserName;

                // Utiliser la classe UserInfoRetriever pour obtenir les informations
                UserInfoRetriever.UserInfo userInfo = UserInfoRetriever.GetUserInfo(username);

                // Mettre à jour l'affichage
                txtUserName.Text = $"Utilisateur : {userInfo.DisplayName}";
                txtUserAge.Text = $"Âge : {userInfo.Age} ans";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la mise à jour des informations utilisateur: {ex.Message}");
            }
        }

        private void PlatformContentControl_LaunchRequested(object sender, string platform)
        {
            // TODO: Peut ajouter une logique supplémentaire ici si nécessaire
            System.Diagnostics.Debug.WriteLine($"Lancement de la plateforme: {platform}");
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Arrêter le moniteur Steam
            steamMonitor.Stop();

            // Si on ne permet pas les comptes personnels, s'assurer que Steam est fermé
            if (!steamMonitor.AllowUserAccounts)
            {
                steamMonitor.KillAllSteamProcesses();
            }

            if (processMonitor != null)
            {
                processMonitor.Stop();
            }

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
            catch (Exception)
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
                // Mettre à jour la plateforme actuelle
                currentPlatform = platformName;

                // Configurer le contrôle de contenu pour cette plateforme
                platformContentControl.ConfigurePlatform(platformName);

                // Mettre en évidence le bouton sélectionné
                HighlightSelectedButton(button);
            }
        }

        // Mettre en évidence le bouton sélectionné
        private void HighlightSelectedButton(Button selectedButton)
        {
            // Réinitialiser tous les boutons
            foreach (var button in platformButtons.Values)
            {
                button.Opacity = 0.7;
            }

            // Mettre en évidence le bouton sélectionné
            selectedButton.Opacity = 1.0;
        }
    }
}