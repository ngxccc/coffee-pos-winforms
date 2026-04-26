# CoffeePOS

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D4)
![UI](https://img.shields.io/badge/UI-AntdUI-0A7EA4)
![Database](https://img.shields.io/badge/Database-PostgreSQL-336791?logo=postgresql&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green)

CoffeePOS is a Point of Sale (POS) system for coffee shops, built with C# (.NET 8) on WinForms, using AntdUI for the interface and PostgreSQL for data storage.

The project follows a clear layered structure (UI -> Service -> Repository), includes dedicated modules for Cashier and Admin roles, supports background queue-based PDF printing, and uses Serilog for logging.

## Quick Start

1. Copy configuration files:

```powershell
copy src/CoffeePOS/appsettings.example.json src/CoffeePOS/appsettings.json
copy src/Migrator/appsettings.example.json src/Migrator/appsettings.json
```

1. Update required values in both appsettings.json files:

- ConnectionStrings:DefaultConnection
- ThirdPartyServices:ImgBB_ApiKey (if image upload is used)

1. Run migration and start the app:

```powershell
./scripts/run.ps1 migrate
./scripts/run.ps1 dev
```

1. Sign in with default seeded accounts:

- Admin: admin / admin123
- Cashier: cashier / 123123

## Table of Contents

- [CoffeePOS](#coffeepos)
  - [Quick Start](#quick-start)
  - [Table of Contents](#table-of-contents)
  - [Quick Overview](#quick-overview)
  - [Core Features](#core-features)
    - [1. Cashier Workspace](#1-cashier-workspace)
    - [2. Admin Workspace](#2-admin-workspace)
    - [3. System Components](#3-system-components)
  - [Architecture and Documentation](#architecture-and-documentation)
  - [Project Structure](#project-structure)
  - [Environment Requirements](#environment-requirements)
  - [Configuration](#configuration)
  - [Run the Project](#run-the-project)
    - [Option 1: Use built-in script (recommended)](#option-1-use-built-in-script-recommended)
    - [Option 2: Use dotnet CLI](#option-2-use-dotnet-cli)
  - [Default Seed Accounts (after migration)](#default-seed-accounts-after-migration)
  - [Build and Publish](#build-and-publish)
  - [Logging](#logging)
  - [Demo UI Screenshots](#demo-ui-screenshots)
    - [Login](#login)
    - [Cashier Workspace](#cashier-workspace)
    - [Admin Dashboard](#admin-dashboard)
    - [Product Management](#product-management)
  - [Troubleshooting](#troubleshooting)
    - [1. App fails to start with configuration errors](#1-app-fails-to-start-with-configuration-errors)
    - [2. Database migration fails](#2-database-migration-fails)
    - [3. Build issues with SDK/runtime](#3-build-issues-with-sdkruntime)
    - [4. Seed accounts cannot log in](#4-seed-accounts-cannot-log-in)

## Quick Overview

- Platform: .NET 8 WinForms (Windows)
- UI framework: AntdUI
- Database: PostgreSQL + Npgsql
- Architecture: Feature-based + lightweight CQRS (separate Command and Query services)
- Printing: QuestPDF + background worker (queue)
- Logging: Serilog rolling file logs

## Core Features

### 1. Cashier Workspace

- Login/change password/logout
- Select products, customize items, manage cart
- Checkout and create bills
- Track shift bill history
- Close shift and print shift reports

### 2. Admin Workspace

- KPI dashboard, charts, top-selling products
- Product/Category management
- Product Size/Topping management
- User management (create, update, deactivate/reactivate)
- Bill management (search, details, cancel/restore)
- Bill report CSV export

### 3. System Components

- Background PDF print queue (non-blocking UI)
- Trash cleanup background worker
- Theme tokenization via UiTheme
- Dependency injection via Microsoft.Extensions.Hosting

## Architecture and Documentation

- Class and architecture diagrams: docs/ARCHITECTURE_CLASS_DIAGRAM.md
- Database diagrams: docs/DATABASE_DIAGRAMS.md
- Business use cases (7 UCs): docs/USE_CASE_SPECIFICATIONS_7UC.md

## Project Structure

```text
CoffeePOS/
 scripts/
  run.ps1
 src/
  CoffeePOS/        # Main WinForms application
  Migrator/         # Database migration tool using DbUp
 docs/               # Architecture, use case, and database docs
```

## Environment Requirements

- Windows 10/11
- .NET SDK 8.x
- PostgreSQL 18+ (recommended)

## Configuration

Create appsettings.json for both projects from the example files:

- src/CoffeePOS/appsettings.example.json -> src/CoffeePOS/appsettings.json
- src/Migrator/appsettings.example.json -> src/Migrator/appsettings.json

At minimum, update:

- ConnectionStrings:DefaultConnection
- ThirdPartyServices:ImgBB_ApiKey (if image upload is used)

## Run the Project

### Option 1: Use built-in script (recommended)

```powershell
# From repository root
./scripts/run.ps1 migrate   # Initialize/update database schema
./scripts/run.ps1 dev       # Run CoffeePOS app
```

Other commands:

```powershell
./scripts/run.ps1 build     # Publish Release single-file
./scripts/run.ps1 help
```

### Option 2: Use dotnet CLI

```powershell
dotnet restore CoffeePOS.slnx
dotnet build CoffeePOS.slnx -v minimal
dotnet run --project src/Migrator/Migrator.csproj
dotnet run --project src/CoffeePOS/CoffeePOS.csproj
```

## Default Seed Accounts (after migration)

- Admin: admin / admin123
- Cashier: cashier / 123123

These accounts are seeded by migration scripts and can be changed after login.

## Build and Publish

Main project is configured with:

- Runtime: win-x64
- PublishSingleFile: true
- ReadyToRun: via scripts/run.ps1 build

You can also publish manually:

```powershell
dotnet publish src/CoffeePOS/CoffeePOS.csproj -c Release -r win-x64 --self-contained /p:PublishReadyToRun=true /p:PublishSingleFile=true
```

## Logging

- Logs are written to the Logs/ directory
- Level and rolling settings are configured in appsettings.json (Serilog)

## Demo UI Screenshots

This section is intentionally prepared for UI demo screenshots.

### Login

![Login Screen](docs/demo/login.png)

### Cashier Workspace

![Cashier Workspace](docs/demo/cashier-workspace.png)

### Admin Dashboard

![Admin Dashboard](docs/demo/admin-dashboard.png)

### Product Management

![Product Management](docs/demo/manage-products.png)

## Troubleshooting

### 1. App fails to start with configuration errors

Symptoms:

- Missing appsettings.json
- Missing DefaultConnection

Fix:

1. Ensure both config files exist:

    - src/CoffeePOS/appsettings.json
    - src/Migrator/appsettings.json

2. Verify ConnectionStrings:DefaultConnection is valid in both files.

### 2. Database migration fails

Symptoms:

- Migrator throws connection/authentication errors

Fix:

1. Check PostgreSQL host, port, username, password, and database name.
2. Confirm PostgreSQL is running and reachable from your machine.
3. Re-run:

```powershell
./scripts/run.ps1 migrate
```

### 3. Build issues with SDK/runtime

Symptoms:

- dotnet build fails due to missing SDK/runtime

Fix:

1. Install .NET SDK 8.x.
2. Validate installation:

    ```powershell
    dotnet --info
    ```

3. Re-run:

```powershell
dotnet build CoffeePOS.slnx -v minimal
```

### 4. Seed accounts cannot log in

Symptoms:

- admin/admin123 or cashier/123123 does not work

Fix:

1. Ensure migration completed successfully.
2. Check if users were changed manually after seeding.
3. Re-run migration on a clean/local dev database if needed.
