using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SERGamesLauncher_V31
{
    public partial class ProcessRestrictionsControl : UserControl, INotifyPropertyChanged
    {
        // Collection observable des restrictions
        private ObservableCollection<ProcessRestriction> processesDisplay;

        // Liste brute pour les opérations
        private List<ProcessRestriction> processes;

        // Moniteur de processus
        private ProcessMonitor processMonitor;

        // État du moniteur
        private bool isMonitorRunning = false;

        public ProcessRestrictionsControl()
        {
            InitializeComponent();

            // Initialiser le moniteur de processus
            processMonitor = new ProcessMonitor();
            processMonitor.ProcessBlocked += ProcessMonitor_ProcessBlocked;
            processMonitor.UserInfoUpdated += ProcessMonitor_UserInfoUpdated;

            // Charger les restrictions
            LoadProcesses();

            // Démarrer le moniteur automatiquement au lancement
            StartMonitor();

            // S'abonner à l'événement Unloaded
            this.Unloaded += ProcessRestrictionsControl_Unloaded;
        }

        // Méthode pour démarrer le moniteur
        private void StartMonitor()
        {
            if (!isMonitorRunning)
            {
                processMonitor.Start();
                isMonitorRunning = true;
                UpdateMonitorStatus(true);
            }
        }

        private void ProcessRestrictionsControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Arrêter le moniteur quand le contrôle est déchargé
            if (isMonitorRunning)
            {
                processMonitor.Stop();
                isMonitorRunning = false;
            }
        }

        // Gérer l'événement de blocage de processus
        private void ProcessMonitor_ProcessBlocked(string processName, int minimumAge, int userAge)
        {
            // Cette méthode est appelée lorsqu'un processus est bloqué
            Console.WriteLine($"Processus bloqué: {processName} (Âge minimum: {minimumAge}, Âge utilisateur: {userAge})");
        }

        // Gérer l'événement de mise à jour des informations utilisateur
        private void ProcessMonitor_UserInfoUpdated(string displayName, int age)
        {
            // Mettre à jour l'affichage sur le thread UI
            Dispatcher.Invoke(() =>
            {
                txtUserName.Text = $"Utilisateur : {displayName}";
                txtUserAge.Text = $"Âge : {age} ans";
            });
        }

        // Charger les restrictions depuis le service
        private void LoadProcesses()
        {
            // Charger les restrictions
            processes = ProcessRestrictionService.LoadProcessRestrictions();

            // Créer la collection observable
            processesDisplay = new ObservableCollection<ProcessRestriction>(processes);

            // Lier au DataGrid
            processesDataGrid.ItemsSource = processesDisplay;
        }

        // Mettre à jour l'affichage
        private void RefreshDisplay()
        {
            processesDisplay.Clear();
            foreach (var process in processes)
            {
                processesDisplay.Add(process);
            }
        }

        // Sauvegarder les changements
        private void SaveProcesses()
        {
            bool success = ProcessRestrictionService.SaveProcessRestrictions(processes);

            if (!success)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    "Une erreur est survenue lors de la sauvegarde des restrictions de processus.",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Mettre à jour le moniteur
            if (isMonitorRunning)
            {
                processMonitor.UpdateRestrictions();
            }
        }

        // Ajouter une nouvelle restriction
        private void AddProcess_Click(object sender, RoutedEventArgs e)
        {
            // Créer la boîte de dialogue
            AddEditProcessDialog dialog = new AddEditProcessDialog();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true && dialog.ProcessRestriction != null)
            {
                // Vérifier si un processus avec le même nom existe déjà
                if (ProcessRestrictionService.ProcessNameExists(processes, dialog.ProcessRestriction.ProcessName))
                {
                    CustomMessageBox.Show(Window.GetWindow(this),
                        $"Une restriction pour le processus '{dialog.ProcessRestriction.ProcessName}' existe déjà.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Ajouter à la liste
                processes.Add(dialog.ProcessRestriction);
                processesDisplay.Add(dialog.ProcessRestriction);

                // Sauvegarder les changements
                SaveProcesses();
            }
        }

        // Modifier une restriction existante - SUPPRESSION DE L'AUTHENTIFICATION
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string processId)
            {
                // SUPPRIMÉ : Demande de mot de passe d'administration
                // On est déjà dans le panel admin, pas besoin de re-authentifier

                // Trouver la restriction
                var process = processes.FirstOrDefault(p => p.Id == processId);
                if (process == null) return;

                // Créer la boîte de dialogue et pré-remplir les champs
                AddEditProcessDialog dialog = new AddEditProcessDialog(process);
                dialog.Owner = Window.GetWindow(this);

                if (dialog.ShowDialog() == true && dialog.ProcessRestriction != null)
                {
                    // Vérifier si le nouveau nom existe déjà
                    if (process.ProcessName != dialog.ProcessRestriction.ProcessName &&
                        ProcessRestrictionService.ProcessNameExists(processes, dialog.ProcessRestriction.ProcessName, processId))
                    {
                        CustomMessageBox.Show(Window.GetWindow(this),
                            $"Une restriction pour le processus '{dialog.ProcessRestriction.ProcessName}' existe déjà.",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Mettre à jour les champs
                    process.ProcessName = dialog.ProcessRestriction.ProcessName;
                    process.MinimumAge = dialog.ProcessRestriction.MinimumAge;
                    process.Description = dialog.ProcessRestriction.Description;
                    process.IsActive = dialog.ProcessRestriction.IsActive;

                    // Mettre à jour l'affichage
                    RefreshDisplay();

                    // Sauvegarder les changements
                    SaveProcesses();
                }
            }
        }

        // Supprimer une restriction - SUPPRESSION DE L'AUTHENTIFICATION
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string processId)
            {
                // SUPPRIMÉ : Demande de mot de passe d'administration
                // On est déjà dans le panel admin, pas besoin de re-authentifier

                // Trouver la restriction
                var process = processes.FirstOrDefault(p => p.Id == processId);
                if (process != null)
                {
                    // Demander confirmation directement
                    MessageBoxResult result = CustomMessageBox.Show(Window.GetWindow(this),
                        $"Êtes-vous sûr de vouloir supprimer la restriction pour '{process.ProcessName}' ?",
                        "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Supprimer de la liste
                        processes.Remove(process);

                        // Mettre à jour l'affichage
                        var displayProcess = processesDisplay.FirstOrDefault(p => p.Id == processId);
                        if (displayProcess != null)
                        {
                            processesDisplay.Remove(displayProcess);
                        }

                        // Sauvegarder les changements
                        SaveProcesses();
                    }
                }
            }
        }

        // Démarrer le moniteur - CORRIGÉ pour redémarrer le moniteur principal
        private void StartMonitor_Click(object sender, RoutedEventArgs e)
        {
            if (!isMonitorRunning)
            {
                // Redémarrer le moniteur local
                processMonitor.Start();
                isMonitorRunning = true;

                // NOUVEAU : Redémarrer aussi le moniteur principal de MainWindow
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.StartProcessMonitor();
                }

                UpdateMonitorStatus(true);
            }
        }

        // Arrêter le moniteur - CORRIGÉ pour accéder au moniteur principal
        private void StopMonitor_Click(object sender, RoutedEventArgs e)
        {
            if (isMonitorRunning)
            {
                // Afficher un message d'avertissement
                CustomMessageBox.Show(Window.GetWindow(this),
                    "Attention: Les restrictions d'âge sont maintenant désactivées temporairement.\n\n" +
                    "Cette option sera réactivée à la fermeture de l'application.",
                    "Restrictions désactivées", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Arrêter le moniteur local
                processMonitor.Stop();
                isMonitorRunning = false;

                // NOUVEAU : Arrêter aussi le moniteur principal de MainWindow
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.StopProcessMonitor();
                }

                UpdateMonitorStatus(false);
            }
        }

        // Mettre à jour l'affichage de l'état du moniteur
        private void UpdateMonitorStatus(bool isRunning)
        {
            btnStartMonitor.IsEnabled = !isRunning;
            btnStopMonitor.IsEnabled = isRunning;

            if (isRunning)
            {
                txtMonitorStatus.Text = "Moniteur : Actif";
                txtMonitorStatus.Foreground = Brushes.LimeGreen;
            }
            else
            {
                txtMonitorStatus.Text = "Moniteur : Inactif";
                txtMonitorStatus.Foreground = Brushes.Orange;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}