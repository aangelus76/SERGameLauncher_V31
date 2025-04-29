// Panel/Folder/FolderPermissionService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Xml.Linq;

namespace SERGamesLauncher_V31
{
    /// <summary>
    /// Service pour gérer les permissions des dossiers
    /// </summary>
    public static class FolderPermissionService
    {
        // Chemin vers le fichier de configuration
        private static readonly string ConfigPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Config", "FolderPermissions.xml");

        // Cache pour éviter de recalculer les SID fréquemment
        private static NTAccount _usersAccount = null;

        /// <summary>
        /// Charge les configurations de permissions de dossiers depuis le fichier
        /// </summary>
        public static List<FolderPermission> LoadFolderPermissions()
        {
            List<FolderPermission> permissions = new List<FolderPermission>();

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
                    var folders = doc.Root?.Element("Folders")?.Elements("Folder");
                    if (folders != null)
                    {
                        foreach (var folder in folders)
                        {
                            FolderPermission permission = new FolderPermission
                            {
                                Id = folder.Attribute("id")?.Value ?? Guid.NewGuid().ToString(),
                                Name = folder.Element("Name")?.Value ?? string.Empty,
                                FolderPath = folder.Element("FolderPath")?.Value ?? string.Empty,
                                IsProtectionEnabled = bool.Parse(folder.Element("IsProtectionEnabled")?.Value ?? "true"),
                                EnableOnStartup = bool.Parse(folder.Element("EnableOnStartup")?.Value ?? "true"),
                                ProtectionLevel = (ProtectionLevel)Enum.Parse(typeof(ProtectionLevel), folder.Element("ProtectionLevel")?.Value ?? "ReadOnly"),
                                LastModified = DateTime.Parse(folder.Element("LastModified")?.Value ?? DateTime.Now.ToString())
                            };

                            permissions.Add(permission);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // En cas d'erreur, retourner une liste vide
            }

            return permissions;
        }

        /// <summary>
        /// Sauvegarde les configurations de permissions de dossiers dans le fichier
        /// </summary>
        public static bool SaveFolderPermissions(List<FolderPermission> permissions)
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

                    // Supprimer l'ancien élément Folders s'il existe
                    root?.Element("Folders")?.Remove();
                }
                else
                {
                    // Sinon, créer un nouveau document
                    doc = new XDocument();
                    root = new XElement("Configuration");
                    doc.Add(root);
                }

                // Créer l'élément Folders
                XElement foldersElement = new XElement("Folders");

                // Ajouter chaque configuration de permission
                foreach (var permission in permissions)
                {
                    XElement folderElement = new XElement("Folder",
                        new XAttribute("id", permission.Id),
                        new XElement("Name", permission.Name),
                        new XElement("FolderPath", permission.FolderPath),
                        new XElement("IsProtectionEnabled", permission.IsProtectionEnabled.ToString().ToLower()),
                        new XElement("EnableOnStartup", permission.EnableOnStartup.ToString().ToLower()),
                        new XElement("ProtectionLevel", permission.ProtectionLevel.ToString()),
                        new XElement("LastModified", permission.LastModified.ToString())
                    );

                    foldersElement.Add(folderElement);
                }

                // Ajouter l'élément au document
                root?.Add(foldersElement);

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
        /// Vérifie si un dossier avec le même nom existe déjà
        /// </summary>
        public static bool FolderNameExists(List<FolderPermission> permissions, string name, string excludeId = null)
        {
            return permissions.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                                && (excludeId == null || p.Id != excludeId));
        }

        /// <summary>
        /// Applique une protection en lecture seule au dossier spécifié
        /// </summary>
        public static bool ApplyProtection(FolderPermission permission)
        {
            try
            {
                // Vérifier si le dossier existe
                if (!Directory.Exists(permission.FolderPath))
                {
                    return false;
                }

                // Obtenir les ACL actuelles du dossier
                DirectorySecurity acl = Directory.GetAccessControl(permission.FolderPath);

                // Utiliser le groupe Utilisateurs
                NTAccount usersAccount = GetUsersAccount();

                // Déterminer les droits à bloquer selon le niveau de protection
                FileSystemRights rightsToBlock = FileSystemRights.Write;
                switch (permission.ProtectionLevel)
                {
                    case ProtectionLevel.ReadOnly:
                        rightsToBlock = FileSystemRights.Write | FileSystemRights.Delete |
                                        FileSystemRights.DeleteSubdirectoriesAndFiles |
                                        FileSystemRights.ChangePermissions |
                                        FileSystemRights.CreateFiles |
                                        FileSystemRights.CreateDirectories;
                        break;
                    case ProtectionLevel.PreventDeletion:
                        rightsToBlock = FileSystemRights.Delete | FileSystemRights.DeleteSubdirectoriesAndFiles;
                        break;
                    case ProtectionLevel.PreventCreation:
                        rightsToBlock = FileSystemRights.CreateFiles | FileSystemRights.CreateDirectories;
                        break;
                }

                // Créer une règle pour interdire les droits spécifiés
                FileSystemAccessRule rule = new FileSystemAccessRule(
                    usersAccount,
                    rightsToBlock,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Deny
                );

                // Supprimer d'abord toute règle existante pour le même utilisateur/groupe et droits
                RemoveMatchingRules(acl, usersAccount);

                // Ajouter la nouvelle règle
                acl.AddAccessRule(rule);

                // Appliquer les nouvelles permissions
                Directory.SetAccessControl(permission.FolderPath, acl);

                // Mettre à jour la date de dernière modification
                permission.LastModified = DateTime.Now;
                permission.IsProtectionEnabled = true;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Retire la protection du dossier spécifié
        /// </summary>
        public static bool RemoveProtection(FolderPermission permission)
        {
            try
            {
                // Vérifier si le dossier existe
                if (!Directory.Exists(permission.FolderPath))
                {
                    return false;
                }

                // Obtenir les ACL actuelles du dossier
                DirectorySecurity acl = Directory.GetAccessControl(permission.FolderPath);

                // Utiliser le groupe Utilisateurs
                NTAccount usersAccount = GetUsersAccount();

                // Supprimer toutes les règles de type "deny" pour le groupe Utilisateurs
                RemoveMatchingRules(acl, usersAccount);

                // Appliquer les nouvelles permissions
                Directory.SetAccessControl(permission.FolderPath, acl);

                // Mettre à jour la date de dernière modification
                permission.LastModified = DateTime.Now;
                permission.IsProtectionEnabled = false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Vérifie si la protection est active pour le dossier spécifié
        /// </summary>
        public static bool IsProtectionActive(FolderPermission permission)
        {
            try
            {
                // Vérifier si le dossier existe
                if (!Directory.Exists(permission.FolderPath))
                {
                    return false;
                }

                // Obtenir les ACL actuelles du dossier
                DirectorySecurity acl = Directory.GetAccessControl(permission.FolderPath);

                // Utiliser le groupe Utilisateurs
                NTAccount usersAccount = GetUsersAccount();

                // Récupérer toutes les règles pour ce groupe
                AuthorizationRuleCollection rules = acl.GetAccessRules(true, true, typeof(NTAccount));

                // Vérifier s'il existe une règle de type "deny" pour le groupe Utilisateurs
                foreach (FileSystemAccessRule rule in rules)
                {
                    if (rule.IdentityReference.Value == usersAccount.Value &&
                        rule.AccessControlType == AccessControlType.Deny)
                    {
                        switch (permission.ProtectionLevel)
                        {
                            case ProtectionLevel.ReadOnly:
                                if ((rule.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write)
                                    return true;
                                break;
                            case ProtectionLevel.PreventDeletion:
                                if ((rule.FileSystemRights & FileSystemRights.Delete) == FileSystemRights.Delete)
                                    return true;
                                break;
                            case ProtectionLevel.PreventCreation:
                                if ((rule.FileSystemRights & FileSystemRights.CreateFiles) == FileSystemRights.CreateFiles)
                                    return true;
                                break;
                        }
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
        /// Retourne l'état actuel des permissions du dossier
        /// </summary>
        public static PermissionStatus GetFolderPermissionStatus(FolderPermission permission)
        {
            try
            {
                if (!Directory.Exists(permission.FolderPath))
                {
                    return PermissionStatus.FolderDoesNotExist;
                }

                bool isProtected = IsProtectionActive(permission);

                if (isProtected && permission.IsProtectionEnabled)
                {
                    return PermissionStatus.Protected;
                }
                else if (!isProtected && !permission.IsProtectionEnabled)
                {
                    return PermissionStatus.Unprotected;
                }
                else if (isProtected && !permission.IsProtectionEnabled)
                {
                    return PermissionStatus.ProtectedButShouldNot;
                }
                else // !isProtected && permission.IsProtectionEnabled
                {
                    return PermissionStatus.UnprotectedButShouldBe;
                }
            }
            catch
            {
                return PermissionStatus.AccessError;
            }
        }

        /// <summary>
        /// Applique les protections pour tous les dossiers configurés pour le démarrage
        /// </summary>
        public static bool ApplyStartupProtections(List<FolderPermission> permissions)
        {
            bool allSucceeded = true;

            foreach (var permission in permissions.Where(p => p.EnableOnStartup))
            {
                if (!ApplyProtection(permission))
                {
                    allSucceeded = false;
                }
            }

            return allSucceeded;
        }

        /// <summary>
        /// Supprime toutes les règles correspondant à l'utilisateur/groupe spécifié
        /// </summary>
        private static void RemoveMatchingRules(DirectorySecurity acl, NTAccount account)
        {
            // Récupérer toutes les règles pour cet utilisateur/groupe
            AuthorizationRuleCollection rules = acl.GetAccessRules(true, true, typeof(NTAccount));

            // Supprimer toutes les règles de type "deny" pour cet utilisateur/groupe
            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.IdentityReference.Value == account.Value &&
                    rule.AccessControlType == AccessControlType.Deny)
                {
                    acl.RemoveAccessRule(rule);
                }
            }
        }

        /// <summary>
        /// Obtient le compte utilisateurs mis en cache ou le crée
        /// </summary>
        private static NTAccount GetUsersAccount()
        {
            if (_usersAccount == null)
            {
                SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                _usersAccount = (NTAccount)sid.Translate(typeof(NTAccount));
            }
            return _usersAccount;
        }
    }

    /// <summary>
    /// État des permissions d'un dossier
    /// </summary>
    public enum PermissionStatus
    {
        /// <summary>
        /// Le dossier est protégé comme prévu
        /// </summary>
        Protected,

        /// <summary>
        /// Le dossier n'est pas protégé comme prévu
        /// </summary>
        Unprotected,

        /// <summary>
        /// Le dossier est protégé mais ne devrait pas l'être
        /// </summary>
        ProtectedButShouldNot,

        /// <summary>
        /// Le dossier n'est pas protégé mais devrait l'être
        /// </summary>
        UnprotectedButShouldBe,

        /// <summary>
        /// Le dossier n'existe pas
        /// </summary>
        FolderDoesNotExist,

        /// <summary>
        /// Erreur d'accès au dossier
        /// </summary>
        AccessError
    }
}