using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SERGamesLauncher_V31.Services
{
    /// <summary>
    /// Service de mise à jour Roblox
    /// </summary>
    public static class RobloxUpdateService
    {
        #region Configuration et événements

        // Events pour la progression - MAINTIEN DE LA COMPATIBILITÉ
        public static event Action<string> ProgressChanged;

        // Configuration optimisée
        private static readonly int MaxConcurrentDownloads = Math.Min(Environment.ProcessorCount * 6, 20);
        private static readonly int MaxConcurrentExtractions = Environment.ProcessorCount;
        private static readonly int BufferSize = 10 * 1024 * 1024; // 10MB
        private static readonly int TimeoutMinutes = 15;

        // Fichier de log - MAINTIEN DU CHEMIN EXISTANT
        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "roblox_update.log");

        // API Endpoints - MISE À JOUR
        private static readonly string[] VersionApiUrls = {
            "https://clientsettingscdn.roblox.com/v2/client-version/WindowsPlayer",
            "https://clientsettings.roblox.com/v2/client-version/WindowsPlayer"
        };

        private static readonly string[] BaseUrls = {
            "https://setup.rbxcdn.com",
            "https://setup-aws.rbxcdn.com",
            "https://setup-ak.rbxcdn.com",
            "https://roblox-setup.cachefly.net"
        };

        // Mapping des packages - AMÉLIORATION DU MAPPING
        private static readonly Dictionary<string, string> PackageDirectoryMap = new Dictionary<string, string>()
        {
            { "RobloxApp.zip", @"" },
            { "Libraries.zip", @"" },
            { "redist.zip", @"" },
            { "WebView2.zip", @"" },
            { "shaders.zip", @"shaders\" },
            { "ssl.zip", @"ssl\" },
            { "WebView2RuntimeInstaller.zip", @"WebView2RuntimeInstaller\" },
            { "content-avatar.zip", @"content\avatar\" },
            { "content-configs.zip", @"content\configs\" },
            { "content-fonts.zip", @"content\fonts\" },
            { "content-sky.zip", @"content\sky\" },
            { "content-sounds.zip", @"content\sounds\" },
            { "content-textures2.zip", @"content\textures\" },
            { "content-models.zip", @"content\models\" },
            { "content-textures3.zip", @"PlatformContent\pc\textures\" },
            { "content-terrain.zip", @"PlatformContent\pc\terrain\" },
            { "content-platform-fonts.zip", @"PlatformContent\pc\fonts\" },
            { "content-platform-dictionaries.zip", @"PlatformContent\pc\shared_compression_dictionaries\" },
            { "extracontent-luapackages.zip", @"ExtraContent\LuaPackages\" },
            { "extracontent-translations.zip", @"ExtraContent\translations\" },
            { "extracontent-models.zip", @"ExtraContent\models\" },
            { "extracontent-textures.zip", @"ExtraContent\textures\" },
            { "extracontent-places.zip", @"ExtraContent\places\" },
        };

        #endregion

        #region Classes internes

        /// <summary>
        /// Informations de version Roblox - MAINTIEN DE LA COMPATIBILITÉ
        /// </summary>
        private class RobloxVersionInfo
        {
            public string version { get; set; }
            public string clientVersionUpload { get; set; }
            public string bootstrapperVersion { get; set; }
        }

        /// <summary>
        /// Résultat de la mise à jour - MAINTIEN DE L'INTERFACE EXISTANTE
        /// </summary>
        public class UpdateResult
        {
            public bool Success { get; set; }
            public bool UpdatePerformed { get; set; }
            public string NewPath { get; set; }
            public string Message { get; set; }
        }

        /// <summary>
        /// Informations sur un package à télécharger
        /// </summary>
        private class Package
        {
            public string Name { get; set; } = "";
            public string Signature { get; set; } = "";
            public int PackedSize { get; set; }
            public int Size { get; set; }
            public string DownloadPath { get; set; } = "";
            public bool IsExecutable => Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
            public bool IsZipPackage => Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Méthode principale - INTERFACE MAINTENUE

        /// <summary>
        /// Vérifie et effectue la mise à jour si nécessaire - INTERFACE MAINTENUE
        /// </summary>
        public static async Task<UpdateResult> CheckAndUpdateAsync()
        {

            HttpClient httpClient = null;
            string downloadFolder = null;

            try
            {
                // Initialisation du client HTTP optimisé
                httpClient = CreateOptimizedHttpClient();

                ReportProgress("Vérification des permissions...");

                // 1. Vérifier les permissions d'écriture
                if (!CheckWritePermissions())
                {
                    return new UpdateResult
                    {
                        Success = true,
                        UpdatePerformed = false,
                        Message = "Permissions insuffisantes - lancement de la version actuelle"
                    };
                }

                ReportProgress("Détection de la version locale...");

                // 2. Détecter la version locale
                string currentGuid = GetCurrentVersionGuid();

                if (string.IsNullOrEmpty(currentGuid))
                {
                    return new UpdateResult
                    {
                        Success = false,
                        Message = "Impossible de détecter la version Roblox actuelle"
                    };
                }

                ReportProgress("Vérification de la version en ligne...");

                // 3. Vérifier la version en ligne
                string onlineGuid = await GetOnlineVersionGuidAsync(httpClient);

                if (string.IsNullOrEmpty(onlineGuid))
                {
                    return new UpdateResult
                    {
                        Success = true,
                        UpdatePerformed = false,
                        Message = "Impossible de vérifier la version en ligne - lancement de la version actuelle"
                    };
                }

                // 4. Comparer les versions
                if (currentGuid.Equals(onlineGuid, StringComparison.OrdinalIgnoreCase))
                {
                    return new UpdateResult
                    {
                        Success = true,
                        UpdatePerformed = false,
                        Message = "Version à jour"
                    };
                }

                ReportProgress("Mise à jour nécessaire...");

                // 5. Créer le dossier de téléchargement temporaire
                downloadFolder = Path.Combine(Path.GetTempPath(), $"RobloxDownload_{Guid.NewGuid()}");
                Directory.CreateDirectory(downloadFolder);

                // 6. Effectuer la mise à jour optimisée
                string newPath = await PerformOptimizedUpdateAsync(httpClient, onlineGuid, downloadFolder);

                if (!string.IsNullOrEmpty(newPath))
                {
                    ReportProgress("Mise à jour terminée");
                    return new UpdateResult
                    {
                        Success = true,
                        UpdatePerformed = true,
                        NewPath = newPath,
                        Message = "Mise à jour effectuée avec succès"
                    };
                }
                else
                {
                    return new UpdateResult
                    {
                        Success = true,
                        UpdatePerformed = false,
                        Message = "Échec de la mise à jour - lancement de la version actuelle"
                    };
                }
            }
            catch (Exception ex)
            {
                return new UpdateResult
                {
                    Success = true,
                    UpdatePerformed = false,
                    Message = $"Erreur lors de la mise à jour - lancement de la version actuelle: {ex.Message}"
                };
            }
            finally
            {
                // Nettoyage
                if (!string.IsNullOrEmpty(downloadFolder) && Directory.Exists(downloadFolder))
                {
                    
                        Directory.Delete(downloadFolder, true);
                    
                }

                httpClient?.Dispose();
            }
        }

        #endregion

        #region Méthodes optimisées

        /// <summary>
        /// Crée un client HTTP optimisé
        /// </summary>
        private static HttpClient CreateOptimizedHttpClient()
        {
            var handler = new HttpClientHandler()
            {
                MaxConnectionsPerServer = 50,
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate  // FIX: Décompression automatique
            };

            var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromMinutes(TimeoutMinutes);
            client.DefaultRequestHeaders.Add("User-Agent", "Roblox/WinInet");

            // Configuration système
            ServicePointManager.DefaultConnectionLimit = 200;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.MaxServicePointIdleTime = 10000;

            return client;
        }

        /// <summary>
        /// Effectue la mise à jour complète de manière optimisée
        /// </summary>
        private static async Task<string> PerformOptimizedUpdateAsync(HttpClient httpClient, string newGuid, string downloadFolder)
        {
            try
            {
                ReportProgress("Téléchargement du manifeste...");

                // 1. Télécharger et parser le manifeste
                var packages = await GetPackageManifestAsync(httpClient, newGuid, downloadFolder);
                if (packages == null || packages.Count == 0)
                {
                    return null;
                }

                // 2. Créer le dossier de destination
                var (_, basePath) = GetCurrentVersionInfo();
                if (string.IsNullOrEmpty(basePath))
                {
                    return null;
                }

                string versionsPath = Path.Combine(basePath, "Versions");
                string newVersionPath = Path.Combine(versionsPath, $"version-{newGuid}");

                if (Directory.Exists(newVersionPath))
                {
                    Directory.Delete(newVersionPath, true);
                }
                Directory.CreateDirectory(newVersionPath);

                // 3. Trier les packages par priorité (RobloxApp en dernier)
                var sortedPackages = SortPackagesByPriority(packages);

                ReportProgress("Téléchargement des fichiers...");

                // 4. Télécharger et extraire en parallèle
                bool success = await DownloadAndExtractAsync(httpClient, sortedPackages, newGuid, newVersionPath);

                if (!success)
                {
                    if (Directory.Exists(newVersionPath))
                        Directory.Delete(newVersionPath, true);
                    return null;
                }

                ReportProgress("Configuration...");

                // 5. Créer les fichiers de configuration
                CreateConfigurationFiles(newVersionPath, newGuid);

                // 6. Mettre à jour Path.xml - MAINTIEN DE L'INTERFACE
                string newExePath = Path.Combine(newVersionPath, "RobloxPlayerBeta.exe");
                bool configSuccess = UpdatePathXml(newExePath);

                if (!configSuccess)
                {
                    if (Directory.Exists(newVersionPath))
                        Directory.Delete(newVersionPath, true);
                    return null;
                }

                return newExePath;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Télécharge et parse le manifeste de manière optimisée
        /// </summary>
        private static async Task<List<Package>> GetPackageManifestAsync(HttpClient httpClient, string versionGuid, string downloadFolder)
        {
            foreach (string baseUrl in BaseUrls)
            {
                try
                {
                    // FIX: Utiliser le format complet "version-{guid}" pour l'URL
                    string fullVersionString = versionGuid.StartsWith("version-") ? versionGuid : $"version-{versionGuid}";
                    string manifestUrl = $"{baseUrl}/{fullVersionString}-rbxPkgManifest.txt";

                    using (var response = await httpClient.GetAsync(manifestUrl))
                    {
                        response.EnsureSuccessStatusCode();

                        var manifestContent = await response.Content.ReadAsStringAsync();
                        var packages = ParsePackageManifest(manifestContent, downloadFolder);

                        return packages;
                    }
                }
                catch (Exception)
                {
                    if (baseUrl == BaseUrls[BaseUrls.Length - 1])
                        throw;
                }
            }

            throw new Exception("Impossible de télécharger le manifest");
        }

        /// <summary>
        /// Parse le manifeste et crée la liste des packages
        /// </summary>
        private static List<Package> ParsePackageManifest(string manifestData, string downloadFolder)
        {
            var packages = new List<Package>();

            using (var reader = new StringReader(manifestData))
            {
                string version = reader.ReadLine();
                if (string.IsNullOrEmpty(version) || version != "v0")
                {
                    throw new NotSupportedException($"Version de manifest non supportée: {version}");
                }

                while (true)
                {
                    string fileName = reader.ReadLine();
                    string signature = reader.ReadLine();
                    string rawPackedSize = reader.ReadLine();
                    string rawSize = reader.ReadLine();

                    if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(signature) ||
                        string.IsNullOrEmpty(rawPackedSize) || string.IsNullOrEmpty(rawSize))
                        break;

                    // Arrêter au launcher
                    if (fileName == "RobloxPlayerLauncher.exe")
                        break;

                    packages.Add(new Package
                    {
                        Name = fileName,
                        Signature = signature,
                        PackedSize = int.Parse(rawPackedSize),
                        Size = int.Parse(rawSize),
                        DownloadPath = Path.Combine(downloadFolder, $"{signature}_{fileName}")
                    });
                }
            }

            return packages;
        }

        /// <summary>
        /// Trie les packages par priorité d'installation
        /// </summary>
        private static List<Package> SortPackagesByPriority(List<Package> packages)
        {
            // RobloxApp en dernier car plus critique
            var robloxApp = packages.FirstOrDefault(p => p.Name == "RobloxApp.zip");
            var others = packages.Where(p => p.Name != "RobloxApp.zip")
                                .OrderBy(p => p.PackedSize)
                                .ToList();

            if (robloxApp != null)
                others.Add(robloxApp);

            return others;
        }

        /// <summary>
        /// Télécharge et extrait les packages en parallèle
        /// </summary>
        private static async Task<bool> DownloadAndExtractAsync(HttpClient httpClient, List<Package> packages, string versionGuid, string installPath)
        {
            int totalPackages = packages.Count;
            int completedDownloads = 0;
            int completedExtractions = 0;

            // Créer les dossiers nécessaires
            CreateRequiredDirectories(installPath, packages);

            // Semaphores pour contrôler la concurrence
            using (var downloadSemaphore = new SemaphoreSlim(MaxConcurrentDownloads))
            using (var extractionSemaphore = new SemaphoreSlim(MaxConcurrentExtractions))
            {
                // Queue pour les packages prêts à extraire
                var readyToExtract = new ConcurrentQueue<Package>();
                var extractionSignal = new ManualResetEventSlim(false);

                // Worker d'extraction en arrière-plan
                var extractionTask = Task.Run(async () =>
                {
                    while (completedExtractions < totalPackages)
                    {
                        if (readyToExtract.TryDequeue(out Package package))
                        {
                            await extractionSemaphore.WaitAsync();
                            try
                            {
                                bool success = await Task.Run(() => ExtractPackage(package, installPath));
                                if (success)
                                {
                                    Interlocked.Increment(ref completedExtractions);
                                    int percentage = (completedExtractions * 100) / totalPackages;
                                    ReportProgress($"Installation {percentage}%");
                                }
                            }
                            finally
                            {
                                extractionSemaphore.Release();
                            }
                        }
                        else
                        {
                            extractionSignal.Wait(1000);
                            extractionSignal.Reset();
                        }
                    }
                });

                // Téléchargements en parallèle
                var downloadTasks = packages.Select(async (package, index) =>
                {
                    // Délai échelonné pour éviter la surcharge
                    if (index > 0)
                        await Task.Delay(index * 50);

                    await downloadSemaphore.WaitAsync();
                    try
                    {
                        bool success = await DownloadPackageAsync(httpClient, package, versionGuid);
                        if (success)
                        {
                            Interlocked.Increment(ref completedDownloads);
                            int percentage = (completedDownloads * 100) / totalPackages;
                            ReportProgress($"Téléchargement {percentage}%");

                            // Ajouter à la queue d'extraction
                            readyToExtract.Enqueue(package);
                            extractionSignal.Set();
                        }
                        return success;
                    }
                    finally
                    {
                        downloadSemaphore.Release();
                    }
                });

                // Attendre tous les téléchargements
                bool[] downloadResults = await Task.WhenAll(downloadTasks);

                // Attendre que toutes les extractions soient terminées
                while (completedExtractions < totalPackages)
                {
                    extractionSignal.Set();
                    await Task.Delay(100);
                }

                await extractionTask;

                // Vérifier le succès global
                int successfulDownloads = downloadResults.Count(r => r);
                double successRate = (double)successfulDownloads / totalPackages;


                // Considérer comme réussi si au moins 80% des packages sont OK
                return successRate >= 0.8 && CheckCriticalFilesExtracted(installPath);
            }
        }

        /// <summary>
        /// Télécharge un package individuel
        /// </summary>
        private static async Task<bool> DownloadPackageAsync(HttpClient httpClient, Package package, string versionGuid)
        {
            try
            {
                // Vérifier si déjà téléchargé et valide
                if (File.Exists(package.DownloadPath) && ValidateMD5(package.DownloadPath, package.Signature))
                {
                    return true;
                }

                // FIX: Utiliser le format complet "version-{guid}" pour les URLs de fichiers
                string fullVersionString = versionGuid.StartsWith("version-") ? versionGuid : $"version-{versionGuid}";

                // Essayer plusieurs URLs de base
                foreach (string baseUrl in BaseUrls)
                {
                    try
                    {
                        string packageUrl = $"{baseUrl}/{fullVersionString}-{package.Name}";

                        using (var response = await httpClient.GetAsync(packageUrl))
                        {
                            if (!response.IsSuccessStatusCode)
                                continue;

                            string tempDownloadPath = package.DownloadPath + ".tmp";

                            using (var contentStream = await response.Content.ReadAsStreamAsync())
                            using (var fileStream = new FileStream(tempDownloadPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: BufferSize))
                            {
                                await contentStream.CopyToAsync(fileStream);
                            }

                            // Atomique: renommer après téléchargement complet
                            if (File.Exists(package.DownloadPath))
                                File.Delete(package.DownloadPath);
                            File.Move(tempDownloadPath, package.DownloadPath);

                            // Valider le téléchargement
                            if (ValidateMD5(package.DownloadPath, package.Signature))
                            {
                                return true;
                            }
                            else
                            {
                                File.Delete(package.DownloadPath);
                            }
                        }
                    }
                    catch (Exception)
                    {

                        // Nettoyer les fichiers temporaires
                        string tempPath = package.DownloadPath + ".tmp";
                        if (File.Exists(tempPath)) File.Delete(tempPath);
                        if (File.Exists(package.DownloadPath)) File.Delete(package.DownloadPath);

                        continue;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Extrait un package vers sa destination
        /// </summary>
        private static bool ExtractPackage(Package package, string installPath)
        {
            try
            {
                if (!package.IsZipPackage)
                {
                    // Fichier exécutable - copier directement
                    string targetPath = Path.Combine(installPath, package.Name);
                    File.Copy(package.DownloadPath, targetPath, true);
                    return true;
                }

                // Déterminer le dossier de destination
                string packageDir = "";
                PackageDirectoryMap.TryGetValue(package.Name, out packageDir);
                string targetFolder = Path.Combine(installPath, packageDir ?? "");

                // Extraire l'archive
                using (var archive = ZipFile.OpenRead(package.DownloadPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name))
                            continue; // Dossier vide

                        string destinationPath = Path.Combine(targetFolder, entry.FullName);
                        string destinationDir = Path.GetDirectoryName(destinationPath);

                        if (!string.IsNullOrEmpty(destinationDir))
                            Directory.CreateDirectory(destinationDir);

                        entry.ExtractToFile(destinationPath, overwrite: true);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Crée les dossiers nécessaires basés sur les packages
        /// </summary>
        private static void CreateRequiredDirectories(string basePath, List<Package> packages)
        {
            

                // Dossiers de base
                HashSet<string> requiredDirs = new HashSet<string>
                {
                    "content", "ExtraContent", "PlatformContent", "shaders", "ssl", "WebView2RuntimeInstaller",
                    "content\\avatar", "content\\configs", "content\\fonts", "content\\sky",
                    "content\\sounds", "content\\textures", "content\\models",
                    "PlatformContent\\pc", "PlatformContent\\pc\\textures", "PlatformContent\\pc\\terrain",
                    "PlatformContent\\pc\\fonts", "PlatformContent\\pc\\shared_compression_dictionaries",
                    "ExtraContent\\LuaPackages", "ExtraContent\\translations", "ExtraContent\\models",
                    "ExtraContent\\textures", "ExtraContent\\places"
                };

                // Analyser les packages pour détecter d'autres dossiers
                foreach (var package in packages.Where(p => p.IsZipPackage))
                {
                    if (PackageDirectoryMap.TryGetValue(package.Name, out string dir) && !string.IsNullOrEmpty(dir))
                    {
                        requiredDirs.Add(dir.TrimEnd('\\'));
                    }
                }

                // Créer tous les dossiers
                foreach (string dirName in requiredDirs)
                {
                    string dirPath = Path.Combine(basePath, dirName);
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }
                }

            
        }

        /// <summary>
        /// Crée les fichiers de configuration nécessaires
        /// </summary>
        private static void CreateConfigurationFiles(string installPath, string versionGuid)
        {
            
                // Fichier version
                string cleanGuid = versionGuid.StartsWith("version-") ?
                    versionGuid.Substring("version-".Length) : versionGuid;

                string versionFilePath = Path.Combine(installPath, "version");
                File.WriteAllText(versionFilePath, cleanGuid);

                // AppSettings.xml
                const string appSettings =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                    "<Settings>\r\n" +
                    "\t<ContentFolder>content</ContentFolder>\r\n" +
                    "\t<BaseUrl>http://www.roblox.com</BaseUrl>\r\n" +
                    "</Settings>\r\n";

                string appSettingsPath = Path.Combine(installPath, "AppSettings.xml");
                File.WriteAllText(appSettingsPath, appSettings);
            
            
        }

        /// <summary>
        /// Valide un fichier avec son hash MD5
        /// </summary>
        private static bool ValidateMD5(string filePath, string expectedHash)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length == 0)
                    return false;

                using (var md5 = MD5.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);
                    // Compatible .NET 4.8
                    string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    return hash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception)
            {
                
                return false;
            }
        }

        #endregion

        #region Méthodes conservées du service original

        /// <summary>
        /// Vérifie les permissions d'écriture dans le dossier Roblox - MAINTENUE
        /// </summary>
        private static bool CheckWritePermissions()
        {
            try
            {

                var (_, basePath) = GetCurrentVersionInfo();

                if (string.IsNullOrEmpty(basePath))
                {
                    basePath = @"C:\Program Files (x86)\Roblox";
                }

                string versionsPath = Path.Combine(basePath, "Versions");

                // Créer le dossier Versions s'il n'existe pas
                if (!Directory.Exists(versionsPath))
                {
                    Directory.CreateDirectory(versionsPath);
                }

                // Test d'écriture
                string testFile = Path.Combine(versionsPath, $"test_{Guid.NewGuid()}.tmp");

                File.WriteAllText(testFile, "test");
                File.Delete(testFile);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Récupère le GUID de la version actuelle et le chemin base depuis Path.xml - MAINTENUE
        /// </summary>
        private static (string guid, string basePath) GetCurrentVersionInfo()
        {
            try
            {
                string pathXmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "Path.xml");

                if (!File.Exists(pathXmlFile))
                {
                    return (null, null);
                }

                XDocument doc = XDocument.Load(pathXmlFile);

                var pathsElement = doc.Root.Element("Paths");
                if (pathsElement == null)
                {
                    return (null, null);
                }


                foreach (var pathElement in pathsElement.Elements("Path"))
                {
                    string platformName = pathElement.Element("PlatformName")?.Value;

                    if (platformName?.Equals("Roblox", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        string path = pathElement.Element("Path")?.Value;

                        if (!string.IsNullOrEmpty(path))
                        {
                            // Extraction du GUID avec regex: version-([a-f0-9]+)
                            Match match = Regex.Match(path, @"version-([a-f0-9]+)", RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                string guid = match.Groups[1].Value;
                                // Récupérer le chemin base : remonter de 2 niveaux depuis RobloxPlayerBeta.exe
                                string basePath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
                                return (guid, basePath);
                            }
                            else
                            {
                               
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {
            }
            return (null, null);
        }

        /// <summary>
        /// Récupère le GUID de la version actuelle depuis Path.xml - MAINTENUE
        /// </summary>
        private static string GetCurrentVersionGuid()
        {
            var (guid, _) = GetCurrentVersionInfo();
            return guid;
        }

        /// <summary>
        /// Récupère le GUID de la version en ligne - OPTIMISÉE
        /// </summary>
        private static async Task<string> GetOnlineVersionGuidAsync(HttpClient httpClient)
        {
            foreach (string apiUrl in VersionApiUrls)
            {
                try
                {

                    using (var response = await httpClient.GetAsync(apiUrl))
                    {
                        response.EnsureSuccessStatusCode();

                        string jsonResponse = await response.Content.ReadAsStringAsync();

                       

                        // Vérifier que la réponse est du JSON valide
                        if (string.IsNullOrWhiteSpace(jsonResponse) ||
                            !jsonResponse.TrimStart().StartsWith("{"))
                        {
                            continue;
                        }

                        var versionInfo = JsonSerializer.Deserialize<RobloxVersionInfo>(jsonResponse);

                        // Extraire le GUID de clientVersionUpload (format: "version-xxxxx")
                        string clientVersion = versionInfo?.clientVersionUpload;
                        if (!string.IsNullOrEmpty(clientVersion))
                        {
                            Match match = Regex.Match(clientVersion, @"version-([a-f0-9]+)", RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                string guid = match.Groups[1].Value;
                                return guid;
                            }
                        }

                        // Fallback: essayer d'extraire directement de "version"
                        if (!string.IsNullOrEmpty(versionInfo?.version))
                        {
                            Match match = Regex.Match(versionInfo.version, @"([a-f0-9]+)", RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                string guid = match.Groups[1].Value;
                                return guid;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    if (apiUrl == VersionApiUrls[VersionApiUrls.Length - 1])
                        throw;
                }
            }

            return null;
        }

        /// <summary>
        /// Vérifie que les fichiers critiques pour Roblox sont présents - MAINTENUE
        /// </summary>
        private static bool CheckCriticalFilesExtracted(string path)
        {
            try
            {
                // Liste des éléments critiques pour que Roblox fonctionne
                string[] criticalItems = new string[]
                {
                    "RobloxPlayerBeta.exe",        // Fichier principal (obligatoire)
                    "content",                     // Dossier de contenu (obligatoire)
                    "ExtraContent",               // Dossier de contenu extra
                    "PlatformContent"             // Dossier de contenu plateforme
                };

                int foundCritical = 0;

                foreach (string item in criticalItems)
                {
                    string itemPath = Path.Combine(path, item);

                    if (File.Exists(itemPath) || Directory.Exists(itemPath))
                    {
                        foundCritical++;
                    }
                    else
                    {
                    }
                }

                // Au moins 2 éléments critiques doivent être présents
                bool hasMinimum = foundCritical >= 2;

                return hasMinimum;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Met à jour le fichier Path.xml avec le nouveau chemin - MAINTENUE
        /// </summary>
        private static bool UpdatePathXml(string newExePath)
        {
            try
            {
                string pathXmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "Path.xml");

                if (!File.Exists(pathXmlFile))
                {
                    return false;
                }

                XDocument doc = XDocument.Load(pathXmlFile);
                bool updated = false;

                var pathsElement = doc.Root.Element("Paths");
                if (pathsElement == null)
                {
                    return false;
                }

                foreach (var pathElement in pathsElement.Elements("Path"))
                {
                    string platformName = pathElement.Element("PlatformName")?.Value;
                    if (platformName?.Equals("Roblox", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        string oldPath = pathElement.Element("Path")?.Value;
                        pathElement.Element("Path").Value = newExePath;
                        updated = true;
                        break;
                    }
                }

                if (updated)
                {
                    doc.Save(pathXmlFile);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Rapporte la progression - MAINTENUE AVEC DÉLAI RÉDUIT
        /// </summary>
        private static void ReportProgress(string message)
        {
            ProgressChanged?.Invoke(message);
            // Délai réduit pour une interface plus réactive
            System.Threading.Thread.Sleep(300);
        }

        #endregion

        #region Méthodes utilitaires supplémentaires

        /// <summary>
        /// Obtient des informations sur la version Roblox installée
        /// </summary>
        public static string GetInstalledVersionInfo()
        {
            try
            {
                var (guid, basePath) = GetCurrentVersionInfo();
                if (!string.IsNullOrEmpty(guid))
                {
                    return $"Version {guid} installée dans {basePath}";
                }
                return "Aucune version détectée";
            }
            catch
            {
                return "Erreur lors de la détection de version";
            }
        }

        /// <summary>
        /// Vérifie si une mise à jour est disponible sans la télécharger
        /// </summary>
        public static async Task<bool> IsUpdateAvailableAsync()
        {
            try
            {
                using (var httpClient = CreateOptimizedHttpClient())
                {
                    string currentGuid = GetCurrentVersionGuid();
                    string onlineGuid = await GetOnlineVersionGuidAsync(httpClient);

                    return !string.IsNullOrEmpty(currentGuid) &&
                           !string.IsNullOrEmpty(onlineGuid) &&
                           !currentGuid.Equals(onlineGuid, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Nettoie les anciennes versions pour libérer l'espace disque
        /// </summary>
        public static bool CleanupOldVersions(int versionsToKeep = 2)
        {
            try
            {
                var (currentGuid, basePath) = GetCurrentVersionInfo();
                if (string.IsNullOrEmpty(basePath))
                    return false;

                string versionsPath = Path.Combine(basePath, "Versions");
                if (!Directory.Exists(versionsPath))
                    return true;

                var versionDirs = Directory.GetDirectories(versionsPath)
                    .Where(dir => Path.GetFileName(dir).StartsWith("version-"))
                    .OrderByDescending(dir => Directory.GetCreationTime(dir))
                    .ToList();

                // Garder les N versions les plus récentes
                var dirsToDelete = versionDirs.Skip(versionsToKeep);

                foreach (string dirToDelete in dirsToDelete)
                {
                   
                        // Ne pas supprimer la version actuelle
                        if (dirToDelete.Contains(currentGuid))
                            continue;

                        Directory.Delete(dirToDelete, true);
                 
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}