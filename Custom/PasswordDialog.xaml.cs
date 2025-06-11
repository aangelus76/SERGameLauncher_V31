using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SERGamesLauncher_V31
{
    public partial class PasswordDialog : Window
    {
        // Mot de passe hashé en SHA256
        private const string HashedPassword = "e648af71db16803a6ed3d8e2f679cbe1475ae62e10019220255748058acb9e1b"; // "admin" en SHA256

        public bool IsAuthenticated { get; private set; } = false;

        // Propriété pour le message de la boîte de dialogue
        private string _dialogMessage = "Veuillez entrer le mot de passe administrateur";
        public string DialogMessage
        {
            get { return _dialogMessage; }
            set
            {
                _dialogMessage = value;
                if (messageTextBlock != null)
                    messageTextBlock.Text = value;
            }
        }

        // Propriété pour le titre personnalisé
        private string _customTitle = "Authentification Admin";
        public string CustomTitle
        {
            get { return _customTitle; }
            set
            {
                _customTitle = value;
                if (titleTextBlock != null)
                    titleTextBlock.Text = value;
                // Également mettre à jour le titre de la fenêtre
                this.Title = value;
            }
        }

        public PasswordDialog()
        {
            InitializeComponent();

            // Permettre le déplacement de la fenêtre sans bordure
            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            // Focus sur le champ de mot de passe
            this.Loaded += (s, e) =>
            {
                passwordBox.Focus();

                // S'assurer que les textes sont à jour
                messageTextBlock.Text = DialogMessage;
                titleTextBlock.Text = CustomTitle;
            };

            // Validation par Enter
            passwordBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    ValidatePassword();
            };
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsAuthenticated = false;
            this.Close();
        }

        private void btnValidate_Click(object sender, RoutedEventArgs e)
        {
            ValidatePassword();
        }

        private void ValidatePassword()
        {
            string password = passwordBox.Password;

            // Hash le mot de passe entré
            string hashedInput = HashPassword(password);

            // Afficher le hash pour débogage
            //MessageBox.Show($"Hash généré: {hashedInput}", "Débogage", MessageBoxButton.OK);

            // Vérifier si le hash correspond
            if (hashedInput == HashedPassword)
            {
                IsAuthenticated = true;
                this.Close();
            }
            else
            {

                CustomMessageBox.Show(this, "Mot de passe incorrect!", "Erreur d'authentification", MessageBoxButton.OK, MessageBoxImage.Error);
                //MessageBox.Show("Mot de passe incorrect!", "Erreur d'authentification", MessageBoxButton.OK, MessageBoxImage.Error);
                passwordBox.Clear();
                passwordBox.Focus();
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}