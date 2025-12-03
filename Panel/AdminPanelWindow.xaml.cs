using System;
using System.Windows;
using System.Windows.Controls;

namespace SERGamesLauncher_V31
{
    public partial class AdminPanelWindow : Window
    {
        private Panel.PlatformVisibilityControl platformVisibilityControl;
        private PathConfigsControl pathConfigsControl;
        private FolderPermissionsControl folderPermissionsControl;
        private ProcessRestrictionsControl processRestrictionsControl;
        private SilentModeScheduleControl silentModeScheduleControl;
        private CustomButtonControl customButtonControl;

        public AdminPanelWindow()
        {
            InitializeComponent();

            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            InitializeUserControls();

            contentPresenter.Content = new TextBlock()
            {
                Text = "Sélectionnez une option dans le menu",
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void InitializeUserControls()
        {
            platformVisibilityControl = new Panel.PlatformVisibilityControl();
            pathConfigsControl = new PathConfigsControl();
            folderPermissionsControl = new FolderPermissionsControl();
            processRestrictionsControl = new ProcessRestrictionsControl();
            silentModeScheduleControl = new SilentModeScheduleControl();
            customButtonControl = new CustomButtonControl();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnSteamAccounts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SteamAccountsControl steamAccountsControl = new SteamAccountsControl();
                contentPresenter.Content = steamAccountsControl;
                System.Diagnostics.Debug.WriteLine("SteamAccountsControl chargé avec succès");
            }
            catch (Exception ex)
            {
                contentPresenter.Content = new TextBlock()
                {
                    Text = $"Erreur lors du chargement du contrôle : {ex.Message}",
                    Foreground = System.Windows.Media.Brushes.Red,
                    FontSize = 18,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                System.Diagnostics.Debug.WriteLine($"Erreur : {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void btnPlatformVisibility_Click(object sender, RoutedEventArgs e)
        {
            contentPresenter.Content = platformVisibilityControl;
        }

        private void btnFolderPermissions_Click(object sender, RoutedEventArgs e)
        {
            contentPresenter.Content = folderPermissionsControl;
        }

        private void btnAppPaths_Click(object sender, RoutedEventArgs e)
        {
            contentPresenter.Content = pathConfigsControl;
        }

        private void btnProcessRestrictions_Click(object sender, RoutedEventArgs e)
        {
            contentPresenter.Content = processRestrictionsControl;
        }

        private void btnSilentModeSchedule_Click(object sender, RoutedEventArgs e)
        {
            contentPresenter.Content = silentModeScheduleControl;
        }

        /// <summary>
        /// NOUVEAU : Gestionnaire pour le bouton personnalisé
        /// </summary>
        private void btnCustomButton_Click(object sender, RoutedEventArgs e)
        {
            // Recréer le contrôle pour rafraîchir les données
            customButtonControl = new CustomButtonControl();
            contentPresenter.Content = customButtonControl;
        }

        /// <summary>
        /// Rafraîchir l'affichage des permissions de dossiers
        /// </summary>
        public void RefreshFolderPermissionsDisplay()
        {
            try
            {
                if (contentPresenter.Content is FolderPermissionsControl)
                {
                    folderPermissionsControl.RefreshFromExternalChange();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur RefreshFolderPermissionsDisplay: {ex.Message}");
            }
        }
    }
}
