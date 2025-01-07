# ecommerce-project-MVVM

## Fonctionnalités principales
- Gestion des produits (ajout, modification, suppression).
- Gestion des utilisateurs (connexion, inscription).
- Gestion des commandes.
- Panier d'achat.
- Tableau de bord (statistiques, gestion administrative).

## Technologies utilisées
- **Langage** : C#, .NET Core.
- **Base de données** : SQL Server.
- **Architecture** : MVVM.
- **Frameworks/bibliothèques** : Entity Framework, ASP.NET.

## Pré-requis
Environnement nécessaire pour exécuter le projet :
- Version de .NET SDK (ex. : .NET 6, .NET 7).
- SQL Server.
- Visual Studio.

## Installation et exécution

### 1. Clonez le dépôt
```bash
git clone https://github.com/Ayouboss02/ecommerce-project-MVVM.git
cd ecommerce-project-MVVM


### Installez les dépendances :
dotnet restore

### Configurez la base de données :
Ajouter une chaîne de connexion dans appsettings.json.
Appliquer les migrations.
dotnet ef database update

### Lancez l'application :
dotnet run
```

## Architecture du projet

Model : Définit les données et la logique métier.
View : Interface utilisateur (UI).
ViewModel : Intermédiaire entre le modèle et la vue.



