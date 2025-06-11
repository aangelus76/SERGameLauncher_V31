// Panel/Path/PathConfigService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SERGamesLauncher_V31
{
    /// <summary>
    /// Service pour gérer les chemins d'accès aux applications
    /// </summary>
    public static class PathConfigService
    {
        // Chemin vers le fichier de configuration
        private static readonly string ConfigPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Config", "Path.xml");

        /// <summary>
        /// Charge les configurations de chemins depuis le fichier
        /// </summary>
        public static List<PathConfig> LoadPathConfigs()
        {
            List<PathConfig> configs = new List<PathConfig>();

            try
            {
                // S'assurer que le dossier de configuration existe
                string configDirectory = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(configDirectory) && configDirectory != null)
                {
                    Directory.CreateDirectory(configDirectory);
                }

                // Vérifier si le fichier existe
                if (File.Exists(ConfigPath))
                {
                    XDocument doc = XDocument.Load(ConfigPath);

                    // Récupérer les éléments de configuration
                    var paths = doc.Root?.Element("Paths")?.Elements("Path");
                    if (paths != null)
                    {
                        foreach (var path in paths)
                        {
                            PathConfig config = new PathConfig
                            {
                                Id = path.Attribute("id")?.Value ?? Guid.NewGuid().ToString(),
                                PlatformName = path.Element("PlatformName")?.Value ?? string.Empty,
                                Path = path.Element("Path")?.Value ?? string.Empty,
                                IsUrl = bool.Parse(path.Element("IsUrl")?.Value ?? "false"),
                                LaunchArguments = path.Element("LaunchArguments")?.Value ?? string.Empty
                            };

                            configs.Add(config);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // En cas d'erreur, retourner une liste vide
            }

            return configs;
        }

        /// <summary>
        /// Sauvegarde les configurations de chemins dans le fichier
        /// </summary>
        public static bool SavePathConfigs(List<PathConfig> configs)
        {
            try
            {
                // S'assurer que le dossier de configuration existe
                string configDirectory = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(configDirectory) && configDirectory != null)
                {
                    Directory.CreateDirectory(configDirectory);
                }

                XDocument doc;
                XElement root;

                // Vérifier si le fichier existe déjà
                if (File.Exists(ConfigPath))
                {
                    // Si oui, charger le document existant
                    doc = XDocument.Load(ConfigPath);
                    root = doc.Root;

                    // Supprimer l'ancien élément Paths s'il existe
                    root?.Element("Paths")?.Remove();
                }
                else
                {
                    // Sinon, créer un nouveau document
                    doc = new XDocument();
                    root = new XElement("Configuration");
                    doc.Add(root);
                }

                // Créer l'élément Paths
                XElement pathsElement = new XElement("Paths");

                // Ajouter chaque configuration de chemin
                foreach (var config in configs)
                {
                    XElement pathElement = new XElement("Path",
                        new XAttribute("id", config.Id),
                        new XElement("PlatformName", config.PlatformName),
                        new XElement("Path", config.Path),
                        new XElement("IsUrl", config.IsUrl.ToString().ToLower()),
                        new XElement("LaunchArguments", config.LaunchArguments)
                    );

                    pathsElement.Add(pathElement);
                }

                // Ajouter l'élément au document
                root?.Add(pathsElement);

                // Sauvegarder le document
                doc.Save(ConfigPath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Vérifie si un chemin existe déjà pour une plateforme
        /// </summary>
        public static bool PlatformPathExists(List<PathConfig> configs, string platformName, string excludeId = null)
        {
            return configs.Any(c => c.PlatformName.Equals(platformName, StringComparison.OrdinalIgnoreCase)
                                && (excludeId == null || c.Id != excludeId));
        }

        /// <summary>
        /// Obtient le chemin pour une plateforme spécifique
        /// </summary>
        public static PathConfig GetPathForPlatform(List<PathConfig> configs, string platformName)
        {
            return configs.FirstOrDefault(c => c.PlatformName.Equals(platformName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Vérifie si un chemin de fichier existe
        /// </summary>
        public static bool ValidatePath(PathConfig config)
        {
            if (config.IsUrl)
            {
                // Pour les URLs, vérifier simplement qu'elle commence par http ou https
                return config.Path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                       config.Path.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                // Pour les chemins locaux, vérifier que le fichier existe
                return File.Exists(config.Path);
            }
        }
    }
}