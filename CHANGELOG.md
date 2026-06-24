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
  - `TuiMessageLoop`: `internal sealed` instance class implementing the
    poll → dispatch → render → cursor → present cycle.
    Keyboard routing: Tab/Shift+Tab → focus manager; Escape → `TuiCommand.Cancel`
    broadcast to the desktop; all other keys → focused view.
    Mouse routing: `Col`/`Row` are recomputed from `PixelX`/`PixelY` via
    `TuiGrid.CellCol`/`CellRow` before `FindAt` hit-test and ancestor bubble-up;
    pure `TuiGroup` instances are skipped during bubble-up to prevent
    double-dispatch through container `HandleEvent`.
    Tracks the last known mouse-pointer pixel position
    (`CursorPixelX`/`CursorPixelY`, `internal`) and draws the custom mouse
    cursor on top of the view tree every frame via
    `TuiRenderContext.DrawMouseCursor`.
    Render step delegates to `TuiRenderContext.RenderFrame`.
  - `TuiApplication`: abstract entry point. Subclass and override `OnInitialize` to
    populate the view tree; call `Run(host, theme, options)` to start the loop.
    Implements `IDisposable`. Typeface resolution uses `SKFontManager.Default` —
    theme packages register their font in their static initializer.
- Custom mouse cursor (M2 roadmap item, completed):
  - `TuiMouseEvent`: added `PixelX`/`PixelY` (`float`, default `0`) carrying the
    real screen-pixel position reported by the host, alongside the existing
    `Col`/`Row` grid coordinates.
  - `SdlHost`: all mouse handlers (`Move`, `ButtonDown`, `ButtonUp`, `Click`,
    `DoubleClick`, `Wheel`) now post `Col=0, Row=0` as explicit
    "not yet computed" placeholders, plus the real `PixelX`/`PixelY`.
    Double-click detection switched from cell-based to pixel-based comparison
    (`_lastClickPixelX`/`_lastClickPixelY`).
  - `TuiMessageLoop`: recomputes `Col`/`Row` from `PixelX`/`PixelY` via
    `TuiGrid.CellCol`/`CellRow` — the single point of truth for pixel→cell
    mapping, fixing a latent M1 issue where `Col`/`Row` were documented as grid
    cells but populated with raw SDL2 pixel coordinates.
  - `TuiColorRole`: added `MouseCursorFill` and `MouseCursorOutline`.
  - `PcTools9Theme`: maps `MouseCursorFill` → White, `MouseCursorOutline` → Black.
  - `TuiRenderContext.DrawMouseCursor(float pixelX, float pixelY)`: renders a
    12×19 pixel-art arrow bitmap at the given screen-pixel position, replacing
    the host's system cursor (`TuiHostOptions.HideSystemCursor`). The bitmap is
    built once via `BuildCursorBitmap` and cached in `_cursorBitmap`, disposed
    with the context.
- `Retro.TUI.Core.Tests`: new test project.
  - `TuiFocusManagerTests`: registration contract, tab-order cycling,
    `SetFocus`/`Clear`, `FocusChanged` event — including no-raise-on-same-view
    and null-on-clear cases.
  - `TuiMessageLoopTests`: Tab/Shift+Tab routing, Escape → Cancel synthesis,
    regular key delivery to focused view, mouse hit-test dispatch, invisible-view
    exclusion, out-of-bounds no-op, pixel→cell `Col`/`Row` recomputation,
    `TuiGroup` bubble-up skip, and mouse-cursor pixel-position tracking.
- `Retro.TUI.Windows`: new project implementing the windows layer (leaf layer,
  not referenced by `Retro.TUI.Core`).
  - `TuiWindow`: top-level container with title bar (system-menu glyph +
    centered title), left and bottom border via `DrawBorder`, and optional drop
    shadow. Active/inactive title bar palette selected via `IsActive`.
    Layout contract: `InnerCol = AbsCol+1`, `InnerRow = AbsRow+2`,
    `InnerWidth = Width-2`, `InnerHeight = Height-3`.
- `TuiGroup.IsFrontmost(TuiView)`: returns whether a child is the frontmost
  (last in z-order) direct child. Used by `TuiWindow.IsActive`.
- `Retro.TUI.Windows.Tests`: new test project — `TuiWindowTests` covering
  construction, layout contract, active-state detection and `Draw`.
  `TuiGroupTests` extended with `IsFrontmost` coverage.
- 410 tests passing across all projects (0 failed).

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

- Mouse bubble-up skip updated: `TuiGroup` plain instances are still skipped
  during bubble-up; `TuiWindow` will override `HasCustomMouseHandling` (M3
  step 3.3) to opt back in. Full resolution pending.
- `TuiApplication.Run` requires a fully initialised `TuiHostOptions`
  (`Title`, `Width`, `Height` are `required`). No default options are provided
  by design — the caller always knows how it wants its window.
- `_cursorBitmap` in `TuiRenderContext` is cached with colors resolved from
  `MouseCursorFill`/`MouseCursorOutline` at first draw. If the active theme
  changes at runtime, the cursor retains stale colors until the
  `TuiRenderContext` is recreated. Revisit if hot theme-switching becomes a
  supported scenario.
- Coverage gaps, both by design (integration-only code, same category as
  `SdlHost` at 18.2 % line / 11.4 % branch):
  - `TuiDesktop.Draw` (5 lines, 0 % coverage): `TuiDesktopTests` exercises only
    inherited `TuiGroup`/`TuiView` members, attributed to those classes by the
    coverage tool. No test in `Retro.TUI.Views.Tests` constructs a live
    `TuiRenderContext` with an active frame. Closeable with a small addition
    mirroring `RenderingContractTests.AssertDrawDoesNotThrow`.
  - `TuiApplication` (47 lines, 0 % coverage) and the `Run` loop body in
    `TuiMessageLoop` (26 lines, part of 63.8 % coverage): require a live
    `ITuiHost` (SDL2) or a `FakeTuiHost` stub equivalent to the one in
    `Retro.TUI.Hosting.Tests.ITuiHostContractTests`. Planned for a shared
    `FakeTuiHost` in a future milestone.

[Unreleased]: https://github.com/OscarNET-SOFTware/Retro.TUI.Framework/compare/HEAD