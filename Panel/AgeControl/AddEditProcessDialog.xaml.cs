using System;
using System.Windows;
using System.Windows.Controls;

namespace SERGamesLauncher_V31
{
    public partial class AddEditProcessDialog : Window
    {
        // Propriété pour retourner l'information de restriction
        public ProcessRestriction ProcessRestriction { get; private set; }

        // Mode d'édition
        //private bool isEditMode = false;
        private string originalId;

        // Constructeur pour l'ajout
        public AddEditProcessDialog()
        {
            InitializeComponent();

            // Permettre le déplacement de la fenêtre sans bordure
            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            // Initialiser une nouvelle restriction
            ProcessRestriction = new ProcessRestriction();

            // Définir le titre
            titleTextBlock.Text = "Ajouter une restriction de processus";
        }

        // Constructeur pour l'édition
        public AddEditProcessDialog(ProcessRestriction restrictionToEdit)
        {
            InitializeComponent();

            // Permettre le déplacement de la fenêtre sans bordure
            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            // Copier les valeurs de la restriction à éditer
            ProcessRestriction = new ProcessRestriction
            {
                Id = restrictionToEdit.Id,
                ProcessName = restrictionToEdit.ProcessName,
                MinimumAge = restrictionToEdit.MinimumAge,
                Description = restrictionToEdit.Description,
                IsActive = restrictionToEdit.IsActive
            };

            // Conserver l'ID original pour la vérification de doublon
            originalId = restrictionToEdit.Id;

            // Pré-remplir les champs
            PreFillFields(restrictionToEdit);

            // Activer le mode édition
            //isEditMode = true;

            // Définir le titre
            titleTextBlock.Text = "Modifier une restriction de processus";
        }

        // Pré-remplir les champs avec la restriction existante
        private void PreFillFields(ProcessRestriction restriction)
        {
            txtProcessName.Text = restriction.ProcessName;
            txtMinimumAge.Text = restriction.MinimumAge.ToString();
            txtDescription.Text = restriction.Description;
            chkIsActive.IsChecked = restriction.IsActive;
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

        // Navigateur de processus en cours
        private void btnBrowseProcess_Click(object sender, RoutedEventArgs e)
        {
            ProcessSelectorDialog dialog = new ProcessSelectorDialog();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.SelectedProcessName))
            {
                txtProcessName.Text = dialog.SelectedProcessName;
            }
        }

        // Valider
        private void btnValidate_Click(object sender, RoutedEventArgs e)
        {
            // Valider les entrées
            if (string.IsNullOrWhiteSpace(txtProcessName.Text))
            {
                CustomMessageBox.Show(this, "Le nom du processus est obligatoire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                txtProcessName.Focus();
                return;
            }

            int minimumAge;
            if (!int.TryParse(txtMinimumAge.Text, out minimumAge) || minimumAge < 0 || minimumAge > 100)
            {
                CustomMessageBox.Show(this, "L'âge minimum doit être un nombre entier entre 0 et 100.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                txtMinimumAge.Focus();
                return;
            }

            // Mise à jour des propriétés
            ProcessRestriction.ProcessName = txtProcessName.Text.Trim();
            ProcessRestriction.MinimumAge = minimumAge;
            ProcessRestriction.Description = txtDescription.Text.Trim();
            ProcessRestriction.IsActive = chkIsActive.IsChecked ?? true;

            // Retourner true
            this.DialogResult = true;
            this.Close();
        }
    }
}