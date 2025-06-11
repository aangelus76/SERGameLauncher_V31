using System;

namespace SERGamesLauncher_V31
{
    /// <summary>
    /// Représente un compte Steam avec ses informations de connexion
    /// </summary>
    public class SteamAccount
    {
        /// <summary>
        /// Identifiant unique du compte
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Nom du poste auquel ce compte est associé
        /// </summary>
        public string PosteName { get; set; } = string.Empty;

        /// <summary>
        /// Nom d'utilisateur Steam
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Mot de passe chiffré
        /// </summary>
        public string EncryptedPassword { get; set; } = string.Empty;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public SteamAccount() { }

        /// <summary>
        /// Constructeur avec les propriétés principales
        /// </summary>
        public SteamAccount(string posteName, string username, string encryptedPassword)
        {
            PosteName = posteName;
            Username = username;
            EncryptedPassword = encryptedPassword;
        }
    }
}