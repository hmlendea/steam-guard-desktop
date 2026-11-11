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

- **In Progress**: Native theming per platform - Confirm the resolved theme on Windows, macOS, GNOME, and KDE follows the host preference without a restart.
- **Planned**: Live account validation - Verify that a generated code matches the official Steam authenticator for the same time window.

## 🗺️ Planned Labour

### Next 30 Days

- Extend the theme tests to cover runtime system theme changes.
- Document the account configuration import format and its validation.

### Next Quarter

- Add confirmation detail inspection for a selected confirmation.
- Validate credential store behaviour on each supported desktop platform.

### No Fixed Horizon

- Provide packaged builds for the supported platforms.
- Add QR-code or manual provisioning if it can be implemented without contacting an untrusted service.

## 🧱 Objectives

| Objective | Target Period | Result | Status |
|-----------|---------------|---------|--------|
| Native theming | Next 30 days | The application follows the host theme and accent colour on every supported platform. | In Progress |
| Packaged distribution | No fixed horizon | Signed builds available for Windows, macOS, and Linux. | Planned |
| Platform credential validation | Next quarter | Verified secret storage on Linux, macOS, and Windows. | Planned |

## ⚠️ Dependencies and Risks

- Steam protocol endpoints - A change to the confirmation endpoints or response shape breaks confirmations - Keep the protocol isolated behind the Steam client and cover it with integration tests.
- Platform credential store availability - A session without a reachable credential store falls back to weaker file encryption - Report unavailability and keep the encrypted fallback documented.
- Steam access token provisioning - Confirmations require a token the application does not obtain itself - Document the requirement and surface a clear re-authentication state.

## 🚫 Out of Scope

- Implementing a Steam password login flow.
- Bundling, modifying, or redistributing the reference Android package.
- Sending account data to any project-operated service.

## 🤝 Contribution

External contributions are welcome. Propose roadmap refinements or implementation suggestions via repository [issues](https://github.com/hmlendea/steam-guard-desktop/issues) and focused pull requests aligned with current priorities.

## 💬 Support

For roadmap questions, proposals, or clarifications, [open an issue](https://github.com/hmlendea/steam-guard-desktop/issues).