[![Build Status](https://github.com/hmlendea/steam-guard-desktop/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/steam-guard-desktop/actions/workflows/dotnet.yml)
[![Security Scan](https://github.com/hmlendea/steam-guard-desktop/actions/workflows/security.yml/badge.svg)](https://github.com/hmlendea/steam-guard-desktop/actions/workflows/security.yml)
[![License](https://img.shields.io/github/license/hmlendea/steam-guard-desktop)](https://github.com/hmlendea/steam-guard-desktop/blob/master/LICENSE)

# Steam Guard Desktop

A cross-platform desktop application that generates Steam Guard authentication codes and manages Steam mobile confirmations.

## 📑 Table of Contents

- [Capabilities](#capabilities)
- [Use Cases](#use-cases)
- [Usage](#usage)
- [Known Limitations](#known-limitations)
- [System Requirements](#system-requirements)
- [Installation](#installation)
  - [Installation from Source](#installation-from-source)
  - [Verification](#verification)
- [Configuration](#configuration)
  - [Settings](#settings)
- [Compatibility](#compatibility)
- [Integrations](#integrations)
- [Privacy and Data](#privacy-and-data)
- [Development](#development)
  - [Requirements](#requirements)
  - [Setup](#setup)
  - [Build](#build)
  - [Run](#run)
  - [Test](#test)
  - [Release](#release)
  - [Dependencies](#dependencies)
- [GitHub Actions](#github-actions)
- [Project Structure](#project-structure)
  - [Projects and Packages](#projects-and-packages)
  - [Directories](#directories)
- [Architecture](#architecture)
- [Roadmap](#roadmap)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [Code of Conduct](#code-of-conduct)
- [Security](#security)
- [Support](#support)
- [Project Engagement](#project-engagement)
- [License](#license)

## ✨ Capabilities

- Generates the current Steam Guard two-factor code from a stored shared secret.
- Calculates and displays the remaining validity of the displayed code and refreshes it when the time window ends.
- Copies the current code to the system clipboard on request without writing it to diagnostics.
- Reports clock drift when the local clock disagrees with the Steam clock.
- Lists pending Steam mobile confirmations with type, creation time, headline, and safe context.
- Approves or denies an individual confirmation and refreshes the list afterwards.
- Stores account secrets in the platform credential store and removes them with the account.
- Imports a documented account configuration file, then deletes the file so that its secrets are not retained on disk.
- Signs in to Steam with an account name and password, exchanging the password over an RSA-encrypted request without persisting it.
- Accepts the guard that Steam requests, whether it is a code sent to the e-mail address or a code from an existing authenticator.
- Stores the access token that a sign-in issues once the account is saved, and revokes the session on sign-out.
- Enrols a Steam Mobile Authenticator and finalises the enrolment with the code that Steam issues.
- Removes an existing authenticator with its revocation code.
- Recovers access when the device that held Steam Guard is no longer available.
- Presents the same interface on Windows, macOS, and Linux, and does not adopt host styling.

## 🎯 Use Cases

- **Desktop two-factor access:** Read the current Steam Guard code on a computer without reaching for a phone.
- **Confirmation review:** Inspect and decide on pending trades, market sales, and account changes from the desktop.
- **Multi-account management:** Keep several labelled accounts and switch between them.
- **Lost-device recovery:** Regain access when the phone that held Steam Guard is no longer available.

## 🚀 Usage

Add an account with its Steam ID 64, shared secret, identity secret, and Steam access token, then select it in the account list. The authenticator panel displays the current code and its countdown; copying the code places it on the clipboard. Refreshing the confirmation list retrieves the pending confirmations, and the approve and deny actions apply a decision to the selected confirmation after an explicit user action.

An account can also be provisioned by signing in with its account name and password. The sign-in accepts the guard that Steam requests, and the resulting session can enrol a new authenticator, remove an existing one with its revocation code, or recover access when the device that held Steam Guard is no longer available. Signing out revokes the session through Steam and clears the form. The password is used only for the encrypted exchange and is never stored.

```bash
dotnet run --project SteamGuardDesktop
```

## ⚠️ Known Limitations

- Only confirmation types that Steam reports with a known numeric identifier can be approved or denied; unknown types are listed but not actionable.
- When the platform credential store is unavailable, secrets are kept in an encrypted file whose key is derived from machine and user identifiers, which is weaker than a hardware-backed store.
- Steam protocol behaviour is validated against the current endpoints only; an endpoint change requires a client update.
- Recovery of an authenticator on a device that is no longer available depends on the codes that Steam sends to the registered e-mail address, phone number, or authenticator.

## 🖥️ System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| Operating system | Linux with GNOME or KDE, macOS, or Windows | The current release of the same platform |
| Runtime | .NET 10.0 SDK for building from source | .NET 10.0 runtime |
| Credential store | Secret Service on Linux, Keychain on macOS, Credential Manager on Windows | The native credential store of the desktop session |
| Network | HTTPS access to Steam for confirmations | A stable internet connection |

## 📦 Installation

The application is currently distributed as source only.

### Installation from Source

```bash
git clone https://github.com/hmlendea/steam-guard-desktop.git
cd steam-guard-desktop
dotnet build SteamGuardDesktop.slnx
```

### Verification

```bash
dotnet test SteamGuardDesktop.slnx
```

## ⚙️ Configuration

Account settings are stored by the application, and the runtime settings are declared in `SteamGuardSettings` and `DiagnosticsSettings`. The secrets themselves are never written to these files. The two privacy preferences are persisted in `settings.json` in the user application-data directory and are changed through the controls in the window. The visual theme is fixed and is not configurable.

### Configuration Files

| File | Scope | Purpose |
|------|-------|---------|
| `settings.json` | Per user | Theme preference and the privacy preferences, in the user application-data directory. |
| An account configuration file | A single import | Steam ID 64, label, shared secret, identity secret, revocation code, and access token; deleted after a successful import. |

An account configuration file uses the subsequent keys. Only `steamid` is mandatory.

| Key | Type | Description |
|-----|------|-------------|
| `steamid` | `string` | The 64-bit Steam identifier of the account. |
| `label` | `string` | A user-facing account label. |
| `shared_secret` | `string` | The Base32 shared secret. |
| `identity_secret` | `string` | The Base64 identity secret. |
| `revocation_code` | `string` | The revocation code. |
| `access_token` | `string` | The Steam access token used by the confirmation endpoints. |

### Settings

The subsequent settings are recognised:
| Section | Key | Type | Default | Required | Description |
|---------|-----|------|---------|----------|-------------|
| Steam Guard | `SteamApiBaseAddress` | `string` | `https://api.steampowered.com/` | Yes | Base address used to query the Steam server time. |
| Steam Guard | `SteamCommunityBaseAddress` | `string` | `https://steamcommunity.com/` | Yes | Base address used for the confirmation endpoints. |
| Steam Guard | `NetworkTimeoutSeconds` | `int` | `30` | Yes | Timeout applied to Steam web requests. |
| Steam Guard | `MaximumResponseBytes` | `int` | `1048576` | Yes | Maximum accepted size of a Steam response. |
| Steam Guard | `IsAccountHiddenWhenInactive` | `bool` | `true` | No | Hides account details while the window is inactive. Persisted in `settings.json`. |
| Steam Guard | `IsSecretRevealAllowed` | `bool` | `false` | No | Allows secrets to be revealed on explicit request. Persisted in `settings.json`. |
| Diagnostics | `MinimumLevel` | `LogLevel` | `Info` | Yes | Minimum severity written to diagnostics. |
| Diagnostics | `IsFileOutputEnabled` | `bool` | `true` | No | Writes diagnostics to a log file in addition to the console. |
| Diagnostics | `LogFilePath` | `string` | `steamguarddesktop.log` | No | Path of the diagnostics log file. |

## 🧩 Compatibility

| Component | Supported Versions | Notes |
|-----------|--------------------|-------|
| .NET | 10.0 | Required for building and running the application. |
| Windows | Windows 10 and newer | Uses the shared application theme and Credential Manager. |
| macOS | Supported macOS releases with Keychain | Uses the shared application theme and Keychain. |
| Linux | Desktop sessions with Secret Service | Uses the shared application theme and the Secret Service credential store. |
| Steam confirmation endpoints | Current Steam web behaviour | Unknown confirmation types are not actionable. |

## 🔌 Integrations

| Integration | Compatibility | Purpose | Required |
|-------------|---------------|---------|----------|
| Steam web services | Current HTTPS endpoints | Provides server time and the mobile confirmation endpoints. | Yes, for confirmations |
| Platform credential store | Secret Service, Keychain, Credential Manager | Stores account secrets outside ordinary configuration files. | Yes, for persistent accounts |

## 🛡️ Privacy and Data

Steam Guard Desktop stores the account metadata and the secrets you provide locally, keeps secrets in the operating system credential store wherever it is available, and sends signed requests only to Steam.

For the detailed description of how the application handles privacy and personal data, see the [Privacy Statement](./PRIVACY.md).

| Data | Purpose | Storage | Retention | Optional |
|------|---------|---------|-----------|----------|
| Steam ID 64 and account label | Identify the configured account | Encrypted account file in the user application-data directory | Until the account is removed | No |
| Shared secret | Generate Steam Guard codes | Platform credential store, or the encrypted fallback | Until the account is removed | No |
| Identity secret | Sign confirmation requests | Platform credential store, or the encrypted fallback | Until the account is removed | No |
| Steam access token | Authenticate confirmation requests | Platform credential store, or the encrypted fallback | Until the account is removed | Yes |
| Revocation code | Remove the authenticator from the account | Platform credential store, or the encrypted fallback | Until the account is removed | Yes |
| Confirmation details | Describe a pending confirmation before a decision | In memory only | Discarded when the confirmation list is refreshed | Yes |
| Theme and privacy preferences | Reproduce the interface and data-handling preferences | `settings.json` in the user application-data directory | Until the file is deleted | Yes |
| Imported account configuration | Provide the secrets of an account once | The file that the user selects | Deleted after a successful import | Yes |
| Diagnostics | Report failures without exposing secrets | Console and optional log file | Until the log file is deleted | Yes |

### Data Locations

| Platform or Scope | Location | Contents |
|-------------------|----------|----------|
| Linux, macOS, and Windows | The user application-data directory, under `SteamGuardDesktop` | Encrypted account metadata, the encrypted secret fallback, and the preference file |
| Linux, macOS, and Windows | The native credential store | Shared, identity, revocation, and access secrets |
| All platforms | The configured diagnostics log file | Redacted operations and status fields |

## 🛠️ Development

### Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Setup

```bash
git clone https://github.com/hmlendea/steam-guard-desktop.git
cd steam-guard-desktop
dotnet restore
```

### Build

```bash
dotnet build SteamGuardDesktop.slnx
```

### Run

```bash
dotnet run --project SteamGuardDesktop
```

### Test

```bash
dotnet test SteamGuardDesktop.slnx
```

### Release

The repository includes `release.sh`, which delegates to the upstream deployment script used by the project maintainer.

```bash
bash ./release.sh 1.0.0
```

This script downloads and executes an external release helper from `https://raw.githubusercontent.com/hmlendea/deployment-scripts/master/release/dotnet/10.0.sh`.

**Note:** Piping into `bash` is an intensely controversial topic. Please review any external scripts before running them in your environment!

### Dependencies

| Package | Version | Scope | Purpose |
|---------|---------|-------|---------|
| `Avalonia` | `11.3.23` | Runtime | Cross-platform desktop windowing and rendering. |
| `Avalonia.Desktop` | `11.3.23` | Runtime | Platform desktop initialisation. |
| `Avalonia.Themes.Fluent` | `11.3.23` | Runtime | Control templates that the shared application theme styles. |
| `CommunityToolkit.Mvvm` | `8.4.2` | Runtime | Observable objects and generated properties. |
| `Microsoft.Extensions.DependencyInjection` | `10.0.13` | Runtime | Service composition. |
| `NuciLog` | `1.2.1` | Runtime | Structured diagnostics output. |
| `NuciLog.Core` | `3.1.0` | Runtime | Logging contracts, operations, and sensitive keys. |
| `NuciWeb.HTTP` | `1.7.3` | Runtime | HTTP client creation and user agent retrieval. |
| `SteamGuard.TOTP` | `1.1.1` | Runtime | Steam Guard code generation. |

## ⚙️ GitHub Actions

| Workflow | Purpose | What it does |
|----------|---------|--------------|
| [ .NET ](https://github.com/hmlendea/steam-guard-desktop/blob/master/.github/workflows/dotnet.yml) | Continuous integration | Restores dependencies, builds the solution, and runs every test project. |
| [ GitHub Release ](https://github.com/hmlendea/steam-guard-desktop/blob/master/.github/workflows/github-release.yml) | Release packaging | Builds the release artefacts for every supported platform from the published tag. |
| [ Security ](https://github.com/hmlendea/steam-guard-desktop/blob/master/.github/workflows/security.yml) | Security scanning | Checks for vulnerable packages, builds the solution, and runs CodeQL analysis. |

## 🗂️ Project Structure

The repository contains one application project and three test projects, all at the same level as the solution file.

### Projects and Packages

| Project | Type | Purpose |
|---------|------|---------|
| `SteamGuardDesktop/SteamGuardDesktop.csproj` | Application | Avalonia shell, domain models, services, Steam client, and persistence. |
| `SteamGuardDesktop.ArchitectureTests/SteamGuardDesktop.ArchitectureTests.csproj` | Test | Layer, project, solution, and source-file convention checks. |
| `SteamGuardDesktop.IntegrationTests/SteamGuardDesktop.IntegrationTests.csproj` | Test | Steam protocol scenarios and encrypted store behaviour. |
| `SteamGuardDesktop.UnitTests/SteamGuardDesktop.UnitTests.csproj` | Test | Codes, signatures, mapping, theming, redaction, and account validation. |

### Directories

| Directory | Purpose |
|-----------|---------|
| `SteamGuardDesktop/Client/Steam` | Signed Steam web requests and response parsing. |
| `SteamGuardDesktop/DataAccess` | Encrypted account metadata storage. |
| `SteamGuardDesktop/Domain` | Account, authenticator, confirmation, and session models. |
| `SteamGuardDesktop/Service` | Account, authenticator, confirmation, diagnostics, platform, storage, theming, and time services. |
| `SteamGuardDesktop/ViewModels` | Application state presented to the views. |
| `SteamGuardDesktop/Views` | Avalonia windows and shared theme resources. |

## 🏗️ Architecture

Steam Guard Desktop is a layered desktop client that separates domain models, application services, the Steam protocol client, persistence, and presentation inside a single assembly.

See the [Architecture Documentation](./ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## 🗺️ Roadmap

See [ROADMAP.md](./ROADMAP.md) for planned work and forthcoming changes.

## 🩺 Troubleshooting

| Symptom | Probable Cause | Resolution |
|---------|----------------|------------|
| The authenticator panel shows no code | The stored shared secret is missing or not Base32 | Re-enter the shared secret for the account. |
| A clock warning is displayed | The local clock differs from the Steam clock by more than 30 seconds | Synchronise the operating system clock. |
| Confirmations cannot be loaded | The Steam access token is missing or expired | Sign in again to obtain a current access token. |
| The sign-in observes that a code is not accepted | Steam only offered approval on an existing device | Approve the sign-in on the device that holds the authenticator. |
| The sign-in reports that the credentials are incorrect | The account name or password was mistyped | Check the credentials and start the sign-in again. |
| The authenticator cannot be enrolled | Steam rejected the activation code or the SMS code was not accepted | Request a new code and complete the enrolment again. |
| The credential store is reported as unavailable | The desktop session has no reachable Secret Service or Keychain | Unlock the session credential store or use a supported desktop session. |

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Submit focused pull requests that conform to the existing code style
- Maintain your branch synchronised with `master`
- Revise the documentation when functionality changes
- Properly test all modifications, including edge cases and error conditions
- Include tests for additional or modified functionality
- Raise a new [issue](https://github.com/hmlendea/steam-guard-desktop/issues) for problems or suggestions

See the [Contributing Guidelines](./CONTRIBUTING.md) for details on how to report issues, suggest enhancements, and submit changes.

## 📜 Code of Conduct

See the [Code of Conduct](./CODE_OF_CONDUCT.md) for the standards of conduct expected in all project spaces.

## 🔒 Security

Steam Guard Desktop treats account secrets as credentials, keeps them in the platform credential store, and never writes them to logs or ordinary configuration files.

For information on reporting security vulnerabilities, see the [Security Policy](./SECURITY.md).

## 💬 Support

For support, please [open an issue](https://github.com/hmlendea/steam-guard-desktop/issues).

## 💝 Project Engagement

Discovered a problem with Steam Guard Desktop or have a suggestion for it? [Open an issue](https://github.com/hmlendea/steam-guard-desktop/issues)!

If you find this project useful, consider starring ⭐️ it on GitHub!

## 📄 License

This project is being distributed under the `GNU General Public License v3.0` or later.
See [LICENSE](./LICENSE) for additional information.