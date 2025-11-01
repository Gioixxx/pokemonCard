<div align="center">

# 🎴 Pokémon Card Manager

### 📊 Un'applicazione desktop WPF professionale per gestire la tua collezione di carte Pokémon

[![.NET Version](https://img.shields.io/badge/.NET-6.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/6.0)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?style=for-the-badge&logo=windows)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)
[![WPF](https://img.shields.io/badge/WPF-Desktop-FF6C00?style=for-the-badge&logo=visual-studio)](https://docs.microsoft.com/dotnet/desktop/wpf/)

**Gestisci la tua collezione • Traccia le vendite • Analizza i profitti**

[🚀 Download](#-installazione) • [📖 Documentazione](#-utilizzo) • [🐛 Segnala Bug](../../issues) • [💡 Richiedi Feature](../../issues)

<div align="center">

[![PayPal](https://img.shields.io/badge/PayPal-Donate-00457C?style=for-the-badge&logo=paypal&logoColor=white)](https://www.paypal.com/donate/?business=SH54EQG4R6X4N&no_recurring=0&item_name=Grazie+mille+%3A%29&currency_code=EUR)

**[💖 Fai una donazione con PayPal](https://www.paypal.com/donate/?business=SH54EQG4R6X4N&no_recurring=0&item_name=Grazie+mille+%3A%29&currency_code=EUR)**

</div>
---

</div>

## 📑 Indice

| Sezione | Descrizione |
|---------|-------------|
| [✨ Funzionalità](#-funzionalità) | Caratteristiche principali dell'applicazione |
| [🖥️ Requisiti](#️-requisiti-di-sistema) | Requisiti di sistema e compatibilità |
| [📦 Installazione](#-installazione) | Guide per installazione e build |
| [🚀 Quick Start](#-quick-start) | Guida rapida per iniziare |
| [🛠️ Sviluppo](#️-sviluppo) | Informazioni per sviluppatori |
| [🏗️ Architettura](#️-architettura) | Struttura e design del progetto |
| [🤝 Contribuire](#-contribuire) | Come contribuire al progetto |

---

## ✨ Funzionalità

<div align="center">

### 🎯 Tutto ciò di cui hai bisogno per gestire la tua collezione

</div>

<table>
<tr>
<td width="50%">

#### 📦 **Gestione Inventario**
- ✅ Aggiungi, modifica ed elimina carte
- ✅ Dettagli completi (nome, set, rarità, quantità)
- ✅ Tracciamento prezzi di acquisto
- ✅ Visualizzazione organizzata

</td>
<td width="50%">

#### 💰 **Registro Vendite**
- ✅ Tracciamento completo delle vendite
- ✅ Calcolo automatico profitti netti
- ✅ Storico transazioni
- ✅ Analisi performance

</td>
</tr>
<tr>
<td width="50%">

#### 📊 **Dashboard Interattiva**
- ✅ Statistiche dettagliate
- ✅ Grafici interattivi (LiveCharts)
- ✅ Analisi trend
- ✅ Metriche chiave

</td>
<td width="50%">

#### 💾 **Esportazione & Backup**
- ✅ Esportazione dati CSV
- ✅ Backup database SQLite
- ✅ Ripristino dati
- ✅ Condivisione dati

</td>
</tr>
</table>

### 🔍 **Calcoli Automatici**

| Metrica | Descrizione |
|---------|-------------|
| 💵 **TotalValue** | Valore totale dell'inventario |
| 📈 **EstimatedProfit** | Profitto stimato per carta |
| 🎯 **ROI** | Return on Investment calcolato |
| 💰 **NetProfit** | Profitto netto dalle vendite |

---

## 🖥️ Requisiti di Sistema

| Componente | Requisito Minimo | Consigliato |
|------------|------------------|-------------|
| **Sistema Operativo** | Windows 7 | Windows 10/11 |
| **Runtime** | .NET 6.0 Runtime | .NET 6.0 Runtime |
| **RAM** | 512 MB | 2 GB |
| **Spazio Disco** | 50 MB | 100 MB |
| **Risoluzione** | 1024x768 | 1920x1080 |

> 💡 **Nota**: Scarica il runtime .NET 6.0 da [qui](https://dotnet.microsoft.com/download/dotnet/6.0) se non già installato.

---

## 📦 Installazione

### 🎯 Opzione 1: Download Precompilato (Consigliato)

<div align="center">

**✨ La soluzione più semplice per iniziare subito**

</div>

1. 📥 Scarica l'ultima release da [Releases](../../releases)
2. 📂 Estrai il file ZIP in una cartella
3. ▶️ Esegui `PokemonCardManager.exe`
4. 🎉 Inizia a gestire la tua collezione!

> ⚡ **Pronto in 30 secondi!**

---

### 🔧 Opzione 2: Build dalla Sorgente

<div align="center">

**👨‍💻 Per sviluppatori e utenti avanzati**

</div>

#### 📋 Prerequisiti

- 🎨 **Visual Studio 2022** o superiore (con .NET Desktop Development)
- 🔨 **.NET 6.0 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/6.0))
- 📦 **Git** ([Download](https://git-scm.com/downloads))

#### 🚀 Istruzioni Passo-Passo

**1️⃣ Clona il repository**
```bash
git clone https://github.com/tuonome/pokemonCard.git
cd pokemonCard
```

**2️⃣ Build Standard**
```powershell
# Metodo 1: Visual Studio
# Apri PokemonCardManager.sln
# Seleziona configurazione "Release"
# Build > Build Solution (Ctrl+Shift+B)

# Metodo 2: Command Line
dotnet build -c Release
```

**3️⃣ Build Single-File Executable** ⭐ Consigliato
```powershell
dotnet publish -c Release -r win-x64 --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true
```

📁 **Output**: `bin\Release\net6.0-windows\win-x64\publish\PokemonCardManager.exe`

> 🎯 **Vantaggi Single-File**: Un solo file eseguibile, nessuna dipendenza esterna!

---

## 🚀 Quick Start

<div align="center">

### ⚡ Inizia subito in 3 semplici passi

</div>

### 📝 **Step 1: Avvio**
```powershell
# Esegui l'applicazione
.\PokemonCardManager.exe
```
✅ Il database verrà creato automaticamente in `%LocalAppData%\PokemonCardManager\`

### 📦 **Step 2: Aggiungi Carte**
1. Naviga su **📦 Inventario** dal menu laterale
2. Clicca su **➕ Aggiungi Carta**
3. Compila i dettagli:
   - Nome carta
   - Set/Serie
   - Rarità
   - Quantità
   - Prezzo di acquisto
4. 💾 Salva

### 💰 **Step 3: Registra Vendite**
1. Vai su **💰 Vendite**
2. Seleziona una carta dall'inventario
3. Inserisci prezzo di vendita e quantità
4. ✅ Il profitto viene calcolato automaticamente!

### 📊 **Visualizza Statistiche**
- Vai su **📊 Dashboard** per vedere grafici e statistiche aggregate
- Analizza ROI, profitti e trend delle vendite

---

## 🛠️ Sviluppo

### 📁 Struttura del Progetto

```
PokemonCardManager/
│
├── 📂 Data/                          # Data Access Layer
│   ├── ApplicationDbContext.cs      # EF Core DbContext
│   └── DesignTimeDbContextFactory.cs # Factory per migrations
│
├── 📂 Models/                        # Domain Entities
│   ├── Card.cs                       # Entità Carta
│   └── Sale.cs                       # Entità Vendita
│
├── 📂 Services/                      # Business Logic
│   ├── ICardService.cs               # Interfaccia servizio carte
│   ├── CardService.cs                # Implementazione
│   ├── ISaleService.cs               # Interfaccia servizio vendite
│   ├── SaleService.cs                # Implementazione
│   ├── IDataExportService.cs         # Interfaccia esportazione
│   └── DataExportService.cs          # Implementazione
│
├── 📂 Views/                         # UI Views
│   ├── InventoryView.xaml            # Vista inventario
│   ├── SalesView.xaml                # Vista vendite
│   ├── DashboardView.xaml            # Vista dashboard
│   ├── SettingsView.xaml             # Vista impostazioni
│   ├── CardDialog.xaml               # Dialog aggiunta/modifica carta
│   └── SaleDialog.xaml               # Dialog registrazione vendita
│
├── 📂 Resources/                     # Risorse UI
│   └── Styles.xaml                   # Stili WPF
│
├── 📂 Migrations/                    # EF Core Migrations
│   └── ...
│
├── App.xaml                          # Application Entry Point
├── App.xaml.cs                       # Application Logic
├── MainWindow.xaml                   # Main Window Shell
├── MainWindow.xaml.cs                # Main Window Logic
└── PokemonCardManager.csproj         # Project File
```

### 🗄️ Gestione Database

L'applicazione utilizza **Entity Framework Core** con **SQLite** per la persistenza dei dati.

#### 📍 Posizione Database
```
%LocalAppData%\PokemonCardManager\pokemoncards.db
```

#### 🔧 Comandi EF Core

| Comando | Descrizione |
|---------|-------------|
| `dotnet ef migrations add NomeMigrazione` | Crea una nuova migrazione |
| `dotnet ef database update` | Applica migrazioni al database |
| `dotnet ef migrations remove` | Rimuove l'ultima migrazione |

> ⚠️ **Nota**: Le migrazioni vengono applicate automaticamente all'avvio dell'applicazione.

### 🏛️ Pattern Architetturale

```
┌─────────────────────────────────────┐
│      Presentation Layer (WPF)       │
│  ┌──────────┐  ┌──────────┐        │
│  │  Views   │  │  Dialogs │        │
│  └──────────┘  └──────────┘        │
└─────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│       Service Layer (DI)            │
│  ┌──────────┐  ┌──────────┐        │
│  │  Card    │  │   Sale   │        │
│  │ Service  │  │ Service  │        │
│  └──────────┘  └──────────┘        │
└─────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│        Data Layer (EF Core)         │
│  ┌──────────────────────────────┐   │
│  │   ApplicationDbContext       │   │
│  └──────────────────────────────┘   │
└─────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│      Database (SQLite)              │
│     pokemoncards.db                 │
└─────────────────────────────────────┘
```

### 🔌 Dependency Injection

I servizi sono registrati tramite `Microsoft.Extensions.DependencyInjection`:

| Servizio | Tipo | Scopo |
|----------|------|-------|
| `ICardService` / `CardService` | Transient | Gestione carte |
| `ISaleService` / `SaleService` | Transient | Gestione vendite |
| `IDataExportService` / `DataExportService` | Transient | Esportazione dati |
| `MainWindow` | Singleton | Finestra principale |

---

## 🏗️ Architettura

### 🎯 Application Entry Point

**`App.xaml.cs`** gestisce:
- ✅ Configurazione Dependency Injection
- ✅ Inizializzazione database SQLite
- ✅ Applicazione automatica migrazioni EF Core
- ✅ Creazione e visualizzazione MainWindow

### 💾 Data Layer

- **Models/**: Entità di dominio con proprietà calcolate
  - `TotalValue`: Valore totale calcolato
  - `EstimatedProfit`: Profitto stimato
  - `ROI`: Return on Investment
  - `NetProfit`: Profitto netto vendite

- **ApplicationDbContext**: DbContext EF Core con:
  - Relazioni configurate tra `Card` e `Sale`
  - Comportamento di eliminazione restrittivo
  - Migrazioni automatiche

### 🔧 Service Layer

- Servizi registrati come **Transient**
- Constructor injection per `ApplicationDbContext`
- Interfacce per testabilità e flessibilità

### 🎨 Presentation Layer

- **MainWindow**: Shell principale con navigazione Frame-based
- **Views**: User Controls WPF standalone
- Navigazione gestita tramite Frame WPF

---

## 📚 Stack Tecnologico

<div align="center">

### 🛠️ Tecnologie e Librerie Utilizzate

</div>

| Tecnologia | Versione | Uso |
|------------|----------|-----|
| **.NET** | 6.0 | Framework principale |
| **WPF** | Built-in | UI Framework |
| **Entity Framework Core** | 6.0.16 | ORM per data access |
| **SQLite** | 6.0.16 | Database embedded |
| **LiveChartsCore** | 2.0.0-beta.701 | Visualizzazione grafici |
| **Microsoft.Extensions.DI** | 6.0.1 | Dependency Injection |

---

## 🤝 Contribuire

<div align="center">

### 🌟 I contributi sono sempre benvenuti!

</div>

### 📝 Processo di Contribuzione

1. 🍴 **Fork** il repository
2. 🌿 Crea un **branch** per la tua feature
   ```bash
   git checkout -b feature/AmazingFeature
   ```
3. 💻 **Sviluppa** la tua feature
4. ✅ **Testa** le modifiche
5. 📝 **Commit** le modifiche
   ```bash
   git commit -m 'Add some AmazingFeature'
   ```
6. 📤 **Push** al branch
   ```bash
   git push origin feature/AmazingFeature
   ```
7. 🔄 Apri una **Pull Request**

### 📋 Linee Guida

- ✅ Segui le convenzioni di codice esistenti
- 📝 Aggiungi commenti per codice complesso
- 📚 Aggiorna la documentazione
- 🧪 Testa sempre le modifiche
- 🎨 Mantieni il codice pulito e leggibile

---

## 📝 Note di Sviluppo

### 💡 Proprietà Calcolate

I modelli utilizzano proprietà calcolate che non sono memorizzate nel database:

```csharp
// Esempio: Proprietà calcolata in Card.cs
public double TotalValue => Quantity * PurchasePrice;
public double EstimatedProfit => TotalValue - (Quantity * PurchasePrice);
public double ROI => PurchasePrice > 0 ? (EstimatedProfit / PurchasePrice) * 100 : 0;
```

### ➕ Aggiungere una Nuova View

1. Crea la view in `Views/YourView.xaml`
2. Aggiungi il metodo di navigazione in `MainWindow.xaml.cs`
3. Aggiungi il pulsante nella sidebar di `MainWindow.xaml`

### 🔌 Accesso ai Servizi

```csharp
// Opzione 1: Constructor Injection (preferito)
public YourView(ICardService cardService)
{
    InitializeComponent();
    _cardService = cardService;
}

// Opzione 2: Manual Resolution
var service = App.Current.Services.GetRequiredService<ICardService>();
```

---

## 📄 Licenza

<div align="center">

Questo progetto è rilasciato sotto licenza **MIT**.

Vedi il file [LICENSE](LICENSE) per i dettagli completi.

</div>

---

## 👤 Autore

<div align="center">

**PokemonCardManager**

[![GitHub](https://img.shields.io/badge/GitHub-@Gioixxx-181717?style=flat-square&logo=github)](https://github.com/Gioixxx)

</div>

---

## 💝 Supporta il Progetto

<div align="center">

### ☕ Se questo progetto ti è utile, considera di supportarlo con una donazione!

</div>

---

## 🙏 Ringraziamenti

<div align="center">

### 🎉 Grazie a tutti coloro che hanno reso possibile questo progetto!

</div>

- 📊 [LiveCharts](https://github.com/beto-rodriguez/LiveCharts2) - Librerie di grafici fantastiche
- 🗄️ [Entity Framework Core](https://github.com/dotnet/efcore) - ORM potente e flessibile
- 🌐 [.NET Community](https://dotnet.microsoft.com/) - Supporto continuo e risorse incredibili
- 🔌 [PokéAPI](https://pokeapi.co/) - API gratuita per i dati dei Pokémon

---

<div align="center">

### ⭐ Se questo progetto ti è utile, considera di lasciare una stella o fare una donazione!

[⬆ Torna all'inizio](#-pokémon-card-manager)

**Made with ❤️ using WPF and .NET**

</div>
