using System;
using System.Windows;

namespace SERGamesLauncher_V31
{
    public partial class AddEditAccountDialog : Window
    {
        // Propriétés pour retourner les informations du compte
        public SteamAccount Account { get; private set; }
        public string PlainPassword { get; private set; }

        // Mode d'édition
        private bool isEditMode = false;
        private string originalId;

        // Constructeur pour l'ajout
        public AddEditAccountDialog()
        {
            InitializeComponent();

            // Permettre le déplacement de la fenêtre sans bordure
            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            // Initialiser un nouveau compte
            Account = new SteamAccount();

            // Définir le titre
            titleTextBlock.Text = "Ajouter un compte Steam";
        }

        // Constructeur pour l'édition
        public AddEditAccountDialog(SteamAccount accountToEdit)
        {
            InitializeComponent();

            // Permettre le déplacement de la fenêtre sans bordure
            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            // Copier les valeurs du compte à éditer
            Account = new SteamAccount
            {
                Id = accountToEdit.Id,
                PosteName = accountToEdit.PosteName,
                Username = accountToEdit.Username,
                EncryptedPassword = accountToEdit.EncryptedPassword
            };

            // Conserver l'ID original pour la vérification de doublon
            originalId = accountToEdit.Id;

            // Pré-remplir les champs
            txtPosteName.Text = Account.PosteName;
            txtUsername.Text = Account.Username;
            // Le mot de passe n'est pas pré-rempli pour des raisons de sécurité

            // Activer le mode édition
            isEditMode = true;

            // Définir le titre
            titleTextBlock.Text = "Modifier un compte Steam";
        }

        // Pré-remplir avec le nom du poste actuel
        private void PreFill_Click(object sender, RoutedEventArgs e)
        {
            txtPosteName.Text = Environment.MachineName;
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
            if (string.IsNullOrWhiteSpace(txtPosteName.Text))
            {
                CustomMessageBox.Show(this, "Le nom du poste est obligatoire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPosteName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                CustomMessageBox.Show(this, "L'identifiant Steam est obligatoire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                txtUsername.Focus();
                return;
            }

            // En mode ajout, le mot de passe est obligatoire
            if (!isEditMode && string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                CustomMessageBox.Show(this, "Le mot de passe est obligatoire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPassword.Focus();
                return;
            }

            // Mise à jour des propriétés
            Account.PosteName = txtPosteName.Text.Trim();
            Account.Username = txtUsername.Text.Trim();
            PlainPassword = txtPassword.Password;

            // Retourner true
            this.DialogResult = true;
            this.Close();
        }
    }
}