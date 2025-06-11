// Panel/Folder/FolderPermissionsControl.xaml.cs
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
        // Collection observable des dossiers
        private ObservableCollection<FolderPermissionViewModel> foldersDisplay;

        // Liste brute pour les opérations
        private List<FolderPermission> folders;

        // Timer pour le rafraîchissement automatique de l'état des dossiers
        private System.Windows.Threading.DispatcherTimer statusRefreshTimer;

        // Flag pour éviter les boucles infinies lors du changement d'état du ToggleSwitch
        private bool isUpdatingToggle = false;

        // Flag pour indiquer si c'est le premier chargement du contrôle
        private static bool isFirstLoad = true;

        // ViewModel pour l'affichage des permissions dans le DataGrid
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

            // S'abonner à l'événement Unloaded
            this.Unloaded += FolderPermissionsControl_Unloaded;
        }

        private void FolderPermissionsControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Arrêter le timer quand le contrôle est déchargé
            if (statusRefreshTimer != null)
            {
                statusRefreshTimer.Stop();
                statusRefreshTimer = null;
            }
        }

        // Démarrer le timer pour actualiser l'état des dossiers
        private void StartStatusRefreshTimer()
        {
            // Créer et configurer le timer
            statusRefreshTimer = new System.Windows.Threading.DispatcherTimer();
            statusRefreshTimer.Interval = TimeSpan.FromSeconds(5); // Actualiser toutes les 5 secondes
            statusRefreshTimer.Tick += (s, e) =>
            {
                // Actualiser l'état de tous les dossiers
                foreach (var folderVM in foldersDisplay)
                {
                    folderVM.UpdateStatus();
                }
                // Mettre à jour l'état du toggle global
                UpdateToggleState();
            };

            // Démarrer le timer
            statusRefreshTimer.Start();
        }

        // Mettre à jour l'état du toggle global en fonction des dossiers
        private void UpdateToggleState()
        {
            if (folders == null || folders.Count == 0)
            {
                // S'il n'y a pas de dossiers, désactiver le toggle
                isUpdatingToggle = true;
                toggleLockAll.IsEnabled = false;
                toggleLockAll.IsChecked = false;
                isUpdatingToggle = false;
                return;
            }

            // Activer le toggle
            toggleLockAll.IsEnabled = true;

            // Vérifier l'état de tous les dossiers
            bool allProtected = folders.All(f => f.IsProtectionEnabled);
            bool noneProtected = folders.All(f => !f.IsProtectionEnabled);

            // Mettre à jour l'état du toggle sans déclencher l'événement
            isUpdatingToggle = true;
            toggleLockAll.IsChecked = allProtected;
            isUpdatingToggle = false;
        }

        // Charger les dossiers depuis le service
        private void LoadFolders()
        {
            // Charger les dossiers
            folders = FolderPermissionService.LoadFolderPermissions();

            // Appliquer la protection à tous les dossiers uniquement au premier chargement
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

            // Créer les modèles d'affichage
            foldersDisplay = new ObservableCollection<FolderPermissionViewModel>();
            foreach (var folder in folders)
            {
                foldersDisplay.Add(new FolderPermissionViewModel(folder));
            }

            // Lier au DataGrid
            foldersDataGrid.ItemsSource = foldersDisplay;
        }

        // Mettre à jour l'affichage
        private void RefreshDisplay()
        {
            foldersDisplay.Clear();
            foreach (var folder in folders)
            {
                foldersDisplay.Add(new FolderPermissionViewModel(folder));
            }
            UpdateToggleState();
        }

        // Sauvegarder les changements
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

        // Ajouter un nouveau dossier protégé
        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            // Créer la boîte de dialogue
            AddEditFolderDialog dialog = new AddEditFolderDialog();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true && dialog.FolderPermission != null)
            {
                // Vérifier si un dossier avec le même nom existe déjà
                if (FolderPermissionService.FolderNameExists(folders, dialog.FolderPermission.Name))
                {
                    CustomMessageBox.Show(Window.GetWindow(this),
                        $"Un dossier protégé avec le nom '{dialog.FolderPermission.Name}' existe déjà.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Par défaut, activer la protection selon l'état actuel du toggle
                dialog.FolderPermission.IsProtectionEnabled = toggleLockAll.IsChecked ?? true;

                // Ajouter à la liste
                folders.Add(dialog.FolderPermission);
                var viewModel = new FolderPermissionViewModel(dialog.FolderPermission);
                foldersDisplay.Add(viewModel);

                // Appliquer la protection si nécessaire
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

                // Sauvegarder les changements
                SaveFolders();

                // Mettre à jour l'état du toggle global
                UpdateToggleState();
            }
        }

        // Modifier un dossier existant - SUPPRESSION DE L'AUTHENTIFICATION
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string folderId)
            {
                // SUPPRIMÉ : Demande de mot de passe d'administration
                // On est déjà dans le panel admin, pas besoin de re-authentifier

                // Trouver le dossier
                var folder = folders.FirstOrDefault(f => f.Id == folderId);
                if (folder == null) return;

                // Créer la boîte de dialogue et pré-remplir les champs
                AddEditFolderDialog dialog = new AddEditFolderDialog(folder);
                dialog.Owner = Window.GetWindow(this);

                if (dialog.ShowDialog() == true && dialog.FolderPermission != null)
                {
                    // Vérifier si le nouveau nom existe déjà
                    if (folder.Name != dialog.FolderPermission.Name &&
                        FolderPermissionService.FolderNameExists(folders, dialog.FolderPermission.Name, folderId))
                    {
                        CustomMessageBox.Show(Window.GetWindow(this),
                            $"Un dossier protégé avec le nom '{dialog.FolderPermission.Name}' existe déjà.",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Mettre à jour les champs
                    folder.Name = dialog.FolderPermission.Name;
                    folder.FolderPath = dialog.FolderPermission.FolderPath;
                    folder.EnableOnStartup = dialog.FolderPermission.EnableOnStartup;
                    folder.ProtectionLevel = dialog.FolderPermission.ProtectionLevel;

                    // Mettre à jour l'affichage
                    RefreshDisplay();

                    // Sauvegarder les changements
                    SaveFolders();
                }
            }
        }

        // Supprimer un dossier - SUPPRESSION DE L'AUTHENTIFICATION
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string folderId)
            {
                // SUPPRIMÉ : Demande de mot de passe d'administration
                // On est déjà dans le panel admin, pas besoin de re-authentifier

                // Trouver le dossier
                var folder = folders.FirstOrDefault(f => f.Id == folderId);
                if (folder != null)
                {
                    // Demander confirmation directement
                    MessageBoxResult result = CustomMessageBox.Show(Window.GetWindow(this),
                        $"Êtes-vous sûr de vouloir supprimer le dossier protégé '{folder.Name}' ?\n\n" +
                        "Cela va également retirer toute protection appliquée au dossier.",
                        "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Retirer la protection si elle est active
                        if (folder.IsProtectionEnabled)
                        {
                            if (!FolderPermissionService.RemoveProtection(folder))
                            {
                                CustomMessageBox.Show(Window.GetWindow(this),
                                    $"La protection n'a pas pu être retirée du dossier '{folder.Name}'.\n" +
                                    "La configuration sera supprimée mais le dossier peut rester protégé.",
                                    "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }

                        // Supprimer de la liste
                        folders.Remove(folder);

                        // Mettre à jour l'affichage
                        var displayFolder = foldersDisplay.FirstOrDefault(f => f.Id == folderId);
                        if (displayFolder != null)
                        {
                            foldersDisplay.Remove(displayFolder);
                        }

                        // Sauvegarder les changements
                        SaveFolders();

                        // Mettre à jour l'état du toggle global
                        UpdateToggleState();
                    }
                }
            }
        }

        // Évènement pour le toggle global de verrouillage/déverrouillage
        private void ToggleLockAll_Changed(object sender, RoutedEventArgs e)
        {
            // Si le changement est dû à la mise à jour programmatique, ne rien faire
            if (isUpdatingToggle || folders == null || folders.Count == 0) return;

            bool isChecked = toggleLockAll.IsChecked ?? false;

            // Appliquer l'action à tous les dossiers
            if (isChecked)
            {
                // Verrouiller tous les dossiers
                foreach (var folder in folders)
                {
                    folder.IsProtectionEnabled = true;
                    FolderPermissionService.ApplyProtection(folder);
                }
            }
            else
            {
                // Déverrouiller tous les dossiers
                foreach (var folder in folders)
                {
                    folder.IsProtectionEnabled = false;
                    FolderPermissionService.RemoveProtection(folder);
                }
            }

            // Mettre à jour l'affichage
            RefreshDisplay();

            // Sauvegarder les changements
            SaveFolders();
        }

        // Actualiser l'état de tous les dossiers
        private void RefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            foreach (var folderVM in foldersDisplay)
            {
                folderVM.UpdateStatus();
            }
            UpdateToggleState();
        }

        // Événements pour les boutons Tout verrouiller/déverrouiller
        private void EnableAll_Click(object sender, RoutedEventArgs e)
        {
            // Définir l'état du toggle à true (ce qui déclenchera l'événement ToggleLockAll_Changed)
            toggleLockAll.IsChecked = true;
        }

        private void DisableAll_Click(object sender, RoutedEventArgs e)
        {
            // Définir l'état du toggle à false (ce qui déclenchera l'événement ToggleLockAll_Changed)
            toggleLockAll.IsChecked = false;
        }

        // Méthode publique statique pour réinitialiser le flag de premier chargement
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