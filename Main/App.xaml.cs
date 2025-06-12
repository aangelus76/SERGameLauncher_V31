using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace SERGamesLauncher_V31
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Référence au mutex pour assurer l'unicité de l'instance
        private System.Threading.Mutex mutex;

        // Flag pour indiquer si on est en mode silencieux
        private bool isSilentMode = false;

        // Importer les fonctions Win32 nécessaires
        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "SERGamesLauncher_V31_Single_Instance_Mutex";
            bool createdNew;

            // Créer un mutex nommé pour détecter une instance déjà en cours
            mutex = new System.Threading.Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                // Une instance est déjà en cours d'exécution
                // Trouver la fenêtre existante et la mettre en premier plan
                Process currentProcess = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);

                foreach (Process process in processes)
                {
                    if (process.Id != currentProcess.Id && process.MainWindowHandle != IntPtr.Zero)
                    {
                        // Restaurer la fenêtre si elle est minimisée
                        ShowWindow(process.MainWindowHandle, ShowWindow_Restore);

                        // Mettre la fenêtre en premier plan
                        SetForegroundWindow(process.MainWindowHandle);

                        // Quitter cette instance
                        Shutdown();
                        return;
                    }
                }

                // Si on n'a pas trouvé l'autre fenêtre, quitter quand même
                Shutdown();
                return;
            }

            // S'assurer que la configuration par défaut existe
            StartupConfigService.EnsureDefaultConfigExists();

            // Vérifier si on est en mode silencieux
            CheckSilentMode();

            // Détecter si l'application est lancée au démarrage de session
            bool isStartupLaunch = IsLaunchedAtStartup();

            // Si aucune instance n'est en cours, lancer normalement
            base.OnStartup(e);

            // Vérifier et créer le raccourci sur le bureau si nécessaire
            ShortcutUtility.EnsureDesktopShortcutExists();

            // Appliquer le délai si c'est un lancement au démarrage
            if (isStartupLaunch)
            {
                // Attendre avant d'afficher la fenêtre
                DelayedWindowShow();
            }
            else
            {
                // Affichage immédiat si lancé manuellement
                ShowMainWindowImmediately();
            }
        }

        /// <summary>
        /// Vérifie si on est dans une plage horaire de mode silencieux
        /// </summary>
        private void CheckSilentMode()
        {
            try
            {
                isSilentMode = SilentModeScheduleService.IsCurrentlyInSilentMode();
                System.Diagnostics.Debug.WriteLine($"Mode silencieux: {(isSilentMode ? "Activé" : "Désactivé")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la vérification du mode silencieux: {ex.Message}");
                isSilentMode = false; // Par défaut, mode normal
            }
        }

        /// <summary>
        /// Détermine si l'application est lancée au démarrage de session
        /// </summary>
        private bool IsLaunchedAtStartup()
        {
            try
            {
                // Méthode 1: Vérifier le temps écoulé depuis le démarrage du système
                TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);

                // Si le système a démarré il y a moins de 2 minutes, considérer comme un démarrage
                if (uptime.TotalMinutes < 2)
                {
                    return true;
                }

                // Méthode 2: Vérifier les arguments de ligne de commande
                string[] args = Environment.GetCommandLineArgs();
                if (args.Contains("--startup") || args.Contains("/startup"))
                {
                    return true;
                }

                // Méthode 3: Vérifier si d'autres processus typiques du démarrage sont en cours
                var explorerProcesses = Process.GetProcessesByName("explorer");
                if (explorerProcesses.Length > 0)
                {
                    var explorerStartTime = explorerProcesses[0].StartTime;
                    var timeSinceExplorerStart = DateTime.Now - explorerStartTime;

                    // Si Explorer a démarré il y a moins de 1 minute, c'est probablement un démarrage
                    if (timeSinceExplorerStart.TotalMinutes < 1)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                // En cas d'erreur, supposer que ce n'est pas un démarrage
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la détection du démarrage: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Affiche la fenêtre principale après un délai configurable
        /// </summary>
        private async void DelayedWindowShow()
        {
            try
            {
                // Charger la configuration de démarrage
                var config = StartupConfigService.LoadStartupConfig();

                // Cacher la fenêtre initialement si le délai est activé
                if (config.EnableStartupDelay && MainWindow != null)
                {
                    MainWindow.Visibility = Visibility.Hidden;
                    MainWindow.WindowState = WindowState.Minimized;
                    MainWindow.ShowInTaskbar = false; // Masquer de la barre des tâches pendant le délai
                }

                // Attendre le délai configuré
                if (config.EnableStartupDelay)
                {
                    await Task.Delay(config.StartupDelaySeconds * 1000);
                }

                // Vérifier si l'application n'a pas été fermée pendant l'attente
                if (MainWindow != null && !this.ShutdownMode.HasFlag(ShutdownMode.OnExplicitShutdown))
                {
                    // Afficher la fenêtre avec ou sans focus selon le mode silencieux
                    this.Dispatcher.Invoke(() =>
                    {
                        ShowMainWindowWithFocus();
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'affichage différé: {ex.Message}");

                // En cas d'erreur, afficher quand même la fenêtre
                ShowMainWindowImmediately();
            }
        }

        /// <summary>
        /// Affiche la fenêtre principale immédiatement
        /// </summary>
        private void ShowMainWindowImmediately()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                ShowMainWindowWithFocus();
            }));
        }

        /// <summary>
        /// Méthode centralisée pour afficher la fenêtre avec ou sans focus selon le mode silencieux
        /// </summary>
        private void ShowMainWindowWithFocus()
        {
            MainWindow mainWindow = this.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                // Restaurer et afficher la fenêtre
                mainWindow.Visibility = Visibility.Visible;
                mainWindow.ShowInTaskbar = true;

                if (isSilentMode)
                {
                    // Mode silencieux: démarrer minimisé, pas de focus forcé
                    mainWindow.WindowState = WindowState.Minimized;
                    System.Diagnostics.Debug.WriteLine("Application lancée en mode silencieux (minimisée)");
                }
                else
                {
                    // Mode normal: fenêtre normale avec focus
                    mainWindow.WindowState = WindowState.Normal;

                    // Activer la fenêtre (rendre active)
                    mainWindow.Activate();

                    // Forcer le focus sur la fenêtre
                    mainWindow.Focus();

                    // Démarrer le timer de focus agressif
                    mainWindow.StartFocusTimer();

                    // Utiliser l'API Windows pour forcer la fenêtre au premier plan
                    IntPtr handle = new WindowInteropHelper(mainWindow).Handle;
                    if (handle != IntPtr.Zero)
                    {
                        SetForegroundWindow(handle);
                    }

                    System.Diagnostics.Debug.WriteLine("Application lancée en mode normal");
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Libérer le mutex lors de la fermeture
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
            }

            base.OnExit(e);
        }

        // Constantes pour ShowWindow
        private const int ShowWindow_Restore = 9;
        private const int ShowWindow_Show = 5;

        // Méthodes helper pour les boutons de la barre de titre
        public static void WindowMinimizeClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var window = Window.GetWindow(button);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        public static void WindowCloseClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var window = Window.GetWindow(button);
            if (window != null)
            {
                window.Close();
            }
        }
    }
}