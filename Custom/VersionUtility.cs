using System;
using System.Reflection;
using System.IO;

namespace SERGamesLauncher_V31
{
    /// <summary>
    /// Classe utilitaire pour récupérer les informations de version de l'application
    /// </summary>
    public static class VersionUtility
    {
        // Cache de l'assembly pour éviter de le récupérer plusieurs fois
        private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();

        #region Propriétés de base
        /// <summary>
        /// Obtient l'objet Version complet
        /// </summary>
        public static DateTime BuildDate => File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
        /// <summary>
        /// Obtient le titre de l'application
        /// </summary>
        public static string Title => GetAssemblyAttribute<AssemblyTitleAttribute>(_assembly, a => a.Title);

        /// <summary>
        /// Obtient la description de l'application
        /// </summary>
        public static string Description => GetAssemblyAttribute<AssemblyDescriptionAttribute>(_assembly, a => a.Description);

        /// <summary>
        /// Obtient le nom de la société
        /// </summary>
        public static string Company => GetAssemblyAttribute<AssemblyCompanyAttribute>(_assembly, a => a.Company);

        /// <summary>
        /// Obtient le nom du produit
        /// </summary>
        public static string Product => GetAssemblyAttribute<AssemblyProductAttribute>(_assembly, a => a.Product);

        /// <summary>
        /// Obtient le copyright
        /// </summary>
        public static string Copyright => GetAssemblyAttribute<AssemblyCopyrightAttribute>(_assembly, a => a.Copyright);

        /// <summary>
        /// Obtient la marque
        /// </summary>
        public static string Trademark => GetAssemblyAttribute<AssemblyTrademarkAttribute>(_assembly, a => a.Trademark);

        /// <summary>
        /// Obtient la version informative (marketing)
        /// </summary>
        public static string InformationalVersion =>
            GetAssemblyAttribute<AssemblyInformationalVersionAttribute>(_assembly, a => a.InformationalVersion);

        #endregion

        #region Informations de version


        /// <summary>
        /// Obtient l'objet Version complet
        /// </summary>
        public static Version Version => _assembly.GetName().Version;

        /// <summary>
        /// Version majeure
        /// </summary>
        public static int MajorVersion => Version.Major;

        /// <summary>
        /// Version mineure
        /// </summary>
        public static int MinorVersion => Version.Minor;

        /// <summary>
        /// Numéro de build
        /// </summary>
        public static int BuildNumber => Version.Build;

        /// <summary>
        /// Numéro de révision
        /// </summary>
        public static int Revision => Version.Revision;

        /// <summary>
        /// Obtient le numéro de version formaté (ex: 3.1.0.0)
        /// </summary>
        public static string FormattedVersion => $"{MajorVersion}.{MinorVersion}.{BuildNumber}.{Revision}";

        /// <summary>
        /// Obtient le numéro de version court (ex: 3.1)
        /// </summary>
        public static string ShortVersion => $"{MajorVersion}.{MinorVersion}";

        #endregion

        #region Informations de build (FileVersion)

        /// <summary>
        /// Obtient la version du fichier complète
        /// </summary>
        public static string FileVersion =>
            GetAssemblyAttribute<AssemblyFileVersionAttribute>(_assembly, a => a.Version);

        /// <summary>
        /// Obtient le numéro de semaine du build
        /// </summary>
        public static string WeekNumber
        {
            get
            {
                string[] parts = FileVersion.Split('.');
                return parts.Length > 0 ? parts[0] : "0";
            }
        }

        /// <summary>
        /// Obtient le numéro de jour du build
        /// </summary>
        public static string DayNumber
        {
            get
            {
                string[] parts = FileVersion.Split('.');
                return parts.Length > 1 ? parts[1] : "0";
            }
        }

        /// <summary>
        /// Obtient l'année du build
        /// </summary>
        public static string YearNumber
        {
            get
            {
                string[] parts = FileVersion.Split('.');
                return parts.Length > 2 ? parts[2] : "0";
            }
        }

        /// <summary>
        /// Obtient le numéro de modification du build
        /// </summary>
        public static string ModificationNumber
        {
            get
            {
                string[] parts = FileVersion.Split('.');
                return parts.Length > 3 ? parts[3] : "0";
            }
        }

        /// <summary>
        /// Obtient le numéro de build formaté (W17.4.2025.31)
        /// </summary>
        public static string FormattedBuildNumber => $"W{WeekNumber}.{DayNumber}.{YearNumber}.{ModificationNumber}";

        #endregion

        #region Méthodes d'affichage

        /// <summary>
        /// Obtient le numéro de version pour l'affichage dans le titre des fenêtres
        /// </summary>
        public static string GetDisplayVersion()
        {
            return $"V.{MajorVersion}.{MinorVersion}.{ModificationNumber}";
        }

        /// <summary>
        /// Obtient un texte formaté avec toutes les informations pour la boîte "À propos"
        /// </summary>
        public static string GetAboutInformation()
        {
            return $"Créateur : {Company}\n" +
                   $"Version : {FormattedVersion}\n" +
                   $"Build : {FormattedBuildNumber}\n" +
                   $"Distribution : {InformationalVersion}\n" +
                   $"{Copyright}";
        }

        #endregion

        #region Méthodes auxiliaires

        /// <summary>
        /// Helper pour récupérer les attributs d'assembly
        /// </summary>
        private static string GetAssemblyAttribute<T>(Assembly assembly, Func<T, string> selector) where T : Attribute
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(T), false);
            if (attributes.Length == 0)
                return string.Empty;

            return selector((T)attributes[0]);
        }

        #endregion
    }
}