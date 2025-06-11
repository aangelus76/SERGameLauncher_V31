// Panel/Path/AddEditPathDialog.xaml.cs
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SERGamesLauncher_V31
{
    public partial class AddEditPathDialog : Window
    {
        // Propriétés pour retourner les informations de configuration
        public PathConfig PathConfig { get; private set; }

        // Mode d'édition
        //private bool isEditMode = false;
        private string originalId;

        // Constructeur pour l'ajout
        public AddEditPathDialog()
        {
            InitializeComponent();

            // Permettre le déplacement de la fenêtre sans bordure
            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            // Initialiser une nouvelle configuration
            PathConfig = new PathConfig();

            // Définir le titre
            titleTextBlock.Text = "Ajouter un chemin d'application";

            // Sélectionner le premier élément de la liste déroulante
            cmbPlatform.SelectedIndex = 0;
        }

        // Constructeur pour l'édition
        public AddEditPathDialog(PathConfig configToEdit)
        {
            InitializeComponent();

            // Permettre le déplacement de la fenêtre sans bordure
            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            // Copier les valeurs de la configuration à éditer
            PathConfig = new PathConfig
            {
                Id = configToEdit.Id,
                PlatformName = configToEdit.PlatformName,
                Path = configToEdit.Path,
                IsUrl = configToEdit.IsUrl,
                LaunchArguments = configToEdit.LaunchArguments
            };

            // Conserver l'ID original pour la vérification de doublon
            originalId = configToEdit.Id;

            // Pré-remplir les champs
            PreFillFields(configToEdit);

            // Activer le mode édition
            //isEditMode = true;

            // Définir le titre
            titleTextBlock.Text = "Modifier un chemin d'application";
        }

        // Pré-remplir les champs avec la configuration existante
        private void PreFillFields(PathConfig config)
        {
            // Type de chemin
            if (config.IsUrl)
            {
                rbUrl.IsChecked = true;
                rbLocal.IsChecked = false;
            }
            else
            {
                rbLocal.IsChecked = true;
                rbUrl.IsChecked = false;
            }

            // Sélectionner la plateforme dans la liste déroulante
            bool platformFound = false;
            for (int i = 0; i < cmbPlatform.Items.Count; i++)
            {
                if (cmbPlatform.Items[i] is ComboBoxItem item &&
                    item.Content.ToString() == config.PlatformName)
                {
                    cmbPlatform.SelectedIndex = i;
                    platformFound = true;
                    break;
                }
            }

            // Si la plateforme n'est pas dans la liste, sélectionner "Custom" et mettre à jour le texte
            if (!platformFound)
            {
                cmbPlatform.SelectedIndex = cmbPlatform.Items.Count - 1; // "Custom" est le dernier élément
                if (cmbPlatform.SelectedItem is ComboBoxItem customItem)
                {
                    customItem.Content = config.PlatformName;
                }
            }

            // Chemin et arguments
            txtPath.Text = config.Path;
            txtArguments.Text = config.LaunchArguments;

            // Mettre à jour l'interface selon le type de chemin
            UpdateUIForPathType(config.IsUrl);
        }

        // Mettre à jour l'interface selon le type de chemin (URL ou local)
        private void UpdateUIForPathType(bool isUrl)
        {
            btnBrowse.IsEnabled = !isUrl;
            txtPath.Text = txtPath.Text.Trim();

            if (isUrl && !txtPath.Text.StartsWith("http"))
            {
                txtPath.Text = "https://";
            }
        }

        // Gérer le changement de type de chemin
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized) // Éviter de déclencher pendant l'initialisation
            {
                UpdateUIForPathType(rbUrl.IsChecked == true);
            }
        }

        // Parcourir pour sélectionner un fichier
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Exécutables (*.exe)|*.exe|Tous les fichiers (*.*)|*.*",
                Title = "Sélectionner une application"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                txtPath.Text = openFileDialog.FileName;
            }
        }

        // Fermer la boîte de dialogue
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Annuler
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Valider
        private void btnValidate_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer la plateforme sélectionnée
            string platformName = "";
            if (cmbPlatform.SelectedItem is ComboBoxItem selectedItem)
            {
                platformName = selectedItem.Content.ToString();
            }

            // Valider les entrées
            if (string.IsNullOrWhiteSpace(platformName))
            {
                CustomMessageBox.Show(this, "La plateforme est obligatoire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                cmbPlatform.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPath.Text))
            {
                CustomMessageBox.Show(this, "Le chemin est obligatoire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPath.Focus();
                return;
            }

            // Vérifier si le chemin est valide
            bool isUrl = rbUrl.IsChecked == true;
            string path = txtPath.Text.Trim();

            if (isUrl)
            {
                // Vérifier le format de l'URL
                if (!path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    CustomMessageBox.Show(this, "L'URL doit commencer par 'http://' ou 'https://'.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtPath.Focus();
                    return;
                }
            }
            else
            {
                // Vérifier si le fichier existe
                if (!System.IO.File.Exists(path))
                {
                    MessageBoxResult result = CustomMessageBox.Show(this,
                        "Le fichier spécifié n'existe pas. Voulez-vous continuer quand même ?",
                        "Fichier introuvable", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                    {
                        txtPath.Focus();
                        return;
                    }
                }
            }

            // Mise à jour des propriétés
            PathConfig.PlatformName = platformName;
            PathConfig.Path = path;
            PathConfig.IsUrl = isUrl;
            PathConfig.LaunchArguments = txtArguments.Text.Trim();

            // Retourner true
            this.DialogResult = true;
            this.Close();
        }

        private void cmbPlatform_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}