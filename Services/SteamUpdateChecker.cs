using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace SERGamesLauncher_V31.Services
{
    /// <summary>
    /// Service de vérification des mises à jour Steam avec système de checksum intelligent
    /// </summary>
    public static class SteamUpdateChecker
    {
        private static readonly string LAST_CHECK_FILE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "steamCheck.json");
        private static readonly HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

        // Liste des AppID à exclure (ajoutez d'autres si nécessaire)
        private static readonly HashSet<string> EXCLUDED_APP_IDS = new HashSet<string>
        {
            "228980"  // Steamworks Common Redistributables
        };

        /// <summary>
        /// Modèle simple pour une mise à jour
        /// </summary>
        public class GameUpdate
        {
            public string AppId { get; set; }
            public string Name { get; set; }
            public string OldBuild { get; set; }
            public string NewBuild { get; set; }
        }

        /// <summary>
        /// Config améliorée avec checksum pour détecter les changements
        /// </summary>
        private class CheckCache
        {
            public DateTime LastCheck { get; set; }
            public List<GameUpdate> Updates { get; set; } = new List<GameUpdate>();
            public string BuildChecksum { get; set; } // Nouveau : checksum des builds installés
            public bool ForceCheckOnNextRun { get; set; } = false; // Nouveau : forcer le check
        }

        /// <summary>
        /// Point d'entrée principal - Système intelligent de cache
        /// </summary>
        public static async Task<List<GameUpdate>> GetUpdatesAsync()
        {
            try
            {
                var cache = LoadCache();

                // 1. Calculer le checksum actuel des jeux installés
                string currentChecksum = await CalculateInstalledGamesChecksumAsync();

                // 2. Déterminer si on doit faire une vérification
                bool shouldCheck = cache.ForceCheckOnNextRun ||
                                 cache.LastCheck.Date < DateTime.Now.Date ||
                                 string.IsNullOrEmpty(cache.BuildChecksum) ||
                                 cache.BuildChecksum != currentChecksum;

                if (!shouldCheck)
                {
                    return cache.Updates;
                }

                // 3. Faire la vérification complète
                var updates = await CheckUpdatesAsync();

                // 4. Sauvegarder le nouveau cache
                SaveCache(new CheckCache
                {
                    LastCheck = DateTime.Now,
                    Updates = updates,
                    BuildChecksum = currentChecksum,
                    ForceCheckOnNextRun = false
                });

                return updates;
            }
            catch
            {
                return new List<GameUpdate>();
            }
        }

        /// <summary>
        /// Calculer un checksum basé sur les builds des jeux installés
        /// </summary>
        private static Task<string> CalculateInstalledGamesChecksumAsync()
        {
            try
            {
                var steamPath = GetSteamPathFromConfig();
                if (string.IsNullOrEmpty(steamPath))
                    return Task.FromResult("NO_STEAM");

                var installedGames = ScanGames(steamPath);

                // Filtrer les AppID exclus et trier pour cohérence
                var relevantGames = installedGames
                    .Where(kvp => !EXCLUDED_APP_IDS.Contains(kvp.Key))
                    .OrderBy(kvp => kvp.Key)
                    .ToList();

                // Créer une chaîne représentative des builds
                var checksumData = string.Join("|", relevantGames.Select(kvp => $"{kvp.Key}:{kvp.Value}"));

                // Calculer le hash SHA256
                using (var sha256 = SHA256.Create())
                {
                    var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(checksumData));
                    return Task.FromResult(Convert.ToBase64String(hash).Substring(0, 16)); // 16 caractères suffisent
                }
            }
            catch
            {
                return Task.FromResult(DateTime.Now.ToString("yyyyMMdd")); // Fallback sur la date
            }
        }

        /// <summary>
        /// Faire la vérification réelle (version optimisée)
        /// </summary>
        private static async Task<List<GameUpdate>> CheckUpdatesAsync()
        {
            var updates = new List<GameUpdate>();

            // 1. Récupérer le chemin Steam
            var steamPath = GetSteamPathFromConfig();
            if (string.IsNullOrEmpty(steamPath))
                return updates;

            // 2. Scanner les jeux installés en excluant les AppID bannis
            var installedGames = ScanGames(steamPath)
                .Where(kvp => !EXCLUDED_APP_IDS.Contains(kvp.Key))
                .Take(30) // Limiter à 30 jeux pour plus de rapidité
                .ToList();

            // 3. Vérifications en parallèle avec limitation
            var semaphore = new SemaphoreSlim(8, 8); // Max 8 requêtes simultanées
            var tasks = installedGames.Select(async game =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var update = await CheckSingleGameAsync(game.Key, game.Value);
                    return update;
                }
                catch
                {
                    return null;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            // 4. Attendre toutes les tâches
            try
            {
                var results = await Task.WhenAll(tasks);
                updates.AddRange(results.Where(r => r != null));
            }
            catch
            {
                // Ignorer les erreurs
            }
            finally
            {
                semaphore.Dispose();
            }
            await Task.CompletedTask;
            return updates;
        }

        /// <summary>
        /// Scanner les jeux en filtrant les exclusions
        /// </summary>
        private static Dictionary<string, string> ScanGames(string steamPath)
        {
            var games = new Dictionary<string, string>();
            var steamappsPaths = new List<string>();

            // Dossier principal Steam
            var mainSteamapps = Path.Combine(steamPath, "steamapps");
            if (Directory.Exists(mainSteamapps))
                steamappsPaths.Add(mainSteamapps);

            // Chercher dans libraryfolders.vdf pour les autres disques
            try
            {
                var libraryFile = Path.Combine(mainSteamapps, "libraryfolders.vdf");
                if (File.Exists(libraryFile))
                {
                    var content = File.ReadAllText(libraryFile);
                    var pathMatches = Regex.Matches(content, @"""path""\s+""([^""]+)""");

                    foreach (Match match in pathMatches)
                    {
                        var libPath = match.Groups[1].Value.Replace("\\\\", "\\");
                        var libSteamapps = Path.Combine(libPath, "steamapps");
                        if (Directory.Exists(libSteamapps) && !steamappsPaths.Contains(libSteamapps))
                            steamappsPaths.Add(libSteamapps);
                    }
                }
            }
            catch { }

            // Scanner tous les dossiers steamapps trouvés
            foreach (var steamappsPath in steamappsPaths)
            {
                try
                {
                    var manifests = Directory.GetFiles(steamappsPath, "appmanifest_*.acf");
                    foreach (var manifest in manifests)
                    {
                        var gameInfo = ParseManifest(manifest);
                        if (gameInfo.HasValue && !EXCLUDED_APP_IDS.Contains(gameInfo.Value.AppId))
                        {
                            games[gameInfo.Value.AppId] = gameInfo.Value.BuildId;
                        }
                    }
                }
                catch { }
            }

            return games;
        }

        /// <summary>
        /// Récupérer le chemin Steam depuis Path.xml existant
        /// </summary>
        private static string GetSteamPathFromConfig()
        {
            try
            {
                var pathConfigs = PathConfigService.LoadPathConfigs();
                var steamConfig = pathConfigs.FirstOrDefault(p =>
                    p.PlatformName.Equals("Steam", StringComparison.OrdinalIgnoreCase));

                if (steamConfig != null && !steamConfig.IsUrl && File.Exists(steamConfig.Path))
                {
                    return Path.GetDirectoryName(steamConfig.Path);
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Parser simple d'un manifest .acf
        /// </summary>
        private static (string AppId, string BuildId)? ParseManifest(string filePath)
        {
            try
            {
                var content = File.ReadAllText(filePath);

                // AppID depuis le nom de fichier
                var appIdMatch = Regex.Match(Path.GetFileName(filePath), @"appmanifest_(\d+)");
                if (!appIdMatch.Success) return null;
                var appId = appIdMatch.Groups[1].Value;

                // BuildID depuis le contenu
                var buildMatch = Regex.Match(content, @"""buildid""\s+""(\d+)""");
                if (!buildMatch.Success) return null;
                var buildId = buildMatch.Groups[1].Value;

                return (appId, buildId);
            }
            catch { return null; }
        }

        /// <summary>
        /// Vérifier un seul jeu via API SteamCMD
        /// </summary>
        private static async Task<GameUpdate> CheckSingleGameAsync(string appId, string currentBuild)
        {
            try
            {
                var url = $"https://api.steamcmd.net/v1/info/{appId}";
                var json = await httpClient.GetStringAsync(url);
                var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("data", out var data) ||
                    !data.TryGetProperty(appId, out var gameData))
                    return null;

                // Nom du jeu
                var gameName = "Jeu inconnu";
                if (gameData.TryGetProperty("common", out var common) &&
                    common.TryGetProperty("name", out var nameEl))
                    gameName = nameEl.GetString();

                // BuildID depuis depots/branches/public/buildid
                var latestBuild = currentBuild;
                if (gameData.TryGetProperty("depots", out var depots) &&
                    depots.TryGetProperty("branches", out var branches) &&
                    branches.TryGetProperty("public", out var publicBranch) &&
                    publicBranch.TryGetProperty("buildid", out var buildEl))
                {
                    latestBuild = buildEl.GetString();
                }

                // Si différent = mise à jour disponible
                if (latestBuild != currentBuild)
                {
                    return new GameUpdate
                    {
                        AppId = appId,
                        Name = gameName,
                        OldBuild = currentBuild,
                        NewBuild = latestBuild
                    };
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Forcer une nouvelle vérification au prochain appel
        /// </summary>
        public static void InvalidateCache()
        {
            try
            {
                var cache = LoadCache();
                cache.ForceCheckOnNextRun = true;
                SaveCache(cache);
            }
            catch { }
        }

        /// <summary>
        /// Forcer une nouvelle vérification immédiate (ignorer le cache)
        /// </summary>
        public static async Task<List<GameUpdate>> ForceCheckAsync()
        {
            try
            {
                if (File.Exists(LAST_CHECK_FILE))
                    File.Delete(LAST_CHECK_FILE);
            }
            catch { }

            return await GetUpdatesAsync();
        }

        /// <summary>
        /// Ajouter ou retirer un AppID des exclusions
        /// </summary>
        public static void AddExcludedAppId(string appId)
        {
            EXCLUDED_APP_IDS.Add(appId);
            InvalidateCache(); // Forcer une nouvelle vérification
        }

        public static void RemoveExcludedAppId(string appId)
        {
            EXCLUDED_APP_IDS.Remove(appId);
            InvalidateCache(); // Forcer une nouvelle vérification
        }

        /// <summary>
        /// Charger le cache
        /// </summary>
        private static CheckCache LoadCache()
        {
            try
            {
                if (File.Exists(LAST_CHECK_FILE))
                {
                    var json = File.ReadAllText(LAST_CHECK_FILE);
                    return JsonSerializer.Deserialize<CheckCache>(json) ?? new CheckCache();
                }
            }
            catch { }

            return new CheckCache();
        }

        /// <summary>
        /// Sauvegarder le cache
        /// </summary>
        private static void SaveCache(CheckCache cache)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LAST_CHECK_FILE));
                var json = JsonSerializer.Serialize(cache, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(LAST_CHECK_FILE, json);
            }
            catch { }
        }

        /// <summary>
        /// Nettoyer les ressources
        /// </summary>
        public static void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}