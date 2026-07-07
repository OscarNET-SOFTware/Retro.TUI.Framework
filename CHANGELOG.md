# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.4.1-alpha.4] - 2026-07-07

### Added

- `TuiEgaColor` enum: 16 named EGA color values (index 0–15), ordered by canonical
  EGA index, excluding the two non-EGA grays reserved for internal roles. Widget-facing
  API for direct color selection without reopening the fixed EGA palette.
- `TuiPalette.Resolve(TuiEgaColor)`: new overload resolving a `TuiEgaColor` to its
  canonical `SKColor` via `TuiEgaPalette`. The role-based overload delegates to it.
- `TuiColorRole.ButtonDisabledBackground`, `ButtonDisabledForeground`,
  `ButtonDisabledAcceleratorForeground`: three new roles for the disabled button state,
  mapped to `#696969` / `#CACACA` / `#CACACA` per the PC Tools 9.x visual reference.
- `TuiCommand.Abort` (20), `TuiCommand.Retry` (21), `TuiCommand.Ignore` (22):
  dialog-command range 20–29.
- `TuiEgaPalette`: static class exposing the 16 standard EGA colors as `SKColor`
  constants (`EgaBlack` through `EgaWhite`), ordered by canonical EGA index 0–15.
- `TuiColorMap`: internal static class mapping every `TuiColorRole` to its canonical
  EGA color. Two non-EGA grays (`#696969` and `#CACACA`) are used for specific roles.
- `Retro.TUI.Widgets` project: new leaf layer (`Widgets → Windows`), establishing
  the M4 widgets layer.
- `TuiLabel`: static, non-focusable single-line text display. `Text` property with
  null-guard and change-only invalidation. `ForegroundColor`/`BackgroundColor`
  (`TuiEgaColor?`) allow selecting any EGA color per instance; `null` (default) uses
  the fixed `LabelForeground`/`LabelBackground` roles. Background always painted
  opaque; text clipped to `Width` without wrapping. No autosize — caller sets `Width`.
- `TuiButton`: focusable, activatable button. Key features:
  - Text with tilde-delimited accelerator marker (`~O~K` → `O` highlighted).
  - Three fixed visual states per PC Tools 9.x: focused (`#FFFFFF` bg), unfocused
    (`#CACACA` bg), disabled (`#696969` bg / `#CACACA` text). No per-instance color
    override — colors are determined exclusively by state.
  - `Command { get; init; }` — immutable after construction (default `TuiCommand.Ok`).
  - `MinWidth = 10` — silently clamped in `Draw`.
  - Activated by `Enter` (when focused), mouse `ButtonDown`, or accelerator key
    (broadcast — works regardless of which button has focus).
  - Accelerator key also transfers focus to the activated button via
    `TuiDesktop.FocusSink` before emitting the command.
  - Static factory properties: `TuiButton.Ok`, `Cancel`, `Yes`, `No`, `Abort`,
    `Retry`, `Ignore` — each returning a new pre-configured instance.
  - Drop shadow via `TuiRenderContext.DrawShadow`.
- `TuiFocusEvent` dispatch: `TuiMessageLoop.EnsureFocusEventWiring` (internal) wires
  `TuiFocusManager.FocusChanged` to `TuiFocusEvent(Lost)` / `TuiFocusEvent(Gained)`
  delivered to affected views via `HandleEvent` in that order. Previously
  `TuiFocusEvent` existed in `Retro.TUI.Events` but was never dispatched.
- `TuiFocusManager.TrySetFocus(TuiView)`: non-throwing variant of `SetFocus` —
  returns `false` if the view is not registered, instead of throwing.
- `TuiFocusManager.IsRegistered(TuiView)`: returns whether a view is in the tab order.
- `TuiGroup.CollectFocusable(ICollection<TuiView>)` (internal): recursively collects
  focusable descendants depth-first. Used by `TuiApplication` to auto-register widgets
  without exposing `Children` publicly.
- `TuiApplication.RegisterFocusableDescendants`: called automatically after
  `OnInitialize()` and before each `RunModal` nested loop — no manual
  `FocusManager.Register` calls required from application code.
- `TuiDesktop.FocusSink`: `Action<TuiView>?` callback, set by `TuiApplication` to
  `FocusManager.TrySetFocus`. Allows widgets (`Retro.TUI.Widgets` layer) to request
  focus without referencing `TuiFocusManager` (Core-layer isolation).
- Auto-focus on mouse `ButtonDown`: `TuiMessageLoop.DispatchMouseEvent` calls
  `_wiredFocusManager.TrySetFocus(current)` when a focusable view is hit, before
  delivering the event. Mirrors Turbo Vision's `TGroup::selectView` philosophy.
- Arrow key navigation: `Left`/`Up` → `FocusPrevious`; `Right`/`Down` → `FocusNext`,
  alongside the existing `Tab`/`Shift+Tab`. Matches PC Tools 9.x button navigation.
- Accelerator broadcast: if the focused view does not consume a character key,
  `DispatchKeyEvent` broadcasts to the desktop tree so any button with a matching
  accelerator can respond regardless of which view currently holds focus.
- `TuiRenderContext.DrawText(SKColor fg, SKColor bg)` and
  `DrawTextClipped(..., SKColor fg, SKColor bg)` and `FillRect(..., SKColor color)`:
  `SKColor` overloads added alongside the existing `TuiColorRole` overloads.
  Role-based overloads now delegate to the `SKColor` overloads (inverted delegation —
  eliminates code duplication).
- `Retro.TUI.Sample.Basic`: EGA palette window (256 fg × bg combinations) and
  button demo window (factory buttons, disabled state, Tab/arrow/accelerator navigation,
  command feedback label).
- 417 tests passing across all projects (0 failed).

### Changed

- `TuiPalette`: replaced the instantiable immutable map with a static class exposing
  `Resolve(TuiColorRole)` and the new `Resolve(TuiEgaColor)` overload. The color
  scheme is fixed and cannot be customized at runtime.
- `TuiTheme`: replaced the instantiable aggregate with a static class holding only
  fixed typography and shadow parameters. IBM VGA 9×16 typeface embedded in
  `Retro.TUI.Theming` and loaded via static constructor.
- `TuiTheme.ShadowOffsetX`/`ShadowOffsetY`: changed from raw pixel values (8 px / 7 px)
  to cell counts (1 / 1). `TuiRenderContext.DrawShadow` multiplies by
  `Grid.CellWidth` / `(Grid.CellHeight / 2 - 1)` to obtain pixel offsets, producing
  proportional shadows for both windows and single-row widgets (buttons).
- `TuiRenderContext`: constructor no longer accepts a `TuiTheme` parameter. Color
  scheme and shadow parameters resolved statically via `TuiPalette` and `TuiTheme`.
- `TuiApplication.Run`: `TuiTheme theme` parameter removed.
- `TuiDesktop.Draw`: replaced `ctx.DrawDesktopPattern(...)` with a solid
  `ctx.FillRect(...)` using `TuiColorRole.DesktopBackground`.
- `TuiColorMap`: `ButtonBackground` corrected to `#CACACA` (was `#FFFFFF`).
  `ButtonFocusBackground` stays `#FFFFFF`. Both now match the PC Tools 9.x reference.
- `TuiWindow.DrawTitleBar` / `TuiDialog.DrawTitleBar`: title text region starts at
  `AbsCol + 2` / `Width - 2` (was `+ 1` / `- 1`) to leave one column of visual
  separation from the `[-]` glyph, which physically overflows into the adjacent cell.
- `TuiWindow.HandleEvent`: unhandled mouse events now `return false` instead of
  `return base.HandleEvent(ev)`, preventing re-dispatch to children without hit-testing
  (which caused buttons to fire when clicking on the window background).
- `TuiFocusManager.FocusNext` / `FocusPrevious`: disabled views are now skipped.
  A new private `FindEnabledFrom(int start, bool forward)` helper cycles the tab order
  and returns the index of the first enabled view.
- Mouse cursor bitmap: replaced Windows 95/NT-style 16×20 shape with the accurate
  12×20 Windows 3.11-style pixel-art arrow per PC Tools 9.x visual reference.
- `[-]` system-menu glyph: corrected to 18×16 px (was 16×16 px). Hit-test and drag
  detection updated accordingly.

### Removed

- `TuiDesktopPattern` enum and all pattern-rendering methods — desktop always renders
  a solid background.
- `TuiColorRole.DesktopPatternDot` — no longer needed.
- `Retro.TUI.Theme.PcTools9` project — font now embedded in `Retro.TUI.Theming`,
  color scheme defined in `TuiColorMap`.
- `docs/theming-reference.md` and `docs/theming-reference.es.md` — superseded by
  the fixed EGA color scheme; inventory now in `TuiColorRole.cs` XML documentation.
- `tests/Retro.TUI.Theming.Tests/PcTools9ThemeTests.cs` and
  `tests/Retro.TUI.Rendering.Tests/PcTools9FontIntegrationTests.cs` — covered by
  new `TuiPaletteTests`.

### Fixed

- `TuiButton.Activate()`: command now routed by walking up to `TuiDesktop.CommandSink`
  directly, avoiding `TuiGroup.HandleEvent` child re-dispatch (which caused
  `StackOverflowException`).
- `TuiButton` accelerator focus transfer: pressing an accelerator key now calls
  `RequestFocus()` → `TuiDesktop.FocusSink` before `Activate()`, so the focused
  button updates visually before the command fires — matching mouse-click behaviour.
- `TuiFocusManager.SetFocus` replaced by `TrySetFocus` in `DispatchMouseEvent` to
  avoid `ArgumentException` when a focusable view is not registered (e.g. in tests
  that use `RecordingView` without a focus manager).

### Known limitations

- `TuiApplication.Run` requires a fully initialised `TuiHostOptions`. No defaults.
- `_cursorBitmap` cached at first draw; stale if theme changes at runtime.
- `icon.png` referenced in `Directory.Build.props` but not yet present in the
  repository — `dotnet pack` warns until it is added.
- `TuiApplication` and `SdlHost` remain at 0 % unit-test coverage (live host
  required); excluded by design.

## [0.3.0-alpha.3] - 2026-06-27

### Added

- Initial project structure and architecture documentation.
- `Retro.TUI.Events` project: `TuiEvent` abstract record hierarchy
  (`TuiKeyEvent`, `TuiMouseEvent`, `TuiCommandEvent`, `TuiTimerEvent`, `TuiFocusEvent`)
  and supporting enums (`TuiKey`, `TuiModifiers`, `TuiMouseAction`,
  `TuiMouseButton`, `TuiFocusAction`, `TuiCommand`).
- `TuiEventQueue`: thread-safe bounded `Channel<TuiEvent>` (capacity 256,
  `DropOldest`) with `TryPost`, `PostAsync`, `TryRead` and `ReadAsync`.
- `Retro.TUI.Events.Tests`: 42 unit tests.
- `scripts/delete-bin-and-obj-folders.cmd` and
  `scripts/run-test-projects-with-code-coverage.cmd`.
- `Retro.TUI.Theming` project: `TuiColorRole` enum (75 semantic color roles),
  `TuiPalette`, `TuiDesktopPattern` and `TuiTheme` aggregate.
- `Retro.TUI.Theme.PcTools9` project: `PcTools9Theme` with canonical PC Tools 9.x
  palette and embedded IBM VGA 9×16 font.
- `Retro.TUI.Theming.Tests`: 52 unit tests.
- `docs/theming-reference.md` and `docs/theming-reference.es.md`.
- `Retro.TUI.Hosting`: `ITuiHost`, `TuiHostOptions`, `SdlHost`, `SdlKeyMapper`.
  SDL resize handling: vetoes resize when `Resizable = false`; posts
  `TuiCommandEvent(Resize)` when `Resizable = true`.
- `TuiCommand.Resize = 8`.
- `Retro.TUI.Hosting.Tests`: `TuiHostOptionsTests`, `ITuiHostContractTests`,
  `SdlKeyMapperTests`.
- `Retro.TUI.Rendering`: `TuiGrid`, `TuiFont`, `TuiRenderContext` (grid-oriented
  drawing API; `BeginFrame`/`EndFrame` internal; `RenderFrame` public).
  `DrawBorder`: 2 px left + 2 px bottom, geometric lines.
- `Retro.TUI.Rendering.Tests`.
- `Retro.TUI.Views`: `TuiView`, `TuiGroup`, `TuiDesktop` (modal stack, `CommandSink`).
  `IModalDialog` interface decouples modal loop from `Retro.TUI.Windows`.
- `TuiView.HasCustomMouseHandling`, `TuiGroup.IsFrontmost`, `TuiGroup.Frontmost`.
- `Retro.TUI.Views.Tests`.
- `Retro.TUI.Core`: `TuiFocusManager`, `TuiMessageLoop` (poll → dispatch → render →
  cursor → present), `TuiApplication` (`Run`, `RunModal`, `OnCommand`, `OnInitialize`).
- `Retro.TUI.Core.Tests`.
- `Retro.TUI.Windows`: `TuiWindow` (title, border, shadow, drag, z-order, `[-]`),
  `TuiDialog` (modal, `IModalDialog`, `Dialog*` color roles).
- `Retro.TUI.Windows.Tests`.
- Custom mouse cursor: 12×19 pixel-art arrow bitmap via `DrawMouseCursor`.
- `Retro.TUI.Sample.Basic`: two draggable windows, z-order, `ConfirmCloseDialog`.
- 461 tests passing across all projects (0 failed).

### Changed

- `PcTools9Theme`: `SKTypeface.FromStream` replaces removed `SKFontManager.RegisterTypeface`.
- `PcTools9Theme`: `DialogBackground` → `EgaBrightCyan`.
- `TuiRenderContext`: added `RenderFrame(SKSurface, Action<TuiRenderContext>)`.
- `TuiWindow.DrawTitleBar`: extracted as `protected virtual`.
- `TuiGroup.Draw`: removed `PushClip`/`PopClip`.

### Fixed

- `SdlHost.Present`: texture unlocked before `RenderCopy`.
- `TuiRenderContext.DrawShadow`: corner overlap removed.
- `TuiRenderContext.DrawBorder`: stroke width corrected to 1 px.
- `TuiWindow.Draw`: window fill added; clip applied only to inner area.
- Mouse drag outside window chrome no longer drops events.
- `_modalWasOpened` flag prevents orphaned capture on synchronous `RunModal`.
- `ButtonUp` inside modal fast path clears `_capturedView`.
- `[-]` double-trigger: hit-test and drag exclude `AbsCol + 1`.
- `TuiDialog.Draw`: uses `DrawChildViews` to avoid `WindowBackground` overwrite.
- `TuiWindow` inactive glyph: uses `WindowTitleInactiveBackground`/`WindowBorder`.

### Removed

- `DrawBorderDouble`: not in PC Tools 9.x reference.

### Known limitations

- `TuiApplication.Run` requires fully initialised `TuiHostOptions`.
- `_cursorBitmap` cached at first draw; stale on runtime theme change.
- `icon.png` referenced but not yet present.
- `TuiApplication` and `SdlHost` at 0 % unit-test coverage by design.

[Unreleased]: https://github.com/OscarNET-SOFTware/Retro.TUI.Framework/compare/v0.4.1-alpha.4...HEAD
[0.4.1-alpha.4]: https://github.com/OscarNET-SOFTware/Retro.TUI.Framework/compare/v0.3.0-alpha.3...v0.4.1-alpha.4
[0.3.0-alpha.3]: https://github.com/OscarNET-SOFTware/Retro.TUI.Framework/releases/tag/v0.3.0-alpha.3