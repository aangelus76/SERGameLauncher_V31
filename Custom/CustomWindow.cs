// CustomWindow.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SERGamesLauncher_V31
{
    public class CustomWindow : Window
    {
        public CustomWindow()
        {
            Style = (Style)FindResource("CustomWindowStyle");

            // Pour permettre le déplacement de la fenêtre sans bordures
            this.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    this.DragMove();
            };

            // Connecter les boutons après le chargement
            this.Loaded += CustomWindow_Loaded;
        }

        private void CustomWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Trouver les boutons dans le template
            Button infoButton = this.Template.FindName("InfoButton", this) as Button;
            Button configButton = this.Template.FindName("ConfigButton", this) as Button;
            Button minimizeButton = this.Template.FindName("MinimizeButton", this) as Button;
            Button closeButton = this.Template.FindName("CloseButton", this) as Button;

            // Attacher les événements
            if (infoButton != null)
            {
                infoButton.Click -= InfoButton_Click; // Éviter les doublons
                infoButton.Click += InfoButton_Click;
            }

            if (configButton != null)
            {
                configButton.Click -= ConfigButton_Click; // Éviter les doublons
                configButton.Click += ConfigButton_Click;
            }

            if (minimizeButton != null)
            {
                minimizeButton.Click -= MinimizeButton_Click; // Éviter les doublons
                minimizeButton.Click += MinimizeButton_Click;
            }

            if (closeButton != null)
            {
                closeButton.Click -= CloseButton_Click; // Éviter les doublons
                closeButton.Click += CloseButton_Click;
            }
        }

        // Événements des boutons
        protected virtual void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            // Afficher les informations personnalisées sur le programme
            // Utiliser une fenêtre plus large (450px) avec un bouton plus large (150px)
            CustomMessageBox.Show(this,
                "Créateur : Belcombe\n" +
                "Version : 3\n" +
                "Build : W18.25.C\n" +
                "Distribution : Exclusivement pour Saint-Étienne-du-Rouvray",
                "À propos de SER-Games Launcher",
                MessageBoxButton.OK, MessageBoxImage.Information,
                450, 250, 150, 30);
        }

        /// <summary>
        /// Ouvre le panneau d'administration après authentification
        /// </summary>
        protected virtual void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Afficher la boîte de dialogue de mot de passe
                PasswordDialog passwordDialog = new PasswordDialog();
                passwordDialog.Owner = this;
                passwordDialog.CustomTitle = "Authentification Admin"; // Titre standard
                passwordDialog.DialogMessage = "Veuillez entrer le mot de passe administrateur";
                passwordDialog.ShowDialog();

                // Si authentification réussie, ouvrir le panneau d'administration
                if (passwordDialog.IsAuthenticated)
                {
                    try
                    {
                        AdminPanelWindow adminPanel = new AdminPanelWindow();
                        adminPanel.Owner = this;

                        // À implémenter : mécanisme de rafraîchissement de la visibilité des plateformes
                        // après la fermeture du panneau d'administration
                        adminPanel.Closed += (s, args) =>
                        {
                            // Si cette fenêtre est la fenêtre principale, mettre à jour la visibilité des plateformes
                            if (this is MainWindow mainWindow)
                            {
                                mainWindow.ApplyPlatformVisibility();
                            }
                        };

                        adminPanel.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de l'ouverture du panneau d'administration : {ex.Message}\n\nStack trace : {ex.StackTrace}",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur générale : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        protected virtual void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Afficher la boîte de dialogue de mot de passe
                PasswordDialog passwordDialog = new PasswordDialog();
                passwordDialog.Owner = this;
                passwordDialog.CustomTitle = "Authentification pour fermeture"; // Titre personnalisé
                passwordDialog.DialogMessage = "Mot de passe requis pour fermer l'application";
                passwordDialog.ShowDialog();

                // Si authentification réussie, fermer l'application
                if (passwordDialog.IsAuthenticated)
                {
                    this.Close();
                }
                // Si non authentifié, ne rien faire (le programme reste ouvert)
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(this, "Mot de passe incorrect!", "Erreur d'authentification", MessageBoxButton.OK, MessageBoxImage.Error);
                //MessageBox.Show($"Erreur lors de la tentative de fermeture : {ex.Message}",
                ///"Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}