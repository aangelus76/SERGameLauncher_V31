// Services/CustomButtonService.cs
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace SERGamesLauncher_V31.Services
{
    /// <summary>
    /// Configuration du bouton personnalisé
    /// </summary>
    public class CustomButtonConfig
    {
        /// <summary>
        /// Activer/désactiver le bouton custom
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Nom affiché sur le bouton (menu gauche)
        /// </summary>
        public string ButtonLabel { get; set; } = "Mon Projet";

        /// <summary>
        /// Titre affiché sur la page
        /// </summary>
        public string PageTitle { get; set; } = "Mon Projet";

        /// <summary>
        /// Message secondaire (comme "Nous prêtons un compte...")
        /// </summary>
        public string PageSubtitle { get; set; } = "Projet en cours";

        /// <summary>
        /// Instructions détaillées
        /// </summary>
        public string PageInstructions { get; set; } = "Description du projet ou instructions pour l'utilisateur.";

        /// <summary>
        /// Type de cible : "url" ou "exe"
        /// </summary>
        public string TargetType { get; set; } = "url";

        /// <summary>
        /// Chemin ou URL à lancer (supporte les placeholders)
        /// </summary>
        public string TargetPath { get; set; } = "";

        /// <summary>
        /// Arguments de lancement (pour exe, supporte les placeholders)
        /// </summary>
        public string LaunchArguments { get; set; } = "";

        /// <summary>
        /// Nom du fichier image (stocké dans Config/CustomButton/)
        /// </summary>
        public string ImageFileName { get; set; } = "";

        /// <summary>
        /// Activer la génération de token
        /// </summary>
        public bool UseToken { get; set; } = false;

        /// <summary>
        /// Format du token (ex: "{FULLNAME},{AGE},{PC}")
        /// </summary>
        public string TokenFormat { get; set; } = "{FULLNAME},{AGE},{PC}";
    }

    /// <summary>
    /// Service de gestion du bouton personnalisé
    /// </summary>
    public static class CustomButtonService
    {
        private static readonly string ConfigFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "CustomButton");
        private static readonly string ConfigFile = Path.Combine(ConfigFolder, "customButton.json");

        /// <summary>
        /// Clé de chiffrement (identique à SteamAccountService)
        /// </summary>
        private static readonly string EncryptionKey = "SERangelus76GameLauncher";

        /// <summary>
        /// Charge la configuration du bouton custom
        /// </summary>
        public static CustomButtonConfig LoadConfig()
        {
            try
            {
                EnsureConfigFolderExists();

                if (File.Exists(ConfigFile))
                {
                    string json = File.ReadAllText(ConfigFile);
                    return JsonSerializer.Deserialize<CustomButtonConfig>(json) ?? new CustomButtonConfig();
                }
            }
            catch (Exception)
            {
                // En cas d'erreur, retourner config par défaut
            }

            return new CustomButtonConfig();
        }

        /// <summary>
        /// Sauvegarde la configuration du bouton custom
        /// </summary>
        public static void SaveConfig(CustomButtonConfig config)
        {
            try
            {
                EnsureConfigFolderExists();

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(ConfigFile, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la sauvegarde : {ex.Message}");
            }
        }

        /// <summary>
        /// Sauvegarde l'image et retourne le nom du fichier
        /// </summary>
        public static string SaveImage(string sourcePath)
        {
            try
            {
                EnsureConfigFolderExists();

                string extension = Path.GetExtension(sourcePath);
                string fileName = $"custom_image{extension}";
                string destPath = Path.Combine(ConfigFolder, fileName);

                // Supprimer l'ancienne image si elle existe
                DeleteExistingImages();

                // Copier la nouvelle image
                File.Copy(sourcePath, destPath, true);

                return fileName;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la sauvegarde de l'image : {ex.Message}");
            }
        }

        /// <summary>
        /// Supprime les images existantes
        /// </summary>
        private static void DeleteExistingImages()
        {
            try
            {
                if (Directory.Exists(ConfigFolder))
                {
                    foreach (var file in Directory.GetFiles(ConfigFolder, "custom_image.*"))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Récupère le chemin complet de l'image
        /// </summary>
        public static string GetImagePath(CustomButtonConfig config)
        {
            if (string.IsNullOrEmpty(config?.ImageFileName))
                return null;

            string path = Path.Combine(ConfigFolder, config.ImageFileName);
            return File.Exists(path) ? path : null;
        }

        /// <summary>
        /// Charge l'image en tant que BitmapImage
        /// </summary>
        public static BitmapImage LoadImage(CustomButtonConfig config)
        {
            string imagePath = GetImagePath(config);
            if (imagePath == null) return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Charge l'image redimensionnée pour le bouton (50x50)
        /// </summary>
        public static BitmapImage LoadButtonImage(CustomButtonConfig config)
        {
            string imagePath = GetImagePath(config);
            if (imagePath == null) return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 50;
                bitmap.DecodePixelHeight = 50;
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Charge l'image redimensionnée pour la page (200x200)
        /// </summary>
        public static BitmapImage LoadPageImage(CustomButtonConfig config)
        {
            string imagePath = GetImagePath(config);
            if (imagePath == null) return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 200;
                bitmap.DecodePixelHeight = 200;
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// S'assure que le dossier de configuration existe
        /// </summary>
        private static void EnsureConfigFolderExists()
        {
            if (!Directory.Exists(ConfigFolder))
            {
                Directory.CreateDirectory(ConfigFolder);
            }
        }

        /// <summary>
        /// Supprime toute la configuration (reset)
        /// </summary>
        public static void ResetConfig()
        {
            try
            {
                if (Directory.Exists(ConfigFolder))
                {
                    Directory.Delete(ConfigFolder, true);
                }
            }
            catch { }
        }

        /// <summary>
        /// Remplace les placeholders dans une chaîne
        /// </summary>
        /// <param name="input">Chaîne avec placeholders</param>
        /// <param name="userInfo">Infos utilisateur</param>
        /// <param name="config">Config pour le token</param>
        public static string ReplacePlaceholders(string input, UserPlaceholderInfo userInfo, CustomButtonConfig config = null)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string result = input;

            // Remplacer les placeholders de base
            result = result.Replace("{FULLNAME}", userInfo.FullName ?? "");
            result = result.Replace("{AGE}", userInfo.Age.ToString());
            result = result.Replace("{PC}", userInfo.ComputerName ?? "");
            result = result.Replace("{TIMESTAMP}", userInfo.Timestamp.ToString());

            // Générer et remplacer le token si nécessaire
            if (result.Contains("{TOKEN}") && config != null && config.UseToken)
            {
                string tokenData = ReplacePlaceholders(config.TokenFormat, userInfo, null);
                string encryptedToken = EncryptToken(tokenData);
                // URL-encode le token pour qu'il passe bien dans l'URL
                result = result.Replace("{TOKEN}", Uri.EscapeDataString(encryptedToken));
            }
            else if (result.Contains("{TOKEN}"))
            {
                result = result.Replace("{TOKEN}", "");
            }

            return result;
        }

        /// <summary>
        /// Chiffre les données du token avec AES (UTF8 pour réduire la taille)
        /// </summary>
        public static string EncryptToken(string plainData)
        {
            try
            {
                if (string.IsNullOrEmpty(plainData))
                    return string.Empty;

                // UTF8 au lieu d'Unicode pour réduire la taille
                byte[] clearBytes = Encoding.UTF8.GetBytes(plainData);

                using (Aes encryptor = Aes.Create())
                {
                    string reversed = new string(EncryptionKey.Reverse().ToArray());
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(
                        EncryptionKey,
                        Encoding.Unicode.GetBytes(reversed),
                        1000, HashAlgorithmName.SHA256);

                    encryptor.Key = pdb.GetBytes(16); // 128 bits
                    encryptor.IV = pdb.GetBytes(16);
                    encryptor.Mode = CipherMode.CBC;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Informations utilisateur pour les placeholders
    /// </summary>
    public class UserPlaceholderInfo
    {
        public string FullName { get; set; }
        public int Age { get; set; }
        public string ComputerName { get; set; }
        public long Timestamp { get; set; }

        /// <summary>
        /// Crée une instance avec les infos actuelles
        /// </summary>
        public static UserPlaceholderInfo CreateCurrent(string fullName, int age)
        {
            return new UserPlaceholderInfo
            {
                FullName = fullName,
                Age = age,
                ComputerName = Environment.MachineName,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }
    }
}
