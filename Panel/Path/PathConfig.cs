// Panel/Path/PathConfig.cs
using System;

namespace SERGamesLauncher_V31
{
    /// <summary>
    /// Représente un chemin d'application pour une plateforme spécifique
    /// </summary>
    public class PathConfig
    {
        /// <summary>
        /// Identifiant unique de la configuration de chemin
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Nom de la plateforme (Steam, Epic, etc.)
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;

        /// <summary>
        /// Chemin d'accès à l'exécutable ou URL
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Indique si c'est une URL (true) ou un chemin d'accès local (false)
        /// </summary>
        public bool IsUrl { get; set; } = false;

        /// <summary>
        /// Arguments de lancement (optionnel)
        /// </summary>
        public string LaunchArguments { get; set; } = string.Empty;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public PathConfig() { }

        /// <summary>
        /// Constructeur avec les propriétés principales
        /// </summary>
        public PathConfig(string platformName, string path, bool isUrl, string launchArguments = "")
        {
            PlatformName = platformName;
            Path = path;
            IsUrl = isUrl;
            LaunchArguments = launchArguments;
        }
    }
}