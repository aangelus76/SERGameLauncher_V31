// ShortcutUtility.cs
using System;
using System.IO;
using IWshRuntimeLibrary;
using System.Reflection;
using System.Diagnostics;

namespace SERGamesLauncher_V31
{
    public static class ShortcutUtility
    {
        /// <summary>
        /// Vérifie si le raccourci du programme existe sur le bureau et le crée si nécessaire
        /// </summary>
        public static void EnsureDesktopShortcutExists()
        {
            try
            {
                // Obtenir le chemin du bureau de l'utilisateur actuel
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                // Nom du raccourci
                string shortcutName = "SER-Games Launcher.lnk";

                // Chemin complet du raccourci
                string shortcutPath = Path.Combine(desktopPath, shortcutName);

                // Vérifier si le raccourci existe déjà - utiliser System.IO.File explicitement
                if (!System.IO.File.Exists(shortcutPath))
                {
                    // Obtenir le chemin de l'exécutable en cours
                    string exePath = Assembly.GetExecutingAssembly().Location;

                    // Créer le raccourci
                    CreateShortcut(shortcutPath, exePath, "Lanceur centralisé de plateformes de jeux");

                    Debug.WriteLine($"Raccourci créé sur le bureau: {shortcutPath}");
                }
                else
                {
                    Debug.WriteLine("Le raccourci existe déjà sur le bureau");
                }
            }
            catch (Exception ex)
            {
                // Journaliser l'erreur mais ne pas interrompre le démarrage de l'application
                Debug.WriteLine($"Erreur lors de la création du raccourci: {ex.Message}");
            }
        }

        /// <summary>
        /// Crée un raccourci Windows (.lnk)
        /// </summary>
        private static void CreateShortcut(string shortcutPath, string targetPath, string description)
        {
            // Créer un objet Shell
            WshShell shell = new WshShell();

            // Créer le raccourci
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

            // Définir les propriétés du raccourci
            shortcut.TargetPath = targetPath;
            shortcut.Description = description;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);

            // Définir l'icône (utiliser l'icône de l'exécutable)
            shortcut.IconLocation = targetPath + ",0";

            // Enregistrer le raccourci
            shortcut.Save();
        }
    }
}