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

AutoSorter is intentionally separated into domain logic, filesystem operations,
platform helpers, and the WPF application layer.

- **App** owns the WPF user interface and application composition/orchestration.
  It references Core, FileSystem, and Helpers.
- **Core** contains domain/business logic. It is independent of WPF and of
  Win32/platform-specific implementation, and references no other project.
- **FileSystem** provides filesystem abstractions and filesystem operations.
  It references no other project.
- **Helpers** provides Windows/Win32 platform-specific helpers. It references
  no other project.

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

Core, FileSystem, and Helpers never reference App, and circular dependencies
are not allowed.

## Goals
Make AutoSorter accessible to everyone, easy to use and install, and useful to gamers.

## How to install
Go to steam page, or the install/program.zip