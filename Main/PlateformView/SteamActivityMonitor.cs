using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SERGamesLauncher_V31
{
    public class SteamActivityMonitor
    {
        private readonly System.Windows.Threading.DispatcherTimer _checkTimer;
        private readonly List<string> _steamProcessNames;
        private string _lastUsername = null;
        private bool _isStarting = false;
        private DateTime _startTime;
        private bool _isShowingMessage = false;
        private string _loginUsersPath = null;

        public bool AllowUserAccounts { get; set; } = false;

        public delegate void UserChangedEventHandler(string username);
        public event UserChangedEventHandler UserChanged;

        public SteamActivityMonitor()
        {
            _steamProcessNames = new List<string> {
                "steam",
                "steamwebhelper",
                "SteamService",
                "steamlauncher",
                "GameOverlayUI"
            };

            // Configurer le timer pour vérifier toutes les 10 secondes
            _checkTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _checkTimer.Tick += CheckSteamActivity;

            // Trouver le chemin du fichier loginusers.vdf
            FindSteamLoginFile();
        }

        private void FindSteamLoginFile()
        {
            try
            {
                // Récupérer le chemin de Steam via la configuration
                List<PathConfig> pathConfigs = PathConfigService.LoadPathConfigs();
                PathConfig pathConfig = PathConfigService.GetPathForPlatform(pathConfigs, "Steam");

                if (pathConfig != null)
                {
                    string steamDir = Path.GetDirectoryName(pathConfig.Path);
                    if (steamDir != null)
                    {
                        _loginUsersPath = Path.Combine(steamDir, "config", "loginusers.vdf");
                        System.Diagnostics.Debug.WriteLine($"Chemin du fichier loginusers.vdf trouvé: {_loginUsersPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la recherche du fichier loginusers.vdf: {ex.Message}");
            }
        }

        public void Start() => _checkTimer.Start();
        public void Stop() => _checkTimer.Stop();

        public void NotifyStarting()
        {
            _isStarting = true;
            _startTime = DateTime.Now;
        }

        public void KillAllSteamProcesses()
        {
            foreach (string processName in _steamProcessNames)
            {
                var processes = Process.GetProcessesByName(processName);
                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                    }
                    catch { }
                }
            }
        }

        public void CheckImmediately()
        {
            if (IsSteamRunning())
            {
                if (!AllowUserAccounts)
                {
                    // Vérifier si Steam est en cours d'exécution
                    if (!IsSteamRunning())
                    {
                        UserChanged?.Invoke("Steam non lancé");
                        return;
                    }

                    // Si le chemin du fichier n'est pas trouvé, réessayer
                    if (_loginUsersPath == null || !File.Exists(_loginUsersPath))
                    {
                        FindSteamLoginFile();
                        if (_loginUsersPath == null || !File.Exists(_loginUsersPath))
                        {
                            UserChanged?.Invoke("Fichier de config non trouvé");
                            return;
                        }
                    }

                    // Lire le fichier de configuration Steam
                    string fileContent = File.ReadAllText(_loginUsersPath);

                    // Récupérer les informations du compte lié à ce poste
                    List<SteamAccount> accounts = SteamAccountService.LoadSteamAccounts();
                    SteamAccount account = SteamAccountService.GetAccountForCurrentComputer(accounts);

                    if (account == null)
                    {
                        UserChanged?.Invoke("Compte non configuré");
                        return;
                    }

                    // Extraire le compte actif (celui avec MostRecent = 1)
                    string currentUsername = ExtractMostRecentAccount(fileContent);

                    // Notifier du compte actuel
                    UserChanged?.Invoke(currentUsername);

                    // Vérifier si c'est le compte configuré pour ce poste
                    if (currentUsername != account.Username && currentUsername != "Déconnecté")
                    {
                        // Si le compte actif n'est pas celui configuré, agir
                        KillAllSteamProcesses();

                        Application.Current.Dispatcher.Invoke(() => {
                            CustomMessageBox.Show(Application.Current.MainWindow,
                                "Steam a été détecté avec un compte non autorisé. Forçage de l'utilisation du compte configuré...",
                                "Sécurité",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        });

                        // Attendre un peu avant de relancer pour s'assurer que tout est fermé
                        Thread.Sleep(2000);
                        RestartSteamWithConfiguredAccount(account);
                    }
                }
            }
        }

        private void CheckSteamActivity(object sender, EventArgs e)
        {
            if (AllowUserAccounts)
            {
                // Si les comptes utilisateurs sont autorisés, on se contente de vérifier si Steam est lancé
                bool isSteamRunning = IsSteamRunning();
                UserChanged?.Invoke(isSteamRunning ? "Compte personnel autorisé" : "Steam non lancé");
                return;
            }

            // Si Steam est en démarrage, attendre un peu
            if (_isStarting)
            {
                if ((DateTime.Now - _startTime).TotalSeconds > 10)
                {
                    _isStarting = false;
                }
                else
                {
                    return;
                }
            }

            // Vérifier le compte actif
            CheckSteamUser();
        }

        private bool IsSteamRunning()
        {
            return Process.GetProcessesByName("steam").Length > 0;
        }

        private void CheckSteamUser()
        {
            try
            {
                // Vérifier si Steam est en cours d'exécution
                if (!IsSteamRunning())
                {
                    UserChanged?.Invoke("Steam non lancé");
                    _lastUsername = null;
                    return;
                }

                // Si le chemin du fichier n'est pas trouvé, réessayer
                if (_loginUsersPath == null || !File.Exists(_loginUsersPath))
                {
                    FindSteamLoginFile();
                    if (_loginUsersPath == null || !File.Exists(_loginUsersPath))
                    {
                        UserChanged?.Invoke("Fichier de config non trouvé");
                        return;
                    }
                }

                // Lire le fichier de configuration Steam
                string fileContent = File.ReadAllText(_loginUsersPath);

                // Récupérer les informations du compte lié à ce poste
                List<SteamAccount> accounts = SteamAccountService.LoadSteamAccounts();
                SteamAccount account = SteamAccountService.GetAccountForCurrentComputer(accounts);

                if (account == null)
                {
                    UserChanged?.Invoke("Compte non configuré");
                    return;
                }

                // Extraire le compte actif (celui avec MostRecent = 1)
                string currentUsername = ExtractMostRecentAccount(fileContent);

                // Si le compte actif est différent du dernier connu, notifier le changement
                if (currentUsername != _lastUsername)
                {
                    _lastUsername = currentUsername;
                    UserChanged?.Invoke(currentUsername);

                    // Vérifier si c'est le compte configuré pour ce poste
                    if (currentUsername != account.Username && currentUsername != "Déconnecté")
                    {
                        // Si le compte actif n'est pas celui configuré, agir
                        HandleUnauthorizedAccount(account);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la vérification du compte Steam: {ex.Message}");
                UserChanged?.Invoke("Erreur de vérification");
            }
        }

        private string ExtractMostRecentAccount(string fileContent)
        {
            try
            {
                // Rechercher la section avec "MostRecent" à 1
                int mostRecentIndex = fileContent.IndexOf("\"MostRecent\"\t\t\"1\"");
                if (mostRecentIndex == -1)
                {
                    return "Déconnecté"; // Aucun compte actif trouvé
                }

                // Rechercher la section du compte actif en remontant depuis MostRecent
                int sectionStart = fileContent.LastIndexOf('{', mostRecentIndex);
                if (sectionStart == -1)
                {
                    return "Déconnecté";
                }

                // Chercher le nom du compte dans cette section
                int accountNameStart = fileContent.IndexOf("\"AccountName\"\t\t\"", sectionStart);
                if (accountNameStart == -1 || accountNameStart > mostRecentIndex + 100) // Limiter la recherche à une distance raisonnable
                {
                    return "Déconnecté";
                }

                accountNameStart += "\"AccountName\"\t\t\"".Length;
                int accountNameEnd = fileContent.IndexOf('"', accountNameStart);

                if (accountNameEnd == -1)
                {
                    return "Déconnecté";
                }

                return fileContent.Substring(accountNameStart, accountNameEnd - accountNameStart);
            }
            catch
            {
                return "Analyse impossible";
            }
        }

        private void HandleUnauthorizedAccount(SteamAccount configuredAccount)
        {
            try
            {
                // Tuer Steam et le relancer avec le bon compte
                if (!_isShowingMessage)
                {
                    _isShowingMessage = true;

                    // Fermer Steam
                    KillAllSteamProcesses();

                    // Informer l'utilisateur
                    Application.Current.Dispatcher.Invoke(() => {
                        CustomMessageBox.Show(Application.Current.MainWindow,
                            "Compte Steam non autorisé détecté. Le compte configuré va être utilisé automatiquement.",
                            "Sécurité",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    });

                    _isShowingMessage = false;

                    // Relancer Steam avec le compte configuré
                    RestartSteamWithConfiguredAccount(configuredAccount);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la gestion du compte non autorisé: {ex.Message}");
            }
        }



        private void RestartSteamWithConfiguredAccount(SteamAccount account)
        {
            try
            {
                if (account == null) return;

                List<PathConfig> pathConfigs = PathConfigService.LoadPathConfigs();
                PathConfig pathConfig = PathConfigService.GetPathForPlatform(pathConfigs, "Steam");

                if (pathConfig == null || !File.Exists(pathConfig.Path)) return;

                // Décrypter le mot de passe
                string password = SteamAccountService.DecryptPassword(account.EncryptedPassword);

                // Lancer Steam avec les identifiants
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = pathConfig.Path,
                    Arguments = $"-noreactlogin -login {account.Username} {password}"
                };

                Process.Start(startInfo);

                // Réinitialiser les variables
                _lastUsername = null;
                _isStarting = true;
                _startTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du redémarrage de Steam: {ex.Message}");
            }
        }
    }
}