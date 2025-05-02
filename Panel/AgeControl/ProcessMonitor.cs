using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace SERGamesLauncher_V31
{
    public class ProcessMonitor
    {
        private readonly DispatcherTimer _checkTimer;
        private List<ProcessRestriction> _restrictions;
        private int _currentUserAge = 3; // Âge par défaut
        private string _currentUserName = "John Doe"; // Nom par défaut

        // Déléguée et événement pour les notifications
        public delegate void ProcessBlockedEventHandler(string processName, int minimumAge, int userAge);
        public event ProcessBlockedEventHandler ProcessBlocked;

        // Déléguée et événement pour les mises à jour d'utilisateur
        public delegate void UserInfoUpdatedEventHandler(string displayName, int age);
        public event UserInfoUpdatedEventHandler UserInfoUpdated;

        public ProcessMonitor()
        {
            // Configurer le timer pour vérifier toutes les 5 secondes
            _checkTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _checkTimer.Tick += CheckRestrictedProcesses;
            _restrictions = new List<ProcessRestriction>();
        }

        public void Start()
        {
            // Charger les restrictions
            _restrictions = ProcessRestrictionService.LoadProcessRestrictions();
            // Récupérer les informations utilisateur
            UpdateUserInfo();
            // Démarrer le timer
            _checkTimer.Start();
        }

        public void Stop()
        {
            _checkTimer.Stop();
        }

        public void UpdateRestrictions()
        {
            _restrictions = ProcessRestrictionService.LoadProcessRestrictions();
        }

        private void UpdateUserInfo()
        {
            try
            {
                // Récupérer le nom d'utilisateur actuel
                string username = Environment.UserName;

                // Utiliser la classe UserInfoRetriever pour obtenir les informations
                UserInfoRetriever.UserInfo userInfo = UserInfoRetriever.GetUserInfo(username);

                // Mettre à jour les informations
                _currentUserAge = userInfo.Age;
                _currentUserName = userInfo.DisplayName;

                // Déclencher l'événement de mise à jour
                UserInfoUpdated?.Invoke(_currentUserName, _currentUserAge);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la mise à jour des informations utilisateur: {ex.Message}");
            }
        }

        private void CheckRestrictedProcesses(object sender, EventArgs e)
        {
            if (_restrictions == null || _restrictions.Count == 0)
                return;

            try
            {
                // Obtenir tous les processus en cours d'exécution
                Process[] runningProcesses = Process.GetProcesses();

                // Vérifier chaque restriction active
                foreach (var restriction in _restrictions.Where(r => r.IsActive))
                {
                    // Vérifier si le processus est en cours d'exécution
                    foreach (var process in runningProcesses)
                    {
                        if (process.ProcessName.Equals(restriction.ProcessName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Vérifier l'âge
                            if (_currentUserAge < restriction.MinimumAge)
                            {
                                // Tuer le processus
                                try
                                {
                                    process.Kill();
                                    ProcessBlocked?.Invoke(restriction.ProcessName, restriction.MinimumAge, _currentUserAge);

                                    // Afficher un message d'information (en mode asynchrone pour éviter de bloquer le thread du timer)
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        CustomMessageBox.Show(Application.Current.MainWindow,
                                            $"L'application {restriction.ProcessName} nécessite un âge minimum de {restriction.MinimumAge} ans.\n\n" +
                                            $"Utilisateur actuel : {_currentUserName}\n" +
                                            $"Âge : {_currentUserAge} ans",
                                            "Accès restreint par âge",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Warning);
                                    }));
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Erreur lors de la tentative de fermeture du processus {restriction.ProcessName}: {ex.Message}");
                                }
                            }
                            break; // Sortir de la boucle une fois le processus trouvé
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la vérification des processus restreints: {ex.Message}");
            }
        }

        public string CurrentUserName => _currentUserName;
        public int CurrentUserAge => _currentUserAge;
    }
}