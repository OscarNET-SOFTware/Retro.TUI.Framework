# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.3.0-alpha.3] - 2026-06-27

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
- `Retro.TUI.Views`: new project implementing the view-tree layer.
  - `TuiView`: abstract base class for all visual elements. Manages relative and
    absolute grid coordinates, `Visible`/`Enabled`/`Focusable` state,
    parent–child linkage, dirty-flag propagation and the `Draw`/`HandleEvent` contract.
  - `TuiGroup`: composable container. Exposes `Add`/`Remove` for child lifecycle.
    `FindAt` performs depth-first reverse-order hit-testing. `HandleEvent` dispatches
    front-to-back until consumed. `DrawChildViews` (protected) iterates children
    back-to-front for use by subclasses that manage their own clipping.
  - `TuiDesktop`: sealed root view. Paints the themed background pattern before
    delegating to `TuiGroup.Draw`. Hosts the modal stack (`PushModal`/`PopModal`/
    `HasModal`/`ActiveModal`) and the `CommandSink` callback for unhandled commands.
  - `IModalDialog`: minimal two-member interface (`CloseRequested`, `Result`) that
    decouples the modal loop in `Retro.TUI.Core` from `Retro.TUI.Windows`,
    preserving the dependency graph.
- `Retro.TUI.Views.Tests`: new test project covering `TuiView`, `TuiGroup` and
  `TuiDesktop` contracts including modal stack and `DrawChildViews`.
- `Retro.TUI.Core`: new project wiring together the hosting, rendering and view-tree
  layers.
  - `TuiFocusManager`: manages keyboard focus across registered focusable views.
    Tab order by registration sequence. `Register`/`Unregister`, `SetFocus`,
    `FocusNext`/`FocusPrevious` (with wrap-around), `IsFocused`, `Clear` and
    the `FocusChanged` event.
  - `TuiMessageLoop`: `internal sealed` instance class implementing the
    poll → dispatch → render → cursor → present cycle.
    - Keyboard routing: Tab/Shift+Tab → focus manager; Escape without modal →
      `onCommand`; Escape with modal → active modal; Enter with modal → active
      modal; all other keys → focused view.
    - Mouse routing: pixel→cell conversion via `TuiGrid`; mouse capture
      (`_capturedView`) for drag correctness; `HasCustomMouseHandling` opt-in
      for bubble-up; modal fast path bypasses hit-test entirely.
    - Modal reentrancy guard: `_modalWasOpened` / `_inNestedLoop` flags prevent
      orphaned mouse capture when `RunModal` is called synchronously from inside
      `HandleEvent` (e.g. `TuiWindow [-]` → `CommandSink` → `RunModal`).
    - Custom mouse cursor drawn each frame via `TuiRenderContext.DrawMouseCursor`.
  - `TuiApplication`: abstract entry point. `Run(host, theme, options)` drives the
    main loop. `RunModal(dialog, desktop)` orchestrates the full modal lifecycle —
    push, nested event loop, pop. `OnCommand` / `OnInitialize` are protected
    virtual extension points. Implements `IDisposable`.
- `Retro.TUI.Core.Tests`: new test project covering `TuiFocusManager` and
  `TuiMessageLoop` contracts including mouse capture, modal routing and
  pixel→cell recomputation.
- `Retro.TUI.Windows`: new project implementing the windows layer (leaf layer,
  not referenced by `Retro.TUI.Core`).
  - `TuiWindow`: top-level draggable container with title bar (system-menu glyph +
    centered title), left and bottom border, optional drop shadow, active/inactive
    palette, z-order (`BringToFront`), and close-button (`[-]`) that emits
    `TuiCommand.Close` via `CommandSink`.
  - `TuiDialog`: modal window extending `TuiWindow` and implementing `IModalDialog`.
    Uses `Dialog*` color roles independently of `Window*` roles. `Close(result)` is
    idempotent. `DrawTitleBar` overridden to use `DialogTitle*` roles.
- `Retro.TUI.Windows.Tests`: new test project covering `TuiWindow` and `TuiDialog`
  contracts including layout, active state, Draw, and modal result.
- `TuiView.HasCustomMouseHandling`: public virtual property (default `false`).
- `TuiGroup.IsFrontmost(TuiView)`: returns whether a child is the frontmost
  direct child. Used by `TuiWindow.IsActive`.
- `TuiGroup.Frontmost`: public read-only property returning the frontmost child,
  or `null` when the group is empty.
- Custom mouse cursor: 12×19 pixel-art arrow bitmap rendered each frame via
  `TuiRenderContext.DrawMouseCursor`, replacing the host system cursor.
- `Retro.TUI.Sample.Basic`: minimal sample demonstrating two draggable windows,
  z-order activation, and the modal stack via `ConfirmCloseDialog`.
- 461 tests passing across all projects (0 failed).

### Changed

- `PcTools9Theme`: registers IBM VGA 9x16 typeface via `SKTypeface.FromStream`
  (replaces removed `SKFontManager.RegisterTypeface` from SkiaSharp 3.x).
- `PcTools9Theme`: `DialogBackground` → `EgaBrightCyan` (`#55FFFF`); dialog
  palette now visually distinct from window palette per PC Tools 9.x reference.
- `TuiRenderContext`: added `RenderFrame(SKSurface, Action<TuiRenderContext>)` —
  full frame inside matched `BeginFrame`/`EndFrame` with `try/finally` guarantee.
- `TuiMessageLoop` event dispatch: when `HasModal` is `true`, all keyboard, mouse
  and command events route exclusively to `ActiveModal`.
- `TuiTheme`: added `SKTypeface? Typeface` for direct typeface injection.
- `TuiWindow.DrawTitleBar`: extracted as `protected virtual` so `TuiDialog` can
  override it to use `Dialog*` color roles without duplicating `Draw` logic.
- `TuiGroup.Draw`: removed `PushClip`/`PopClip` — clip is each view's own
  responsibility.

### Fixed

- `SdlHost.Present`: texture was never unlocked before `RenderCopy`, causing a
  null `SKSurface` on the second frame.
- `TuiRenderContext.DrawShadow`: shadow offsets are now in pixels (not grid cells);
  corner overlap removed.
- `TuiRenderContext.DrawBorder`: stroke width corrected to 1 px.
- `TuiWindow.Draw`: window fill added; clip applied only to the inner area.
- Mouse drag: moving the cursor outside the window chrome during drag no longer
  drops the event (`_capturedView` capture in `TuiMessageLoop`).
- Mouse capture orphan on modal open: `_modalWasOpened` flag prevents establishing
  capture when `RunModal` was called synchronously inside `HandleEvent`.
- Mouse capture orphan on modal close: `ButtonUp` inside the modal fast path
  clears `_capturedView` so subsequent clicks are not swallowed.
- Close-button double-trigger: `[-]` hit-test excludes `AbsCol + 1` from drag
  detection; drag begin guard excludes the same two cells.
- `TuiDialog.Draw`: uses `DrawChildViews` instead of `base.Draw` to avoid
  `TuiWindow.Draw` overwriting the `DialogBackground` fill with `WindowBackground`.
- `TuiWindow` inactive glyph: `[-]` now uses `WindowTitleInactiveBackground` /
  `WindowBorder` when the window is inactive.

### Removed

- `DrawBorderDouble`: not present in PC Tools 9.x visual reference.
  History preserved in Git.

### Known limitations

- `TuiApplication.Run` requires a fully initialised `TuiHostOptions`
  (`Title`, `Width`, `Height` are `required`). No default options provided by
  design — the caller always knows how it wants its window.
- `_cursorBitmap` in `TuiRenderContext` is cached with colors resolved at first
  draw. If the active theme changes at runtime, the cursor retains stale colors
  until `TuiRenderContext` is recreated.
- `icon.png` is referenced in `src/Directory.Build.props` for NuGet packaging
  but not yet present in the repository. `dotnet pack` will warn until it is added.
- Coverage gaps (integration-only code, same category as `SdlHost`):
  - `TuiApplication` (0 % coverage): requires a live `ITuiHost` or a shared
    `FakeTuiHost`. Planned for M4.
  - `SdlHost` (0 % line / 0 % branch): SDL2 integration; not unit-testable
    without a display. Excluded by design.

[Unreleased]: https://github.com/OscarNET-SOFTware/Retro.TUI.Framework/compare/v0.3.0-alpha.3...HEAD
[0.3.0-alpha.3]: https://github.com/OscarNET-SOFTware/Retro.TUI.Framework/releases/tag/v0.3.0-alpha.3