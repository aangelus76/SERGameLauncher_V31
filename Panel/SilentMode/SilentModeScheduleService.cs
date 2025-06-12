using System;
using System.IO;
using System.Text.Json;

namespace SERGamesLauncher_V31
{
    /// <summary>
    /// Service pour gérer la configuration du planning silencieux
    /// </summary>
    public static class SilentModeScheduleService
    {
        // Chemin vers le fichier de configuration
        private static readonly string ConfigPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Config", "silentModeSchedule.json");

        /// <summary>
        /// Charge la configuration du planning silencieux
        /// </summary>
        public static SilentModeSchedule LoadSchedule()
        {
            SilentModeSchedule schedule = new SilentModeSchedule();

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
                    // Lire le contenu du fichier JSON
                    string jsonContent = File.ReadAllText(ConfigPath);

                    // Désérialiser le JSON
                    if (!string.IsNullOrEmpty(jsonContent))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            WriteIndented = true
                        };

                        schedule = JsonSerializer.Deserialize<SilentModeSchedule>(jsonContent, options) ?? new SilentModeSchedule();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement du planning silencieux: {ex.Message}");
                // En cas d'erreur, retourner une configuration par défaut
            }

            return schedule;
        }

        /// <summary>
        /// Sauvegarde la configuration du planning silencieux
        /// </summary>
        public static bool SaveSchedule(SilentModeSchedule schedule)
        {
            try
            {
                // S'assurer que le dossier de configuration existe
                string configDirectory = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(configDirectory) && configDirectory != null)
                {
                    Directory.CreateDirectory(configDirectory);
                }

                // Sérialiser en JSON avec indentation
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string jsonContent = JsonSerializer.Serialize(schedule, options);

                // Écrire dans le fichier
                File.WriteAllText(ConfigPath, jsonContent);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la sauvegarde du planning silencieux: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Valide et corrige une heure pour la plage après-midi
        /// </summary>
        public static string ValidateAfternoonTime(string time, bool isEndTime)
        {
            // Valider le format d'heure
            if (!IsValidTimeFormat(time))
            {
                return isEndTime ? "00:00" : "13:00";
            }

            if (!TimeSpan.TryParse(time, out TimeSpan timeSpan))
            {
                return isEndTime ? "00:00" : "13:00";
            }

            // Limites pour l'après-midi : 13:00 - 00:00 (minuit)
            TimeSpan minAfternoon = new TimeSpan(13, 0, 0);
            TimeSpan maxAfternoon = new TimeSpan(0, 0, 0); // Minuit

            // Pour l'heure de début - FORCER minimum 13:00
            if (!isEndTime)
            {
                if (timeSpan < minAfternoon)
                {
                    return "13:00";
                }
                // Pour le début, éviter 00:00
                if (timeSpan == maxAfternoon)
                {
                    return "13:00";
                }
            }
            else
            {
                // Pour l'heure de fin
                if (timeSpan < minAfternoon && timeSpan != maxAfternoon)
                {
                    return "00:00";
                }
            }

            return timeSpan.ToString(@"hh\:mm");
        }

        /// <summary>
        /// Valide et corrige une heure pour la plage matin
        /// </summary>
        public static string ValidateMorningTime(string time, bool isEndTime)
        {
            // Valider le format d'heure
            if (!IsValidTimeFormat(time))
            {
                return isEndTime ? "13:00" : "08:00";
            }

            if (!TimeSpan.TryParse(time, out TimeSpan timeSpan))
            {
                return isEndTime ? "13:00" : "08:00";
            }

            // Limites pour le matin : 08:00 - 13:00
            TimeSpan minMorning = new TimeSpan(8, 0, 0);
            TimeSpan maxMorning = new TimeSpan(13, 0, 0);

            if (timeSpan < minMorning)
            {
                return "08:00";
            }
            else if (timeSpan > maxMorning)
            {
                return "13:00";
            }

            return timeSpan.ToString(@"hh\:mm");
        }

        /// <summary>
        /// Vérifie si le format d'heure est valide (HH:MM avec heures 0-23 et minutes 0-59)
        /// </summary>
        private static bool IsValidTimeFormat(string time)
        {
            if (string.IsNullOrWhiteSpace(time) || time.Length != 5 || time[2] != ':')
                return false;

            if (!int.TryParse(time.Substring(0, 2), out int hours) || hours < 0 || hours > 23)
                return false;

            if (!int.TryParse(time.Substring(3, 2), out int minutes) || minutes < 0 || minutes > 59)
                return false;

            return true;
        }

        /// <summary>
        /// Vérifie si le fichier de configuration existe
        /// </summary>
        public static bool ConfigFileExists()
        {
            return File.Exists(ConfigPath);
        }

        /// <summary>
        /// Valide la cohérence d'une plage horaire (début < fin)
        /// </summary>
        public static (string start, string end) ValidateTimeRange(string startTime, string endTime, bool isMorning)
        {
            // D'abord valider individuellement
            if (isMorning)
            {
                startTime = ValidateMorningTime(startTime, false);
                endTime = ValidateMorningTime(endTime, true);
            }
            else
            {
                startTime = ValidateAfternoonTime(startTime, false);
                endTime = ValidateAfternoonTime(endTime, true);
            }

            // Ensuite vérifier la cohérence
            if (TimeSpan.TryParse(startTime, out TimeSpan start) && TimeSpan.TryParse(endTime, out TimeSpan end))
            {
                // Cas spécial pour après-midi avec fin à 00:00 (minuit)
                if (!isMorning && end == TimeSpan.Zero)
                {
                    // 00:00 représente minuit, donc OK
                    return (startTime, endTime);
                }

                // Vérifier que début < fin
                if (start >= end)
                {
                    // Corriger automatiquement
                    if (isMorning)
                    {
                        return ("08:00", "13:00");
                    }
                    else
                    {
                        return ("13:00", "00:00");
                    }
                }
            }

            return (startTime, endTime);
        }

        /// <summary>
        /// Obtient le chemin complet du fichier de configuration
        /// </summary>
        public static string GetConfigFilePath()
        {
            return ConfigPath;
        }

        /// <summary>
        /// Crée une configuration par défaut si elle n'existe pas
        /// </summary>
        public static void EnsureDefaultConfigExists()
        {
            if (!File.Exists(ConfigPath))
            {
                SaveSchedule(new SilentModeSchedule());
            }
        }

        /// <summary>
        /// Remet la configuration aux valeurs par défaut
        /// </summary>
        public static bool ResetToDefaults()
        {
            return SaveSchedule(new SilentModeSchedule());
        }

        /// <summary>
        /// Vérifie si l'heure actuelle correspond au mode silencieux
        /// </summary>
        public static bool IsCurrentlyInSilentMode()
        {
            try
            {
                SilentModeSchedule schedule = LoadSchedule();
                return schedule.IsInSilentMode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la vérification du mode silencieux: {ex.Message}");
                return false; // En cas d'erreur, mode normal
            }
        }
    }
}