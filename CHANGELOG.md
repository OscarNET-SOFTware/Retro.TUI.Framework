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
- `Retro.TUI.Hosting`: new project implementing the hosting layer.
  - `ITuiHost`: window host contract abstracting SDL2 from the rest of the framework.
  - `TuiHostOptions`: immutable initialization options record with internal validation.
  - `SdlHost`: `ITuiHost` implementation backed by SDL2 (Silk.NET) with a software
    renderer (SDL streaming texture + SkiaSharp zero-copy surface).
  - `SdlKeyMapper`: internal static SDL2 keycode and modifier mapper.
  - Handles `SDL_WINDOWEVENT_RESIZED`: vetoes resize when `Resizable = false`
    (restores original dimensions via `SDL_SetWindowSize`); rebuilds texture and
    Skia surface and posts `TuiCommandEvent(Resize)` when `Resizable = true`.
    Guards against spurious same-size events from Wayland compositors.
- `TuiCommand.Resize = 8`: new system command posted by `SdlHost` on window resize.
- `Retro.TUI.Hosting.Tests`: new test project.
  - `TuiHostOptionsTests`: validation contract for `TuiHostOptions`.
  - `ITuiHostContractTests`: `ITuiHost` lifecycle contract via `FakeTuiHost` stub.
  - `SdlKeyMapperTests`: full coverage of `ToTuiKey` and `ToTuiModifiers`.
- `Retro.TUI.Rendering`: new project with the grid-oriented rendering layer.
  No view or control accesses SkiaSharp directly; all drawing goes through
  `TuiRenderContext`.
  - `TuiGrid`: character grid metrics derived from the active font and window
    dimensions. Provides cell↔pixel coordinate conversion utilities.
  - `TuiFont`: wraps a theme-supplied `SKTypeface` and exposes a configured
    `SKFont` with alias edging for sharp pixel-art rendering.
  - `TuiRenderContext`: main drawing API for views and controls. Implements
    `IDisposable`. `BeginFrame`/`EndFrame` are `internal`; the public surface
    exposes only grid-coordinate drawing primitives.
  - `DrawBorder`: geometric primitive — 2 px left edge + 2 px bottom edge,
    per PC Tools 9.x visual reference. No character box-drawing glyphs.
- `Retro.TUI.Rendering.Tests`: contract tests (no SDL2/GPU required) and
  font integration tests against `PcTools9Theme`.
- `Retro.TUI.Framework.slnx`: updated with all M1 and M2 projects.
- `Retro.TUI.Views`: new project implementing the view-tree layer.
  - `TuiView`: abstract base class for all visual elements. Manages relative and
    absolute grid coordinates (`Col`/`Row`/`AbsCol`/`AbsRow`), `Visible`/`Enabled`/
    `Focusable` state, parent–child linkage, dirty-flag propagation (`Invalidate`/
    `IsDirty`/`ClearDirty`), and the `Draw`/`HandleEvent` contract.
  - `TuiGroup`: composable container. Exposes `Add`/`Remove` for child lifecycle
    (sets and clears `Parent`). `FindAt` performs depth-first reverse-order
    hit-testing, skipping invisible children. `Draw` iterates children back-to-front
    with per-child `PushClip`/`PopClip`. `HandleEvent` dispatches front-to-back
    until consumed.
  - `TuiDesktop`: sealed root view. Fills the screen with the themed background
    pattern via `TuiRenderContext.DrawDesktopPattern` before delegating to
    `TuiGroup.Draw`.
- `Retro.TUI.Views.Tests`: new test project.
  - `TuiViewTests`: tree linkage, absolute coordinate accumulation, dirty-flag
    propagation, default state contracts.
  - `TuiGroupTests`: `Add`/`Remove` lifecycle, `FindAt` hit-testing (visible,
    invisible, overlapping and nested children), `HandleEvent` dispatch order.
  - `TuiDesktopTests`: inheritance contracts, child management delegation,
    `FindAt` on the root container.
- `Retro.TUI.Core`: new project wiring together the hosting, rendering and view-tree
  layers.
  - `TuiFocusChangedEventArgs`: `EventArgs` subtype carrying the newly focused view
    (or `null` when focus is cleared).
  - `TuiFocusManager`: manages keyboard focus across registered focusable views.
    Maintains tab order by registration sequence. Exposes `Register`/`Unregister`,
    `SetFocus`, `FocusNext`/`FocusPrevious` (with wrap-around), `IsFocused`, `Clear`
    and the `FocusChanged` event.
  - `TuiMessageLoop`: `internal sealed` poll → dispatch → render cycle.
    Keyboard routing: Tab/Shift+Tab → focus manager; Escape → `TuiCommand.Cancel`
    broadcast to the desktop; all other keys → focused view. Mouse routing: `FindAt`
    hit-test followed by ancestor bubble-up; pure `TuiGroup` instances are skipped
    during bubble-up to prevent double-dispatch through container `HandleEvent`.
    Render step delegates to `TuiRenderContext.RenderFrame`.
  - `TuiApplication`: abstract entry point. Subclass and override `OnInitialize` to
    populate the view tree; call `Run(host, theme, options)` to start the loop.
    Implements `IDisposable`. Typeface resolution uses `SKFontManager.Default` —
    theme packages register their font in their static initializer.
- `Retro.TUI.Core.Tests`: new test project.
  - `TuiFocusManagerTests`: registration contract, tab-order cycling,
    `SetFocus`/`Clear`, `FocusChanged` event — including no-raise-on-same-view
    and null-on-clear cases.
  - `TuiMessageLoopTests`: Tab/Shift+Tab routing, Escape → Cancel synthesis,
    regular key delivery to focused view, mouse hit-test dispatch, invisible-view
    exclusion and out-of-bounds no-op.

### Changed

- `PcTools9Theme`: registers the IBM VGA 9x16 typeface (`PxPlus IBM VGA 9x16`)
  via a static field initializer using `SKTypeface.FromStream`. Replaces the
  previous `SKFontManager.RegisterTypeface` call removed in SkiaSharp 3.x.
- `TuiRenderContext`: added `RenderFrame(SKSurface, Action<TuiRenderContext>)` —
  executes a full frame inside a matched `BeginFrame`/`EndFrame` pair with
  `try/finally` guarantee, allowing `Retro.TUI.Core` to control the render cycle
  without requiring `InternalsVisibleTo` access to `Retro.TUI.Rendering` internals.

### Removed

- `DrawBorderDouble`: not present in PC Tools 9.x visual reference.
  History preserved in Git.

### Known limitations

- Mouse bubble-up skips all `TuiGroup` subclasses (`current is TuiGroup`),
  including future `TuiWindow` instances that override `HandleEvent` with their
  own logic. To be revisited in M3 when `TuiWindow` is implemented — candidate
  solutions: `IMouseEventHandler` interface or `HandlesMouseEvents` property
  on `TuiView`.
- `TuiApplication.Run` requires a fully initialised `TuiHostOptions`
  (`Title`, `Width`, `Height` are `required`). No default options are provided
  by design — the caller always knows how it wants its window.

[Unreleased]: https://github.com/OscarNET-SOFTware/Retro.TUI.Framework/compare/HEAD
