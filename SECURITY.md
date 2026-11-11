# Security Policy

This policy states which releases receive security maintenance, how to report a suspected vulnerability privately, and which report categories the maintainers treat as in scope.

## 📑 Table of Contents

- [Supported Versions](#supported-versions)
- [Reporting a Vulnerability](#reporting-a-vulnerability)
- [Scope](#scope)
- [Disclosure Policy](#disclosure-policy)
- [Automated Security Scanning](#automated-security-scanning)
- [Safe Harbour](#safe-harbour)
- [Recognition](#recognition)

## 🛡️ Supported Versions

Use this table to indicate which project versions currently receive security maintenance.

| Version | Distribution Channel | Supported |
|---------|--------------------|-----------|
| Latest version | Source checkout of this repository | ✅ |
| Latest version | GitHub Releases | ❌ |
| Latest version | Unofficial third-party distribution channels | ❌ |
| Preceding versions | Any distribution channel | ❌ |

## 🚨 Reporting a Vulnerability

Please do not disclose suspected vulnerabilities publicly before maintainers have had an opportunity to validate and remediate them.

To report a vulnerability:
- [GitHub Security Advisories](https://github.com/hmlendea/steam-guard-desktop/security/advisories)
- Contact the maintainers directly

## 📌 Scope

The original maintainers are not responsible for changes, modifications, or security issues introduced by downstream forks.

The subsequent report categories are in scope for this repository:
- Handling of Steam Guard shared, identity, revocation, and access secrets.
- Generation and validation of confirmation request signatures.
- Parsing of Steam web responses and translation of protocol failures.
- Storage of secrets in the platform credential store and in the encrypted fallback file.
- Redaction of sensitive values in diagnostics.

The subsequent categories are out of scope unless explicitly stated to the contrary:
- Vulnerabilities in Steam itself, in third-party NuGet packages, or in the operating system credential store.
- Compromise of secrets that were obtained outside the application.
- Modification of Steam protocol behaviour performed by Valve.

## 📢 Disclosure Policy

This project follows coordinated disclosure:
1. Vulnerabilities are investigated privately.
2. A remediation plan is prepared and validated.
3. Public disclosure is published after a fix, mitigation, or agreed risk decision is available.
4. Credit is attributed in accordance with reporter preference and project policy.

## 🔍 Automated Security Scanning

This repository runs automated security scans on every push, every pull request, and every week. The scan includes:
- **Dependency vulnerabilities** reports packages with known advisories, including transitive dependencies.
- **CodeQL analysis** uploads static analysis results for the C# codebase.

Results are published to the [Security](https://github.com/hmlendea/steam-guard-desktop/security) tab and the [Actions](https://github.com/hmlendea/steam-guard-desktop/actions/workflows/security.yml) workflow page.

## 🧾 Safe Harbour

If your investigation is conducted in good faith, confined to authorised scope, and disclosed responsibly, the maintainers will not pursue action for policy-compliant activity.

## 🙏 Recognition

We appreciate responsible disclosure. Reporters who desire public attribution may be acknowledged in release notes, advisories, or a dedicated acknowledgements section.