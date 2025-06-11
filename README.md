# SERGamesLauncher_V31

Un lanceur unifié pour différentes plateformes de jeux, développé en C# avec WPF pour Saint-Étienne-du-Rouvray.

![Logo du Launcher](Images/IconeLauncher.ico)

## Contexte du projet

Le SERGamesLauncher est un lanceur centralisé qui permet d'accéder à différentes plateformes de jeux depuis une interface unique. Ce projet a été développé pour Saint-Étienne-du-Rouvray afin de faciliter la gestion et l'accès aux jeux dans un environnement contrôlé. 

Le launcher intègre les plateformes suivantes :
- Steam
- Epic Games
- Roblox
- CrazyGames (site web)
- BoardGameArena (site web)
- Xbox Game Pass

## Fonctionnalités principales

Le launcher fonctionne selon deux principes clés :

1. **Mode utilisateur** : 
   - Interface simple permettant de lancer les différentes plateformes
   - Certaines plateformes peuvent utiliser des identifiants pré-configurés (ex: Steam)
   - D'autres requièrent l'utilisation d'un compte personnel (ex: Epic Games)
   - Système de contrôle parental par âge pour restreindre l'accès à certaines applications
   - Gestion des règles d'utilisation et acceptation de conditions

2. **Mode administrateur** :
   - Protégé par mot de passe
   - Gestion des comptes Steam pour l'injection automatique des identifiants
   - Configuration de la visibilité des plateformes dans l'interface
   - Gestion des chemins d'applications
   - Gestion des permissions de dossiers pour protéger les fichiers système
   - Contrôle d'âge pour les applications

## Fonctionnalités de sécurité

Le launcher intègre plusieurs mécanismes de sécurité avancés :

- **Authentification administrateur** : Accès protégé par mot de passe pour toutes les fonctions d'administration
- **Protection de session Steam** : Surveillance active des sessions Steam avec réauthentification automatique en cas de déconnexion
- **Gestion des permissions de dossiers** : Protection en lecture seule, anti-suppression ou anti-création pour les dossiers sensibles
- **Contrôle parental par âge** : Restriction d'accès aux applications basée sur l'âge de l'utilisateur connecté
- **Prévention multi-instances** : Une seule instance du launcher peut être exécutée à la fois
- **Chiffrement des données sensibles** : Cryptage AES pour les mots de passe des comptes Steam

## Structure du projet

```
SERGamesLauncher_V31/
├── Main/                     # Point d'entrée de l'application
│   ├── App.xaml              # Styles globaux et ressources
│   ├── App.xaml.cs           # Logique d'application
│   ├── MainWindow.xaml       # Interface principale
│   └── MainWindow.xaml.cs    # Code-behind interface principale
├── Custom/                   # Composants personnalisés
│   ├── Converters.cs         # Convertisseurs de valeurs
│   ├── CustomMessageBox.xaml # Boîte de dialogue personnalisée
│   ├── CustomWindow.cs       # Classe de base pour fenêtres personnalisées
│   ├── PasswordDialog.xaml   # Dialogue de mot de passe
│   └── VersionUtility.cs     # Utilitaire de gestion de version
├── Panel/                    # Panneau d'administration
│   ├── AdminPanelWindow.xaml # Fenêtre principale d'administration
│   ├── AgeControl/           # Gestion du contrôle parental par âge
│   ├── ButtonView/           # Gestion de la visibilité des boutons
│   ├── PermissionFolders/    # Gestion des permissions de dossiers
│   ├── Path/                 # Gestion des chemins d'applications
│   └── Steam/                # Gestion des comptes Steam
├── Images/                   # Ressources d'images
└── Properties/               # Propriétés du projet
```

## Spécificités techniques

### Style partagé et architecture visuelle

Le projet utilise une architecture visuelle cohérente avec :

1. **Thème sombre personnalisé** :
   - Fond principal : #474747
   - Zone de contenu : #1E1E1E
   - Panneau gauche : #242424
   - Barre de titre : #171717
   - Accents verts : #268531 (boutons)
   - Accents rouges : #821e1e (actions critiques)

2. **Styles centralisés** :
   - Tous les styles sont définis dans `App.xaml`
   - Styles réutilisables pour les boutons, bordures, textes
   - Design moderne avec coins arrondis et effets d'ombre

3. **Fenêtres personnalisées sans bordures Windows** :
   - Classe de base `CustomWindow` utilisée pour toutes les fenêtres
   - Implémentation personnalisée de la barre de titre
   - Gestion unifiée des événements de fenêtre

4. **Composants d'interface réutilisables** :
   - `CustomMessageBox` pour remplacer les MessageBox standard
   - `PasswordDialog` pour les authentifications
   - Contrôles utilisateur spécifiques (UserControls)

### Sécurité et chiffrement

- Authentification protégée par mot de passe pour accéder au panneau d'administration
- Hachage SHA-256 pour le stockage du mot de passe administrateur
- Chiffrement AES pour les mots de passe des comptes Steam
- Permissions contrôlées pour les dossiers sensibles
- Surveillance active des processus pour le contrôle parental

### Stockage des données

- Utilisation de fichiers XML pour les configurations de visibilité, chemins et permissions
- Utilisation de JSON pour les comptes Steam (cryptés)
- Stockage dans un dossier `Config` à côté de l'exécutable
- Séparation des configurations par type de données

### Injection d'identifiants

- Fonctionnalité permettant de démarrer automatiquement Steam avec des identifiants préconfigurés
- Association des comptes au nom de l'ordinateur
- Surveillance de session pour prévenir les déconnexions non autorisées
- Option de basculement en mode comptes personnels (protégée par mot de passe)

### Contrôle parental

- Récupération des informations utilisateur depuis l'Active Directory
- Restrictions d'accès basées sur l'âge pour les applications
- Surveillance en temps réel des processus en cours d'exécution
- Fermeture automatique des applications non autorisées pour l'âge de l'utilisateur

## Installation et prérequis

### Prérequis
- .NET Framework 4.8
- Windows 7 ou supérieur
- Visual Studio 2019 ou supérieur pour le développement
- Accès administrateur pour certaines fonctionnalités (permissions de dossiers)

### Installation
1. Télécharger la dernière version du SERGamesLauncher
2. Exécuter l'installateur et suivre les instructions
3. Configurer les chemins d'accès aux plateformes dans le panneau d'administration
4. Configurer les comptes Steam si nécessaire

### Authentification administrateur
Le mot de passe par défaut pour l'accès administrateur est : `admin`

## Détails d'implémentation

### Module de monitoring Steam
Le launcher intègre un système sophistiqué de surveillance des sessions Steam qui :
- Détecte le lancement et l'arrêt de Steam
- Identifie le compte connecté via l'analyse du fichier loginusers.vdf
- Force l'utilisation du compte configuré pour le poste si nécessaire
- Propose un mode administrateur permettant les comptes personnels

### Système de permissions de dossiers
Le système de protection de dossiers permet de :
- Appliquer une protection en lecture seule sur les dossiers sensibles
- Empêcher la suppression de fichiers et dossiers critiques
- Empêcher la création de nouveaux fichiers dans certains dossiers
- Restaurer automatiquement les protections au démarrage
- Vérifier périodiquement l'état des protections

### Contrôle d'âge des applications
- Surveillance constante des processus en cours d'exécution
- Comparaison avec une base de données d'applications restreintes par âge
- Détection de l'âge de l'utilisateur via Active Directory
- Fermeture automatique des applications non autorisées
- Interface d'administration pour configurer les restrictions

## Licence et crédits

Développé par Angelus76 pour Saint-Étienne-du-Rouvray.

© 2025 Tous droits réservés.