// Panel/Folder/AddEditFolderDialog.xaml.cs
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Xml.Linq;

namespace SERGamesLauncher_V31
{
    public partial class AddEditFolderDialog : Window
    {
        // Propriété pour retourner l'information de permission
        public FolderPermission FolderPermission { get; private set; }

        // Mode d'édition
        //private bool isEditMode = false;
        private string originalId;

        // Constructeur pour l'ajout
        public AddEditFolderDialog()
        {
            InitializeComponent();

            // Permettre le déplacement de la fenêtre sans bordure
            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            // Initialiser une nouvelle permission
            FolderPermission = new FolderPermission();

            // Définir le titre
            titleTextBlock.Text = "Ajouter un dossier protégé";

            // Sélectionner le premier élément de la liste déroulante de niveau de protection
            cmbProtectionLevel.SelectedIndex = 0;
        }

        // Constructeur pour l'édition
        public AddEditFolderDialog(FolderPermission permissionToEdit)
        {
            InitializeComponent();

            // Permettre le déplacement de la fenêtre sans bordure
            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            // Copier les valeurs de la permission à éditer
            FolderPermission = new FolderPermission
            {
                Id = permissionToEdit.Id,
                Name = permissionToEdit.Name,
                FolderPath = permissionToEdit.FolderPath,
                IsProtectionEnabled = permissionToEdit.IsProtectionEnabled,
                EnableOnStartup = permissionToEdit.EnableOnStartup,
                ProtectionLevel = permissionToEdit.ProtectionLevel,
                LastModified = permissionToEdit.LastModified
            };

            // Conserver l'ID original pour la vérification de doublon
            originalId = permissionToEdit.Id;

            // Pré-remplir les champs
            PreFillFields(permissionToEdit);

            // Activer le mode édition
            //isEditMode = true;

            // Définir le titre
            titleTextBlock.Text = "Modifier un dossier protégé";
        }

        // Pré-remplir les champs avec la permission existante
        private void PreFillFields(FolderPermission permission)
        {
            txtName.Text = permission.Name;
            txtFolderPath.Text = permission.FolderPath;
            chkEnableOnStartup.IsChecked = permission.EnableOnStartup;

            // Sélectionner le niveau de protection dans la liste déroulante
            switch (permission.ProtectionLevel)
            {
                case ProtectionLevel.ReadOnly:
                    cmbProtectionLevel.SelectedIndex = 0;
                    break;
                case ProtectionLevel.PreventDeletion:
                    cmbProtectionLevel.SelectedIndex = 1;
                    break;
                case ProtectionLevel.PreventCreation:
                    cmbProtectionLevel.SelectedIndex = 2;
                    break;
                default:
                    cmbProtectionLevel.SelectedIndex = 0;
                    break;
            }
        }

        // Parcourir pour sélectionner un dossier
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Utiliser Microsoft.Win32.OpenFileDialog comme une alternative à FolderBrowserDialog
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "Sélectionner un dossier à protéger";
            dialog.FileName = "Sélectionner"; // Évite d'afficher un nom de fichier par défaut
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;
            dialog.ValidateNames = false;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

            // Cette astuce permet d'utiliser OpenFileDialog pour choisir un dossier
            dialog.FileName = "Dossier sélectionné";

            if (dialog.ShowDialog() == true)
            {
                // Récupère le chemin du dossier (enlève le nom de fichier)
                string folderPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                txtFolderPath.Text = folderPath;
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
            // Valider les entrées
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                CustomMessageBox.Show(this, "Le nom du dossier est obligatoire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFolderPath.Text))
            {
                CustomMessageBox.Show(this, "Le chemin du dossier est obligatoire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                txtFolderPath.Focus();
                return;
            }

            // Vérifier si le dossier existe
            if (!Directory.Exists(txtFolderPath.Text))
            {
                MessageBoxResult result = CustomMessageBox.Show(this,
                    "Le dossier spécifié n'existe pas. Voulez-vous le créer ?",
                    "Dossier introuvable", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(txtFolderPath.Text);
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show(this, $"Impossible de créer le dossier : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        txtFolderPath.Focus();
                        return;
                    }
                }
                else
                {
                    txtFolderPath.Focus();
                    return;
                }
            }

            // Déterminer le niveau de protection
            ProtectionLevel protectionLevel = ProtectionLevel.ReadOnly;
            ComboBoxItem selectedItem = cmbProtectionLevel.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                switch (selectedItem.Tag?.ToString())
                {
                    case "ReadOnly":
                        protectionLevel = ProtectionLevel.ReadOnly;
                        break;
                    case "PreventDeletion":
                        protectionLevel = ProtectionLevel.PreventDeletion;
                        break;
                    case "PreventCreation":
                        protectionLevel = ProtectionLevel.PreventCreation;
                        break;
                }
            }

            // Mise à jour des propriétés
            FolderPermission.Name = txtName.Text.Trim();
            FolderPermission.FolderPath = txtFolderPath.Text.Trim();
            FolderPermission.EnableOnStartup = chkEnableOnStartup.IsChecked ?? true;
            FolderPermission.ProtectionLevel = protectionLevel;

            // Retourner true
            this.DialogResult = true;
            this.Close();
        }
    }
}