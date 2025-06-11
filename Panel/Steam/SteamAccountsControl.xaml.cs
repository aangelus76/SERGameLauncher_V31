using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SERGamesLauncher_V31
{
    public partial class SteamAccountsControl : UserControl
    {
        // Collection observable des comptes avec propriété supplémentaire pour l'affichage
        private ObservableCollection<SteamAccountDisplayModel> accountsDisplay;

        // Liste brute des comptes pour les opérations
        private List<SteamAccount> accounts;

        // État pour savoir si les mots de passe sont visibles
        private bool arePasswordsVisible = false;

        // Timer pour masquer automatiquement les mots de passe
        private DispatcherTimer passwordHideTimer;

        // Nom du poste actuel
        private string currentComputerName = Environment.MachineName;

        // Classe pour afficher les comptes dans le DataGrid avec propriété supplémentaire
        private class SteamAccountDisplayModel : INotifyPropertyChanged
        {
            private SteamAccount _account;
            private string _passwordDisplay;
            private bool _showPassword;
            private bool _isCurrentComputer;

            public string Id => _account.Id;
            public string PosteName => _account.PosteName;
            public string Username => _account.Username;
            public string EncryptedPassword => _account.EncryptedPassword;

            public bool IsCurrentComputer
            {
                get => _isCurrentComputer;
                set
                {
                    if (_isCurrentComputer != value)
                    {
                        _isCurrentComputer = value;
                        OnPropertyChanged(nameof(IsCurrentComputer));
                    }
                }
            }

            public string PasswordDisplay
            {
                get => _passwordDisplay;
                set
                {
                    if (_passwordDisplay != value)
                    {
                        _passwordDisplay = value;
                        OnPropertyChanged(nameof(PasswordDisplay));
                    }
                }
            }

            public SteamAccountDisplayModel(SteamAccount account, bool showPassword = false, bool isCurrentComputer = false)
            {
                _account = account;
                _showPassword = showPassword;
                _isCurrentComputer = isCurrentComputer;
                UpdatePasswordDisplay();
            }

            public void UpdatePasswordDisplay()
            {
                if (_showPassword)
                {
                    // Déchiffrer et afficher le mot de passe réel
                    PasswordDisplay = SteamAccountService.DecryptPassword(EncryptedPassword);
                }
                else
                {
                    // Afficher des astérisques (de longueur fixe)
                    PasswordDisplay = new string('*', 16);
                }
            }

            public void SetShowPassword(bool show)
            {
                _showPassword = show;
                UpdatePasswordDisplay();
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public SteamAccountsControl()
        {
            InitializeComponent();
            LoadAccounts();

            // S'abonner à l'événement Unloaded
            this.Unloaded += SteamAccountsControl_Unloaded;
        }

        private void SteamAccountsControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Arrêter le timer si nécessaire
            if (passwordHideTimer != null && passwordHideTimer.IsEnabled)
            {
                passwordHideTimer.Stop();
                passwordHideTimer = null;
            }
        }

        // Charge les comptes depuis le service
        private void LoadAccounts()
        {
            // Charger les comptes
            accounts = SteamAccountService.LoadSteamAccounts();

            // Créer les modèles d'affichage
            accountsDisplay = new ObservableCollection<SteamAccountDisplayModel>();
            foreach (var account in accounts)
            {
                bool isCurrentComputer = account.PosteName.Equals(currentComputerName, StringComparison.OrdinalIgnoreCase);
                accountsDisplay.Add(new SteamAccountDisplayModel(account, arePasswordsVisible, isCurrentComputer));
            }

            // Lier au DataGrid
            accountsDataGrid.ItemsSource = accountsDisplay;
        }

        // Bascule l'affichage des mots de passe
        private void ShowPasswords_Click(object sender, RoutedEventArgs e)
        {
            // Si on veut afficher les mots de passe, demander le mot de passe d'administration
            if (!arePasswordsVisible)
            {
                PasswordDialog passwordDialog = new PasswordDialog();
                passwordDialog.Owner = Window.GetWindow(this);
                passwordDialog.DialogMessage = "Veuillez entrer le mot de passe administrateur pour afficher les mots de passe";
                passwordDialog.ShowDialog();

                // Vérifier si l'authentification a réussi
                if (!passwordDialog.IsAuthenticated)
                {
                    return; // Ne pas continuer si l'authentification a échoué
                }
            }
            else
            {
                // Si on est en train de masquer manuellement, annuler le timer
                if (passwordHideTimer != null && passwordHideTimer.IsEnabled)
                {
                    passwordHideTimer.Stop();
                    passwordHideTimer = null;
                }
            }

            // Inverser l'état
            arePasswordsVisible = !arePasswordsVisible;

            // Mettre à jour le texte du bouton
            btnShowPasswords.Content = arePasswordsVisible ? "Masquer les MDP" : "Voir les MDP";

            // Mettre à jour l'affichage des mots de passe
            foreach (var accountDisplay in accountsDisplay)
            {
                accountDisplay.SetShowPassword(arePasswordsVisible);
            }

            // Si les mots de passe sont maintenant visibles, démarrer le timer pour les masquer automatiquement
            if (arePasswordsVisible)
            {
                StartPasswordHideTimer();
            }
        }

        private void StartPasswordHideTimer()
        {
            // Créer et configurer le timer
            passwordHideTimer = new DispatcherTimer();
            passwordHideTimer.Interval = TimeSpan.FromSeconds(10);
            passwordHideTimer.Tick += (s, e) =>
            {
                // Masquer les mots de passe après 30 secondes
                arePasswordsVisible = false;
                btnShowPasswords.Content = "Voir les MDP";

                foreach (var accountDisplay in accountsDisplay)
                {
                    accountDisplay.SetShowPassword(false);
                }

                // Arrêter le timer
                passwordHideTimer.Stop();
                passwordHideTimer = null;
            };

            // Démarrer le timer
            passwordHideTimer.Start();
        }

        // Ajouter un nouveau compte
        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            // Créer la boîte de dialogue
            AddEditAccountDialog dialog = new AddEditAccountDialog();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true && dialog.Account != null)
            {
                // Vérifier si le nom de poste existe déjà
                if (SteamAccountService.PosteNameExists(accounts, dialog.Account.PosteName))
                {
                    CustomMessageBox.Show(Window.GetWindow(this),
                        $"Un compte avec le poste '{dialog.Account.PosteName}' existe déjà.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Chiffrer le mot de passe
                dialog.Account.EncryptedPassword = SteamAccountService.EncryptPassword(dialog.PlainPassword);

                // Ajouter à la liste
                accounts.Add(dialog.Account);

                // Vérifier si c'est le poste actuel
                bool isCurrentComputer = dialog.Account.PosteName.Equals(currentComputerName, StringComparison.OrdinalIgnoreCase);
                accountsDisplay.Add(new SteamAccountDisplayModel(dialog.Account, arePasswordsVisible, isCurrentComputer));

                // Sauvegarder les changements
                SaveAccounts();
            }
        }

        // Modifier un compte existant - SUPPRESSION DE L'AUTHENTIFICATION
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string accountId)
            {
                // SUPPRIMÉ : Demande de mot de passe d'administration
                // On est déjà dans le panel admin, pas besoin de re-authentifier

                // Trouver le compte
                var account = accounts.FirstOrDefault(a => a.Id == accountId);
                if (account == null) return;

                // Créer la boîte de dialogue et pré-remplir les champs
                AddEditAccountDialog dialog = new AddEditAccountDialog(account);
                dialog.Owner = Window.GetWindow(this);

                if (dialog.ShowDialog() == true && dialog.Account != null)
                {
                    // Vérifier si le nouveau nom de poste existe déjà
                    if (SteamAccountService.PosteNameExists(accounts, dialog.Account.PosteName, accountId))
                    {
                        CustomMessageBox.Show(Window.GetWindow(this),
                            $"Un compte avec le poste '{dialog.Account.PosteName}' existe déjà.",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Mettre à jour les champs
                    account.PosteName = dialog.Account.PosteName;
                    account.Username = dialog.Account.Username;

                    // Mettre à jour le mot de passe si nécessaire
                    if (!string.IsNullOrEmpty(dialog.PlainPassword))
                    {
                        account.EncryptedPassword = SteamAccountService.EncryptPassword(dialog.PlainPassword);
                    }

                    // Mettre à jour l'affichage
                    RefreshDisplay();

                    // Sauvegarder les changements
                    SaveAccounts();
                }
            }
        }

        // Supprimer un compte - SUPPRESSION DE L'AUTHENTIFICATION
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string accountId)
            {
                // SUPPRIMÉ : Demande de mot de passe d'administration
                // On est déjà dans le panel admin, pas besoin de re-authentifier

                // Trouver le compte
                var account = accounts.FirstOrDefault(a => a.Id == accountId);
                if (account != null)
                {
                    // Demander confirmation directe
                    MessageBoxResult result = CustomMessageBox.Show(Window.GetWindow(this),
                        $"Êtes-vous sûr de vouloir supprimer le compte '{account.PosteName}' ?",
                        "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Supprimer de la liste
                        accounts.Remove(account);

                        // Mettre à jour l'affichage
                        var displayAccount = accountsDisplay.FirstOrDefault(a => a.Id == accountId);
                        if (displayAccount != null)
                        {
                            accountsDisplay.Remove(displayAccount);
                        }

                        // Sauvegarder les changements
                        SaveAccounts();
                    }
                }
            }
        }

        // Mettre à jour l'affichage
        private void RefreshDisplay()
        {
            accountsDisplay.Clear();
            foreach (var account in accounts)
            {
                bool isCurrentComputer = account.PosteName.Equals(currentComputerName, StringComparison.OrdinalIgnoreCase);
                accountsDisplay.Add(new SteamAccountDisplayModel(account, arePasswordsVisible, isCurrentComputer));
            }
        }

        // Sauvegarder les changements
        private void SaveAccounts()
        {
            SteamAccountService.SaveSteamAccounts(accounts);
        }
    }
}