# Privacy and Personal Data

This document describes how Steam Guard Desktop handles account data and secrets. The application runs entirely on the user's own computer, has no project-operated server, and does not transmit account data anywhere other than to Steam.

## 📑 Table of Contents

- [What This Document Covers](#what-this-document-covers)
- [Data We Handle](#data-we-handle)
  - [Data Provided to the Application](#data-provided-to-the-application)
  - [Data Generated or Collected by the Application](#data-generated-or-collected-by-the-application)
  - [Data Received from Integrations](#data-received-from-integrations)
- [Processing and Use](#processing-and-use)
- [Storage, Retention, and Deletion](#storage-retention-and-deletion)
- [External Processing and Integrations](#external-processing-and-integrations)
- [User Controls and Requests](#user-controls-and-requests)
- [Data Protection and Security](#data-protection-and-security)
- [Document Changes](#document-changes)
- [Contact](#contact)

## 🔎 What This Document Covers

This document describes how Steam Guard Desktop at https://github.com/hmlendea/steam-guard-desktop manages personal data. It covers the application conduct and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

## 📥 Data We Handle

### Data Provided to the Application

- The Steam ID 64 and an optional account label that identify a configured account.
- The shared secret used to generate Steam Guard codes.
- The identity secret used to sign confirmation requests.
- An optional revocation code and an optional Steam access token.

### Data Generated or Collected by the Application

- The device identifier derived from the Steam ID 64 and reported to Steam.
- Structured diagnostics that describe operations and outcomes without containing secrets.
- The names of the credential store entries that hold the account secrets.

### Data Received from Integrations

- The Steam server time and the pending confirmation list, including confirmation identifiers, nonces, types, creation times, headlines, and safe summary text.
- A value indicating whether the current Steam session has expired.

## 🧭 Processing and Use

The application processes the data described above for these verified functions:
- Code generation - Shared secret
- Confirmation listing - Steam ID 64, device identifier, identity secret, access token
- Confirmation decisions - Confirmation identifiers, nonces, and the decision type
- Clock drift detection - Steam server time and the local clock
- Troubleshooting - Structured diagnostics and failure reasons

## 🗄️ Storage, Retention, and Deletion

Account metadata is stored in an encrypted file in the user application-data directory under `SteamGuardDesktop`. Secrets are stored in the operating system credential store whenever it is available, and otherwise in an encrypted file whose key is derived from machine and user identifiers. Data is retained until the account is removed, and removing an account deletes both the metadata and every stored secret that belongs to it. Diagnostics are written to the console and, when file output is enabled, to a log file until that file is deleted.

## 🔗 External Processing and Integrations

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| Steam web services | Provides the server time and the mobile confirmation endpoints, and receives signed confirmation requests | Steam ID 64, device identifier, confirmation identifiers, confirmation nonces, and signatures | `SteamGuardSettings.SteamApiBaseAddress` and `SteamGuardSettings.SteamCommunityBaseAddress` |

## ⚙️ User Controls and Requests

The following documented controls or request procedures are available:
- Remove account - Deletes the account metadata and every stored secret that belongs to it.
- Hide account details while inactive - Enabled by `SteamGuardSettings.IsAccountHiddenWhenInactive`.
- Reveal secrets - Disabled by default and controlled by `SteamGuardSettings.IsSecretRevealAllowed`.
- Disable diagnostics file output - Controlled by `DiagnosticsSettings.IsFileOutputEnabled`.

## 🛡️ Data Protection and Security

Secrets are stored only in the operating system credential store or in an encrypted file protected with AES-256-GCM. Sensitive values are declared as sensitive structured log fields so that diagnostics mask them, and request addresses are sanitised before they are recorded. Account removal deletes the stored secrets.

The original maintainers are not responsible for changes, modifications, or privacy issues introduced by downstream forks.

## 🔄 Document Changes

Update this document when application data processes, storage, integrations, or deployment responsibilities change. The current version is published at https://github.com/hmlendea/steam-guard-desktop/blob/master/PRIVACY.md.

## 📬 Contact

For questions about application data handling, contact the project maintainers. Include the operating system and the application version; do not send passwords, access tokens, or other secrets.