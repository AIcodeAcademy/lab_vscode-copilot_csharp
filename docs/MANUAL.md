# Archetype CSharp CLI — User Manual

This manual helps you install, run, and use the CLI effectively. It also lists common options, exit codes, and troubleshooting tips.

## Prerequisites

- .NET SDK 9.0.304 or newer (the repo pins 9.0.304 via `global.json`)
- Internet access for weather and GeoIP commands

## Installation

Clone the repository and restore/build:

```bash
# Clone
git clone https://github.com/AIDDbot/ArchetypeCSharpCLI.git
cd ArchetypeCSharpCLI

# Restore & build
dotnet build
```

Optional: run tests to verify everything is OK:

```bash
dotnet test
```

## Quick start

Show help (default when no args):

```bash
dotnet run --project src/ArchetypeCSharpCLI -- --help
```

Show version:

```bash
dotnet run --project src/ArchetypeCSharpCLI -- --version
# or
dotnet run --project src/ArchetypeCSharpCLI -- -v
```

## Commands

### hello
Print a greeting.

Usage:
```bash
dotnet run --project src/ArchetypeCSharpCLI -- hello --name "Ada"
# short option
dotnet run --project src/ArchetypeCSharpCLI -- hello -n "Ada"
```

Notes:
- `--name|-n` is required and must be non-empty.

### weather
Show current weather for your current IP location or for given coordinates.

Usage:
```bash
# Use GeoIP to resolve your coordinates
dotnet run --project src/ArchetypeCSharpCLI -- weather

# Provide coordinates explicitly
dotnet run --project src/ArchetypeCSharpCLI -- weather --lat 40.4168 --lon -3.7038

# Set a timeout (seconds) and units (metric|imperial)
dotnet run --project src/ArchetypeCSharpCLI -- weather --lat 40.4168 --lon -3.7038 --timeout 10 --units metric

# Print provider raw JSON
dotnet run --project src/ArchetypeCSharpCLI -- weather --raw
```

Options:
- `--lat <decimal>` latitude in decimal degrees
- `--lon <decimal>` longitude in decimal degrees
- `--timeout <int>` operation timeout in seconds
- `--units <metric|imperial>` output units (default: metric)
- `--raw` print raw provider JSON instead of human-friendly line

Output:
- Human-friendly multiline report with emojis and details: temperature, wind speed/direction, humidity (when available), condition, observation time and source.

Example output (metric):

```
☀️ Weather Report
🌡️ Temperature: 27.3°C
💨 Wind: 12.5 km/h NE
💧 Humidity: 58%
Observed at: 2025-09-02 14:03:00Z
Condition: Clear sky
Source: open-meteo
```

## Configuration

Configuration precedence:
1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. Environment variables

Environment selection: `DOTNET_ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT` (default `Production`).

Relevant keys:
- `App:HttpTimeoutSeconds` (or flat `HttpTimeoutSeconds`): default HTTP timeout for all clients.
- `App:LogLevel` (or flat `LogLevel`): default console log level.

Environment variable examples (Windows bash):
```bash
# Nested keys
export App__HttpTimeoutSeconds=10
export App__LogLevel=Debug

# Flat keys (lower precedence than nested)
export HttpTimeoutSeconds=15
export LogLevel=Warning
```

## Exit codes

- 0 — Success
- 1 — Network failure or timeout
- 2 — Validation error or HTTP 4xx client error
- 3 — HTTP 5xx server error
- 4 — Unexpected error

These are stable and suitable for scripting.

## Troubleshooting

- See help for a command: add `--help` after the command.
- Validation error (code 2): review option names and values; `--name` must be non-empty for `hello`.
- Network/timeout (code 1): check connectivity, proxy, and `--timeout` small values.
- Raw JSON (`--raw`) can help inspect provider responses for `weather`.
- Increase verbosity: set `App__LogLevel=Debug` and rerun.

## Uninstall / Clean

```bash
# Clean build outputs
 dotnet clean
```
