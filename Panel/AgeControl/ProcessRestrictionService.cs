using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SERGamesLauncher_V31
{
    public static class ProcessRestrictionService
    {
        // Chemin vers le fichier de configuration
        private static readonly string ConfigPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Config", "ProcessRestrictions.xml");

        public static List<ProcessRestriction> LoadProcessRestrictions()
        {
            List<ProcessRestriction> restrictions = new List<ProcessRestriction>();

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
                    var processes = doc.Root?.Element("Processes")?.Elements("Process");
                    if (processes != null)
                    {
                        foreach (var process in processes)
                        {
                            ProcessRestriction restriction = new ProcessRestriction
                            {
                                Id = process.Attribute("id")?.Value ?? Guid.NewGuid().ToString(),
                                ProcessName = process.Element("ProcessName")?.Value ?? string.Empty,
                                MinimumAge = int.Parse(process.Element("MinimumAge")?.Value ?? "0"),
                                Description = process.Element("Description")?.Value ?? string.Empty,
                                IsActive = bool.Parse(process.Element("IsActive")?.Value ?? "true")
                            };

                            restrictions.Add(restriction);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // En cas d'erreur, retourner une liste vide
            }

            return restrictions;
        }

        public static bool SaveProcessRestrictions(List<ProcessRestriction> restrictions)
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

                    // Supprimer l'ancien élément Processes s'il existe
                    root?.Element("Processes")?.Remove();
                }
                else
                {
                    // Sinon, créer un nouveau document
                    doc = new XDocument();
                    root = new XElement("Configuration");
                    doc.Add(root);
                }

                // Créer l'élément Processes
                XElement processesElement = new XElement("Processes");

                // Ajouter chaque configuration de restriction
                foreach (var restriction in restrictions)
                {
                    XElement processElement = new XElement("Process",
                        new XAttribute("id", restriction.Id),
                        new XElement("ProcessName", restriction.ProcessName),
                        new XElement("MinimumAge", restriction.MinimumAge),
                        new XElement("Description", restriction.Description),
                        new XElement("IsActive", restriction.IsActive.ToString().ToLower())
                    );

                    processesElement.Add(processElement);
                }

                // Ajouter l'élément au document
                root?.Add(processesElement);

                // Sauvegarder le document
                doc.Save(ConfigPath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool ProcessNameExists(List<ProcessRestriction> restrictions, string processName, string excludeId = null)
        {
            return restrictions.Any(r => r.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase)
                                && (excludeId == null || r.Id != excludeId));
        }
    }
}