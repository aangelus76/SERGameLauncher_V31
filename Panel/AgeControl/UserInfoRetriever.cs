// Models/UserInfoRetriever.cs
using System;
using System.DirectoryServices;
using System.Globalization;

namespace SERGamesLauncher_V31
{
    public class UserInfoRetriever
    {
        // Structure pour stocker les informations de l'utilisateur
        public class UserInfo
        {
            public string DisplayName { get; set; } = "John Doe";
            public string BirthDate { get; set; } = string.Empty;
            public int Age { get; set; } = 90;
            public bool HasValidBirthDate { get; set; } = false;
            public bool HasValidDisplayName { get; set; } = false;
        }

        public static UserInfo GetUserInfo(string username)
        {
            UserInfo userInfo = new UserInfo();

            try
            {
                // Se connecter au domaine neos.lan explicitement
                string ldapPath = "LDAP://neos.lan";
                DirectoryEntry directoryEntry = new DirectoryEntry(ldapPath);

                // Créer la recherche
                DirectorySearcher searcher = new DirectorySearcher(directoryEntry);
                searcher.Filter = $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={username}))";

                // Ajouter les propriétés à rechercher spécifiquement
                searcher.PropertiesToLoad.Add("dateOfBirth");
                searcher.PropertiesToLoad.Add("displayName");

                // Effectuer la recherche
                SearchResult result = searcher.FindOne();

                if (result != null)
                {
                    // Récupérer le displayName
                    if (result.Properties.Contains("displayName") && result.Properties["displayName"].Count > 0)
                    {
                        string displayNameValue = result.Properties["displayName"][0]?.ToString();

                        if (!string.IsNullOrWhiteSpace(displayNameValue))
                        {
                            userInfo.DisplayName = displayNameValue;
                            userInfo.HasValidDisplayName = true;
                            System.Diagnostics.Debug.WriteLine($"DisplayName trouvé: {displayNameValue}");
                        }
                        else
                        {
                            // DisplayName vide ou null - utiliser la valeur par défaut
                            userInfo.DisplayName = "John Doe";
                            userInfo.HasValidDisplayName = false;
                        }
                    }
                    else
                    {
                        // DisplayName non trouvé - utiliser la valeur par défaut
                        userInfo.DisplayName = "John Doe";
                        userInfo.HasValidDisplayName = false;
                    }

                    // Récupérer la date de naissance (dateOfBirth)
                    if (result.Properties.Contains("dateOfBirth") && result.Properties["dateOfBirth"].Count > 0)
                    {
                        string birthDateValue = result.Properties["dateOfBirth"][0]?.ToString();

                        if (!string.IsNullOrWhiteSpace(birthDateValue))
                        {
                            userInfo.BirthDate = birthDateValue;

                            try
                            {
                                DateTime birthDate;

                                // Tenter de parser selon différents formats
                                if (DateTime.TryParseExact(birthDateValue, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                                          DateTimeStyles.None, out birthDate) ||
                                    DateTime.TryParseExact(birthDateValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                                                          DateTimeStyles.None, out birthDate) ||
                                    DateTime.TryParseExact(birthDateValue, "MM/dd/yyyy", CultureInfo.InvariantCulture,
                                                          DateTimeStyles.None, out birthDate))
                                {
                                    // Calculer l'âge
                                    userInfo.Age = CalculateAge(birthDate);
                                    userInfo.HasValidBirthDate = true;

                                    // Standardiser le format de date
                                    userInfo.BirthDate = birthDate.ToString("dd/MM/yyyy");

                                    System.Diagnostics.Debug.WriteLine($"Date de naissance standardisée: {userInfo.BirthDate}");
                                    System.Diagnostics.Debug.WriteLine($"Âge calculé: {userInfo.Age} ans");
                                }
                                else
                                {
                                    // Si le format n'est pas reconnu, utiliser l'âge par défaut
                                    userInfo.Age = 90;
                                    userInfo.HasValidBirthDate = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Erreur lors du traitement de la date: {ex.Message}. Âge par défaut: 90 ans");
                                userInfo.Age = 90;
                                userInfo.HasValidBirthDate = false;
                            }
                        }
                        else
                        {
                            // Date de naissance vide ou null - utiliser l'âge par défaut
                            userInfo.Age = 90;
                            userInfo.HasValidBirthDate = false;
                        }
                    }
                    else
                    {
                        // Attribut dateOfBirth non trouvé - utiliser l'âge par défaut
                        userInfo.Age = 90;
                        userInfo.HasValidBirthDate = false;
                    }
                }
                else
                {
                    // Aucun utilisateur trouvé - utiliser les valeurs par défaut
                    userInfo.DisplayName = "John Doe";
                    userInfo.Age = 90;
                    userInfo.HasValidDisplayName = false;
                    userInfo.HasValidBirthDate = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la récupération des informations utilisateur: {ex.Message}");
                // En cas d'erreur - utiliser les valeurs par défaut
                userInfo.DisplayName = "John Doe";
                userInfo.Age = 90;
                userInfo.HasValidDisplayName = false;
                userInfo.HasValidBirthDate = false;
            }

            return userInfo;
        }

        private static int CalculateAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            // Vérifier si l'anniversaire est déjà passé cette année
            if (birthDate.Date > today.AddYears(-age))
                age--;

            return age;
        }
    }
}