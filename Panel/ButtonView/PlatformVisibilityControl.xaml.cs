using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SERGamesLauncher_V31.Panel
{
    public partial class PlatformVisibilityControl : UserControl
    {
        // Dictionnaire pour suivre l'état de visibilité des plateformes
        private Dictionary<string, bool> platformVisibility;

        // Flag pour éviter de sauvegarder pendant le chargement initial
        private bool isInitializing = true;

        public PlatformVisibilityControl()
        {
            InitializeComponent();

            // Charger la configuration existante
            platformVisibility = PlatformConfigService.LoadPlatformVisibility();

            // Mettre à jour l'interface avec les états chargés
            UpdateToggles();

            // Initialisation terminée
            isInitializing = false;
        }

        // Mise à jour des toggles selon l'état du dictionnaire
        private void UpdateToggles()
        {
            toggleSteam.IsChecked = platformVisibility.ContainsKey("Steam") && platformVisibility["Steam"];
            toggleEpic.IsChecked = platformVisibility.ContainsKey("Epic") && platformVisibility["Epic"];
            toggleCrazy.IsChecked = platformVisibility.ContainsKey("Crazy") && platformVisibility["Crazy"];
            toggleRoblox.IsChecked = platformVisibility.ContainsKey("Roblox") && platformVisibility["Roblox"];
            toggleBGA.IsChecked = platformVisibility.ContainsKey("BGA") && platformVisibility["BGA"];
            toggleXbox.IsChecked = platformVisibility.ContainsKey("Xbox") && platformVisibility["Xbox"];
        }

        // Sauvegarde la configuration sans afficher de message
        private void SaveConfigurationSilently()
        {
            PlatformConfigService.SavePlatformVisibility(platformVisibility);
        }

        // Sauvegarde la configuration et affiche un message de confirmation
        private void SaveConfigurationWithConfirmation()
        {
            bool success = PlatformConfigService.SavePlatformVisibility(platformVisibility);

            if (success)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    "La configuration de visibilité des plateformes a été enregistrée avec succès.\n\n" +
                    "Les changements seront appliqués au redémarrage de l'application.",
                    "Configuration sauvegardée", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    "Une erreur est survenue lors de la sauvegarde de la configuration.",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Gestionnaire d'événement pour les toggles
        private void PlatformToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if (sender is ToggleButton toggle && toggle.Tag is string platformName)
            {
                platformVisibility[platformName] = toggle.IsChecked ?? true;
                SaveConfigurationSilently(); // Sauvegarde silencieuse sans message
            }
        }

        // Activer toutes les plateformes
        private void EnableAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var key in platformVisibility.Keys.ToArray())
            {
                platformVisibility[key] = true;
            }

            UpdateToggles();
            SaveConfigurationWithConfirmation(); // Affiche un message car c'est une action explicite
        }

        // Désactiver toutes les plateformes
        private void DisableAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var key in platformVisibility.Keys.ToArray())
            {
                platformVisibility[key] = false;
            }

            UpdateToggles();
            SaveConfigurationWithConfirmation(); // Affiche un message car c'est une action explicite
        }
    }
}