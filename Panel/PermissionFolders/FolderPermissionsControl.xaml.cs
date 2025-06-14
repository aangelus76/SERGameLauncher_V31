using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace SERGamesLauncher_V31
{
    public partial class FolderPermissionsControl : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<FolderPermissionViewModel> foldersDisplay;
        private List<FolderPermission> folders;
        private System.Windows.Threading.DispatcherTimer statusRefreshTimer;
        private bool isUpdatingToggle = false;
        private static bool isFirstLoad = true;

        public class FolderPermissionViewModel : INotifyPropertyChanged
        {
            private FolderPermission _permission;
            private PermissionStatus _status;

            public string Id => _permission.Id;
            public string Name => _permission.Name;
            public string FolderPath => _permission.FolderPath;
            public bool EnableOnStartup
            {
                get => _permission.EnableOnStartup;
                set
                {
                    if (_permission.EnableOnStartup != value)
                    {
                        _permission.EnableOnStartup = value;
                        OnPropertyChanged(nameof(EnableOnStartup));
                    }
                }
            }

            public ProtectionLevel ProtectionLevel => _permission.ProtectionLevel;

            public string ProtectionLevelDisplay
            {
                get
                {
                    switch (ProtectionLevel)
                    {
                        case ProtectionLevel.ReadOnly:
                            return "Lecture seule";
                        case ProtectionLevel.PreventDeletion:
                            return "Anti-suppression";
                        case ProtectionLevel.PreventCreation:
                            return "Anti-création";
                        default:
                            return "Lecture seule";
                    }
                }
            }

            public bool IsProtectionEnabled
            {
                get => _permission.IsProtectionEnabled;
                set
                {
                    if (_permission.IsProtectionEnabled != value)
                    {
                        _permission.IsProtectionEnabled = value;
                        OnPropertyChanged(nameof(IsProtectionEnabled));
                    }
                }
            }

            public PermissionStatus Status
            {
                get => _status;
                set
                {
                    if (_status != value)
                    {
                        _status = value;
                        OnPropertyChanged(nameof(Status));
                        OnPropertyChanged(nameof(StatusDisplay));
                        OnPropertyChanged(nameof(StatusColor));
                    }
                }
            }

            public string StatusDisplay
            {
                get
                {
                    switch (Status)
                    {
                        case PermissionStatus.Protected:
                            return "Protégé";
                        case PermissionStatus.Unprotected:
                            return "Non protégé";
                        case PermissionStatus.ProtectedButShouldNot:
                            return "Protégé (devrait être libre)";
                        case PermissionStatus.UnprotectedButShouldBe:
                            return "Non protégé (devrait être protégé)";
                        case PermissionStatus.FolderDoesNotExist:
                            return "Dossier inexistant";
                        case PermissionStatus.AccessError:
                            return "Erreur d'accès";
                        default:
                            return "Inconnu";
                    }
                }
            }

            public Brush StatusColor
            {
                get
                {
                    switch (Status)
                    {
                        case PermissionStatus.Protected:
                            return Brushes.LimeGreen;
                        case PermissionStatus.Unprotected:
                            return Brushes.Orange;
                        case PermissionStatus.ProtectedButShouldNot:
                        case PermissionStatus.UnprotectedButShouldBe:
                            return Brushes.Yellow;
                        case PermissionStatus.FolderDoesNotExist:
                        case PermissionStatus.AccessError:
                            return Brushes.Red;
                        default:
                            return Brushes.Gray;
                    }
                }
            }

            public DateTime LastModified => _permission.LastModified;
            public string LastModifiedDisplay => LastModified.ToString("dd/MM/yyyy HH:mm:ss");

            public FolderPermissionViewModel(FolderPermission permission)
            {
                _permission = permission;
                _status = FolderPermissionService.GetFolderPermissionStatus(permission);
            }

            public void UpdateStatus()
            {
                Status = FolderPermissionService.GetFolderPermissionStatus(_permission);
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public FolderPermissionsControl()
        {
            InitializeComponent();
            LoadFolders();
            UpdateToggleState();
            StartStatusRefreshTimer();

            this.Unloaded += FolderPermissionsControl_Unloaded;
        }

        private void FolderPermissionsControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (statusRefreshTimer != null)
            {
                statusRefreshTimer.Stop();
                statusRefreshTimer = null;
            }
        }

        private void StartStatusRefreshTimer()
        {
            statusRefreshTimer = new System.Windows.Threading.DispatcherTimer();
            statusRefreshTimer.Interval = TimeSpan.FromSeconds(5);
            statusRefreshTimer.Tick += (s, e) =>
            {
                foreach (var folderVM in foldersDisplay)
                {
                    folderVM.UpdateStatus();
                }
                UpdateToggleState();
            };

            statusRefreshTimer.Start();
        }

        private void UpdateToggleState()
        {
            if (folders == null || folders.Count == 0)
            {
                isUpdatingToggle = true;
                toggleLockAll.IsEnabled = false;
                toggleLockAll.IsChecked = false;
                isUpdatingToggle = false;
                return;
            }

            toggleLockAll.IsEnabled = true;

            bool allProtected = folders.All(f => f.IsProtectionEnabled);
            bool noneProtected = folders.All(f => !f.IsProtectionEnabled);

            isUpdatingToggle = true;
            toggleLockAll.IsChecked = allProtected;
            isUpdatingToggle = false;
        }

        private void LoadFolders()
        {
            folders = FolderPermissionService.LoadFolderPermissions();

            if (isFirstLoad)
            {
                foreach (var folder in folders)
                {
                    folder.IsProtectionEnabled = true;
                    FolderPermissionService.ApplyProtection(folder);
                }
                FolderPermissionService.SaveFolderPermissions(folders);
                isFirstLoad = false;
            }

            foldersDisplay = new ObservableCollection<FolderPermissionViewModel>();
            foreach (var folder in folders)
            {
                foldersDisplay.Add(new FolderPermissionViewModel(folder));
            }

            foldersDataGrid.ItemsSource = foldersDisplay;
        }

        private void RefreshDisplay()
        {
            foldersDisplay.Clear();
            foreach (var folder in folders)
            {
                foldersDisplay.Add(new FolderPermissionViewModel(folder));
            }
            UpdateToggleState();
        }

        private void SaveFolders()
        {
            bool success = FolderPermissionService.SaveFolderPermissions(folders);

            if (!success)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    "Une erreur est survenue lors de la sauvegarde des dossiers protégés.",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // NOUVELLE MÉTHODE STATIQUE : Utilisée par PlatformContentControl
        public static void SetProtectionForAllFolders(bool enableProtection)
        {
            try
            {
                var folders = FolderPermissionService.LoadFolderPermissions();

                foreach (var folder in folders)
                {
                    folder.IsProtectionEnabled = enableProtection;

                    if (enableProtection)
                    {
                        FolderPermissionService.ApplyProtection(folder);
                    }
                    else
                    {
                        FolderPermissionService.RemoveProtection(folder);
                    }
                }

                FolderPermissionService.SaveFolderPermissions(folders);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur SetProtectionForAllFolders: {ex.Message}");
            }
        }

        // NOUVELLE MÉTHODE : Rafraîchir depuis l'extérieur
        public void RefreshFromExternalChange()
        {
            try
            {
                folders = FolderPermissionService.LoadFolderPermissions();
                RefreshDisplay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur RefreshFromExternalChange: {ex.Message}");
            }
        }

        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            AddEditFolderDialog dialog = new AddEditFolderDialog();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true && dialog.FolderPermission != null)
            {
                if (FolderPermissionService.FolderNameExists(folders, dialog.FolderPermission.Name))
                {
                    CustomMessageBox.Show(Window.GetWindow(this),
                        $"Un dossier protégé avec le nom '{dialog.FolderPermission.Name}' existe déjà.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                dialog.FolderPermission.IsProtectionEnabled = toggleLockAll.IsChecked ?? true;

                folders.Add(dialog.FolderPermission);
                var viewModel = new FolderPermissionViewModel(dialog.FolderPermission);
                foldersDisplay.Add(viewModel);

                if (dialog.FolderPermission.IsProtectionEnabled)
                {
                    if (!FolderPermissionService.ApplyProtection(dialog.FolderPermission))
                    {
                        CustomMessageBox.Show(Window.GetWindow(this),
                            $"La protection n'a pas pu être appliquée au dossier '{dialog.FolderPermission.Name}'.",
                            "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    viewModel.UpdateStatus();
                }

                SaveFolders();
                UpdateToggleState();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string folderId)
            {
                var folder = folders.FirstOrDefault(f => f.Id == folderId);
                if (folder == null) return;

                AddEditFolderDialog dialog = new AddEditFolderDialog(folder);
                dialog.Owner = Window.GetWindow(this);

                if (dialog.ShowDialog() == true && dialog.FolderPermission != null)
                {
                    if (folder.Name != dialog.FolderPermission.Name &&
                        FolderPermissionService.FolderNameExists(folders, dialog.FolderPermission.Name, folderId))
                    {
                        CustomMessageBox.Show(Window.GetWindow(this),
                            $"Un dossier protégé avec le nom '{dialog.FolderPermission.Name}' existe déjà.",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    folder.Name = dialog.FolderPermission.Name;
                    folder.FolderPath = dialog.FolderPermission.FolderPath;
                    folder.ProtectionLevel = dialog.FolderPermission.ProtectionLevel;
                    folder.EnableOnStartup = dialog.FolderPermission.EnableOnStartup;
                    folder.LastModified = DateTime.Now;

                    if (folder.IsProtectionEnabled)
                    {
                        FolderPermissionService.ApplyProtection(folder);
                    }
                    else
                    {
                        FolderPermissionService.RemoveProtection(folder);
                    }

                    RefreshDisplay();
                    SaveFolders();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string folderId)
            {
                var folder = folders.FirstOrDefault(f => f.Id == folderId);
                if (folder == null) return;

                var result = CustomMessageBox.Show(Window.GetWindow(this),
                    $"Êtes-vous sûr de vouloir supprimer la protection du dossier '{folder.Name}' ?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (folder.IsProtectionEnabled)
                    {
                        if (!FolderPermissionService.RemoveProtection(folder))
                        {
                            CustomMessageBox.Show(Window.GetWindow(this),
                                $"Impossible de supprimer la protection du dossier '{folder.Name}'.",
                                "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    folders.Remove(folder);

                    var displayFolder = foldersDisplay.FirstOrDefault(f => f.Id == folderId);
                    if (displayFolder != null)
                    {
                        foldersDisplay.Remove(displayFolder);
                    }

                    SaveFolders();
                    UpdateToggleState();
                }
            }
        }

        private void ToggleLockAll_Changed(object sender, RoutedEventArgs e)
        {
            if (isUpdatingToggle || folders == null || folders.Count == 0) return;

            bool isChecked = toggleLockAll.IsChecked ?? false;

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

            RefreshDisplay();
            SaveFolders();
        }

        private void RefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            foreach (var folderVM in foldersDisplay)
            {
                folderVM.UpdateStatus();
            }
            UpdateToggleState();
        }

        private void EnableAll_Click(object sender, RoutedEventArgs e)
        {
            toggleLockAll.IsChecked = true;
        }

        private void DisableAll_Click(object sender, RoutedEventArgs e)
        {
            toggleLockAll.IsChecked = false;
        }

        public static void ResetFirstLoadFlag()
        {
            isFirstLoad = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}