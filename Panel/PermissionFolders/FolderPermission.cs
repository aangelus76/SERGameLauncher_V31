// Panel/Folder/FolderPermission.cs
using System;

namespace SERGamesLauncher_V31
{
    /// <summary>
    /// Représente un dossier avec des permissions contrôlées
    /// </summary>
    public class FolderPermission
    {
        /// <summary>
        /// Identifiant unique de la configuration de permission
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Nom descriptif pour ce dossier contrôlé
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Chemin d'accès complet au dossier
        /// </summary>
        public string FolderPath { get; set; } = string.Empty;

        /// <summary>
        /// Indique si la protection est actuellement active
        /// </summary>
        public bool IsProtectionEnabled { get; set; } = true;

        /// <summary>
        /// Indique si la protection doit être activée au démarrage
        /// </summary>
        public bool EnableOnStartup { get; set; } = true;

        /// <summary>
        /// Niveau de protection (Read, ReadWrite)
        /// </summary>
        public ProtectionLevel ProtectionLevel { get; set; } = ProtectionLevel.ReadOnly;

        /// <summary>
        /// Date de la dernière modification des permissions
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.Now;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public FolderPermission() { }

        /// <summary>
        /// Constructeur avec les propriétés principales
        /// </summary>
        public FolderPermission(string name, string folderPath, bool enableOnStartup = true, ProtectionLevel protectionLevel = ProtectionLevel.ReadOnly)
        {
            Name = name;
            FolderPath = folderPath;
            EnableOnStartup = enableOnStartup;
            ProtectionLevel = protectionLevel;
            IsProtectionEnabled = enableOnStartup;
        }
    }

    /// <summary>
    /// Niveau de protection pour les dossiers
    /// </summary>
    public enum ProtectionLevel
    {
        /// <summary>
        /// Lecture seule (bloque l'écriture, la suppression et la modification)
        /// </summary>
        ReadOnly,

        /// <summary>
        /// Bloque uniquement la suppression de fichiers/dossiers
        /// </summary>
        PreventDeletion,

        /// <summary>
        /// Bloque la création de nouveaux fichiers/dossiers
        /// </summary>
        PreventCreation
    }
}