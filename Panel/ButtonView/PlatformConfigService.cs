using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace SERGamesLauncher_V31.Panel
{
    /// <summary>
    /// Service de gestion de la configuration des plateformes
    /// </summary>
    public static class PlatformConfigService
    {
        // Chemin vers le fichier de configuration
        private static readonly string ConfigPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Config", "platformConfig.xml");

        // Nom par défaut des plateformes
        private static readonly List<string> DefaultPlatforms = new List<string>
        {
            "Steam", "Epic", "Crazy", "Roblox", "BGA", "Xbox"
        };

        /// <summary>
        /// Charge la configuration de visibilité des plateformes
        /// </summary>
        /// <returns>Dictionnaire contenant la visibilité de chaque plateforme</returns>
        public static Dictionary<string, bool> LoadPlatformVisibility()
        {
            // Initialiser avec les valeurs par défaut (toutes visibles)
            Dictionary<string, bool> platformVisibility = new Dictionary<string, bool>();

            foreach (string platform in DefaultPlatforms)
            {
                platformVisibility[platform] = true;
            }

            try
            {
                // S'assurer que le dossier de configuration existe
                string configDirectory = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(configDirectory) && configDirectory != null)
                {
                    Directory.CreateDirectory(configDirectory);
                }

                if (File.Exists(ConfigPath))
                {
                    XDocument doc = XDocument.Load(ConfigPath);

                    // Récupérer les éléments de visibilité
                    var platforms = doc.Root?.Element("PlatformVisibility");
                    if (platforms != null)
                    {
                        foreach (var platform in platforms.Elements("Platform"))
                        {
                            string name = platform.Attribute("name")?.Value;
                            bool isVisible = bool.Parse(platform.Attribute("visible")?.Value ?? "true");

                            if (!string.IsNullOrEmpty(name) && platformVisibility.ContainsKey(name))
                            {
                                platformVisibility[name] = isVisible;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // En cas d'erreur, utiliser les valeurs par défaut (ne pas afficher de message)
            }

            return platformVisibility;
        }

        /// <summary>
        /// Sauvegarde la configuration de visibilité des plateformes
        /// </summary>
        /// <param name="platformVisibility">Dictionnaire contenant la visibilité de chaque plateforme</param>
        /// <returns>True si la sauvegarde a réussi, sinon False</returns>
        public static bool SavePlatformVisibility(Dictionary<string, bool> platformVisibility)
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

                    // Supprimer l'ancien élément de visibilité s'il existe
                    root?.Element("PlatformVisibility")?.Remove();
                }
                else
                {
                    // Sinon, créer un nouveau document
                    doc = new XDocument();
                    root = new XElement("Configuration");
                    doc.Add(root);
                }

                // Créer l'élément de visibilité
                XElement platformsElement = new XElement("PlatformVisibility");

                // Ajouter chaque plateforme
                foreach (var platform in platformVisibility)
                {
                    platformsElement.Add(new XElement("Platform",
                        new XAttribute("name", platform.Key),
                        new XAttribute("visible", platform.Value.ToString().ToLower())
                    ));
                }

                // Ajouter l'élément au document
                root?.Add(platformsElement);

                // Sauvegarder le document
                doc.Save(ConfigPath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}