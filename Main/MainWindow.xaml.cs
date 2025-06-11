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
                UserInfoRetriever.UserInfo userInfo = UserInfoRetriever.GetUserInfo(username);

                // Mettre à jour l'affichage
                txtUserName.Text = $"Utilisateur : {userInfo.DisplayName}";
                txtUserAge.Text = $"Âge : {userInfo.Age} ans";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la mise à jour des informations utilisateur: {ex.Message}");
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
                bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                    .IsInRole(WindowsBuiltInRole.Administrator);

                // Mettre à jour l'affichage
                if (isAdmin)
                {
                    txtUserRole.Text = "Mode : Admin";
                    txtUserRole.Foreground = new SolidColorBrush(Color.FromRgb(0x26, 0x85, 0x31)); // Vert
                }
                else
                {
                    txtUserRole.Text = "Mode : Utilisateur";
                    txtUserRole.Foreground = new SolidColorBrush(Color.FromRgb(0xE3, 0x91, 0x1E)); // Orange
                }
            }
            catch (Exception ex)
            {
                // En cas d'erreur, afficher "Inconnu"
                txtUserRole.Text = "Mode : Inconnu";
                txtUserRole.Foreground = new SolidColorBrush(Color.FromRgb(0xBB, 0xBD, 0xBB)); // Gris
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la détection du rôle utilisateur: {ex.Message}");
            }
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
            // TODO: Peut ajouter une logique supplémentaire ici si nécessaire
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
                // Charger la configuration des dossiers protégés
                var folders = FolderPermissionService.LoadFolderPermissions();

                // Verrouiller tous les dossiers
                foreach (var folder in folders)
                {
                    folder.IsProtectionEnabled = true;
                    FolderPermissionService.ApplyProtection(folder);
                }

                // Sauvegarder les modifications
                FolderPermissionService.SaveFolderPermissions(folders);
            }
            catch (Exception ex)
            {
                // En cas d'erreur, ne pas bloquer la fermeture de l'application
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
                // Charger la configuration des dossiers protégés
                var folders = FolderPermissionService.LoadFolderPermissions();

                // Appliquer les protections pour les dossiers configurés
                FolderPermissionService.ApplyStartupProtections(folders);
            }
            catch (Exception ex)
            {
                // En cas d'erreur, ne pas afficher de message pour ne pas perturber le démarrage
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
        // Méthode publique pour pouvoir être appelée depuis CustomWindow
        public void ApplyPlatformVisibility()
        {
            try
            {
                // Charger la configuration de visibilité
                Dictionary<string, bool> platformVisibility = Panel.PlatformConfigService.LoadPlatformVisibility();

                // Appliquer la visibilité à chaque bouton
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
                // En cas d'erreur, ne pas afficher de message
                // Tous les boutons restent visibles par défaut
            }
        }

        private void ReglementButton_Click(object sender, RoutedEventArgs e)
        {
            // Vérifier si le changement de vue est autorisé
            if (!canChangePlatform)
            {
                return;
            }

            // Réinitialiser la plateforme actuelle
            currentPlatform = null;

            // Afficher la vue d'information
            platformContentControl.ShowInfoView();

            // Mettre en évidence le bouton Règlement
            HighlightSelectedButton(btnReglement);
        }

        // Navigation entre plateformes - modifiée pour vérifier si la navigation est autorisée
        private void PlatformButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string platformName)
            {
                // Vérifier si le changement de plateforme est autorisé
                if (!canChangePlatform)
                {
                    // Ne rien faire si un lancement est en cours
                    return;
                }

                // Mettre à jour la plateforme actuelle
                currentPlatform = platformName;

                // Configurer le contrôle de contenu pour cette plateforme
                platformContentControl.ConfigurePlatform(platformName);

                // Mettre en évidence le bouton sélectionné
                HighlightSelectedButton(button);
            }
        }

        // Mettre en évidence le bouton sélectionné
        private void HighlightSelectedButton(Button selectedButton)
        {
            // Créer les styles
            var normalStyle = new Style(typeof(Button), (Style)FindResource("PlatformButtonStyle"));
            var selectedStyle = new Style(typeof(Button), (Style)FindResource("PlatformButtonStyle"));
            selectedStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0x28, 0x80, 0x1F))));

            // Appliquer les styles
            foreach (var button in platformButtons.Values)
            {
                button.Opacity = 1.0;
                button.Style = normalStyle;
            }

            selectedButton.Style = selectedStyle;
        }

        // Méthode pour forcer l'affichage de l'icône dans la barre des tâches
        private void ForceTaskbarIcon()
        {
            try
            {
                // Accéder au handle de la fenêtre
                IntPtr hwnd = new WindowInteropHelper(this).Handle;

                // Modifier le style de la fenêtre pour forcer l'affichage dans la barre des tâches
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

                // Si la fenêtre est minimisée, la restaurer
                if (IsIconic(hwnd))
                {
                    ShowWindow(hwnd, SW_RESTORE);
                }

                // Essayer plusieurs techniques pour forcer la fenêtre au premier plan
                ShowWindow(hwnd, SW_SHOW);
                BringWindowToTop(hwnd);

                // Faire clignoter la fenêtre pour attirer l'attention (seulement au démarrage)
                if (!hasFocusBeenForced)
                {
                    FlashWindow(hwnd, true);
                }

                // Forcer la fenêtre au premier plan
                SetForegroundWindow(hwnd);

                // S'assurer que la fenêtre est visible
                this.Visibility = Visibility.Visible;
                this.WindowState = WindowState.Normal;
                this.ShowInTaskbar = true;

                // Technique agressive : Topmost temporaire
                if (!hasFocusBeenForced)
                {
                    this.Topmost = true;

                    // Permettre aux événements de se propager
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Background,
                        new Action(delegate { }));

                    // Désactiver Topmost après un court délai
                    Task.Delay(1000).ContinueWith(t =>
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            this.Topmost = false;
                            this.Activate();

                            // Marquer que le focus a été forcé
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