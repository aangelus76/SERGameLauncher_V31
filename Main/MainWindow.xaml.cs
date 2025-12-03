using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Security.Principal;
using SERGamesLauncher_V31.Services;

namespace SERGamesLauncher_V31
{
    public partial class MainWindow : CustomWindow
    {
        // Collection de boutons de plateforme pour un accès facile
        private Dictionary<string, Button> platformButtons;

        // Plateforme actuellement sélectionnée
        private string currentPlatform = null;

        // Contrôle de contenu de plateforme
        private PlatformContentControl platformContentControl;

        // Moniteur Steam
        private SteamActivityMonitor steamMonitor;

        private ProcessMonitor processMonitor;

        // Variable pour contrôler si le changement de plateforme est autorisé
        private bool canChangePlatform = true;

        // Variable pour le système de focus
        private bool hasFocusBeenForced = false;

        // NOUVEAU : Propriété pour savoir si on est en mode Admin
        public bool IsAdminMode { get; private set; } = false;

        // NOUVEAU : Infos utilisateur pour les placeholders
        public UserInfoRetriever.UserInfo CurrentUserInfo { get; private set; }

        // Importation des API Windows pour le forçage du focus
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_APPWINDOW = 0x00040000;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;

        public MainWindow()
        {
            FolderPermissionsControl.ResetFirstLoadFlag();

            InitializeComponent();

            // Initialiser le moniteur Steam
            steamMonitor = new SteamActivityMonitor();

            // Démarrer le moniteur Steam dès le lancement de l'application
            steamMonitor.Start();

            // Vérifier immédiatement si Steam est déjà en cours d'exécution
            steamMonitor.CheckImmediately();

            // Initialiser le contrôle de contenu de plateforme
            platformContentControl = new PlatformContentControl(steamMonitor);
            contentContainer.Child = platformContentControl;

            // S'abonner à l'événement de changement d'état de navigation
            platformContentControl.NavigationStateChanged += PlatformContentControl_NavigationStateChanged;

            // Initialiser le dictionnaire de boutons après l'initialisation des composants
            InitializePlatformButtons();

            // Appliquer les protections de dossiers
            ApplyFolderProtections();

            // Appliquer la configuration de visibilité des plateformes
            ApplyPlatformVisibility();

            // Initialiser et démarrer le moniteur de processus
            processMonitor = new ProcessMonitor();
            processMonitor.Start();
            processMonitor.UserInfoUpdated += ProcessMonitor_UserInfoUpdated;

            // Récupérer les informations utilisateur
            UpdateUserInfo();

            // Mettre à jour l'indicateur de rôle utilisateur
            UpdateUserRoleIndicator();

            // NOUVEAU : Charger le bouton personnalisé
            RefreshCustomButton();

            // S'abonner aux événements de lancement
            platformContentControl.LaunchRequested += PlatformContentControl_LaunchRequested;

            // S'abonner à l'événement de fermeture
            this.Closing += MainWindow_Closing;

            // Configuration de l'affichage et du focus
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(btnReglement);

            // S'assurer que la fenêtre est affichée dans la barre des tâches
            ForceTaskbarIcon();

            // Note: Le timer de focus sera démarré par App.xaml.cs via StartFocusTimer()
        }

        /// <summary>
        /// Démarre le timer de focus (appelé par App.xaml.cs après le délai ou immédiatement)
        /// </summary>
        public void StartFocusTimer()
        {
            // Charger la configuration pour les paramètres de focus
            var config = StartupConfigService.LoadStartupConfig();

            // Timer qui essaie de forcer le focus
            DispatcherTimer focusTimer = new DispatcherTimer();
            focusTimer.Interval = TimeSpan.FromMilliseconds(config.FocusRetryIntervalMs);
            int attemptCount = 0;

            focusTimer.Tick += (s, e) =>
            {
                // Incrémenter le compteur de tentatives
                attemptCount++;

                // Forcer le focus avec les API Windows
                ForceWindowFocusAndVisibility();

                // Vérifier si la fenêtre a le focus ou si on a atteint le nombre maximum de tentatives
                if (this.IsActive || attemptCount >= config.MaxFocusAttempts || hasFocusBeenForced)
                {
                    focusTimer.Stop();
                }
            };

            // Attendre un court instant avant de commencer à forcer le focus
            Task.Delay(500).ContinueWith(t =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    focusTimer.Start();
                });
            });
        }

        // Méthode pour gérer les changements d'état de navigation
        private void PlatformContentControl_NavigationStateChanged(object sender, bool isNavigationAllowed)
        {
            // Mettre à jour l'état local de navigation
            canChangePlatform = isNavigationAllowed;

            // Mettre à jour visuellement l'état des boutons de navigation
            foreach (var button in platformButtons.Values)
            {
                button.IsEnabled = isNavigationAllowed;
            }
            
            // NOUVEAU : Mettre à jour aussi le bouton custom s'il est visible
            if (btnCustom.Visibility == Visibility.Visible)
            {
                btnCustom.IsEnabled = isNavigationAllowed;
            }
        }

        // Gestionnaire d'événement pour les mises à jour des informations utilisateur
        private void ProcessMonitor_UserInfoUpdated(string displayName, int age)
        {
            // Mettre à jour l'affichage sur le thread UI
            Dispatcher.Invoke(() =>
            {
                txtUserName.Text = $"Utilisateur : {displayName}";
                txtUserAge.Text = $"Âge : {age} ans";
            });
        }

        // Méthode pour mettre à jour les informations utilisateur
        private void UpdateUserInfo()
        {
            try
            {
                // Récupérer le nom d'utilisateur actuel
                string username = Environment.UserName;

                // Utiliser la classe UserInfoRetriever pour obtenir les informations
                CurrentUserInfo = UserInfoRetriever.GetUserInfo(username);

                // Mettre à jour l'affichage
                txtUserName.Text = $"Utilisateur : {CurrentUserInfo.DisplayName}";
                txtUserAge.Text = $"Âge : {CurrentUserInfo.Age} ans";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la mise à jour des informations utilisateur: {ex.Message}");
                // Créer un UserInfo par défaut en cas d'erreur
                CurrentUserInfo = new UserInfoRetriever.UserInfo();
            }
        }

        /// <summary>
        /// Met à jour l'indicateur de rôle utilisateur (Admin/Utilisateur)
        /// </summary>
        private void UpdateUserRoleIndicator()
        {
            try
            {
                // Vérifier si l'utilisateur actuel a des privilèges administrateur
                IsAdminMode = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                    .IsInRole(WindowsBuiltInRole.Administrator);

                // Mettre à jour l'affichage
                if (IsAdminMode)
                {
                    txtUserRole.Text = "Mode : Admin";
                    txtUserRole.Foreground = new SolidColorBrush(Color.FromRgb(0x26, 0x85, 0x31)); // Vert
                }
                else
                {
                    txtUserRole.Text = "Mode : Utilisateur";
                    txtUserRole.Foreground = new SolidColorBrush(Color.FromRgb(0xE3, 0x91, 0x1E)); // Orange
                }

                // NOUVEAU : Informer le PlatformContentControl du mode admin
                if (platformContentControl != null)
                {
                    platformContentControl.SetAdminMode(IsAdminMode);
                }
            }
            catch (Exception ex)
            {
                // En cas d'erreur, afficher "Inconnu"
                txtUserRole.Text = "Mode : Inconnu";
                txtUserRole.Foreground = new SolidColorBrush(Color.FromRgb(0xBB, 0xBD, 0xBB)); // Gris
                IsAdminMode = false;
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la détection du rôle utilisateur: {ex.Message}");
            }
        }

        /// <summary>
        /// NOUVEAU : Rafraîchit l'affichage du bouton personnalisé
        /// </summary>
        public void RefreshCustomButton()
        {
            try
            {
                var config = CustomButtonService.LoadConfig();

                if (config.IsEnabled)
                {
                    // Afficher le bouton
                    btnCustom.Visibility = Visibility.Visible;

                    // Mettre à jour le texte
                    txtCustomButton.Text = config.ButtonLabel;

                    // Charger l'image
                    var image = CustomButtonService.LoadButtonImage(config);
                    if (image != null)
                    {
                        imgCustomButton.Source = image;
                    }
                    else
                    {
                        // Image par défaut si pas d'image configurée
                        try
                        {
                            imgCustomButton.Source = (System.Windows.Media.Imaging.BitmapImage)Application.Current.Resources["IconeLauncher"];
                        }
                        catch
                        {
                            // Pas d'image par défaut disponible
                        }
                    }
                }
                else
                {
                    btnCustom.Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
                btnCustom.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// NOUVEAU : Gestionnaire de clic sur le bouton personnalisé
        /// </summary>
        private void CustomButton_Click(object sender, RoutedEventArgs e)
        {
            if (!canChangePlatform) return;

            currentPlatform = "Custom";
            platformContentControl.ConfigurePlatform("Custom");
            HighlightSelectedButton(btnCustom);
        }

        /// <summary>
        /// Arrête temporairement le moniteur de processus (appelé depuis le panel admin)
        /// </summary>
        public void StopProcessMonitor()
        {
            if (processMonitor != null)
            {
                processMonitor.Stop();
            }
        }

        /// <summary>
        /// Redémarre le moniteur de processus (appelé depuis le panel admin)
        /// </summary>
        public void StartProcessMonitor()
        {
            if (processMonitor != null)
            {
                processMonitor.Start();
            }
        }

        private void PlatformContentControl_LaunchRequested(object sender, string platform)
        {
            System.Diagnostics.Debug.WriteLine($"Lancement de la plateforme: {platform}");
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Arrêter le moniteur Steam
            steamMonitor.Stop();

            // Si on ne permet pas les comptes personnels, s'assurer que Steam est fermé
            if (!steamMonitor.AllowUserAccounts)
            {
                steamMonitor.KillAllSteamProcesses();
            }

            if (processMonitor != null)
            {
                processMonitor.Stop();
            }

            // Verrouiller tous les dossiers à la fermeture de l'application
            try
            {
                var folders = FolderPermissionService.LoadFolderPermissions();

                foreach (var folder in folders)
                {
                    folder.IsProtectionEnabled = true;
                    FolderPermissionService.ApplyProtection(folder);
                }

                FolderPermissionService.SaveFolderPermissions(folders);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du verrouillage des dossiers à la fermeture: {ex.Message}");
            }
        }

        /// <summary>
        /// Applique les protections de dossiers configurées pour le démarrage
        /// </summary>
        private void ApplyFolderProtections()
        {
            try
            {
                var folders = FolderPermissionService.LoadFolderPermissions();
                FolderPermissionService.ApplyStartupProtections(folders);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'application des protections de dossiers: {ex.Message}");
            }
        }

        // Initialiser le dictionnaire de boutons
        private void InitializePlatformButtons()
        {
            platformButtons = new Dictionary<string, Button>
            {
                { "Reglement", btnReglement },
                { "Steam", btnSteam },
                { "Epic", btnEpic },
                { "Crazy", btnCrazy },
                { "Roblox", btnRoblox },
                { "BGA", btnBGA },
                { "Xbox", btnXbox }
            };
        }

        // Appliquer la configuration de visibilité des plateformes
        public void ApplyPlatformVisibility()
        {
            try
            {
                Dictionary<string, bool> platformVisibility = Panel.PlatformConfigService.LoadPlatformVisibility();

                foreach (var platform in platformVisibility)
                {
                    if (platformButtons.TryGetValue(platform.Key, out Button button))
                    {
                        button.Visibility = platform.Value ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            catch (Exception)
            {
                // En cas d'erreur, tous les boutons restent visibles par défaut
            }
        }

        private void ReglementButton_Click(object sender, RoutedEventArgs e)
        {
            if (!canChangePlatform)
            {
                return;
            }

            currentPlatform = null;
            platformContentControl.ShowInfoView();
            HighlightSelectedButton(btnReglement);
        }

        private void PlatformButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string platformName)
            {
                if (!canChangePlatform)
                {
                    return;
                }

                currentPlatform = platformName;
                platformContentControl.ConfigurePlatform(platformName);
                HighlightSelectedButton(button);
            }
        }

        // Mettre en évidence le bouton sélectionné
        private void HighlightSelectedButton(Button selectedButton)
        {
            var normalStyle = new Style(typeof(Button), (Style)FindResource("PlatformButtonStyle"));
            var selectedStyle = new Style(typeof(Button), (Style)FindResource("PlatformButtonStyle"));
            selectedStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0x28, 0x80, 0x1F))));

            foreach (var button in platformButtons.Values)
            {
                button.Opacity = 1.0;
                button.Style = normalStyle;
            }

            // NOUVEAU : Aussi réinitialiser le bouton custom
            if (btnCustom.Visibility == Visibility.Visible)
            {
                btnCustom.Style = normalStyle;
            }

            selectedButton.Style = selectedStyle;
        }

        // Méthode pour forcer l'affichage de l'icône dans la barre des tâches
        private void ForceTaskbarIcon()
        {
            try
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_APPWINDOW);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du forçage de l'icône dans la barre des tâches: {ex.Message}");
            }
        }

        // Méthode améliorée pour forcer le focus et la visibilité de la fenêtre
        private void ForceWindowFocusAndVisibility()
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;

                if (IsIconic(hwnd))
                {
                    ShowWindow(hwnd, SW_RESTORE);
                }

                ShowWindow(hwnd, SW_SHOW);
                BringWindowToTop(hwnd);

                if (!hasFocusBeenForced)
                {
                    FlashWindow(hwnd, true);
                }

                SetForegroundWindow(hwnd);

                this.Visibility = Visibility.Visible;
                this.WindowState = WindowState.Normal;
                this.ShowInTaskbar = true;

                if (!hasFocusBeenForced)
                {
                    this.Topmost = true;

                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Background,
                        new Action(delegate { }));

                    Task.Delay(1000).ContinueWith(t =>
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            this.Topmost = false;
                            this.Activate();
                            hasFocusBeenForced = true;
                        });
                    });
                }
                else
                {
                    this.Activate();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du forçage du focus: {ex.Message}");
            }
        }
    }
}
