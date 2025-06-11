using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SERGamesLauncher_V31
{
    public partial class ProcessSelectorDialog : Window
    {
        // Collection des processus
        private ObservableCollection<ProcessInfo> processes;

        // Processus sélectionné
        public string SelectedProcessName { get; private set; }

        // Classe pour stocker les informations d'un processus
        public class ProcessInfo
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public string WindowTitle { get; set; }
            public int InstanceCount { get; set; } = 1;

            public ProcessInfo(string name, int id, string windowTitle)
            {
                Name = name;
                Id = id;
                WindowTitle = windowTitle;
            }

            public override string ToString()
            {
                return $"{Name} ({WindowTitle})";
            }
        }

        public ProcessSelectorDialog()
        {
            InitializeComponent();

            // Permettre le déplacement de la fenêtre sans bordure
            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            // Initialiser la collection
            processes = new ObservableCollection<ProcessInfo>();

            // Lier au ListView
            processListView.ItemsSource = processes;

            // S'abonner à l'événement de filtre TextChanged
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // Charger les processus
            LoadProcesses();

            // Configurer la vue
            ConfigureListView();
        }

        private void ConfigureListView()
        {
            // Créer la vue
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(processListView.ItemsSource);

            // Trier par nom
            view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Name", System.ComponentModel.ListSortDirection.Ascending));

            // Grouper par première lettre
            view.GroupDescriptions.Add(new PropertyGroupDescription("Name", new FirstLetterConverter()));

            // Ajouter un filtre
            view.Filter = ProcessFilter;
        }

        private bool ProcessFilter(object item)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
                return true;

            var process = item as ProcessInfo;
            return process != null &&
                   (process.Name.IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   (process.WindowTitle != null && process.WindowTitle.IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Actualiser la vue pour appliquer le filtre
            CollectionViewSource.GetDefaultView(processListView.ItemsSource).Refresh();
        }

        private void LoadProcesses()
        {
            try
            {
                // Récupérer tous les processus
                Process[] systemProcesses = Process.GetProcesses();

                // Liste des processus système à ignorer
                HashSet<string> systemProcessNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "svchost", "csrss", "smss", "lsass", "winlogon", "services", "explorer",
                    "spoolsv", "wininit", "dwm", "conhost", "rundll32", "taskhost", "taskhostw",
                    "taskmgr", "devenv", "mmc", "wmiprvse", "dllhost", "ctfmon", "sihost",
                    "fontdrvhost", "searchindexer", "searchui", "smartscreen", "shellexperiencehost",
                    "runtimebroker", "applicationframehost"
                };

                // Dictionnaire pour fusionner les instances multiples
                Dictionary<string, ProcessInfo> uniqueProcesses = new Dictionary<string, ProcessInfo>();

                // Filtrer et ajouter les processus
                foreach (Process process in systemProcesses)
                {
                    try
                    {
                        // Ignorer les processus système
                        if (systemProcessNames.Contains(process.ProcessName))
                            continue;

                        // Essayer de récupérer le titre de la fenêtre
                        string windowTitle = string.Empty;
                        try
                        {
                            if (!process.HasExited && process.MainWindowHandle != IntPtr.Zero)
                            {
                                windowTitle = process.MainWindowTitle;
                            }
                        }
                        catch { }

                        // Ajouter uniquement les processus avec un nom significatif
                        if (!string.IsNullOrEmpty(process.ProcessName) &&
                            !process.ProcessName.StartsWith("Microsoft") &&
                            process.ProcessName.Length > 2)
                        {
                            // Vérifier si ce processus est déjà dans notre dictionnaire
                            if (uniqueProcesses.TryGetValue(process.ProcessName, out ProcessInfo existingProcess))
                            {
                                // Si oui, incrémenter le compteur d'instances et mettre à jour le titre si nécessaire
                                existingProcess.InstanceCount++;
                                if (string.IsNullOrEmpty(existingProcess.WindowTitle) && !string.IsNullOrEmpty(windowTitle))
                                {
                                    existingProcess.WindowTitle = windowTitle;
                                }
                            }
                            else
                            {
                                // Sinon, ajouter une nouvelle entrée
                                uniqueProcesses.Add(process.ProcessName, new ProcessInfo(process.ProcessName, process.Id, windowTitle));
                            }
                        }
                    }
                    catch { }
                }

                // Ajouter les processus uniques à la collection
                foreach (var process in uniqueProcesses.Values)
                {
                    processes.Add(process);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des processus : {ex.Message}");
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            processes.Clear();
            LoadProcesses();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (processListView.SelectedItem is ProcessInfo selectedProcess)
            {
                SelectedProcessName = selectedProcess.Name;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                CustomMessageBox.Show(this, "Veuillez sélectionner un processus dans la liste.", "Sélection requise", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void processListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (processListView.SelectedItem is ProcessInfo)
            {
                btnSelect_Click(sender, e);
            }
        }
    }

    // Convertisseur pour grouper par première lettre
    public class FirstLetterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value as string;
            if (string.IsNullOrEmpty(str))
                return "#";

            char firstChar = str[0];
            if (char.IsLetter(firstChar))
                return firstChar.ToString().ToUpper();

            return "#";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}