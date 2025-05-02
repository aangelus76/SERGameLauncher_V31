using System;
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
            public int Age { get; set; } = 3;
            public bool HasValidBirthDate { get; set; } = false;
            public bool HasValidDisplayName { get; set; } = false;
        }

        public static UserInfo GetUserInfo(string username)
        {
            UserInfo userInfo = new UserInfo();

            try
            {
                // Pour des raisons de simplicité, nous utilisons des valeurs par défaut
                // Dans un environnement réel, il faudrait implémenter l'accès à Active Directory
                // ou une autre source de données utilisateur

                userInfo.DisplayName = Environment.UserName; // Utiliser le nom d'utilisateur actuel
                userInfo.HasValidDisplayName = true;

                // Simuler une date de naissance (pour test uniquement)
                // En production, la date réelle serait récupérée depuis AD ou une DB
                Random random = new Random();
                int age = random.Next(80, 99); // Âge aléatoire entre 3 et 17 ans
                userInfo.Age = age;

                // Calculer une date de naissance à partir de l'âge
                DateTime birthDate = DateTime.Today.AddYears(-age);
                userInfo.BirthDate = birthDate.ToString("dd/MM/yyyy");
                userInfo.HasValidBirthDate = true;

                System.Diagnostics.Debug.WriteLine($"Informations utilisateur simulées : {userInfo.DisplayName}, {userInfo.Age} ans");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la récupération des informations utilisateur: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("Utilisation des valeurs par défaut.");
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