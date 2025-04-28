using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace SERGamesLauncher_V31
{
    /// <summary>
    /// Service pour gérer les comptes Steam avec chiffrement AES
    /// </summary>
    public static class SteamAccountService
    {
        // Chemin vers le fichier de configuration des comptes Steam
        private static readonly string ConfigPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Config", "steamAccounts.json");

        // Nom de variable délibérément générique pour la clé de chiffrement
        private static readonly string accountProperty = "SERangelus76GameLauncher";

        /// <summary>
        /// Charge la liste des comptes Steam depuis le fichier de configuration
        /// </summary>
        public static List<SteamAccount> LoadSteamAccounts()
        {
            List<SteamAccount> accounts = new List<SteamAccount>();

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

                    // Désérialiser le JSON en liste d'objets SteamAccount
                    if (!string.IsNullOrEmpty(jsonContent))
                    {
                        accounts = JsonSerializer.Deserialize<List<SteamAccount>>(jsonContent) ?? new List<SteamAccount>();
                    }
                }
            }
            catch (Exception)
            {
                // En cas d'erreur, retourner une liste vide
            }

            return accounts;
        }

        /// <summary>
        /// Sauvegarde la liste des comptes Steam dans le fichier de configuration
        /// </summary>
        public static bool SaveSteamAccounts(List<SteamAccount> accounts)
        {
            try
            {
                // S'assurer que le dossier de configuration existe
                string configDirectory = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(configDirectory) && configDirectory != null)
                {
                    Directory.CreateDirectory(configDirectory);
                }

                // Sérialiser la liste en JSON
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonContent = JsonSerializer.Serialize(accounts, options);

                // Écrire dans le fichier
                File.WriteAllText(ConfigPath, jsonContent);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Vérifie si un nom de poste existe déjà dans la liste
        /// </summary>
        public static bool PosteNameExists(List<SteamAccount> accounts, string posteName, string excludeId = null)
        {
            return accounts.Any(a => a.PosteName.Equals(posteName, StringComparison.OrdinalIgnoreCase)
                                && (excludeId == null || a.Id != excludeId));
        }

        /// <summary>
        /// Obtient le compte correspondant au nom du poste actuel
        /// </summary>
        public static SteamAccount GetAccountForCurrentComputer(List<SteamAccount> accounts)
        {
            string computerName = Environment.MachineName;
            return accounts.FirstOrDefault(a => a.PosteName.Equals(computerName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Chiffre un mot de passe avec AES
        /// </summary>
        public static string EncryptPassword(string plainPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(plainPassword))
                    return string.Empty;

                byte[] clearBytes = Encoding.Unicode.GetBytes(plainPassword);

                // Dérivation de clé à partir du "sel" pour obtenir une clé AES 128 bits
                using (Aes encryptor = Aes.Create())
                {
                    // Obtenir une clé 128 bits et un vecteur d'initialisation
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(
                        accountProperty,
                        Encoding.Unicode.GetBytes(accountProperty.Reverse().ToString()),
                        1000, HashAlgorithmName.SHA256);

                    encryptor.Key = pdb.GetBytes(16); // 128 bits
                    encryptor.IV = pdb.GetBytes(16);
                    encryptor.Mode = CipherMode.CBC;

                    // Chiffrement des données
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
                // En cas d'erreur, retourner une chaîne vide
                return string.Empty;
            }
        }

        /// <summary>
        /// Déchiffre un mot de passe avec AES
        /// </summary>
        public static string DecryptPassword(string encryptedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedPassword))
                    return string.Empty;

                byte[] cipherBytes = Convert.FromBase64String(encryptedPassword);

                // Dérivation de clé à partir du "sel" pour obtenir une clé AES 128 bits
                using (Aes encryptor = Aes.Create())
                {
                    // Obtenir la même clé et le même vecteur d'initialisation que pour le chiffrement
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(
                        accountProperty,
                        Encoding.Unicode.GetBytes(accountProperty.Reverse().ToString()),
                        1000, HashAlgorithmName.SHA256);

                    encryptor.Key = pdb.GetBytes(16); // 128 bits
                    encryptor.IV = pdb.GetBytes(16);
                    encryptor.Mode = CipherMode.CBC;

                    // Déchiffrement des données
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        return Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception)
            {
                // En cas d'erreur, retourner une chaîne vide
                return string.Empty;
            }
        }
    }
}