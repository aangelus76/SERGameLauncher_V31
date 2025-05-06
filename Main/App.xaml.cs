using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows.Interop;
using System.Windows.Threading;

namespace SERGamesLauncher_V31
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Référence au mutex pour assurer l'unicité de l'instance
        private System.Threading.Mutex mutex;

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

            // Si aucune instance n'est en cours, lancer normalement
            base.OnStartup(e);
            // Placer la fenêtre au premier plan
            this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                MainWindow mainWindow = this.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    // S'assurer que la fenêtre est visible
                    mainWindow.WindowState = WindowState.Normal;

                    // Activer la fenêtre (rendre active)
                    mainWindow.Activate();

                    // Forcer le focus sur la fenêtre
                    mainWindow.Focus();

                    // Utiliser l'API Windows pour forcer la fenêtre au premier plan
                    SetForegroundWindow(new WindowInteropHelper(mainWindow).Handle);
                }
            }));
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