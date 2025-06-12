# SERGamesLauncher_V31

Un lanceur unifié pour différentes plateformes de jeux, développé en C# avec WPF pour Saint-Étienne-du-Rouvray.

![Logo du Launcher](Images/IconeLauncher.ico)

## Vue d'ensemble

Le SERGamesLauncher est un lanceur centralisé qui permet d'accéder à différentes plateformes de jeux depuis une interface unique. Ce projet a été développé pour Saint-Étienne-du-Rouvray afin de faciliter la gestion et l'accès aux jeux dans un environnement contrôlé avec des fonctionnalités de sécurité avancées.

### Plateformes supportées
- **Steam** (avec gestion automatique des comptes)
- **Epic Games**
- **Roblox**
- **CrazyGames** (site web)
- **BoardGameArena** (site web)
- **Xbox Game Pass**

## Fonctionnalités principales

### Interface utilisateur
- **Mode utilisateur** : Interface simple pour lancer les plateformes
- **Mode administrateur** : Panneau de configuration avancé (protégé par mot de passe)
- **Design moderne** : Thème sombre avec coins arrondis et effets visuels
- **Navigation intuitive** : Sélection de plateforme avec aperçu et instructions

### Gestion de Steam avancée
- **Comptes préconfigurés** : Injection automatique des identifiants par poste
- **Surveillance de session** : Détection des changements de compte non autorisés
- **Redémarrage automatique** : Force l'utilisation du compte configuré
- **Mode administrateur** : Option pour autoriser les comptes personnels
- **Contrôle des mises à jour** : Option pour bloquer/autoriser les mises à jour Steam

### Sécurité et contrôle d'accès
- **Authentification administrateur** : Accès protégé par SHA-256
- **Chiffrement AES** : Protection des mots de passe Steam
- **Contrôle parental par âge** : Restrictions basées sur l'âge utilisateur (Active Directory)
- **Protection de dossiers** : Contrôle des permissions (lecture seule, anti-suppression, anti-création)
- **Surveillance des processus** : Fermeture automatique des applications non autorisées
- **Mode silencieux programmable** : Démarrage discret selon planning horaire

### Fonctionnalités d'administration
- **Gestion des comptes Steam** : Configuration par poste avec chiffrement
- **Visibilité des plateformes** : Masquage sélectif des boutons
- **Chemins d'applications** : Configuration des exécutables et URLs
- **Permissions de dossiers** : Protection automatique au démarrage
- **Restrictions d'âge** : Configuration des processus interdits par âge
- **Planning silencieux** : Configuration des plages horaires de démarrage discret

## Architecture technique

### Technologies utilisées
- **.NET Framework 4.8**
- **WPF (Windows Presentation Foundation)**
- **Chiffrement AES** pour les données sensibles
- **SHA-256** pour l'authentification
- **XML/JSON** pour le stockage des configurations
- **Active Directory** pour les informations utilisateur

### Structure du projet
```
SERGamesLauncher_V31/
├── Main/                          # Interface principale
│   ├── App.xaml(.cs)             # Application et styles globaux
│   ├── MainWindow.xaml(.cs)      # Fenêtre principale
│   └── PlateformView/            # Contrôles de plateforme
├── Panel/                        # Panneau d'administration
│   ├── AdminPanelWindow.xaml(.cs) # Fenêtre d'administration
│   ├── Steam/                    # Gestion des comptes Steam
│   ├── ButtonView/               # Visibilité des plateformes
│   ├── Path/                     # Chemins d'applications
│   ├── PermissionFolders/        # Permissions de dossiers
│   ├── AgeControl/               # Contrôle parental
│   └── SilentMode/               # Planning silencieux
├── Custom/                       # Composants personnalisés
│   ├── CustomWindow.cs           # Classe de base des fenêtres
│   ├── CustomMessageBox.xaml(.cs) # Boîtes de dialogue
│   ├── PasswordDialog.xaml(.cs)  # Authentification
│   └── VersionUtility.cs         # Gestion des versions
└── Images/                       # Ressources graphiques
```

### Design et interface
- **Thème sombre cohérent** : Palette de couleurs professionnelle
- **Fenêtres sans bordures** : Design moderne avec barres de titre personnalisées
- **Animations et transitions** : Effets visuels pour une expérience fluide
- **Responsive** : Interface adaptée aux différentes résolutions
- **Accessibilité** : Contrastes appropriés et navigation au clavier

### Stockage des données
- **Comptes Steam** : JSON chiffré (`Config/steamAccounts.json`)
- **Visibilité plateformes** : XML (`Config/platformConfig.xml`)
- **Chemins applications** : XML (`Config/Path.xml`)
- **Permissions dossiers** : XML (`Config/FolderPermissions.xml`)
- **Restrictions processus** : XML (`Config/ProcessRestrictions.xml`)
- **Planning silencieux** : JSON (`Config/silentModeSchedule.json`)
- **Configuration démarrage** : XML (`Config/startupConfig.xml`)

## Fonctionnalités de sécurité avancées

### Protection des sessions Steam
```csharp
// Surveillance continue des comptes Steam
SteamActivityMonitor monitor = new SteamActivityMonitor();
monitor.UserChanged += (username) => {
    // Vérification du compte autorisé
    // Redémarrage automatique si nécessaire
};
```

### Contrôle parental intelligent
```csharp
// Récupération automatique de l'âge depuis AD
UserInfo userInfo = UserInfoRetriever.GetUserInfo(username);
ProcessMonitor monitor = new ProcessMonitor();
// Fermeture des applications non autorisées pour l'âge
```

### Protection de dossiers système
```csharp
// Application de permissions personnalisées
FolderPermissionService.ApplyProtection(folder);
// Types : ReadOnly, PreventDeletion, PreventCreation
```

## Installation et configuration

### Prérequis
- **Windows 7** ou supérieur
- **.NET Framework 4.8**
- **Droits administrateur** (pour certaines fonctionnalités)
- **Active Directory** (optionnel, pour le contrôle parental)

### Installation
1. Télécharger la dernière version
2. Exécuter l'installateur
3. Le raccourci bureau sera créé automatiquement
4. Configurer les chemins d'applications via le panneau admin

### Configuration initiale
1. **Mot de passe administrateur** : `admin` (par défaut)
2. **Configurer les chemins** : Panel Admin → Chemins d'applications
3. **Comptes Steam** : Panel Admin → Comptes Steam (par poste)
4. **Restrictions d'âge** : Panel Admin → Restrictions d'âge
5. **Permissions dossiers** : Panel Admin → Permissions dossiers

## Utilisation

### Interface utilisateur
1. **Sélection de plateforme** : Clic sur le bouton de gauche
2. **Acceptation des règles** : Case à cocher obligatoire
3. **Lancement** : Bouton "Lancer" (cooldown de 30 secondes)
4. **Navigation bloquée** : Pendant le lancement

### Administration
1. **Accès** : Bouton de configuration dans la barre de titre
2. **Authentification** : Saisie du mot de passe administrateur
3. **Configuration** : Menu de gauche pour chaque section
4. **Sauvegarde automatique** : Modifications appliquées immédiatement

### Mode silencieux
- **Configuration** : Panel Admin → Planning silencieux
- **Fonctionnement** : Démarrage minimisé selon les plages horaires
- **Granularité** : Matin/après-midi par jour de la semaine

## Sécurité et authentification

### Niveaux de sécurité
- **Utilisateur** : Accès aux plateformes autorisées uniquement
- **Administrateur local** : Indicateur visuel dans l'interface
- **Administrateur application** : Accès complet au panneau de configuration

### Mécanismes de protection
- **Instance unique** : Mutex empêchant les instances multiples
- **Fermeture protégée** : Authentification requise pour quitter
- **Surveillance continue** : Processus et sessions Steam
- **Chiffrement fort** : AES pour les données sensibles
- **Hash sécurisé** : SHA-256 pour l'authentification

## Développement et maintenance

### Technologies de développement
- **Visual Studio 2019+**
- **WPF Designer**
- **NuGet Packages** : System.Text.Json, System.Security.Cryptography

### Compilation
```bash
# Configuration Release recommandée
dotnet build --configuration Release
```

### Debugging
- **Logs intégrés** : `System.Diagnostics.Debug.WriteLine`
- **Gestion d'erreurs** : Try-catch avec récupération gracieuse
- **Interface de debug** : Messages d'erreur utilisateur-friendly

### Maintenance
- **Sauvegarde config** : Dossier `Config/` à conserver
- **Mise à jour** : Remplacement de l'exécutable principal
- **Logs système** : Event Viewer Windows pour diagnostics avancés

## Dépannage

### Problèmes courants
- **Steam ne démarre pas** : Vérifier les chemins et comptes configurés
- **Permissions refusées** : Exécuter en tant qu'administrateur
- **Active Directory inaccessible** : Mode dégradé avec âge par défaut (90 ans)
- **Configuration corrompue** : Supprimer les fichiers du dossier `Config/`

### Réinitialisation
1. Fermer l'application
2. Supprimer le dossier `Config/`
3. Redémarrer → Configuration par défaut restaurée

## Crédits et licence

- **Développeur** : Angelus76 (Anthony Colombel)
- **Client** : Saint-Étienne-du-Rouvray
- **Version** : 3.1
- **Année** : 2025
- **Licence** : Tous droits réservés

### Informations de version
- **Version courte** : V.3.1
- **Build** : Gestion automatique par semaine/jour/année
- **Distribution** : Production
- **Dernière mise à jour** : Date de compilation automatique

---

*Ce launcher respecte les règles de sécurité et les contraintes d'usage définies par Saint-Étienne-du-Rouvray pour l'accès contrôlé aux plateformes de jeux.*