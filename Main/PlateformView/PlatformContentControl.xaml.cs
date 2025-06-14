using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Windows.Threading;

namespace SERGamesLauncher_V31
{
    public partial class PlatformContentControl : UserControl
    {
        private string currentPlatform = null;
        private SteamActivityMonitor steamMonitor;
        private bool allowUserSteamAccounts = false;
        private bool allowSteamUpdates = false;

        // Nouveaux champs pour la protection contre les clics multiples
        private DispatcherTimer launchCooldownTimer;
        private DispatcherTimer countdownTimer;
        private string originalButtonText;
        private bool isLaunchButtonCoolingDown = false;
        private int remainingSeconds = 30;

        // Propriété pour limiter le changement de vue
        public bool AllowViewChange { get; private set; } = true;

        // Propriété pour gérer l'état du cooldown
        private bool IsCooldownActive { get; set; } = false;

        // Événement lorsque l'état de navigation change
        public event EventHandler<bool> NavigationStateChanged;

        public event EventHandler<string> LaunchRequested;

        // Constructeur qui accepte un SteamActivityMonitor externe
        public PlatformContentControl(SteamActivityMonitor monitor)
        {
            InitializeComponent();

            // Utiliser le moniteur Steam externe
            steamMonitor = monitor;
            steamMonitor.UserChanged += SteamMonitor_UserChanged;

            // Initialiser le timer de cooldown
            launchCooldownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            launchCooldownTimer.Tick += LaunchCooldownTimer_Tick;

            // Initialiser le timer de décompte (1 seconde)
            countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            countdownTimer.Tick += CountdownTimer_Tick;

            // Stocker le texte original du bouton
            originalButtonText = btnLaunch.Content.ToString();

            // S'abonner aux changements de la checkbox
            chkConsent.Checked += OnCheckboxChanged;
            chkConsent.Unchecked += OnCheckboxChanged;
        }

        // Gestionnaire pour le changement d'utilisateur Steam
        private void SteamMonitor_UserChanged(string username)
        {
            // Mettre à jour l'affichage du compte actuel
            Dispatcher.Invoke(() =>
            {
                txtCurrentSteamAccount.Text = string.IsNullOrEmpty(username) ? "Compte: Non détecté" : $"Compte: {username}";
            });
        }

        // Gestionnaire pour le changement de la checkbox
        private void OnCheckboxChanged(object sender, RoutedEventArgs e)
        {
            UpdateButtonState();
        }

        // Mettre à jour l'état du bouton selon les conditions
        private void UpdateButtonState()
        {
            btnLaunch.IsEnabled = chkConsent.IsChecked == true && !isLaunchButtonCoolingDown && !IsCooldownActive;
        }

        // Afficher la vue d'information
        public void ShowInfoView()
        {
            infoView.Visibility = Visibility.Visible;
            platformView.Visibility = Visibility.Collapsed;
            currentPlatform = null;
            HideSteamControls();
        }

        // Configurer la plateforme
        public void ConfigurePlatform(string platformName)
        {
            if (IsCooldownActive)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    "Un lancement est en cours. Veuillez attendre la fin du cooldown avant de changer de plateforme.",
                    "Lancement en cours", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            currentPlatform = platformName;
            infoView.Visibility = Visibility.Collapsed;
            platformView.Visibility = Visibility.Visible;

            // Réinitialiser la case à cocher
            chkConsent.IsChecked = false;

            // Configurations spécifiques à la plateforme
            switch (platformName)
            {
                case "Steam":
                    txtPlatformTitle.Text = "Steam";
                    txtAccountMessage.Text = "Nous prêtons un compte Steam";
                    txtInstructions.Text = "Un compte Steam configuré pour ce poste sera utilisé automatiquement. Toute tentative de déconnexion ou de changement de compte sera annulée.";
                    imgPlatformLogo.Source = (BitmapImage)Application.Current.Resources["SteamColor"];
                    // Activer le suivi de compte Steam et afficher les contrôles associés
                    txtCurrentSteamAccount.Visibility = Visibility.Visible;
                    steamControlsPanel.Visibility = Visibility.Visible;
                    // Mettre à jour l'état des toggles pour qu'ils correspondent à l'état actuel
                    toggleAllowUserAccounts.IsChecked = steamMonitor.AllowUserAccounts;
                    allowUserSteamAccounts = steamMonitor.AllowUserAccounts;
                    // Initialiser le toggle mises à jour à false
                    toggleAllowSteamUpdates.IsChecked = false;
                    allowSteamUpdates = false;
                    break;
                case "Epic":
                    txtPlatformTitle.Text = "Epic Games";
                    txtAccountMessage.Text = "Nous ne prêtons pas de compte Epic Games";
                    txtInstructions.Text = "Vous devez utiliser votre propre compte Epic Games.";
                    imgPlatformLogo.Source = (BitmapImage)Application.Current.Resources["EpicColor"];
                    HideSteamControls();
                    break;
                case "Crazy":
                    txtPlatformTitle.Text = "CrazyGames (site de jeux en ligne)";
                    txtAccountMessage.Text = "Accès en ligne libre et gratuit";
                    txtInstructions.Text = "Aucun compte n'est nécessaire pour jouer à la plupart des jeux.";
                    imgPlatformLogo.Source = (BitmapImage)Application.Current.Resources["CrazyColor"];
                    HideSteamControls();
                    break;
                case "Roblox":
                    txtPlatformTitle.Text = "Roblox";
                    txtAccountMessage.Text = "Nous ne prêtons pas de compte Roblox";
                    txtInstructions.Text = "Vous devez utiliser votre propre compte Roblox";
                    imgPlatformLogo.Source = (BitmapImage)Application.Current.Resources["RobloxColor"];
                    HideSteamControls();
                    break;
                case "BGA":
                    txtPlatformTitle.Text = "BoardGameArena (jeux de société en ligne)";
                    txtAccountMessage.Text = "Nous ne prêtons pas de compte BGA";
                    txtInstructions.Text = "Vous devez utiliser votre propre compte BoradGameArena";
                    imgPlatformLogo.Source = (BitmapImage)Application.Current.Resources["BGAColor"];
                    HideSteamControls();
                    break;
                case "Xbox":
                    txtPlatformTitle.Text = "Xbox Game Pass";
                    txtAccountMessage.Text = "Nous ne prêtons pas de compte Xbox";
                    txtInstructions.Text = "Vous devez utiliser votre propre compte Microsoft pour accéder au Xbox Game Pass.";
                    imgPlatformLogo.Source = (BitmapImage)Application.Current.Resources["XboxColor"];
                    HideSteamControls();
                    break;
                default:
                    ShowInfoView();
                    break;
            }
        }

        private void HideSteamControls()
        {
            txtCurrentSteamAccount.Visibility = Visibility.Collapsed;
            steamControlsPanel.Visibility = Visibility.Collapsed;
        }

        // Gestionnaire pour le bouton de lancement
        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPlatform == null || isLaunchButtonCoolingDown) return;

            originalButtonText = btnLaunch.Content.ToString();

            remainingSeconds = 30;
            btnLaunch.Content = $"Exécution ({remainingSeconds})";

            IsCooldownActive = true;
            UpdateButtonState();

            isLaunchButtonCoolingDown = true;

            chkConsent.IsEnabled = false;

            AllowViewChange = false;
            NavigationStateChanged?.Invoke(this, false);

            LaunchRequested?.Invoke(this, currentPlatform);

            launchCooldownTimer.Start();
            countdownTimer.Start();

            List<PathConfig> pathConfigs = PathConfigService.LoadPathConfigs();
            PathConfig pathConfig = PathConfigService.GetPathForPlatform(pathConfigs, currentPlatform);

            if (pathConfig == null)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    $"La plateforme {currentPlatform} n'est pas configurée. Veuillez contacter un administrateur.",
                    "Plateforme non configurée", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!pathConfig.IsUrl && !File.Exists(pathConfig.Path))
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    $"Le chemin d'accès pour {currentPlatform} est invalide. Veuillez contacter un administrateur.",
                    "Chemin invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (currentPlatform == "Steam")
            {
                LaunchSteam(pathConfig);
            }
            else
            {
                LaunchPlatform(pathConfig);
            }
        }

        // Timer de décompte
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            btnLaunch.Content = $"{originalButtonText} ({remainingSeconds}s)";
            remainingSeconds--;

            if (remainingSeconds < 0)
            {
                countdownTimer.Stop();
            }
        }

        // Timer de cooldown principal
        private void LaunchCooldownTimer_Tick(object sender, EventArgs e)
        {
            launchCooldownTimer.Stop();
            countdownTimer.Stop();

            IsCooldownActive = false;
            AllowViewChange = true;
            isLaunchButtonCoolingDown = false;
            btnLaunch.Content = originalButtonText;

            chkConsent.IsEnabled = true;
            UpdateButtonState();

            NavigationStateChanged?.Invoke(this, true);
        }

        // Lancer Steam avec le compte configuré
        private void LaunchSteam(PathConfig pathConfig)
        {
            try
            {
                List<SteamAccount> steamAccounts = SteamAccountService.LoadSteamAccounts();
                SteamAccount account = SteamAccountService.GetAccountForCurrentComputer(steamAccounts);
                if (account == null)
                {
                    CustomMessageBox.Show(Window.GetWindow(this),
                        "Aucun compte Steam n'est configuré pour ce poste. Veuillez contacter un administrateur.",
                        "Compte Steam manquant", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!File.Exists(pathConfig.Path))
                {
                    CustomMessageBox.Show(Window.GetWindow(this),
                        $"Le fichier Steam n'existe pas à l'emplacement configuré: {pathConfig.Path}\nVeuillez contacter un administrateur.",
                        "Chemin invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string password = SteamAccountService.DecryptPassword(account.EncryptedPassword);

                string baseArguments = $"-noreactlogin -login {account.Username} {password}";
                string finalArguments = allowSteamUpdates ? baseArguments : $"{baseArguments} -no-browser +@NoUpdates";

                ProcessStartInfo accountStartInfo = new ProcessStartInfo
                {
                    FileName = pathConfig.Path,
                    Arguments = finalArguments
                };

                Process process = Process.Start(accountStartInfo);

                steamMonitor.NotifyStarting();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    $"Erreur lors du lancement de Steam: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LaunchPlatform(PathConfig pathConfig)
        {
            try
            {
                if (pathConfig.IsUrl)
                {
                    Process.Start(pathConfig.Path);
                }
                else
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = pathConfig.Path
                    };

                    if (!string.IsNullOrWhiteSpace(pathConfig.LaunchArguments))
                    {
                        startInfo.Arguments = pathConfig.LaunchArguments;
                    }

                    Process.Start(startInfo);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    $"Erreur lors du lancement de {currentPlatform}: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Gestionnaire pour le toggle d'autorisation des comptes personnels - CORRIGÉ
        private void ToggleAllowUserAccounts_Changed(object sender, RoutedEventArgs e)
        {
            bool newState = toggleAllowUserAccounts.IsChecked ?? false;

            if (newState && !allowUserSteamAccounts)
            {
                PasswordDialog passwordDialog = new PasswordDialog();
                passwordDialog.Owner = Window.GetWindow(this);
                passwordDialog.DialogMessage = "Veuillez entrer le mot de passe administrateur pour autoriser les comptes personnels";
                passwordDialog.ShowDialog();

                if (!passwordDialog.IsAuthenticated)
                {
                    toggleAllowUserAccounts.IsChecked = false;
                    return;
                }

                CustomMessageBox.Show(Window.GetWindow(this),
                    "Attention: Les comptes personnels sont maintenant autorisés sur Steam.\n\nCette option sera désactivée à la fermeture de l'application.",
                    "Mode administrateur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            allowUserSteamAccounts = newState;
            steamMonitor.AllowUserAccounts = allowUserSteamAccounts;

            // NOUVEAU : Appliquer la même logique que le panel admin
            // Si comptes personnels autorisés = pas de protection sur les dossiers
            FolderPermissionsControl.SetProtectionForAllFolders(!newState);

            // Notifier le panel admin du changement
            NotifyAdminPanelOfChange();
        }

        private void ToggleAllowSteamUpdates_Changed(object sender, RoutedEventArgs e)
        {
            bool newState = toggleAllowSteamUpdates.IsChecked ?? false;

            if (newState && !allowSteamUpdates)
            {
                PasswordDialog passwordDialog = new PasswordDialog();
                passwordDialog.Owner = Window.GetWindow(this);
                passwordDialog.DialogMessage = "Veuillez entrer le mot de passe administrateur pour autoriser les mises à jour Steam";
                passwordDialog.ShowDialog();

                if (!passwordDialog.IsAuthenticated)
                {
                    toggleAllowSteamUpdates.IsChecked = false;
                    return;
                }

                CustomMessageBox.Show(Window.GetWindow(this),
                    "Attention: Les mises à jour Steam sont maintenant autorisées.\n\nCette option sera désactivée à la fermeture de l'application.",
                    "Mode administrateur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            allowSteamUpdates = newState;

            // COPIE EXACTE de ToggleLockAll_Changed du panel admin
            try
            {
                var folders = FolderPermissionService.LoadFolderPermissions();
                if (folders == null || folders.Count == 0) return;

                bool isChecked = !newState; // Inverse: si mises à jour autorisées = pas de protection

                if (isChecked)
                {
                    // Verrouiller tous les dossiers
                    foreach (var folder in folders)
                    {
                        folder.IsProtectionEnabled = true;
                        FolderPermissionService.ApplyProtection(folder);
                    }
                }
                else
                {
                    // Déverrouiller tous les dossiers
                    foreach (var folder in folders)
                    {
                        folder.IsProtectionEnabled = false;
                        FolderPermissionService.RemoveProtection(folder);
                    }
                }

                // Sauvegarder les changements
                FolderPermissionService.SaveFolderPermissions(folders);

                // Notifier le panel admin du changement
                NotifyAdminPanelOfChange();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur toggle mise à jour: {ex.Message}");
            }
        }

        // NOUVELLE MÉTHODE : Notifier le panel admin du changement
        private void NotifyAdminPanelOfChange()
        {
            try
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is AdminPanelWindow adminPanel)
                    {
                        adminPanel.RefreshFolderPermissionsDisplay();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur notification admin: {ex.Message}");
            }
        }
    }
}