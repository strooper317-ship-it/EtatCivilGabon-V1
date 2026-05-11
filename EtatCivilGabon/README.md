# EtatCivilGabon — Phase 1 : Initialisation

## Structure du projet

```
EtatCivilGabon/
├── Controllers/              (Phase 2+)
├── Models/
│   ├── Enums.cs              ← StatutDemande, RoleUtilisateur
│   ├── Utilisateur.cs        ← Extension IdentityUser
│   ├── TypeActe.cs
│   ├── Demande.cs
│   ├── PieceJointe.cs
│   └── Notification.cs
├── ViewModels/               (Phase 2+)
├── Views/                    (Phase 2+)
├── Data/
│   ├── AppDbContext.cs       ← DbContext + seed types d'actes
│   ├── DbInitializer.cs      ← Rôles + admin par défaut
│   └── Migrations/
│       └── InitialCreate.cs  ← Migration de référence
├── Services/                 (Phase 3+)
├── wwwroot/                  (Phase 2+)
├── Program.cs                ← Point d'entrée
├── appsettings.json          ← Config BDD + mail
└── EtatCivilGabon.csproj     ← Packages NuGet
```

---

## Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/fr-fr/sql-server) ou LocalDB (inclus avec Visual Studio)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou VS Code

---

## Démarrage rapide

### 1. Cloner / ouvrir le projet
```bash
cd EtatCivilGabon
```

### 2. Configurer la base de données
Dans `appsettings.json`, la chaîne de connexion par défaut utilise **LocalDB** :
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EtatCivilGabonDb;Trusted_Connection=True"
```
Pour SQL Server Express, remplacer par :
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=EtatCivilGabonDb;Trusted_Connection=True"
```

### 3. Créer et appliquer la migration
```bash
# Depuis le dossier du projet
dotnet ef migrations add InitialCreate
dotnet ef database update
```
Ou dans le **Package Manager Console** de Visual Studio :
```powershell
Add-Migration InitialCreate
Update-Database
```

### 4. Lancer l'application
```bash
dotnet run
```
L'application s'ouvre sur `https://localhost:5001`

---

## Comptes créés automatiquement au démarrage

| Rôle           | Email                    | Mot de passe   |
|----------------|--------------------------|----------------|
| Administrateur | admin@etatcivil.ga       | Admin@2024!    |
| Agent          | agent@etatcivil.ga       | Agent@2024!    |

> ⚠️ Changer ces mots de passe en production.

---

## Types d'actes initialisés (cahier des charges §2.3)

| Code     | Type d'acte      | Délai cible   |
|----------|------------------|---------------|
| ACT-NAI  | Acte de naissance | 5 jours ouvrés|
| ACT-MAR  | Acte de mariage   | 7 jours ouvrés|
| ACT-DEC  | Acte de décès     | 5 jours ouvrés|

---

## Packages NuGet installés

| Package                                          | Usage                        |
|--------------------------------------------------|------------------------------|
| Microsoft.EntityFrameworkCore.SqlServer 8.0.0    | ORM base de données          |
| Microsoft.EntityFrameworkCore.Tools 8.0.0        | Commandes migrations         |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | Authentification + rôles    |
| MailKit 4.3.0                                    | Notifications email (Phase 5)|
| Microsoft.EntityFrameworkCore.Sqlite 8.0.0       | BDD légère pour dev/tests    |

---

## Prochaines phases

- **Phase 2** — Authentification : inscription, connexion, gestion des rôles
- **Phase 3** — Module Citoyen : formulaire de demande, upload, suivi
- **Phase 4** — Module Agent : back-office, changement de statut
- **Phase 5** — Notifications : emails automatiques via MailKit
- **Phase 6** — Administration : tableau de bord, statistiques, export CSV
- **Phase 7** — Tests & Livraison
