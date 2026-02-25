# SERGamesLauncher_V31

Un lanceur unifié pour différentes plateformes de jeux, développé en C# avec WPF pour Saint-Étienne-du-Rouvray.

![Logo du Launcher](Images/IconeLauncher.ico)

## Vue d'ensemble

Le SERGamesLauncher est un lanceur centralisé qui permet d'accéder à différentes plateformes de jeux depuis une interface unique. Ce projet a été développé pour Saint-Étienne-du-Rouvray afin de faciliter la gestion et l'accès aux jeux dans un environnement contrôlé avec des fonctionnalités de sécurité avancées.

### Plateformes supportées
- **Steam** (avec gestion automatique des comptes)
- **Epic Games**
- **Roblox** (avec mise à jour automatique optimisée)
- **CrazyGames** (site web)
- **BoardGameArena** (site web)
- **Xbox Game Pass**
- **Bouton personnalisé** (configurable dynamiquement)

## Fonctionnalités principales

### Interface utilisateur
- **Mode utilisateur** : Interface simple pour lancer les plateformes
- **Mode administrateur** : Panneau de configuration avancé (protégé par mot de passe)
- **Mode Admin Windows** : Bypass automatique des mots de passe si lancé en admin Windows
- **Design moderne** : Thème sombre avec coins arrondis et effets visuels
- **Navigation intuitive** : Sélection de plateforme avec aperçu et instructions
- **Indicateur de rôle** : Affichage du mode (Utilisateur/Admin) dans l'interface avec couleur

### Bouton personnalisé dynamique
- **Configuration via panneau admin** : Création de bouton sans recompilation
- **Support URL et exécutables** : Lance un site web ou une application
- **Système de placeholders** : Variables dynamiques remplacées au lancement
- **Token chiffré AES-128** : Authentification sécurisée vers serveurs externes
- **Image personnalisée** : Upload et redimensionnement automatique

#### Placeholders disponibles
| Placeholder | Description |
|-------------|-------------|
| `{FULLNAME}` | Nom complet depuis Active Directory |
| `{AGE}` | Âge calculé depuis la date de naissance AD |
| `{PC}` | Nom du poste (Environment.MachineName) |
| `{TIMESTAMP}` | Timestamp Unix en secondes |
| `{TOKEN}` | Données chiffrées AES-128 (URL-encoded) |

#### Exemple d'utilisation
```
URL: https://monsite.com/jeu?auth={TOKEN}&user={FULLNAME}
Format token: {FULLNAME},{AGE},{PC},{TIMESTAMP}
```

### Mode Administrateur Windows
Lorsque le programme est lancé avec des privilèges administrateur Windows :
- **Bypass mot de passe** : Accès direct au panneau admin
- **Fermeture sans mot de passe** : Fermeture directe de l'application
- **Cooldown réduit** : 10 secondes au lieu de 30 après un lancement
- **Toggles Steam sans mot de passe** : Activation directe des comptes perso/mises à jour
- **Indicateur visuel** : "Mode : Admin" en vert dans l'interface

### Gestion de Steam avancée
- **Comptes préconfigurés** : Injection automatique des identifiants par poste
- **Surveillance de session** : Détection des changements de compte non autorisés
- **Redémarrage automatique** : Force l'utilisation du compte configuré
- **Mode administrateur** : Option pour autoriser les comptes personnels
- **Contrôle des mises à jour** : Option pour bloquer/autoriser les mises à jour Steam
- **Notifications de mise à jour** : Système intelligent de vérification des mises à jour de jeux Steam
- **Conservation des états** : Les toggles restent activés lors des changements de page

### Système de mise à jour Roblox optimisé
- **Service de mise à jour automatique** (`RobloxUpdateService`) : Mise à jour intelligente de Roblox
- **Téléchargement en parallèle** : Jusqu'à 20 téléchargements simultanés pour une mise à jour rapide
- **Gestion des versions** : Détection automatique des nouvelles versions via l'API Roblox
- **Récupération des erreurs** : Multiples serveurs de secours pour assurer la fiabilité
- **Nettoyage automatique** : Suppression des anciennes versions pour libérer l'espace disque
- **Mapping précis des packages** : Plus de 25 types de packages Roblox gérés automatiquement

### Gestion du démarrage configurable
- **Service de configuration de démarrage** (`StartupConfigService`) : Contrôle avancé du démarrage
- **Délai de démarrage** : Option de démarrage silencieux avec délai configurable (5 sec par défaut)
- **Mode focus intelligent** : Contrôle précis du focus de fenêtre avec tentatives multiples
- **Configuration XML** : Stockage des paramètres dans `Config/startupConfig.xml`

### Sécurité et contrôle d'accès
- **Authentification administrateur** : Accès protégé par SHA-256
- **Chiffrement AES-128** : Protection des mots de passe Steam et tokens
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
- **Bouton personnalisé** : Configuration complète d'un bouton dynamique

## Architecture technique

### Technologies utilisées
- **.NET Framework 4.8**
- **WPF (Windows Presentation Foundation)**
- **Chiffrement AES-128** pour les données sensibles et tokens
- **SHA-256** pour l'authentification
- **XML/JSON** pour le stockage des configurations
- **Active Directory** pour les informations utilisateur
- **HttpClient optimisé** pour les mises à jour Roblox
- **Compression ZIP** pour l'extraction des packages

### Structure du projet
```
SERGamesLauncher_V31/
├── Main/                          # Interface principale
│   ├── MainWindow.xaml            # Fenêtre principale
│   ├── PlateformView/             # Vues des plateformes
│   └── CustomMessageBox/          # Boîtes de dialogue personnalisées
├── Panel/                         # Panneau d'administration
│   ├── AdminPanel.xaml            # Interface admin
│   ├── ButtonView/                # Configuration visibilité
│   ├── Path/                      # Configuration chemins
│   ├── Steam/                     # Configuration Steam
│   ├── FolderPermissions/         # Permissions dossiers
│   ├── AgeControl/                # Contrôle parental
│   ├── SilentMode/                # Mode silencieux
│   └── CustomButton/              # Configuration bouton personnalisé
├── Services/                      # Services métier
│   ├── RobloxUpdateService.cs     # Mises à jour Roblox
│   ├── SteamUpdateChecker.cs      # Vérification mises à jour Steam
│   └── CustomButtonService.cs     # Gestion bouton personnalisé
├── Custom/                        # Utilitaires
│   ├── CustomWindow.cs            # Fenêtre personnalisée avec support admin
│   ├── StartupConfigService.cs    # Configuration démarrage
│   └── VersionUtility.cs          # Gestion versions
└── Config/                        # Fichiers de configuration
    ├── adminPassword.txt          # Mot de passe admin (chiffré)
    ├── steamAccounts.xml          # Comptes Steam (chiffrés)
    ├── platformVisibility.xml     # Visibilité plateformes
    ├── pathConfigs.xml            # Chemins applications
    ├── folderPermissions.xml      # Permissions dossiers
    ├── processRestrictions.xml    # Restrictions processus
    ├── silentModeSchedule.json    # Planning silencieux
    ├── startupConfig.xml          # Configuration démarrage
    └── CustomButton/              # Configuration bouton personnalisé
        └── customButton.json      # Paramètres du bouton
```

### Stockage des configurations
- **Mot de passe admin** : Texte chiffré AES (`Config/adminPassword.txt`)
- **Comptes Steam** : XML chiffré (`Config/steamAccounts.xml`)
- **Visibilité plateformes** : XML (`Config/platformVisibility.xml`)
- **Chemins applications** : XML (`Config/pathConfigs.xml`)
- **Permissions dossiers** : XML (`Config/folderPermissions.xml`)
- **Restrictions processus** : XML (`Config/processRestrictions.xml`)
- **Planning silencieux** : JSON (`Config/silentModeSchedule.json`)
- **Configuration démarrage** : XML (`Config/startupConfig.xml`)
- **Bouton personnalisé** : JSON (`Config/CustomButton/customButton.json`)

## Protocole de chiffrement des tokens

Le système utilise AES-128-CBC pour chiffrer les tokens d'authentification.

### Paramètres de chiffrement
| Paramètre | Valeur |
|-----------|--------|
| Algorithme | AES-128-CBC |
| Clé | `XX??XX` |
| Sel | Clé inversée en UTF-16LE |
| Dérivation | PBKDF2, 1000 itérations, SHA256 |
| Encoding données | UTF-8 |
| Encoding transport | Base64 → URL-encoded |

### Exemple de déchiffrement Node.js
```javascript
const crypto = require('crypto');

function decryptToken(encryptedToken) {
    const key = "XX??XX";
    const reversed = key.split('').reverse().join('');
    const salt = Buffer.from(reversed, 'utf16le');
    
    const derived = crypto.pbkdf2Sync(key, salt, 1000, 32, 'sha256');
    const aesKey = derived.slice(0, 16);
    const iv = derived.slice(16, 32);
    
    const decoded = decodeURIComponent(encryptedToken);
    const encryptedBytes = Buffer.from(decoded, 'base64');
    
    const decipher = crypto.createDecipheriv('aes-128-cbc', aesKey, iv);
    let decrypted = decipher.update(encryptedBytes);
    decrypted = Buffer.concat([decrypted, decipher.final()]);
    
    return decrypted.toString('utf8');
}
```

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

### Gestion avancée des mises à jour Roblox
```csharp
// Vérification et mise à jour automatique de Roblox
var result = await RobloxUpdateService.CheckAndUpdateAsync();
if (result.Success && result.UpdatePerformed) {
    // Nouvelle version installée
    string newPath = result.NewPath;
}

// Nettoyage des anciennes versions
RobloxUpdateService.CleanupOldVersions(versionsToKeep: 2);

// Vérification de disponibilité de mise à jour sans téléchargement
bool updateAvailable = await RobloxUpdateService.IsUpdateAvailableAsync();
```

### Vérification des mises à jour Steam
```csharp
// Vérification intelligente des mises à jour Steam avec cache
var updates = await SteamUpdateChecker.GetUpdatesAsync();
if (updates.Any()) {
    // Affichage des notifications de mise à jour
    foreach (var update in updates) {
        Console.WriteLine($"{update.Name}: {update.OldBuild} -> {update.NewBuild}");
    }
}
```

### Bouton personnalisé avec token chiffré
```csharp
// Génération d'un token chiffré avec les infos utilisateur
var userInfo = UserPlaceholderInfo.CreateCurrent("Jean Dupont", 15);
string url = "https://site.com?auth={TOKEN}";
string finalUrl = CustomButtonService.ReplacePlaceholders(url, userInfo, config);
// Résultat : https://site.com?auth=Kx7mN2pQ8v...
```

## Installation et configuration

### Prérequis
- **Windows 7** ou supérieur
- **.NET Framework 4.8**
- **Droits administrateur** (pour certaines fonctionnalités)
- **Active Directory** (optionnel, pour le contrôle parental)
- **Connexion Internet** (pour les mises à jour Roblox automatiques)

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
6. **Bouton personnalisé** : Panel Admin → Bouton personnalisé

## Utilisation

### Interface utilisateur
1. **Sélection de plateforme** : Clic sur le bouton de gauche
2. **Acceptation des règles** : Case à cocher obligatoire
3. **Lancement** : Bouton "Lancer" (cooldown de 30 secondes, 10s en mode admin)
4. **Navigation bloquée** : Pendant le lancement
5. **Indicateur de rôle** : Visualisation du niveau d'accès (Utilisateur/Admin)

### Administration
1. **Accès** : Bouton de configuration dans la barre de titre
2. **Authentification** : Saisie du mot de passe (bypass si admin Windows)
3. **Configuration** : Menu de gauche pour chaque section
4. **Sauvegarde automatique** : Modifications appliquées immédiatement

### Configuration du bouton personnalisé
1. **Activer le bouton** : Toggle dans le panneau admin
2. **Uploader une image** : Redimensionnement automatique
3. **Configurer les textes** : Nom, titre, sous-titre, instructions
4. **Définir la cible** : URL ou exécutable
5. **Activer le token** : Si authentification requise
6. **Définir le format** : `{FULLNAME},{AGE},{PC}` par exemple

### Mode silencieux
- **Configuration** : Panel Admin → Planning silencieux
- **Fonctionnement** : Démarrage minimisé selon les plages horaires
- **Granularité** : Matin/après-midi par jour de la semaine

## Sécurité et authentification

### Niveaux de sécurité
- **Utilisateur** : Accès aux plateformes autorisées uniquement
- **Administrateur Windows** : Bypass des mots de passe, cooldown réduit
- **Administrateur application** : Accès complet au panneau de configuration

### Mécanismes de protection
- **Instance unique** : Mutex empêchant les instances multiples
- **Fermeture protégée** : Authentification requise pour quitter (sauf admin Windows)
- **Surveillance continue** : Processus et sessions Steam
- **Chiffrement fort** : AES-128 pour les données sensibles et tokens
- **Hash sécurisé** : SHA-256 pour l'authentification
- **Démarrage sécurisé** : Contrôle du focus et du démarrage discret

## Développement et maintenance

### Technologies de développement
- **Visual Studio 2019+**
- **WPF Designer**
- **NuGet Packages** : System.Text.Json, System.Security.Cryptography, System.IO.Compression

### Compilation
```bash
# Configuration Release recommandée
dotnet build --configuration Release
```

### Debugging
- **Logs intégrés** : `System.Diagnostics.Debug.WriteLine`
- **Gestion d'erreurs** : Try-catch avec récupération gracieuse
- **Interface de debug** : Messages d'erreur utilisateur-friendly
- **Logs Roblox** : Fichier `roblox_update.log` pour le diagnostic des mises à jour

### Maintenance
- **Sauvegarde config** : Dossier `Config/` à conserver
- **Mise à jour** : Remplacement de l'exécutable principal
- **Logs système** : Event Viewer Windows pour diagnostics avancés
- **Nettoyage automatique** : Service de nettoyage des anciennes versions Roblox

## Dépannage

### Problèmes courants
- **Steam ne démarre pas** : Vérifier les chemins et comptes configurés
- **Permissions refusées** : Exécuter en tant qu'administrateur
- **Active Directory inaccessible** : Mode dégradé avec âge par défaut (90 ans)
- **Configuration corrompue** : Supprimer les fichiers du dossier `Config/`

### Dépannage Roblox
- **Échec de mise à jour** : Vérification automatique des serveurs de secours
- **Espace disque insuffisant** : Nettoyage automatique des anciennes versions
- **Connexion Internet** : Vérification de la connectivité avant mise à jour
- **Permissions Roblox** : Vérification des droits d'écriture dans le répertoire Roblox

### Dépannage bouton personnalisé
- **Bouton non visible** : Vérifier que le toggle est activé dans le panneau admin
- **Token invalide** : Vérifier le format et les placeholders utilisés
- **Image non affichée** : Formats supportés : PNG, JPG, BMP, GIF

### Problèmes de démarrage
- **Démarrage trop lent** : Ajuster le délai dans `startupConfig.xml`
- **Focus perdu** : Augmenter le nombre de tentatives de focus
- **Configuration corrompue** : Le service recrée automatiquement les paramètres par défaut

### Réinitialisation
1. Fermer l'application
2. Supprimer le dossier `Config/`
3. Redémarrer → Configuration par défaut restaurée

---

## Notes de fonctionnalités par build

> **Version du programme : 3.1** (constante)  
> **Build actuel : W49-25.C2**

### Build W23-25.F2 - Ajout du système de planning
> *Point de départ des notes de mise à jour*

#### Fonctionnalités ajoutées
- **Système de planning silencieux** (`SilentMode/`)
  - Service `SilentModeScheduleService.cs` pour la gestion de la configuration
  - Classes `SilentModeSchedule.cs` et `DaySchedule.cs` pour la structure des données
  - Interface `SilentModeScheduleControl.xaml` pour la configuration utilisateur
  - Stockage JSON dans `Config/silentModeSchedule.json`
  - Planning par jour avec créneaux matin/après-midi
  - Validation automatique des heures et correction des incohérences

#### Améliorations techniques
- Introduction du stockage JSON avec `System.Text.Json`
- Gestion avancée des plages horaires avec validation
- Interface utilisateur dédiée dans le panneau d'administration

---

### Build W32-25.D2 - Optimisations et nouvelles fonctionnalités

#### Fonctionnalités ajoutées
- **Service de mise à jour Roblox** (`Services/RobloxUpdateService.cs`)
  - Système complet de mise à jour automatique de Roblox
  - Téléchargement en parallèle (jusqu'à 20 connexions simultanées)
  - Support de 25+ types de packages Roblox avec mapping précis
  - API multiples avec serveurs de secours pour la fiabilité
  - Nettoyage automatique des anciennes versions
  - Buffer optimisé de 10MB pour les opérations de fichier

- **Service de configuration de démarrage** (`Custom/StartupConfigService.cs`)
  - Contrôle avancé du comportement au démarrage
  - Délai de démarrage configurable (5 sec par défaut)
  - Gestion intelligente du focus de fenêtre avec tentatives multiples
  - Configuration via XML dans `Config/startupConfig.xml`
  - Réinitialisation automatique en cas de configuration corrompue

#### Améliorations de l'interface
- **Indicateur de rôle utilisateur** dans `MainWindow.xaml`
  - Affichage du mode (Utilisateur/Administrateur)
  - Informations utilisateur enrichies (nom, âge, rôle)

#### Améliorations techniques
- **Compression et extraction optimisée**
  - Ajout des références `System.IO.Compression` et `System.IO.Compression.FileSystem`
  - Gestion parallèle de l'extraction des packages ZIP
- **HttpClient optimisé** pour les mises à jour réseau
- **Gestion d'erreurs renforcée** avec récupération gracieuse
- **Logging avancé** avec fichier `roblox_update.log`

#### Nouvelles classes et services
- `Services/RobloxUpdateService.cs` - Gestion complète des mises à jour Roblox
- `Custom/StartupConfigService.cs` - Configuration du démarrage de l'application

#### Fichiers de configuration nouveaux
- `Config/startupConfig.xml` - Paramètres de démarrage
- Logs de mise à jour Roblox dans le répertoire de base

---

### Build W33-25.B2 - Notifications Steam et améliorations UX

#### Fonctionnalités ajoutées
- **Service de vérification des mises à jour Steam** (`Services/SteamUpdateChecker.cs`)
  - Système intelligent de cache avec checksum pour éviter les vérifications inutiles
  - Vérification automatique des mises à jour de jeux Steam installés
  - Cache journalier avec détection de changements dans les jeux installés
  - Exclusion automatique des redistributables et outils (ex: Steamworks Common)
  - Gestion des timeouts et récupération d'erreurs gracieuse

- **Notifications interactives de mise à jour Steam** dans `PlatformContentControl.xaml`
  - Notification cliquable affichant le nombre de mises à jour disponibles
  - Interface détaillée listant jusqu'à 15 jeux avec mises à jour
  - Gestion intelligente des pluriels et messages d'état
  - Indicateur de chargement pendant la vérification
  - Gestion d'erreurs avec possibilité de réessayer

#### Améliorations de l'interface
- **CustomMessageBox avec tailles personnalisées**
  - Support des dimensions fixes pour un meilleur affichage
  - Optimisation pour l'affichage des listes de mises à jour Steam
  - Amélioration de la présentation des messages d'erreur

- **Intégration Steam enrichie**
  - Vérification automatique des mises à jour lors de l'affichage de Steam
  - Interface utilisateur plus réactive avec états de chargement
  - Messages contextuels selon le nombre de mises à jour disponibles

#### Améliorations techniques
- **Cache intelligent** avec système de checksum MD5
  - Détection automatique des changements dans la bibliothèque Steam
  - Évite les vérifications réseau inutiles
  - Persistence des données entre les sessions

- **Optimisations performance**
  - Timeout configuré à 10 secondes pour les requêtes HTTP
  - Gestion asynchrone complète pour éviter le blocage de l'interface
  - Nettoyage automatique des ressources HttpClient

#### Nouvelles classes et services
- `Services/SteamUpdateChecker.cs` - Service complet de vérification des mises à jour Steam
- Classe `GameUpdate` - Modèle pour les informations de mise à jour
- Classe `CheckCache` - Gestion du cache intelligent avec checksum

#### Fichiers de configuration nouveaux
- `Config/steamCheck.json` - Cache des vérifications Steam avec métadonnées

---

### Build W49-25.C2 - Mode Admin Windows et Bouton Personnalisé

#### Fonctionnalités ajoutées
- **Mode Administrateur Windows** (`Custom/CustomWindow.cs`)
  - Détection automatique des privilèges admin Windows
  - Bypass du mot de passe pour l'accès au panneau admin
  - Bypass du mot de passe pour la fermeture de l'application
  - Indicateur visuel "Mode : Admin" en vert dans l'interface

- **Cooldown adaptatif** (`Main/PlateformView/PlatformContentControl.xaml.cs`)
  - 10 secondes de cooldown en mode admin Windows
  - 30 secondes de cooldown en mode utilisateur standard
  - Bypass mot de passe pour toggles Steam (comptes perso/mises à jour) en mode admin

- **Système de bouton personnalisé** (`Panel/CustomButton/`, `Services/CustomButtonService.cs`)
  - Interface de configuration complète dans le panneau admin
  - Support URL et exécutables avec arguments
  - Système de placeholders dynamiques : `{FULLNAME}`, `{AGE}`, `{PC}`, `{TIMESTAMP}`, `{TOKEN}`
  - Chiffrement AES-128 des tokens pour authentification externe
  - Upload d'image avec redimensionnement automatique (50x50 bouton, 200x200 page)
  - Configuration JSON dans `Config/CustomButton/customButton.json`

#### Améliorations de l'interface
- **Conservation des états Steam**
  - Les toggles (comptes perso, mises à jour) restent activés lors des changements de page
  - Synchronisation avec l'état du SteamActivityMonitor

- **Bouton dynamique dans le menu principal**
  - Apparaît entre "Règlement" et les plateformes si activé
  - Image et texte configurables depuis le panneau admin
  - Rafraîchissement en temps réel après modification

#### Nouvelles classes et services
- `Services/CustomButtonService.cs` - Gestion complète du bouton personnalisé
  - `CustomButtonConfig` - Classe de configuration
  - `UserPlaceholderInfo` - Structure pour les données utilisateur
  - Méthodes de chiffrement/déchiffrement AES-128
  - Gestion des images avec redimensionnement
- `Panel/CustomButton/CustomButtonControl.xaml` - Interface de configuration
- `Panel/CustomButton/CustomButtonControl.xaml.cs` - Logique de l'interface

#### Fichiers modifiés
- `Custom/CustomWindow.cs` - Support mode admin Windows
- `Main/MainWindow.xaml` - Ajout du bouton personnalisé
- `Main/MainWindow.xaml.cs` - Gestion du mode admin et rafraîchissement bouton
- `Main/PlateformView/PlatformContentControl.xaml.cs` - Cooldown adaptatif, conservation états toggles
- `Panel/AdminPanelWindow.xaml` - Ajout bouton "Bouton personnalisé"
- `Panel/AdminPanelWindow.xaml.cs` - Initialisation du contrôle CustomButton

#### Fichiers de configuration nouveaux
- `Config/CustomButton/customButton.json` - Configuration du bouton personnalisé
- `Config/CustomButton/custom_image.*` - Image du bouton (extension variable)

#### Protocole de déchiffrement externe
Documentation complète du protocole AES-128 pour déchiffrement côté serveur (Node.js, PHP, Python, etc.)

---

## Crédits et licence

- **Développeur** : Angelus76
- **Client** : Saint-Étienne-du-Rouvray
- **Version** : 3.1
- **Année** : 2025
- **Licence** : Tous droits réservés

### Informations de version
- **Version du programme** : 3.1 (constante)
- **Build actuel** : W49-25.C2 (Semaine 49, Année 2025, Jour C=Mercredi, Modification 2)
- **Distribution** : Production
- **Dernière mise à jour** : Mercredi 3 Décembre 2025

---

*Ce launcher respecte les règles de sécurité et les contraintes d'usage définies par Saint-Étienne-du-Rouvray pour l'accès contrôlé aux plateformes de jeux.*
