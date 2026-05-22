# Contributing to Retro.TUI.Framework

Thank you for your interest in contributing to **Retro.TUI.Framework**!  
This document describes the conventions, workflow, and guidelines to follow.

---

## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [Branch strategy](#2-branch-strategy)
3. [Commit conventions](#3-commit-conventions)
4. [Coding conventions](#4-coding-conventions)
5. [Pull request process](#5-pull-request-process)
6. [Reporting issues](#6-reporting-issues)

---

## 1. Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`10.0.300` or later)
- [Visual Studio 2026](https://visualstudio.microsoft.com/) or
  [Visual Studio Code](https://code.visualstudio.com/) with C# Dev Kit
- [Git](https://git-scm.com/) 2.40 or later

---

## 2. Branch strategy

This project follows a **GitFlow**-based workflow:

| Branch | Purpose |
|---|---|
| `main` | Stable, tagged releases only |
| `develop` | Integration branch — always buildable |
| `feature/xxx` | New features or milestones |
| `release/vX.Y.Z` | Release preparation |
| `hotfix/xxx` | Critical fixes on `main` |

### Typical feature workflow

```
git checkout develop
git checkout -b feature/my-feature
# ... work ...
git push origin feature/my-feature
# Open Pull Request → develop
```

### Branch naming

- `feature/milestone-1-events`
- `feature/widget-listbox`
- `hotfix/fix-focus-manager-null-ref`
- `release/v0.1.0`

---

## 3. Commit conventions

This project follows [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/).

### Format

```
<type>(<scope>): <short description>

[optional body]

[optional footer]
```

### Types

| Type | Use |
|---|---|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation only |
| `style` | Formatting, no logic change |
| `refactor` | Code restructure, no feature/fix |
| `test` | Adding or updating tests |
| `chore` | Build, tooling, dependencies |
| `perf` | Performance improvement |

### Examples

```
feat(events): add TuiMouseEvent record with all action types
fix(rendering): correct baseline Y calculation for CellH > 16
docs(architecture): update view layer contract signatures
chore(deps): update SkiaSharp to 3.119.3
```

---

## 4. Coding conventions

All code must follow the [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
and the rules defined in `.editorconfig`.

### Key rules

- `PascalCase` for types, properties, methods, and public events.
- `_camelCase` for private fields.
- `I` prefix for interfaces: `ITuiHost`, `ITuiInputProvider`.
- `Tui` prefix for all public framework classes: `TuiView`, `TuiDialog`.
- XML doc comments (`///`) on all public API members.
- Nullable reference types enabled — no `#nullable disable`.
- No warnings suppressed without a documented reason.
- `record` for immutable value types (events, metrics).
- `init`-only setters for configuration properties.

### Namespace structure

```
Retro.TUI.Events        → event records and commands
Retro.TUI.Theming       → palette, theme, color roles
Retro.TUI.Hosting       → ITuiHost, SdlHost
Retro.TUI.Rendering     → TuiRenderContext, TuiGrid
Retro.TUI.Core          → TuiApplication, message loop
Retro.TUI.Views         → TuiView, TuiGroup, TuiDesktop
Retro.TUI.Windows       → TuiWindow, TuiDialog
Retro.TUI.Widgets       → controls
```

---

## 5. Pull request process

1. Ensure the code builds without warnings (`TreatWarningsAsErrors = true`).
2. Ensure all existing tests pass.
3. Add tests for any new public API.
4. Update `CHANGELOG.md` under `[Unreleased]`.
5. Target the `develop` branch — never `main` directly.
6. Provide a clear description of what the PR does and why.

---

## 6. Reporting issues

Use [GitHub Issues](https://github.com/OscarNET-SOFTware/Retro.TUI.Framework/issues)
to report bugs or request features.

When reporting a bug, please include:

- .NET SDK version (`dotnet --version`)
- Visual Studio / IDE version
- Operating system and version
- Minimal reproduction steps
- Expected vs. actual behaviour

---

*Thank you for helping make Retro.TUI.Framework better!*
