using System;
using System.IO;
using System.Xml.Linq;

namespace SERGamesLauncher_V31
{
    /// <summary>
    /// Service pour gérer la configuration du démarrage de l'application
    /// </summary>
    public static class StartupConfigService
    {
        // Chemin vers le fichier de configuration
        private static readonly string ConfigPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Config", "startupConfig.xml");

        /// <summary>
        /// Configuration par défaut
        /// </summary>
        public static class DefaultConfig
        {
            public const int StartupDelaySeconds = 5;
            public const bool EnableStartupDelay = true;
            public const int MaxFocusAttempts = 10;
            public const int FocusRetryIntervalMs = 1000;
        }

        /// <summary>
        /// Classe pour stocker la configuration de démarrage
        /// </summary>
        public class StartupConfig
        {
            public int StartupDelaySeconds { get; set; } = DefaultConfig.StartupDelaySeconds;
            public bool EnableStartupDelay { get; set; } = DefaultConfig.EnableStartupDelay;
            public int MaxFocusAttempts { get; set; } = DefaultConfig.MaxFocusAttempts;
            public int FocusRetryIntervalMs { get; set; } = DefaultConfig.FocusRetryIntervalMs;
        }

        /// <summary>
        /// Charge la configuration de démarrage
        /// </summary>
        public static StartupConfig LoadStartupConfig()
        {
            StartupConfig config = new StartupConfig();

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

                    var startupElement = doc.Root?.Element("Startup");
                    if (startupElement != null)
                    {
                        config.StartupDelaySeconds = int.Parse(startupElement.Element("DelaySeconds")?.Value ?? DefaultConfig.StartupDelaySeconds.ToString());
                        config.EnableStartupDelay = bool.Parse(startupElement.Element("EnableDelay")?.Value ?? DefaultConfig.EnableStartupDelay.ToString());
                        config.MaxFocusAttempts = int.Parse(startupElement.Element("MaxFocusAttempts")?.Value ?? DefaultConfig.MaxFocusAttempts.ToString());
                        config.FocusRetryIntervalMs = int.Parse(startupElement.Element("FocusRetryIntervalMs")?.Value ?? DefaultConfig.FocusRetryIntervalMs.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement de la configuration de démarrage: {ex.Message}");
                // En cas d'erreur, utiliser les valeurs par défaut
            }

            return config;
        }

        /// <summary>
        /// Sauvegarde la configuration de démarrage
        /// </summary>
        public static bool SaveStartupConfig(StartupConfig config)
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
                    doc = XDocument.Load(ConfigPath);
                    root = doc.Root;
                    root?.Element("Startup")?.Remove();
                }
                else
                {
                    doc = new XDocument();
                    root = new XElement("Configuration");
                    doc.Add(root);
                }

                // Créer l'élément de configuration de démarrage
                XElement startupElement = new XElement("Startup",
                    new XElement("DelaySeconds", config.StartupDelaySeconds),
                    new XElement("EnableDelay", config.EnableStartupDelay.ToString().ToLower()),
                    new XElement("MaxFocusAttempts", config.MaxFocusAttempts),
                    new XElement("FocusRetryIntervalMs", config.FocusRetryIntervalMs)
                );

                root?.Add(startupElement);
                doc.Save(ConfigPath);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la sauvegarde de la configuration de démarrage: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Crée une configuration par défaut si elle n'existe pas
        /// </summary>
        public static void EnsureDefaultConfigExists()
        {
            if (!File.Exists(ConfigPath))
            {
                SaveStartupConfig(new StartupConfig());
            }
        }

        /// <summary>
        /// Remet la configuration aux valeurs par défaut
        /// </summary>
        public static bool ResetToDefaults()
        {
            return SaveStartupConfig(new StartupConfig());
        }

        /// <summary>
        /// Vérifie si le fichier de configuration existe
        /// </summary>
        public static bool ConfigFileExists()
        {
            return File.Exists(ConfigPath);
        }

        /// <summary>
        /// Obtient le chemin complet du fichier de configuration
        /// </summary>
        public static string GetConfigFilePath()
        {
            return ConfigPath;
        }
    }
}