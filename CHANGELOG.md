# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Initial project structure and architecture documentation.
- `Retro.TUI.Events` project: `TuiEvent` abstract record hierarchy
  (`TuiKeyEvent`, `TuiMouseEvent`, `TuiCommandEvent`, `TuiTimerEvent`, `TuiFocusEvent`)
  and supporting enums (`TuiKey`, `TuiModifiers`, `TuiMouseAction`,
  `TuiMouseButton`, `TuiFocusAction`, `TuiCommand`).
- `TuiEventQueue`: thread-safe bounded `Channel<TuiEvent>` (capacity 256,
  `DropOldest`) with `TryPost`, `PostAsync`, `TryRead` and `ReadAsync`.
- `Retro.TUI.Events.Tests`: 42 unit tests — 100 % line and branch coverage.
- `scripts/delete-bin-and-obj-folders.cmd`: cleans `bin/` and `obj/` folders
  under `src/`, `tests/` and `samples/`.
- `scripts/run-test-projects-with-code-coverage.cmd`: runs all test projects
  and generates an HTML coverage report via ReportGenerator.
- `Retro.TUI.Theming` project: `TuiColorRole` enum (75 semantic color roles),
  `TuiPalette` (immutable role-to-`SKColor` map with `With` override support),
  `TuiDesktopPattern` enum and `TuiTheme` aggregate (palette, font, shadow and
  desktop pattern settings).
- `Retro.TUI.Theme.PcTools9` project: `PcTools9Theme` static class with the
  canonical Central Point Software PC Tools 9.x palette (8 colors, 75 roles).
- `Retro.TUI.Theming.Tests`: 52 unit tests — 100 % line and branch coverage.
- `docs/theming-reference.md` and `docs/theming-reference.es.md`: complete
  color role inventory and PC Tools 9.x palette reference.
- `themes/Directory.Build.props`: shared MSBuild configuration for theme projects.

[Unreleased]: https://github.com/OscarNET-SOFTware/Retro.TUI.Framework/compare/HEAD
