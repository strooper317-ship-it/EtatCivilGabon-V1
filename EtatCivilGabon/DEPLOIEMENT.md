# Guide de Déploiement — EtatCivilGabon

## Structure finale du projet

```
EtatCivilGabon/                     ← Application principale
├── Controllers/
│   ├── CompteController.cs         ← Inscription, connexion, déconnexion
│   ├── HomeController.cs           ← Page d'accueil + redirection par rôle
│   ├── CitoyenController.cs        ← Espace citoyen (demandes, suivi)
│   ├── AgentController.cs          ← Back-office agent (traitement dossiers)
│   └── AdminController.cs          ← Administration (stats, agents, export)
├── Models/
│   ├── Enums.cs                    ← StatutDemande, RoleUtilisateur
│   ├── Utilisateur.cs              ← Extension IdentityUser
│   ├── TypeActe.cs
│   ├── Demande.cs
│   ├── PieceJointe.cs
│   └── Notification.cs
├── ViewModels/
│   ├── CompteViewModels.cs
│   ├── CitoyenViewModels.cs
│   ├── AgentViewModels.cs
│   └── AdminViewModels.cs
├── Views/
│   ├── Shared/_Layout.cshtml       ← Layout tricolore Gabon
│   ├── Home/Index.cshtml
│   ├── Compte/                     ← Inscription, Connexion, ChangerMDP
│   ├── Citoyen/                    ← Index, NouvelleDemande, Detail, Suivi
│   ├── Agent/                      ← Index, Traiter
│   └── Admin/                      ← Index, Agents, CreerAgent, Demandes
├── Data/
│   ├── AppDbContext.cs
│   ├── DbInitializer.cs
│   └── Migrations/
├── Services/
│   ├── NumeroSuiviGenerator.cs
│   ├── IDemandeService.cs / DemandeService.cs
│   ├── INotificationService.cs / NotificationService.cs
│   └── MailSettings.cs
├── wwwroot/css/site.css
├── Program.cs
├── appsettings.json
└── EtatCivilGabon.csproj

EtatCivilGabon.Tests/               ← Projet de tests xUnit
├── Helpers/TestDbHelper.cs
├── Services/
│   ├── NumeroSuiviGeneratorTests.cs
│   └── DemandeServiceTests.cs
├── ViewModels/ViewModelValidationTests.cs
├── Models/ModeleTests.cs
├── Data/AppDbContextTests.cs
└── EtatCivilGabon.Tests.csproj
```

---

## Prérequis

| Outil | Version |
|---|---|
| .NET SDK | 8.0 ou supérieur |
| Visual Studio | 2022 (Community, Pro ou Enterprise) |
| SQL Server | Express 2019+ ou LocalDB |
| Git | Optionnel |

---

## 1. Installation locale (développement)

### Étape 1 — Ouvrir la solution
```
Fichier → Ouvrir → Projet/Solution → EtatCivilGabon.sln
```

### Étape 2 — Configurer la base de données
Éditer `appsettings.json` :
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EtatCivilGabonDb;Trusted_Connection=True"
}
```
Pour SQL Server Express :
```json
"Server=.\\SQLEXPRESS;Database=EtatCivilGabonDb;Trusted_Connection=True"
```

### Étape 3 — Créer la base de données
Dans le **Package Manager Console** (Outils → NuGet → Console) :
```powershell
Add-Migration InitialCreate
Update-Database
```
Ou en ligne de commande :
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Étape 4 — Configurer les e-mails (optionnel)
Éditer `appsettings.json` :
```json
"MailSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "UseSsl": true,
  "NomExpediteur": "État Civil Gabon",
  "EmailExpediteur": "votre@gmail.com",
  "MotDePasse": "votre_mot_de_passe_app_gmail"
}
```
> Pour Gmail : Paramètres → Sécurité → Validation en 2 étapes → Mots de passe d'application

### Étape 5 — Lancer l'application
```bash
dotnet run
# ou F5 dans Visual Studio
```
Accès : `https://localhost:5001`

---

## 2. Comptes par défaut

| Rôle | Email | Mot de passe |
|---|---|---|
| Administrateur | admin@etatcivil.ga | Admin@2024! |
| Agent de test | agent@etatcivil.ga | Agent@2024! |

> ⚠️ **Changer immédiatement ces mots de passe en production.**

---

## 3. Lancer les tests

```bash
# Depuis la racine de la solution
dotnet test

# Avec détail des résultats
dotnet test --verbosity normal

# Dans Visual Studio
Tester → Exécuter tous les tests (Ctrl+R, A)
```

### Couverture des tests
| Catégorie | Tests | Ce qui est testé |
|---|---|---|
| Services | 14 | NumeroSuiviGenerator, DemandeService (CRUD, filtres, stats) |
| ViewModels | 12 | Validation de tous les formulaires |
| Modèles | 10 | Propriétés, valeurs par défaut, enums, seed |
| DbContext | 7 | CRUD, cascade, contrainte unicité |
| **Total** | **43** | |

---

## 4. Déploiement en production (IIS / Windows Server)

### Étape 1 — Publier l'application
```bash
dotnet publish -c Release -o ./publish
```

### Étape 2 — Configurer IIS
1. Installer le module **ASP.NET Core Hosting Bundle**
2. Créer un site IIS pointant sur le dossier `./publish`
3. Pool d'applications : **No Managed Code**

### Étape 3 — Base de données production
Dans `appsettings.json` (ou variable d'environnement) :
```json
"DefaultConnection": "Server=MON_SERVEUR;Database=EtatCivilGabonDb;User Id=sa;Password=MOT_DE_PASSE;"
```

### Étape 4 — Appliquer les migrations
```bash
dotnet ef database update --connection "VOTRE_CHAINE_CONNEXION_PROD"
```

### Étape 5 — Variables sensibles
Ne jamais stocker les mots de passe dans `appsettings.json` en production.
Utiliser les **Variables d'environnement** ou **ASP.NET Core Secret Manager** :
```bash
dotnet user-secrets set "MailSettings:MotDePasse" "mon_mot_de_passe"
```

---

## 5. Checklist avant mise en production

- [ ] Mots de passe admin et agents changés
- [ ] Chaîne de connexion production configurée
- [ ] Configuration SMTP e-mail vérifiée
- [ ] HTTPS activé (certificat SSL)
- [ ] Dossier `wwwroot/uploads` créé et accessible en écriture
- [ ] Migrations appliquées sur la base de production
- [ ] Tests unitaires tous au vert (`dotnet test`)
- [ ] Sauvegarde automatique de la base configurée

---

## 6. Fonctionnalités livrées

| Phase | Module | Statut |
|---|---|---|
| 1 | Modèles & base de données | ✅ |
| 2 | Authentification (3 rôles) | ✅ |
| 3 | Module Citoyen | ✅ |
| 4 | Module Agent | ✅ |
| 5 | Notifications e-mail (MailKit) | ✅ |
| 6 | Administration & export CSV | ✅ |
| 7 | Tests & documentation | ✅ |
