# SERGamesLauncher_V31

Un lanceur unifié pour différentes plateformes de jeux, développé en C# avec WPF.

![Logo du Launcher](Images/IconeLauncher.ico)

## Contexte du projet

Le SERGamesLauncher est un launcher centralisé qui permet d'accéder à différentes plateformes de jeux depuis une interface unique. Ce projet a été développé pour Saint-Étienne-du-Rouvray afin de faciliter la gestion et l'accès aux jeux dans un environnement contrôlé. 

Le launcher intègre les plateformes suivantes :
- Steam
- Epic Games
- Roblox
- CrazyGames (site web)
- BoardGameArena (site web)
- Xbox Game Pass

## Principe de fonctionnement

Le launcher fonctionne selon deux principes clés :

1. **Mode utilisateur** : 
   - Interface simple permettant de lancer les différentes plateformes
   - Certaines plateformes peuvent utiliser des identifiants pré-configurés (ex: Steam)
   - D'autres requièrent l'utilisation d'un compte personnel (ex: Epic Games)

2. **Mode administrateur** :
   - Protégé par mot de passe
   - Permet la gestion des comptes Steam pour l'injection automatique
   - Configuration de la visibilité des plateformes dans l'interface
   - Gestion des chemins d'applications
   - Gestion des permissions de dossiers (à implémenter)

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
│   └── PasswordDialog.xaml.cs
├── Panel/                    # Panneau d'administration
│   ├── AdminPanelWindow.xaml # Fenêtre principale d'administration
│   ├── ButtonView/           # Gestion de la visibilité des boutons
│   ├── Steam/                # Gestion des comptes Steam
│   └── Path/                 # Gestion des chemins d'applications
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

### Stockage des données

- Utilisation de fichiers XML pour les configurations de visibilité et de chemins
- Utilisation de JSON pour les comptes Steam (cryptés)
- Stockage dans un dossier `Config` à côté de l'exécutable

### Injection d'identifiants

Fonctionnalité permettant de démarrer automatiquement Steam avec des identifiants préconfigurés, associés au nom de l'ordinateur.

## Étapes réalisées

1. **Interface principale** :
   - Design complet de l'interface avec thème sombre personnalisé
   - Système de navigation entre les différentes plateformes
   - Affichage conditionnel des messages selon la plateforme

2. **Système d'authentification** :
   - Dialogue de mot de passe sécurisé pour l'administration
   - Protection des opérations sensibles (modification, suppression)
   - Authentification requise pour la fermeture de l'application

3. **Panneau d'administration** :
   - Interface de gestion avec navigation par onglets
   - Gestion des comptes Steam avec chiffrement
   - Configuration de la visibilité des plateformes
   - Gestion des chemins d'applications

4. **Composants personnalisés** :
   - Fenêtres sans bordures avec style unifié
   - Boîtes de dialogue personnalisées
   - Styles réutilisables pour l'ensemble de l'application

## Étapes à réaliser

1. **Système de lancement des applications** :
   - Implémentation complète du lancement des applications locales
   - Intégration avec les chemins d'applications configurés
   - Injection automatique des identifiants Steam

2. **Gestion des permissions dossier** :
   - Interface pour la configuration des permissions
   - Application des restrictions sur les dossiers spécifiés
   - Vérification des droits d'accès

3. **Détection automatique des installations** :
   - Recherche automatique des chemins d'installation des jeux
   - Détection des plateformes installées
   - Proposition d'ajout automatique

4. **Améliorations futures** :
   - Système de plugins pour ajouter facilement de nouvelles plateformes
   - Statistiques d'utilisation
   - Mise à jour automatique du launcher
   - Personnalisation avancée de l'interface

## Utilisation du projet

### Prérequis
- .NET Framework 4.8
- Windows 7 ou supérieur
- Visual Studio 2019 ou supérieur pour le développement

### Compilation
1. Ouvrir la solution `SERGamesLauncher_V31.sln` dans Visual Studio
2. Restaurer les packages NuGet si nécessaire
3. Compiler la solution en mode Debug ou Release

### Authentification administrateur
Le mot de passe par défaut pour l'accès administrateur est : `admin`

## Licence et crédits

Développé par Belcomb pour Saint-Étienne-du-Rouvray.

© 2025 Tous droits réservés.