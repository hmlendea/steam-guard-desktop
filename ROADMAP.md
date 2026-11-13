# Steam Guard Desktop Roadmap

This roadmap describes the current focus, the planned labour for the next horizons, and the objectives for Steam Guard Desktop. It reflects intended work rather than a commitment to a fixed schedule.

The application is feature-complete and every automated check passes. The labour that remains is almost entirely verification against real conditions, because the development environment has no display server and no live Steam account.

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

- **In Progress**: Live account validation - Verify that a generated code matches the official Steam authenticator for the same time window (Next 30 days).
- **In Progress**: Platform credential validation - Verify secret storage on macOS and Windows, where the Linux Secret Service round trip is already verified (Next quarter).
- **In Progress**: Release readiness - Merge `dev` into `master` and publish the first tagged release (Next 30 days).

## 🗺️ Planned Labour

### Next 30 Days

- Merge `dev` into `master`, because the release workflow, the README links, and the workflow badges all target `master`, which currently lags `dev`.
- Publish the first tagged release and confirm that the workflow attaches an archive for every supported runtime identifier.
- Validate a generated code against the official Steam authenticator for the same time window.
- Re-check the sign-in, confirmation, server-time, and confirmation-detail responses against a live session, now that the accepted-size limit is enforced while reading.
- Confirm on macOS and Windows that a credential store which is present but unreachable is reported unavailable, as it now is on Linux.

### Next Quarter

- Confirm credential store behaviour and application startup on macOS and Windows.
- Confirm that the interface renders identically on Windows, macOS, GNOME, and KDE, for which the shared-resource invariant is already enforced by an architecture test.
- Add support for the remaining confirmation types once their numeric identifiers and protocol behaviour are confirmed.

### No Fixed Horizon

- Add QR-code provisioning for account enrolment if it can be implemented without contacting an untrusted service.
- Publish a checksum beside every release archive and document the verification step for users.

## 🧱 Objectives

| Objective | Target Period | Result | Status |
|-----------|---------------|---------|--------|
| Live account validation | Next 30 days | A generated code matches the official Steam authenticator for the same time window. | In Progress |
| Release readiness | Next 30 days | `master` carries the current work and a published release attaches an archive for every supported runtime identifier. | In Progress |
| Platform credential validation | Next quarter | Verified secret storage on macOS and Windows, after Linux. | In Progress |
| Credential store capability probe | Next 30 days | A session whose credential store is present but unreachable is reported as unavailable instead of failing on every write. | Complete |

## ⚠️ Dependencies and Risks

- Branch divergence - The release workflow, README links, and badges target `master`, which lags `dev`, so a tag placed on `master` would package older work - Merge `dev` into `master` before the first release and place each tag on `master`.
- Steam protocol endpoints - A change to the sign-in, confirmation, or authenticator endpoints breaks the corresponding feature - Keep every protocol isolated behind a client and cover it with integration tests.
- Unverified live endpoints - Only the credential sign-in key exchange has been exercised against Steam; every other response shape is verified against a scripted handler alone - Re-check each endpoint against a live session before the first release.
- Credential store reachability on macOS and Windows - The reachability probe is exercised on Linux only, because the development environment has no macOS or Windows host - Confirm the probe on both platforms before the first release.
- Encrypted fallback is machine-bound - The fallback key is derived from the machine and user identifiers, so a copied or renamed profile cannot be decrypted and its accounts must be added again - An unreadable file is moved to a `.unreadable` sibling at startup and the constraint is documented.
- Unsigned release artefacts - The release script produces unsigned archives, so hosts report an unknown publisher - Publish a checksum beside each archive and document the verification step for users.
- Shared release script - Release packaging depends on a remotely hosted script that can change independently of this repository - The workflow and `release.sh` currently name two different upstream URLs that serve byte-identical content; keep both verified against each other.
- Steam access token provisioning - Confirmations require a token that the application obtains only through a sign-in - Surface a clear re-authentication state when the token expires.
- Account recovery depends on a sign-in - Recovery cannot complete without an authenticated session - Keep the password out of persistence and require an explicit confirmation step.
- No display server in development - The interface cannot be rendered or exercised during development, so its behaviour is verified through view-model tests and architecture invariants alone - Confirm the interface on a real desktop before the first release.

## 🚫 Out of Scope

- Bundling, modifying, or redistributing the reference Android package.
- Sending account data to any project-operated service.
- Persisting a Steam password beyond the sign-in exchange.

## 🤝 Contribution

External contributions are welcome. Propose roadmap refinements or implementation suggestions via repository [issues](https://github.com/hmlendea/steam-guard-desktop/issues) and focused pull requests aligned with current priorities.

## 💬 Support

For roadmap questions, proposals, or clarifications, [open an issue](https://github.com/hmlendea/steam-guard-desktop/issues).
