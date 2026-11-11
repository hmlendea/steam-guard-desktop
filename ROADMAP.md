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

- **In Progress**: Native theming per platform - Apply the resolved theme variant, accent colour, and follow host theme changes at runtime.
- **In Progress**: Authenticator lifecycle - Refresh the displayed code automatically at expiry and report clock drift detected at load.
- **In Progress**: Privacy controls - Honour the inactive-window account lock and the explicit secret reveal preference.
- **In Progress**: Confirmation inspection - Show the details of a selected confirmation before a decision is applied.
- **Planned**: Live account validation - Verify that a generated code matches the official Steam authenticator for the same time window.

## 🗺️ Planned Labour

### Next 30 Days

- Extend the theme tests to cover runtime system theme changes.
- Document the account configuration import format and its validation.
- Add a Steam login flow so that an account can be provisioned without copying secrets by hand.
- Implement authenticator removal and re-enrolment so that a user who lost the device holding Steam Guard can recover access.

### Next Quarter

- Validate credential store behaviour on each supported desktop platform.
- Add support for the remaining confirmation types once their protocol behaviour is confirmed.

### No Fixed Horizon

- Provide packaged builds for the supported platforms.
- Add QR-code or manual provisioning if it can be implemented without contacting an untrusted service.

##Automatic code refresh | Next 30 days | The displayed code and countdown refresh without a manual action. | In Progress |
| Privacy controls | Next 30 days | Account details hide while the window is inactive and secrets stay hidden unless revealed explicitly. | In Progress |
| Confirmation details | Next 30 days | A selected confirmation can be inspected before a decision is applied. | In Progress |
| Account recovery | Next 30 days | A user without access to the original device can remove and re-enrol the authenticator. | Planned |
| Packaged distribution | No fixed horizon | Signed builds available for Windows, macOS, and Linux. | Planned |
| Platform credential validation | Next quarter | Verified secret storage on Linux, macOS, and Windows. | Planned |

## ⚠️ Dependencies and Risks

- Steam protocol endpoints - A change to the confirmation endpoints or response shape breaks confirmations - Keep the protocol isolated behind the Steam client and cover it with integration tests.
- Platform credential store availability - A session without a reachable credential store falls back to weaker file encryption - Report unavailability and keep the encrypted fallback documented.
- Steam access token provisioning - Confirmations require a token the application does not obtain itself - Document the requirement and surface a clear re-authentication state.
- Account recovery depends on a Steam login flow - Recovery cannot be completed without an authenticated session - Keep the password out of persistence and require an explicit confirmation step
## ⚠️ Dependencies and Risks

- Steam protocol endpoints - A change to the confirmation endpoints or response shape breaks confirmations - Keep the protocol isolated behind the Steam client and cover it with integration tests.
- Bundling, modifying, or redistributing the reference Android package.
- Sending account data to any project-operated service.
- Persisting a Steam password beyond the login exchange required to obtain a session
## 🚫 Out of Scope

- Implementing a Steam password login flow.
- Bundling, modifying, or redistributing the reference Android package.
- Sending account data to any project-operated service.

## 🤝 Contribution

External contributions are welcome. Propose roadmap refinements or implementation suggestions via repository [issues](https://github.com/hmlendea/steam-guard-desktop/issues) and focused pull requests aligned with current priorities.

## 💬 Support

For roadmap questions, proposals, or clarifications, [open an issue](https://github.com/hmlendea/steam-guard-desktop/issues).