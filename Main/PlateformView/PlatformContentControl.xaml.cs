using SERGamesLauncher_V31.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Linq;

namespace SERGamesLauncher_V31
{
    public partial class PlatformContentControl : UserControl
    {
        private string currentPlatform = null;
        private SteamActivityMonitor steamMonitor;
        private bool allowUserSteamAccounts = false;
        private bool allowSteamUpdates = false;

        // Champs pour la protection contre les clics multiples
        private DispatcherTimer launchCooldownTimer;
        private DispatcherTimer countdownTimer;
        private string originalButtonText;
        private bool isLaunchButtonCoolingDown = false;
        private int remainingSeconds = 30;

        // Champ pour Roblox
        private bool isRobloxUpdating = false;

        // Propriété pour limiter le changement de vue
        public bool AllowViewChange { get; private set; } = true;

        // Propriété pour gérer l'état du cooldown
        private bool IsCooldownActive { get; set; } = false;

        // Événement lorsque l'état de navigation change
        public event EventHandler<bool> NavigationStateChanged;

        public event EventHandler<string> LaunchRequested;

        private string cooldownButtonText = "Exécution";

        // NOUVEAU : Mode Admin (skip password, cooldown 10s)
        private bool isAdminMode = false;

        // NOUVEAU : Durée du cooldown selon le mode
        private int CooldownDuration => isAdminMode ? 10 : 30;

        // Constructeur qui accepte un SteamActivityMonitor externe
        public PlatformContentControl(SteamActivityMonitor monitor)
        {
            InitializeComponent();

            // Utiliser le moniteur Steam externe
            steamMonitor = monitor;
            steamMonitor.UserChanged += SteamMonitor_UserChanged;

            // Initialiser le timer de cooldown (sera mis à jour selon le mode)
            launchCooldownTimer = new DispatcherTimer();
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

        /// <summary>
        /// NOUVEAU : Définit le mode Admin
        /// </summary>
        public void SetAdminMode(bool adminMode)
        {
            isAdminMode = adminMode;
            // Mettre à jour l'intervalle du timer selon le mode
            launchCooldownTimer.Interval = TimeSpan.FromSeconds(CooldownDuration);
        }

        // Gestionnaire pour le changement d'utilisateur Steam
        private void SteamMonitor_UserChanged(string username)
        {
            Dispatcher.Invoke(() =>
            {
                txtCurrentSteamAccount.Text = string.IsNullOrEmpty(username) ?
                    "Aucun compte détecté" : $"Compte actuel : {username}";
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
            bool consentGiven = chkConsent.IsChecked == true;
            bool platformSelected = !string.IsNullOrEmpty(currentPlatform);
            bool notInCooldown = !isLaunchButtonCoolingDown && !isRobloxUpdating;

            btnLaunch.IsEnabled = consentGiven && platformSelected && notInCooldown;
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
                    ShowSteamControls();
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
                // NOUVEAU : Plateforme Custom
                case "Custom":
                    ConfigureCustomPlatform();
                    break;
                default:
                    ShowInfoView();
                    break;
            }

            // Mettre à jour l'état du bouton
            UpdateButtonState();
        }

        /// <summary>
        /// NOUVEAU : Configure la plateforme personnalisée
        /// </summary>
        private void ConfigureCustomPlatform()
        {
            var customConfig = CustomButtonService.LoadConfig();
            txtPlatformTitle.Text = customConfig.PageTitle;
            txtAccountMessage.Text = customConfig.PageSubtitle;
            txtInstructions.Text = customConfig.PageInstructions;

            // Charger l'image de la page
            var customImage = CustomButtonService.LoadPageImage(customConfig);
            if (customImage != null)
            {
                imgPlatformLogo.Source = customImage;
            }

            HideSteamControls();
        }

        // Afficher les contrôles Steam
        private void ShowSteamControls()
        {
            txtCurrentSteamAccount.Visibility = Visibility.Visible;
            steamControlsPanel.Visibility = Visibility.Visible;

            // Synchroniser avec l'état actuel du moniteur (pour les comptes perso)
            toggleAllowUserAccounts.IsChecked = steamMonitor.AllowUserAccounts;
            allowUserSteamAccounts = steamMonitor.AllowUserAccounts;

            // MODIFIÉ : Conserver l'état des mises à jour au lieu de le réinitialiser
            toggleAllowSteamUpdates.IsChecked = allowSteamUpdates;

            _ = CheckSteamUpdatesAsync();
        }

        // Masquer les contrôles Steam
        private void HideSteamControls()
        {
            txtCurrentSteamAccount.Visibility = Visibility.Collapsed;
            steamControlsPanel.Visibility = Visibility.Collapsed;
            steamUpdateNotificationBorder.Visibility = Visibility.Collapsed;
        }

        // Vérification des mises à jour Steam
        private async Task CheckSteamUpdatesAsync()
        {
            try
            {
                var updates = await SteamUpdateChecker.GetUpdatesAsync();

                if (updates.Any())
                {
                    steamUpdateNotification.Text = $"🔄 {updates.Count} mise{(updates.Count > 1 ? "s" : "")} à jour disponible{(updates.Count > 1 ? "s" : "")} - Cliquez ici";
                    steamUpdateNotificationBorder.Visibility = Visibility.Visible;
                }
                else
                {
                    steamUpdateNotificationBorder.Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
                steamUpdateNotificationBorder.Visibility = Visibility.Collapsed;
            }
        }

        // Click sur notification de mise à jour Steam
        private async void SteamUpdateNotification_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string originalText = steamUpdateNotification.Text;
                steamUpdateNotification.Text = "🔄 Chargement...";

                var updates = await SteamUpdateChecker.GetUpdatesAsync();

                if (updates != null && updates.Any())
                {
                    var message = "Jeux Steam avec mises à jour disponibles :\n\n";
                    foreach (var update in updates.Take(15))
                    {
                        message += $"• {update.Name}\n";
                    }

                    if (updates.Count > 15)
                    {
                        message += $"\n... et {updates.Count - 15} autre{(updates.Count - 15 > 1 ? "s" : "")} jeu{(updates.Count - 15 > 1 ? "x" : "")}\n";
                    }

                    message += $"\nLancez Steam pour installer les mises à jour.";

                    CustomMessageBox.Show(Window.GetWindow(this), message,
                        "Mises à jour Steam", MessageBoxButton.OK, MessageBoxImage.Information,
                        500, 400);
                }
                else
                {
                    CustomMessageBox.Show(Window.GetWindow(this),
                        "Aucune mise à jour disponible pour le moment.",
                        "Mises à jour Steam", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                steamUpdateNotification.Text = originalText;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    $"Erreur lors de la récupération des mises à jour :\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning,
                    450, 250);

                steamUpdateNotification.Text = "🔄 Erreur - Cliquez pour réessayer";
            }
        }

        // Gestionnaire pour le bouton de lancement
        private async void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPlatform == null || isLaunchButtonCoolingDown) return;

            originalButtonText = btnLaunch.Content.ToString();

            // Logique spéciale pour Roblox
            if (currentPlatform.Equals("Roblox", StringComparison.OrdinalIgnoreCase))
            {
                await HandleRobloxLaunchAsync();
                return;
            }

            // NOUVEAU : Logique pour Custom
            if (currentPlatform.Equals("Custom", StringComparison.OrdinalIgnoreCase))
            {
                LaunchCustomPlatform();
                StartNormalCooldown();
                return;
            }

            // Logique normale pour les autres plateformes
            StartNormalCooldown();

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

        /// <summary>
        /// NOUVEAU : Lance la plateforme personnalisée avec support des placeholders et tokens
        /// </summary>
        private void LaunchCustomPlatform()
        {
            try
            {
                var config = CustomButtonService.LoadConfig();

                if (string.IsNullOrEmpty(config.TargetPath))
                {
                    CustomMessageBox.Show(Window.GetWindow(this),
                        "Aucune cible configurée pour ce bouton.",
                        "Configuration manquante", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Récupérer les infos utilisateur actuelles
                var userInfo = GetCurrentUserInfo();

                // Remplacer les placeholders dans l'URL/chemin
                string finalPath = CustomButtonService.ReplacePlaceholders(config.TargetPath, userInfo, config);

                if (config.TargetType == "url")
                {
                    // Ouvrir l'URL
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = finalPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    // Vérifier que l'exe existe
                    string exePath = config.TargetPath;
                    if (!exePath.Contains("{") && !File.Exists(exePath))
                    {
                        CustomMessageBox.Show(Window.GetWindow(this),
                            $"Le fichier n'existe pas :\n{exePath}",
                            "Fichier introuvable", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = finalPath,
                        UseShellExecute = true
                    };

                    if (!string.IsNullOrWhiteSpace(config.LaunchArguments))
                    {
                        // Remplacer les placeholders dans les arguments aussi
                        startInfo.Arguments = CustomButtonService.ReplacePlaceholders(config.LaunchArguments, userInfo, config);
                    }

                    Process.Start(startInfo);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    $"Erreur lors du lancement :\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// NOUVEAU : Récupère les infos de l'utilisateur actuel pour les placeholders
        /// </summary>
        private UserPlaceholderInfo GetCurrentUserInfo()
        {
            try
            {
                // Récupérer depuis la MainWindow
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow?.CurrentUserInfo != null)
                {
                    return UserPlaceholderInfo.CreateCurrent(
                        mainWindow.CurrentUserInfo.DisplayName,
                        mainWindow.CurrentUserInfo.Age
                    );
                }
            }
            catch { }

            // Fallback
            return UserPlaceholderInfo.CreateCurrent("Utilisateur", 0);
        }

        // Gestion du lancement Roblox avec mise à jour
        private async Task HandleRobloxLaunchAsync()
        {
            try
            {
                isRobloxUpdating = true;
                UpdateButtonState();
                btnLaunch.Content = "En MAJ";
                chkConsent.IsEnabled = false;
                AllowViewChange = false;
                NavigationStateChanged?.Invoke(this, false);

                RobloxUpdateService.ProgressChanged += OnRobloxUpdateProgress;

                var result = await RobloxUpdateService.CheckAndUpdateAsync();

                RobloxUpdateService.ProgressChanged -= OnRobloxUpdateProgress;

                if (result.Success)
                {
                    if (result.UpdatePerformed)
                    {
                        btnLaunch.Content = "Terminé";
                        await Task.Delay(1000);
                        LaunchRoblox(result.NewPath);
                    }
                    else
                    {
                        LaunchRoblox(null);
                    }
                }
                else
                {
                    CustomMessageBox.Show(Window.GetWindow(this), result.Message,
                        "Mise à jour Roblox", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LaunchRoblox(null);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    $"Erreur lors de la mise à jour: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                LaunchRoblox(null);
            }
            finally
            {
                isRobloxUpdating = false;
                RobloxUpdateService.ProgressChanged -= OnRobloxUpdateProgress;
                txtRobloxProgress.Visibility = Visibility.Collapsed;
                txtRobloxProgress.Text = "";
                StartNormalCooldown();
            }
        }

        // Gestionnaire de progression Roblox
        private void OnRobloxUpdateProgress(string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(message))
                {
                    btnLaunch.Content = "En MAJ";
                    btnLaunch.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B35"));
                    btnLaunch.Cursor = Cursors.No;
                    txtRobloxProgress.Text = message;
                    txtRobloxProgress.Visibility = Visibility.Visible;
                }
            });
        }

        // Lancer Roblox
        private void LaunchRoblox(string customPath)
        {
            if (string.IsNullOrEmpty(customPath))
            {
                List<PathConfig> pathConfigs = PathConfigService.LoadPathConfigs();
                PathConfig pathConfig = PathConfigService.GetPathForPlatform(pathConfigs, currentPlatform);
                if (pathConfig != null)
                    customPath = pathConfig.Path;
            }

            if (string.IsNullOrEmpty(customPath) || !File.Exists(customPath))
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    "Impossible de trouver l'exécutable Roblox. Veuillez contacter un administrateur.",
                    "Erreur Roblox", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = customPath,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    $"Erreur lors du lancement de Roblox: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Démarrer le cooldown normal
        private void StartNormalCooldown()
        {
            // MODIFIÉ : Utiliser CooldownDuration selon le mode admin
            remainingSeconds = CooldownDuration;
            launchCooldownTimer.Interval = TimeSpan.FromSeconds(CooldownDuration);
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
        }

        // Timer de décompte
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            btnLaunch.Content = $"{cooldownButtonText} ({remainingSeconds}s)";
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
            btnLaunch.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#268531"));
            btnLaunch.Cursor = Cursors.Hand;

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
                        "Fichier manquant", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string password = SteamAccountService.DecryptPassword(account.EncryptedPassword);

                string baseArguments = $"-noreactlogin -login {account.Username} {password}";
                string finalArguments = allowSteamUpdates ? baseArguments : $"{baseArguments} -no-browser +@NoUpdates";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = pathConfig.Path,
                    Arguments = finalArguments
                };

                Process.Start(startInfo);

                steamMonitor.NotifyStarting();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    $"Impossible de lancer Steam: {ex.Message}",
                    "Erreur de lancement", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Lancer d'autres plateformes
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

        // ============================================================================
        // TOGGLES STEAM - MODIFIÉS POUR SUPPORTER LE MODE ADMIN
        // ============================================================================

        // Gestionnaire pour le toggle d'autorisation des comptes personnels
        private void ToggleAllowUserAccounts_Changed(object sender, RoutedEventArgs e)
        {
            bool newState = toggleAllowUserAccounts.IsChecked ?? false;

            // MODIFIÉ : Skip password si mode admin
            if (newState && !allowUserSteamAccounts && !isAdminMode)
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

            FolderPermissionsControl.SetProtectionForAllFolders(!newState);
            NotifyAdminPanelOfChange();
        }

        // Gestionnaire pour le toggle des mises à jour Steam
        private void ToggleAllowSteamUpdates_Changed(object sender, RoutedEventArgs e)
        {
            bool newState = toggleAllowSteamUpdates.IsChecked ?? false;

            // MODIFIÉ : Skip password si mode admin
            if (newState && !allowSteamUpdates && !isAdminMode)
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

            try
            {
                var folders = FolderPermissionService.LoadFolderPermissions();
                if (folders == null || folders.Count == 0) return;

                bool isChecked = !newState;

                if (isChecked)
                {
                    foreach (var folder in folders)
                    {
                        folder.IsProtectionEnabled = true;
                        FolderPermissionService.ApplyProtection(folder);
                    }
                }
                else
                {
                    foreach (var folder in folders)
                    {
                        folder.IsProtectionEnabled = false;
                        FolderPermissionService.RemoveProtection(folder);
                    }
                }

                FolderPermissionService.SaveFolderPermissions(folders);
                NotifyAdminPanelOfChange();
            }
            catch (Exception)
            {
                // Gestion d'erreur silencieuse
            }
        }

        // Notifier le panel admin du changement
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
            catch (Exception)
            {
                // Gestion d'erreur silencieuse
            }
        }

        // Nettoyage
        public void Dispose()
        {
            launchCooldownTimer?.Stop();
            countdownTimer?.Stop();
            steamMonitor.UserChanged -= SteamMonitor_UserChanged;
        }
    }
}
