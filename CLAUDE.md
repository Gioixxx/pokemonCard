# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A WPF desktop application for managing Pokemon card collections, tracking sales, and analyzing profits. Built with .NET 6.0 targeting Windows, using Entity Framework Core with SQLite for data persistence and LiveCharts for data visualization.

## Build and Development Commands

### Standard Build
```powershell
# Build in Visual Studio: Build > Build Solution (Release configuration)
dotnet build -c Release
```

### Single-File Executable Build
```powershell
# Creates a single self-contained executable with all dependencies
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```
Output location: `bin\Release\net6.0-windows\win-x64\publish\PokemonCardManager.exe`

### Running the Application
```powershell
# Run from Visual Studio: F5 or Debug > Start Debugging
dotnet run
```

### Database Management
```powershell
# Add a new migration (after model changes)
dotnet ef migrations add MigrationName

# Apply migrations to database
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove
```

## Architecture

### Application Entry Point
- **App.xaml.cs**: Application startup, dependency injection configuration, and database initialization
  - Configures DbContext with SQLite database location: `%LocalAppData%\PokemonCardManager\pokemoncards.db`
  - Registers services using `Microsoft.Extensions.DependencyInjection`
  - Automatically runs EF Core migrations on startup

### Data Layer
- **Models/**: Domain entities
  - `Card`: Represents Pokemon cards with purchase tracking and calculated properties (TotalValue, EstimatedProfit, ROI)
  - `Sale`: Sales transactions with foreign key to Card and calculated NetProfit
- **Data/ApplicationDbContext.cs**: EF Core DbContext
  - Configures relationship between Sale and Card with restricted delete behavior
  - DbSets: Cards, Sales

### Service Layer (Dependency Injected)
Services follow interface-based pattern for testability and flexibility:
- **ICardService/CardService**: CRUD operations for cards
- **ISaleService/SaleService**: Sales management
- **IDataExportService/DataExportService**: CSV export and database backup functionality

All services are registered as Transient in DI container and receive `ApplicationDbContext` via constructor injection.

### Presentation Layer
- **MainWindow.xaml/.cs**: Navigation shell with side menu
  - Uses Frame navigation to load different views
  - Manages active button state for visual feedback
- **Views/**: Feature-specific user controls
  - `InventoryView`: Card collection management
  - `SalesView`: Sales recording and tracking
  - `DashboardView`: Statistics and charts (using LiveCharts)
  - `SettingsView`: Data export and backup
  - `CardDialog`, `SaleDialog`: Modal dialogs for add/edit operations

### Key Dependencies
- **Entity Framework Core 6.0.16**: ORM with SQLite provider
- **LiveChartsCore.SkiaSharpView.WPF 2.0.0-beta.701**: Chart visualization
- **Microsoft.Extensions.DependencyInjection 6.0.1**: Service container

## Development Notes

### MVVM Pattern
The application uses a simplified MVVM pattern with code-behind views rather than full ViewModels. Services act as the business logic layer.

### Navigation
Navigation is handled through WPF Frame in MainWindow. To add a new view:
1. Create the view in Views/ folder
2. Add navigation method in MainWindow.xaml.cs
3. Add navigation button in MainWindow.xaml sidebar

### Database Changes
When modifying models:
1. Update the model class in Models/
2. Create a migration: `dotnet ef migrations add DescriptiveName`
3. The migration applies automatically on next app startup (via `dbContext.Database.Migrate()` in App.xaml.cs)

### Service Access
Services are resolved through dependency injection. To access a service in a view:
1. Accept the service via constructor parameter
2. Ensure the view is resolved through the DI container (or manually resolve the service using `App.Current.Services`)

### Calculated Properties
Models use calculated properties (TotalValue, EstimatedProfit, ROI, NetProfit) that are not stored in database. These are computed on-the-fly from stored values.
