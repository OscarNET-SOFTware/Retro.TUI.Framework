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

[Unreleased]: https://github.com/OscarNET-SOFTware/Retro.TUI.Framework/compare/HEAD
