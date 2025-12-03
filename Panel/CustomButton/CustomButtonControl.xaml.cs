// Panel/CustomButton/CustomButtonControl.xaml.cs
using Microsoft.Win32;
using SERGamesLauncher_V31.Services;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SERGamesLauncher_V31
{
    public partial class CustomButtonControl : UserControl
    {
        private CustomButtonConfig currentConfig;

        public CustomButtonControl()
        {
            InitializeComponent();
            LoadConfiguration();
        }

        /// <summary>
        /// Charge la configuration existante
        /// </summary>
        private void LoadConfiguration()
        {
            currentConfig = CustomButtonService.LoadConfig();

            // Appliquer les valeurs aux contrôles
            toggleEnabled.IsChecked = currentConfig.IsEnabled;
            txtStatus.Text = currentConfig.IsEnabled ? "Activé" : "Désactivé";

            txtButtonLabel.Text = currentConfig.ButtonLabel;
            txtPageTitle.Text = currentConfig.PageTitle;
            txtPageSubtitle.Text = currentConfig.PageSubtitle;
            txtPageInstructions.Text = currentConfig.PageInstructions;

            // Type de cible
            if (currentConfig.TargetType == "exe")
            {
                rbExe.IsChecked = true;
            }
            else
            {
                rbUrl.IsChecked = true;
            }

            txtTargetPath.Text = currentConfig.TargetPath;
            txtLaunchArguments.Text = currentConfig.LaunchArguments;

            // Token
            toggleUseToken.IsChecked = currentConfig.UseToken;
            txtTokenFormat.Text = currentConfig.TokenFormat;
            tokenPanel.Visibility = currentConfig.UseToken ? Visibility.Visible : Visibility.Collapsed;

            // Image
            if (!string.IsNullOrEmpty(currentConfig.ImageFileName))
            {
                var image = CustomButtonService.LoadButtonImage(currentConfig);
                if (image != null)
                {
                    imgPreview.Source = image;
                    txtImagePath.Text = currentConfig.ImageFileName;
                }
            }

            UpdateUIState();
        }

        /// <summary>
        /// Met à jour l'état de l'UI selon le type de cible
        /// </summary>
        private void UpdateUIState()
        {
            // Protection contre les appels pendant l'initialisation
            if (txtTargetLabel == null || btnBrowse == null || argumentsPanel == null || rbExe == null)
                return;

            bool isExe = rbExe.IsChecked == true;

            txtTargetLabel.Text = isExe ? "Chemin de l'exécutable" : "URL à ouvrir";
            btnBrowse.Visibility = isExe ? Visibility.Visible : Visibility.Collapsed;
            argumentsPanel.Visibility = isExe ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Toggle activer/désactiver
        /// </summary>
        private void ToggleEnabled_Changed(object sender, RoutedEventArgs e)
        {
            bool isEnabled = toggleEnabled.IsChecked == true;
            txtStatus.Text = isEnabled ? "Activé" : "Désactivé";
        }

        /// <summary>
        /// Changement du type de cible
        /// </summary>
        private void TargetType_Changed(object sender, RoutedEventArgs e)
        {
            UpdateUIState();
        }

        /// <summary>
        /// Toggle génération de token
        /// </summary>
        private void ToggleUseToken_Changed(object sender, RoutedEventArgs e)
        {
            bool useToken = toggleUseToken.IsChecked == true;
            tokenPanel.Visibility = useToken ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Sélection d'une image
        /// </summary>
        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Sélectionner une image",
                Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Tous les fichiers|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Sauvegarder l'image
                    string fileName = CustomButtonService.SaveImage(dialog.FileName);
                    currentConfig.ImageFileName = fileName;

                    // Mettre à jour l'aperçu
                    var image = CustomButtonService.LoadButtonImage(currentConfig);
                    if (image != null)
                    {
                        imgPreview.Source = image;
                        txtImagePath.Text = fileName;
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(Window.GetWindow(this),
                        $"Erreur lors de la sauvegarde de l'image :\n{ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Parcourir pour sélectionner un exécutable
        /// </summary>
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Sélectionner un exécutable",
                Filter = "Exécutables|*.exe|Tous les fichiers|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                txtTargetPath.Text = dialog.FileName;
            }
        }

        /// <summary>
        /// Enregistrer la configuration
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtButtonLabel.Text))
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    "Le nom du bouton est obligatoire.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtButtonLabel.Focus();
                return;
            }

            if (toggleEnabled.IsChecked == true && string.IsNullOrWhiteSpace(txtTargetPath.Text))
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    "Veuillez spécifier une URL ou un chemin d'exécutable.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTargetPath.Focus();
                return;
            }

            // Validation URL
            if (rbUrl.IsChecked == true && !string.IsNullOrWhiteSpace(txtTargetPath.Text))
            {
                string url = txtTargetPath.Text.Trim();
                // Autoriser les placeholders dans l'URL
                if (!url.Contains("{") && 
                    !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    CustomMessageBox.Show(Window.GetWindow(this),
                        "L'URL doit commencer par 'http://' ou 'https://'.",
                        "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtTargetPath.Focus();
                    return;
                }
            }

            // Validation exe
            if (rbExe.IsChecked == true && !string.IsNullOrWhiteSpace(txtTargetPath.Text))
            {
                string exePath = txtTargetPath.Text.Trim();
                // Ne pas vérifier si contient des placeholders
                if (!exePath.Contains("{") && !File.Exists(exePath))
                {
                    var result = CustomMessageBox.Show(Window.GetWindow(this),
                        "Le fichier spécifié n'existe pas. Voulez-vous continuer quand même ?",
                        "Fichier introuvable", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                    {
                        txtTargetPath.Focus();
                        return;
                    }
                }
            }

            try
            {
                // Mettre à jour la configuration
                currentConfig.IsEnabled = toggleEnabled.IsChecked == true;
                currentConfig.ButtonLabel = txtButtonLabel.Text.Trim();
                currentConfig.PageTitle = txtPageTitle.Text.Trim();
                currentConfig.PageSubtitle = txtPageSubtitle.Text.Trim();
                currentConfig.PageInstructions = txtPageInstructions.Text.Trim();
                currentConfig.TargetType = rbExe.IsChecked == true ? "exe" : "url";
                currentConfig.TargetPath = txtTargetPath.Text.Trim();
                currentConfig.LaunchArguments = txtLaunchArguments.Text.Trim();
                currentConfig.UseToken = toggleUseToken.IsChecked == true;
                currentConfig.TokenFormat = txtTokenFormat.Text.Trim();

                // Sauvegarder
                CustomButtonService.SaveConfig(currentConfig);

                CustomMessageBox.Show(Window.GetWindow(this),
                    "Configuration enregistrée avec succès.\n\nLes modifications sont appliquées immédiatement.",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                // Notifier la MainWindow pour rafraîchir le bouton
                NotifyMainWindow();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    $"Erreur lors de l'enregistrement :\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Réinitialiser la configuration
        /// </summary>
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            var result = CustomMessageBox.Show(Window.GetWindow(this),
                "Voulez-vous vraiment réinitialiser la configuration du bouton personnalisé ?\n\nCette action supprimera également l'image.",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CustomButtonService.ResetConfig();
                currentConfig = new CustomButtonConfig();
                LoadConfiguration();

                imgPreview.Source = null;
                txtImagePath.Text = "Aucune image sélectionnée";

                CustomMessageBox.Show(Window.GetWindow(this),
                    "Configuration réinitialisée.",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                NotifyMainWindow();
            }
        }

        /// <summary>
        /// Notifie la MainWindow pour rafraîchir le bouton custom
        /// </summary>
        private void NotifyMainWindow()
        {
            try
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow mainWindow)
                    {
                        mainWindow.RefreshCustomButton();
                        break;
                    }
                }
            }
            catch { }
        }
    }
}
