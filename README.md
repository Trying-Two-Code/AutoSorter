# AutoSorter
A solution to the unorganized mess that is a gamers pc.

## Repository layout

```
AutoSorter/
├── App/          # WPF application: UI, composition, orchestration
├── Core/         # Domain / business logic (no WPF, no Win32)
├── Helpers/      # Windows,Filesystem,...  helpers
├── bin/          # Build output, centralised at the solution root (one folder per project)
└── obj/          # Intermediate build files, centralised at the solution root (one folder per project)
```

## Architecture

- **App** provides the WPF user interface and application composition. It uses Core and Helpers through the application backend.

- **Core** contains the main algorithms and business logic of AutoSorter. It is independent of WPF and platform-specific implementation details.

- **Helpers** provides low-level helper functionality such as Win32 APIs, filesystem operations, and other platform-specific utilities. It does not depend on other projects.

Dependency direction:

```
                    ┌───────────┐
                    │    App    │
                    └─────┬─────┘
                          │
           ┌──────────────┼──────────────┐
           ↓                             ↓
        Core                          Helpers
```

Core, and Helpers never reference App, and circular dependencies
are not allowed.

## Goals
Make AutoSorter accessible to everyone, easy to use and install, and useful to gamers.

## How to install
Installation instructions will be added once a stable release is available.