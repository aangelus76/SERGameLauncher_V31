// Panel/Path/PathConfigsControl.xaml.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SERGamesLauncher_V31
{
    public partial class PathConfigsControl : UserControl
    {
        // Collection observable pour l'affichage
        private ObservableCollection<PathConfig> pathConfigs;

        // Liste brute pour les opérations
        private List<PathConfig> configs;

        public PathConfigsControl()
        {
            InitializeComponent();
            LoadPathConfigs();
        }

        // Charger les configurations de chemins
        private void LoadPathConfigs()
        {
            // Charger les configurations
            configs = PathConfigService.LoadPathConfigs();

            // Créer la collection observable
            pathConfigs = new ObservableCollection<PathConfig>(configs);

            // Lier au DataGrid
            pathsDataGrid.ItemsSource = pathConfigs;
        }

        // Ajouter un nouveau chemin
        private void AddPath_Click(object sender, RoutedEventArgs e)
        {
            // Créer la boîte de dialogue
            AddEditPathDialog dialog = new AddEditPathDialog();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true && dialog.PathConfig != null)
            {
                // Vérifier si un chemin existe déjà pour cette plateforme
                if (PathConfigService.PlatformPathExists(configs, dialog.PathConfig.PlatformName))
                {
                    CustomMessageBox.Show(Window.GetWindow(this),
                        $"Un chemin pour la plateforme '{dialog.PathConfig.PlatformName}' existe déjà.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Ajouter à la liste
                configs.Add(dialog.PathConfig);
                pathConfigs.Add(dialog.PathConfig);

                // Sauvegarder les changements
                SavePathConfigs();
            }
        }

        // Modifier un chemin existant - SUPPRESSION DE L'AUTHENTIFICATION
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pathId)
            {
                // SUPPRIMÉ : Demande de mot de passe d'administration
                // On est déjà dans le panel admin, pas besoin de re-authentifier

                // Trouver la configuration
                var pathConfig = configs.FirstOrDefault(p => p.Id == pathId);
                if (pathConfig == null) return;

                // Créer la boîte de dialogue et pré-remplir les champs
                AddEditPathDialog dialog = new AddEditPathDialog(pathConfig);
                dialog.Owner = Window.GetWindow(this);

                if (dialog.ShowDialog() == true && dialog.PathConfig != null)
                {
                    // Vérifier si le nouveau nom de plateforme existe déjà
                    if (dialog.PathConfig.PlatformName != pathConfig.PlatformName &&
                        PathConfigService.PlatformPathExists(configs, dialog.PathConfig.PlatformName, pathId))
                    {
                        CustomMessageBox.Show(Window.GetWindow(this),
                            $"Un chemin pour la plateforme '{dialog.PathConfig.PlatformName}' existe déjà.",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Mettre à jour les champs
                    pathConfig.PlatformName = dialog.PathConfig.PlatformName;
                    pathConfig.Path = dialog.PathConfig.Path;
                    pathConfig.IsUrl = dialog.PathConfig.IsUrl;
                    pathConfig.LaunchArguments = dialog.PathConfig.LaunchArguments;

                    // Mettre à jour l'affichage
                    RefreshDisplay();

                    // Sauvegarder les changements
                    SavePathConfigs();
                }
            }
        }

        // Supprimer un chemin - SUPPRESSION DE L'AUTHENTIFICATION
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pathId)
            {
                // SUPPRIMÉ : Demande de mot de passe d'administration
                // On est déjà dans le panel admin, pas besoin de re-authentifier

                // Trouver la configuration
                var pathConfig = configs.FirstOrDefault(p => p.Id == pathId);
                if (pathConfig != null)
                {
                    // Confirmer la suppression directement
                    MessageBoxResult result = CustomMessageBox.Show(Window.GetWindow(this),
                        $"Êtes-vous sûr de vouloir supprimer le chemin pour '{pathConfig.PlatformName}' ?",
                        "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Supprimer de la liste
                        configs.Remove(pathConfig);

                        // Mettre à jour l'affichage
                        var displayConfig = pathConfigs.FirstOrDefault(p => p.Id == pathId);
                        if (displayConfig != null)
                        {
                            pathConfigs.Remove(displayConfig);
                        }

                        // Sauvegarder les changements
                        SavePathConfigs();
                    }
                }
            }
        }

        // Mettre à jour l'affichage
        private void RefreshDisplay()
        {
            pathConfigs.Clear();
            foreach (var config in configs)
            {
                pathConfigs.Add(config);
            }
        }

        // Sauvegarder les changements
        private void SavePathConfigs()
        {
            bool success = PathConfigService.SavePathConfigs(configs);

            if (!success)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    "Une erreur est survenue lors de la sauvegarde des chemins d'accès.",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}