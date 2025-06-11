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
        private bool allowSteamUpdates = false; // NOUVEAU : État du toggle mises à jour
        //private bool isSteamMonitorStarted = false;

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

            // S'abonner à l'événement Unloaded pour nettoyer les ressources
            this.Unloaded += PlatformContentControl_Unloaded;
        }

        private void PlatformContentControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Nettoyer les timers
            if (launchCooldownTimer != null)
            {
                launchCooldownTimer.Stop();
                launchCooldownTimer.Tick -= LaunchCooldownTimer_Tick;
            }

            if (countdownTimer != null)
            {
                countdownTimer.Stop();
                countdownTimer.Tick -= CountdownTimer_Tick;
            }

            if (chkConsent != null)
            {
                chkConsent.Checked -= OnCheckboxChanged;
                chkConsent.Unchecked -= OnCheckboxChanged;
            }
        }

        // Event handler pour la checkbox
        private void OnCheckboxChanged(object sender, RoutedEventArgs e)
        {
            UpdateButtonState();
        }

        // Méthode complète pour gérer l'état du bouton
        private void UpdateButtonState()
        {
            // Le bouton est activé uniquement si :
            // 1. La checkbox est cochée
            // 2. Aucun cooldown en cours
            btnLaunch.IsEnabled = (chkConsent.IsChecked ?? false) && !IsCooldownActive;
        }

        // Méthode pour gérer le timer de décompte (1 seconde)
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;

            if (remainingSeconds <= 0)
            {
                // Le décompte est terminé, arrêter ce timer
                countdownTimer.Stop();
            }
            else
            {
                // Mettre à jour le texte du bouton avec le nombre de secondes restantes
                btnLaunch.Content = $"Exécution ({remainingSeconds})";
            }
        }

        // Méthode pour gérer la fin du timer principal
        private void LaunchCooldownTimer_Tick(object sender, EventArgs e)
        {
            // Restaurer le texte du bouton
            btnLaunch.Content = originalButtonText;
            launchCooldownTimer.Stop();
            isLaunchButtonCoolingDown = false;

            // Marquer que le cooldown est terminé
            IsCooldownActive = false;

            // Réactiver la checkbox
            chkConsent.IsEnabled = true;

            // Mettre à jour l'état du bouton
            UpdateButtonState();

            // Permettre à nouveau le changement de vue
            AllowViewChange = true;
            NavigationStateChanged?.Invoke(this, true);
        }

        private void SteamMonitor_UserChanged(string username)
        {
            // Mettre à jour l'affichage du compte Steam actuel
            Dispatcher.Invoke(() =>
            {
                txtCurrentSteamAccount.Text = $"Compte: {username}";
            });
        }

        // Afficher la vue d'information
        public void ShowInfoView()
        {
            infoView.Visibility = Visibility.Visible;
            platformView.Visibility = Visibility.Collapsed;
            currentPlatform = null;

            // Ne pas arrêter le moniteur Steam, car il est géré par MainWindow
            //isSteamMonitorStarted = false;
        }

        // Configurer et afficher une plateforme spécifique
        public void ConfigurePlatform(string platformName)
        {
            // Vérifier si le changement de plateforme est autorisé
            if (!AllowViewChange)
            {
                // Afficher un message indiquant que l'application est en cours d'exécution
                CustomMessageBox.Show(Window.GetWindow(this),
                    $"Une application est en cours de lancement.\nVeuillez attendre {remainingSeconds} secondes avant de changer de plateforme.",
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
                    steamControlsPanel.Visibility = Visibility.Visible; // MODIFIÉ : Utiliser le panel au lieu des contrôles individuels
                    // Indiquer que Steam est sélectionné
                    //isSteamMonitorStarted = true;
                    // Mettre à jour l'état des toggles pour qu'ils correspondent à l'état actuel
                    toggleAllowUserAccounts.IsChecked = steamMonitor.AllowUserAccounts;
                    allowUserSteamAccounts = steamMonitor.AllowUserAccounts;
                    // NOUVEAU : Initialiser le toggle mises à jour à false
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
                    // Cas par défaut, ne devrait pas arriver
                    ShowInfoView();
                    break;
            }
        }

        private void HideSteamControls()
        {
            // Cacher les contrôles spécifiques à Steam
            txtCurrentSteamAccount.Visibility = Visibility.Collapsed;
            steamControlsPanel.Visibility = Visibility.Collapsed; // MODIFIÉ : Utiliser le panel

            // Indiquer que Steam n'est plus sélectionné
            //isSteamMonitorStarted = false;
        }

        // Gestionnaire pour le bouton de lancement - modifié pour gérer le cooldown
        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPlatform == null || isLaunchButtonCoolingDown) return;

            // Désactiver le bouton et changer son texte
            originalButtonText = btnLaunch.Content.ToString();

            // Initialiser le compte à rebours
            remainingSeconds = 30;
            btnLaunch.Content = $"Exécution ({remainingSeconds})";

            // Marquer que le cooldown est actif
            IsCooldownActive = true;
            // Mettre à jour l'état du bouton
            UpdateButtonState();

            isLaunchButtonCoolingDown = true;

            // Désactiver la checkbox pendant le cooldown
            chkConsent.IsEnabled = false;

            // Bloquer la navigation entre les vues
            AllowViewChange = false;
            NavigationStateChanged?.Invoke(this, false);

            // Démarrer les timers
            launchCooldownTimer.Start();
            countdownTimer.Start();

            // Vérifier si la plateforme est configurée
            List<PathConfig> pathConfigs = PathConfigService.LoadPathConfigs();
            PathConfig pathConfig = PathConfigService.GetPathForPlatform(pathConfigs, currentPlatform);

            if (pathConfig == null)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    $"La plateforme {currentPlatform} n'est pas configurée. Veuillez contacter un administrateur.",
                    "Plateforme non configurée", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Vérifier que le chemin existe
            if (!pathConfig.IsUrl && !File.Exists(pathConfig.Path))
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    $"Le chemin d'accès pour {currentPlatform} est invalide. Veuillez contacter un administrateur.",
                    "Chemin invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Pour Steam, utiliser le moniteur Steam et les identifiants
            if (currentPlatform == "Steam")
            {
                LaunchSteam();
            }
            else
            {
                // Pour les autres plateformes, lancer normalement
                LaunchPlatform(pathConfig);
            }

            // Déclencher l'événement de lancement
            LaunchRequested?.Invoke(this, currentPlatform);
        }

        private void LaunchSteam()
        {
            try
            {
                // Si on autorise les comptes personnels, lancer Steam normalement
                if (allowUserSteamAccounts)
                {
                    var configs = PathConfigService.LoadPathConfigs();
                    var config = PathConfigService.GetPathForPlatform(configs, "Steam");

                    if (config != null)
                    {
                        // NOUVEAU : Ajouter l'argument pour les mises à jour si autorisées
                        string personalArgs = allowSteamUpdates ? "" : "-no-browser +@NoUpdates";

                        ProcessStartInfo personalStartInfo = new ProcessStartInfo
                        {
                            FileName = config.Path,
                            Arguments = personalArgs
                        };

                        Process.Start(personalStartInfo);
                    }
                    return;
                }

                // Récupérer le compte Steam pour ce poste
                List<SteamAccount> steamAccounts = SteamAccountService.LoadSteamAccounts();
                SteamAccount account = SteamAccountService.GetAccountForCurrentComputer(steamAccounts);

                if (account == null)
                {
                    CustomMessageBox.Show(Window.GetWindow(this),
                        "Aucun compte Steam n'est configuré pour ce poste. Veuillez contacter un administrateur.",
                        "Compte non configuré", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Récupérer le chemin de Steam
                var pathConfigs = PathConfigService.LoadPathConfigs();
                var pathConfig = PathConfigService.GetPathForPlatform(pathConfigs, "Steam");

                if (pathConfig == null || !File.Exists(pathConfig.Path))
                {
                    CustomMessageBox.Show(Window.GetWindow(this),
                        "Le chemin d'accès à Steam est invalide. Veuillez contacter un administrateur.",
                        "Chemin invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Décrypter le mot de passe
                string password = SteamAccountService.DecryptPassword(account.EncryptedPassword);

                // NOUVEAU : Construire les arguments avec ou sans mises à jour
                string baseArguments = $"-noreactlogin -login {account.Username} {password}";
                string finalArguments = allowSteamUpdates ? baseArguments : $"{baseArguments} -no-browser +@NoUpdates";

                // Lancer Steam avec les identifiants
                ProcessStartInfo accountStartInfo = new ProcessStartInfo
                {
                    FileName = pathConfig.Path,
                    Arguments = finalArguments
                };

                Process process = Process.Start(accountStartInfo);

                // Notifier le moniteur que Steam est en cours de démarrage
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
                    // Ouvrir l'URL dans le navigateur par défaut
                    Process.Start(pathConfig.Path);
                }
                else
                {
                    // Lancer l'application avec les arguments si spécifiés
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

        // Gestionnaire pour le toggle d'autorisation des comptes personnels
        private void ToggleAllowUserAccounts_Changed(object sender, RoutedEventArgs e)
        {
            // Lire l'état souhaité
            bool newState = toggleAllowUserAccounts.IsChecked ?? false;

            // Si on veut activer les comptes personnels, demander le mot de passe d'administrateur
            if (newState && !allowUserSteamAccounts)
            {
                // Créer et configurer la boîte de dialogue de mot de passe
                PasswordDialog passwordDialog = new PasswordDialog();
                passwordDialog.Owner = Window.GetWindow(this);
                passwordDialog.DialogMessage = "Veuillez entrer le mot de passe administrateur pour autoriser les comptes personnels";
                passwordDialog.ShowDialog();

                // Vérifier si l'authentification a réussi
                if (!passwordDialog.IsAuthenticated)
                {
                    // Annuler le changement si l'authentification a échoué
                    toggleAllowUserAccounts.IsChecked = false;
                    return;
                }

                // Afficher un message d'avertissement
                CustomMessageBox.Show(Window.GetWindow(this),
                    "Attention: Les comptes personnels sont maintenant autorisés sur Steam.\n\n" +
                    "Cette option sera désactivée à la fermeture de l'application.",
                    "Mode administrateur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Mettre à jour l'état
            allowUserSteamAccounts = toggleAllowUserAccounts.IsChecked ?? false;

            // Mettre à jour l'état du moniteur Steam
            steamMonitor.AllowUserAccounts = allowUserSteamAccounts;
        }

        // NOUVEAU : Gestionnaire pour le toggle d'autorisation des mises à jour Steam
        private void ToggleAllowSteamUpdates_Changed(object sender, RoutedEventArgs e)
        {
            // Lire l'état souhaité
            bool newState = toggleAllowSteamUpdates.IsChecked ?? false;

            // Si on veut activer les mises à jour, demander le mot de passe d'administrateur
            if (newState && !allowSteamUpdates)
            {
                // Créer et configurer la boîte de dialogue de mot de passe
                PasswordDialog passwordDialog = new PasswordDialog();
                passwordDialog.Owner = Window.GetWindow(this);
                passwordDialog.DialogMessage = "Veuillez entrer le mot de passe administrateur pour autoriser les mises à jour Steam";
                passwordDialog.ShowDialog();

                // Vérifier si l'authentification a réussi
                if (!passwordDialog.IsAuthenticated)
                {
                    // Annuler le changement si l'authentification a échoué
                    toggleAllowSteamUpdates.IsChecked = false;
                    return;
                }

                // Afficher un message d'avertissement
                CustomMessageBox.Show(Window.GetWindow(this),
                    "Attention: Les mises à jour Steam sont maintenant autorisées.\n\n" +
                    "Cette option sera désactivée à la fermeture de l'application.",
                    "Mode administrateur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Mettre à jour l'état
            allowSteamUpdates = toggleAllowSteamUpdates.IsChecked ?? false;
        }
    }
}