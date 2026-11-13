# Steam Guard Desktop Roadmap

This roadmap describes the current focus, the planned labour for the next horizons, and the objectives for Steam Guard Desktop. It reflects intended work rather than a commitment to a fixed schedule.

## 📑 Table of Contents

- [Current Focus](#current-focus)
- [Planned Labour](#planned-labour)
  - [Next 30 Days](#next-30-days)
  - [Next Quarter](#next-quarter)
  - [No Fixed Horizon](#no-fixed-horizon)
- [Objectives](#objectives)
- [Dependencies and Risks](#dependencies-and-risks)
- [Out of Scope](#out-of-scope)
- [Contribution](#contribution)
- [Support](#support)

## 🎯 Current Focus

- **In Progress**: Live account validation - Verify that a generated code matches the official Steam authenticator for the same time window.
- **In Progress**: Release artefact verification - Confirm that a published release attaches a packaged archive for every supported runtime identifier.
- **Planned**: Platform credential validation - Verify secret storage on Linux, macOS, and Windows.

## 🗺️ Planned Labour

### Next 30 Days

- Validate a generated code against the official Steam authenticator for the same time window.
- Publish the first tagged release and confirm that the workflow attaches an archive for every supported runtime identifier.
- Confirm credential store behaviour and application startup on macOS and Windows, where the Linux secret store has already been verified.

### Next Quarter

- Add support for the remaining confirmation types once their protocol behaviour is confirmed.
- Confirm that the interface renders identically on Windows, macOS, GNOME, and KDE, for which the shared-resource invariant is already enforced by an architecture test.

### No Fixed Horizon

- Add QR-code provisioning for account enrolment if it can be implemented without contacting an untrusted service.

## 🧱 Objectives

| Objective | Target Period | Result | Status |
|-----------|---------------|---------|--------|
| Live account validation | Next 30 days | A generated code matches the official Steam authenticator for the same time window. | In Progress |
| Packaged distribution | Next 30 days | A published release produces a packaged build for every supported runtime identifier; the local release script already produces the seven expected archives. | In Progress |
| Platform credential validation | Next quarter | Verified secret storage on Linux, macOS, and Windows; the Linux Secret Service round trip is verified. | In Progress |

## ⚠️ Dependencies and Risks

- Steam protocol endpoints - A change to the sign-in, confirmation, or authenticator endpoints breaks the corresponding feature - Keep every protocol isolated behind a client and cover it with integration tests.
- Platform credential store availability - A session without a reachable credential store falls back to weaker file encryption - Report unavailability and keep the encrypted fallback documented.
- Steam access token provisioning - Confirmations require a token that the application obtains only through a sign-in - Surface a clear re-authentication state when the token expires.
- Account recovery depends on a sign-in - Recovery cannot complete without an authenticated session - Keep the password out of persistence and require an explicit confirmation step.
- Shared release script - Release packaging depends on a remotely hosted script that can change independently of this repository - Keep the workflow pinned to the same script that `release.sh` uses and verify a published release after each change.
- Unsigned release artefacts - The release script produces unsigned archives, so hosts report an unknown publisher - Publish a checksum beside each archive and document the verification step for users.

## 🚫 Out of Scope

- Bundling, modifying, or redistributing the reference Android package.
- Sending account data to any project-operated service.
- Persisting a Steam password beyond the sign-in exchange.

## 🤝 Contribution

External contributions are welcome. Propose roadmap refinements or implementation suggestions via repository [issues](https://github.com/hmlendea/steam-guard-desktop/issues) and focused pull requests aligned with current priorities.

## 💬 Support

For roadmap questions, proposals, or clarifications, [open an issue](https://github.com/hmlendea/steam-guard-desktop/issues).