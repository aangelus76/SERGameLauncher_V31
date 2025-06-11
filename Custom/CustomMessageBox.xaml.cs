using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SERGamesLauncher_V31
{
    public partial class CustomMessageBox : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        // Dimensions personnalisées pour les boutons
        private double ButtonWidth { get; set; } = 120;
        private double ButtonHeight { get; set; } = 40;

        // Constructeur privé - utiliser les méthodes statiques au lieu de l'instancier directement
        private CustomMessageBox()
        {
            InitializeComponent();

            // Permettre le déplacement de la fenêtre sans bordure
            this.MouseLeftButtonDown += (s, e) => this.DragMove();
        }

        // Constructeur avec paramètres de taille personnalisée
        private CustomMessageBox(double width, double height, double buttonWidth, double buttonHeight)
        {
            InitializeComponent();

            // Appliquer les dimensions personnalisées
            this.Width = width;
            this.Height = height;

            // Stocker les dimensions des boutons pour une utilisation ultérieure
            this.ButtonWidth = buttonWidth;
            this.ButtonHeight = buttonHeight;

            // Permettre le déplacement de la fenêtre sans bordure
            this.MouseLeftButtonDown += (s, e) => this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            // Si on ferme avec le X, on considère que c'est Cancel/Non
            if (Result == MessageBoxResult.None)
                Result = MessageBoxResult.Cancel;
            this.Close();
        }

        // Configure la boîte de dialogue en fonction des paramètres
        private void ConfigureDialog(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            // Définir le message et le titre
            messageTextBlock.Text = message;
            titleTextBlock.Text = title;

            // Configurer l'icône
            string iconPath = "";
            switch (icon)
            {
                case MessageBoxImage.Error:
                    iconPath = "/SERGamesLauncher_V31;component/Images/IconError.png";
                    this.Background = new SolidColorBrush(Color.FromRgb(0x47, 0x27, 0x27)); // Fond rouge foncé pour erreur
                    break;
                case MessageBoxImage.Warning:
                    iconPath = "/SERGamesLauncher_V31;component/Images/IconWarning.png";
                    break;
                case MessageBoxImage.Information:
                    iconPath = "/SERGamesLauncher_V31;component/Images/IconInfo.png";
                    break;
                case MessageBoxImage.Question:
                    iconPath = "/SERGamesLauncher_V31;component/Images/IconQuestion.png";
                    break;
                default:
                    iconPath = "/SERGamesLauncher_V31;component/Images/IconeLauncher.ico";
                    break;
            }

            try
            {
                iconImage.Source = new BitmapImage(new Uri(iconPath, UriKind.Relative));
            }
            catch
            {
                // En cas d'erreur, utiliser l'icône par défaut
                iconImage.Source = new BitmapImage(new Uri("/SERGamesLauncher_V31;component/Images/IconeLauncher.ico", UriKind.Relative));
            }

            // Configurer les boutons
            ConfigureButtons(buttons);
        }

        // Configure les boutons selon le type demandé
        private void ConfigureButtons(MessageBoxButton buttons)
        {
            buttonGrid.Children.Clear();
            buttonGrid.ColumnDefinitions.Clear();

            // Définir les colonnes selon le nombre de boutons
            switch (buttons)
            {
                case MessageBoxButton.OK:
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    AddButton("OK", MessageBoxResult.OK, 0);
                    break;
                case MessageBoxButton.OKCancel:
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    AddButton("Annuler", MessageBoxResult.Cancel, 0);
                    AddButton("OK", MessageBoxResult.OK, 1);
                    break;
                case MessageBoxButton.YesNo:
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    AddButton("Non", MessageBoxResult.No, 0);
                    AddButton("Oui", MessageBoxResult.Yes, 1);
                    break;
                case MessageBoxButton.YesNoCancel:
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    AddButton("Annuler", MessageBoxResult.Cancel, 0);
                    AddButton("Non", MessageBoxResult.No, 1);
                    AddButton("Oui", MessageBoxResult.Yes, 2);
                    break;
                default:
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    AddButton("OK", MessageBoxResult.OK, 0);
                    break;
            }
        }

        // Ajoute un bouton à la grille
        private void AddButton(string text, MessageBoxResult result, int column)
        {
            Button button = new Button
            {
                Content = text,
                Style = (Style)FindResource("LaunchButtonStyle"),
                Width = ButtonWidth,
                Height = ButtonHeight,
                Margin = new Thickness(5)
            };

            button.Click += (s, e) =>
            {
                Result = result;
                this.Close();
            };

            // Ajouter à la grille
            Grid.SetColumn(button, column);
            buttonGrid.Children.Add(button);
        }

        // Méthodes statiques pour afficher la boîte de dialogue (remplace MessageBox.Show)

        public static MessageBoxResult Show(string message)
        {
            return Show(message, "Message", MessageBoxButton.OK, MessageBoxImage.None);
        }

        public static MessageBoxResult Show(string message, string title)
        {
            return Show(message, title, MessageBoxButton.OK, MessageBoxImage.None);
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons)
        {
            return Show(message, title, buttons, MessageBoxImage.None);
        }

        public static MessageBoxResult Show(Window owner, string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            CustomMessageBox msgBox = new CustomMessageBox();
            msgBox.ConfigureDialog(message, title, buttons, icon);
            msgBox.Owner = owner;
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            CustomMessageBox msgBox = new CustomMessageBox();
            msgBox.ConfigureDialog(message, title, buttons, icon);
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        // Méthodes avec paramètres de taille personnalisée
        public static MessageBoxResult Show(Window owner, string message, string title, MessageBoxButton buttons,
                                           MessageBoxImage icon, double width, double height)
        {
            CustomMessageBox msgBox = new CustomMessageBox(width, height, 120, 40);
            msgBox.ConfigureDialog(message, title, buttons, icon);
            msgBox.Owner = owner;
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        public static MessageBoxResult Show(Window owner, string message, string title, MessageBoxButton buttons,
                                           MessageBoxImage icon, double width, double height,
                                           double buttonWidth, double buttonHeight)
        {
            CustomMessageBox msgBox = new CustomMessageBox(width, height, buttonWidth, buttonHeight);
            msgBox.ConfigureDialog(message, title, buttons, icon);
            msgBox.Owner = owner;
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons,
                                           MessageBoxImage icon, double width, double height)
        {
            CustomMessageBox msgBox = new CustomMessageBox(width, height, 120, 40);
            msgBox.ConfigureDialog(message, title, buttons, icon);
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons,
                                           MessageBoxImage icon, double width, double height,
                                           double buttonWidth, double buttonHeight)
        {
            CustomMessageBox msgBox = new CustomMessageBox(width, height, buttonWidth, buttonHeight);
            msgBox.ConfigureDialog(message, title, buttons, icon);
            msgBox.ShowDialog();
            return msgBox.Result;
        }
    }
}