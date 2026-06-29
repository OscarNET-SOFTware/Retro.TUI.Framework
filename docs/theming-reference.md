# Retro.TUI.Framework — Theming Reference

> Theming system reference document.  
> Complements [`architecture.md`](architecture.md) with the complete inventory
> of color roles and the `Retro.TUI.Theme.PcTools9` theme palette.

---

## Table of Contents

1. [Table A — TuiColorRole Inventory](#1-table-a--tuicolorrole-inventory)
   - [1.1 Desktop](#11-desktop)
   - [1.2 Application title bar](#12-application-title-bar)
   - [1.3 Window](#13-window)
   - [1.4 Dialog](#14-dialog)
   - [1.5 Menu bar](#15-menu-bar)
   - [1.6 Menu popup](#16-menu-popup)
   - [1.7 Status bar](#17-status-bar)
   - [1.8 Controls — Label](#18-controls--label)
   - [1.9 Controls — Input](#19-controls--input)
   - [1.10 Controls — Button](#110-controls--button)
   - [1.11 Controls — CheckBox and RadioButton](#111-controls--checkbox-and-radiobutton)
   - [1.12 Controls — ListBox](#112-controls--listbox)
   - [1.13 Controls — ScrollBar](#113-controls--scrollbar)
   - [1.14 Deferred elements](#114-deferred-elements)
2. [Table B — PC Tools 9.x Palette](#2-table-b--pc-tools-9x-palette)
   - [2.1 Full palette by TuiColorRole](#21-full-palette-by-tuicolorrole)
   - [2.2 Unique colors in the palette](#22-unique-colors-in-the-palette)

---

## 1. Table A — TuiColorRole Inventory

Cross-reference of two sources:

- **Turbo Vision** — palette entries from the _Programming Guide_ (Borland, 1992).
- **PC Tools 9.x** — visual behavior analysis of Central Point Software (~1991).

Status legend:

| Tag | Meaning |
|---|---|
| `implemented` | Present in `TuiColorRole` and assigned a value in `PcTools9Theme` |
| `deferred` | Reserved for a future milestone |

---

### 1.1 Desktop

| TuiColorRole | Turbo Vision ref. | PC Tools color | Notes | Status |
|---|---|---|---|---|
| `DesktopBackground` | `apColor[0]` bg | `#696969` | Desktop area fill color | `implemented` |

---

### 1.2 Application title bar

| TuiColorRole | Turbo Vision ref. | PC Tools color | Notes | Status |
|---|---|---|---|---|
| `AppTitleBackground` | `apColor[1]` | `#5555FF` | Same blue as windows and dialogs | `implemented` |
| `AppTitleForeground` | `apColor[2]` | `#FFFFFF` | Centered title text | `implemented` |
| `AppTitleClockForeground` | — | `#FFFF55` | Clock uses its own role, independent of `AppTitleForeground` | `implemented` |

---

### 1.3 Window

| TuiColorRole | Turbo Vision ref. | PC Tools color | Notes | Status |
|---|---|---|---|---|
| `WindowBackground` | `wpColor[0]` | `#5555FF` | Window client area fill | `implemented` |
| `WindowForeground` | `wpColor[1]` | `#FFFFFF` | Default text inside the window | `implemented` |
| `WindowBorder` | `wpColor[4]` | `#000000` | Active window outer border | `implemented` |
| `WindowTitleBackground` | `wpColor[2]` | `#FFFFFF` | Active window title bar | `implemented` |
| `WindowTitleForeground` | `wpColor[3]` | `#000000` | Active window title text | `implemented` |
| `WindowTitleInactiveBackground` | `wpColor[6]*` | `#CACACA` | Background window title bar | `implemented` |
| `WindowTitleInactiveForeground` | `wpColor[7]*` | `#696969` | Background window title text | `implemented` |
| `WindowCloseButtonBackground` | — | `#FFFFFF` | Close/restore button glyph area background | `implemented` |
| `WindowCloseButtonForeground` | — | `#000000` | Close/restore button glyph | `implemented` |
| `WindowShadow` | `wpColor[5]` | `#000000` | Pure black; opacity controlled by `TuiTheme.ShadowOpacity` | `implemented` |

---

### 1.4 Dialog

| TuiColorRole | Turbo Vision ref. | PC Tools color | Notes | Status |
|---|---|---|---|---|
| `DialogBackground` | `dpColor[0]` | `#5555FF` | Dialog client area fill | `implemented` |
| `DialogForeground` | `dpColor[1]` | `#FFFFFF` | Default text inside the dialog | `implemented` |
| `DialogBorder` | `dpColor[4]` | `#000000` | Dialog outer border | `implemented` |
| `DialogTitleBackground` | `dpColor[2]` | `#FFFFFF` | Normal dialog title bar | `implemented` |
| `DialogTitleForeground` | `dpColor[3]` | `#000000` | Normal dialog title text | `implemented` |
| `DialogTitleWarningBackground` | — | `#FF5555` | Warning or error dialog title bar | `implemented` |
| `DialogHighlightForeground` | — | `#FFFF55` | Emphasis text inside a dialog, distinct from normal foreground | `implemented` |

---

### 1.5 Menu bar

| TuiColorRole | Turbo Vision ref. | PC Tools color | Notes | Status |
|---|---|---|---|---|
| `MenuBackground` | `CMenuView[1]` | `#FFFFFF` | Horizontal menu bar background | `implemented` |
| `MenuForeground` | `CMenuView[1]` | `#000000` | Normal item text in the bar | `implemented` |
| `MenuHotkeyForeground` | `CMenuView[3]` Shortcut | `#AA0000` | Accelerator letter in bar items. Turbo Vision: _Shortcut color_ | `implemented` |
| `MenuSelectedBackground` | `CMenuView[2]` | `#000000` | Active item background in the bar | `implemented` |
| `MenuSelectedForeground` | `CMenuView[2]` | `#FFFFFF` | Active item text in the bar | `implemented` |
| `MenuDisabledForeground` | `CMenuView[5]` | `#696969` | Disabled item text | `implemented` |
| `MenuSeparator` | `CMenuView[6]` | `#000000` | Horizontal separator line in the popup | `implemented` |

---

### 1.6 Menu popup

The menu bar and popup share the same white background with black text. The popup adds its own border and shadow.

| TuiColorRole | Turbo Vision ref. | PC Tools color | Notes | Status |
|---|---|---|---|---|
| `MenuPopupBackground` | `CMenuView[1]` | `#FFFFFF` | Drop-down panel background | `implemented` |
| `MenuPopupForeground` | `CMenuView[1]` | `#000000` | Normal item text in the popup | `implemented` |
| `MenuPopupHotkeyForeground` | `CMenuView[3]` | `#AA0000` | Accelerator letter of an unselected popup item | `implemented` |
| `MenuPopupSelectedBackground` | `CMenuView[2]` | `#000000` | Selected popup item background | `implemented` |
| `MenuPopupSelectedForeground` | `CMenuView[2]` | `#FFFFFF` | Selected popup item text | `implemented` |
| `MenuPopupSelectedHotkeyForeground` | `CMenuView[4]` Selected Shortcut | `#AA0000` | Accelerator letter of the selected popup item | `implemented` |
| `MenuPopupSubMenuArrow` | — | `#FFFFFF` | Sub-menu indicator `▶` on items with nested popups | `implemented` |
| `MenuPopupBorder` | — | `#000000` | Popup panel border | `implemented` |
| `MenuPopupShadow` | — | `#000000` | Popup shadow; opacity via `TuiTheme.ShadowOpacity` | `implemented` |

---

### 1.7 Status bar

| TuiColorRole | Turbo Vision ref. | PC Tools color | Notes | Status |
|---|---|---|---|---|
| `StatusBackground` | `CStatusLine[1]` | `#696969` | Same value as `DesktopBackground` | `implemented` |
| `StatusForeground` | `CStatusLine[1]` | `#FFFFFF` | Normal item text | `implemented` |
| `StatusKeyBackground` | `CStatusLine[2]` | `#696969` | Same background; no visual differentiation from `StatusBackground` | `implemented` |
| `StatusKeyForeground` | `CStatusLine[2]` | `#FFFF55` | Function-key number (F1–F10) | `implemented` |

---

### 1.8 Controls — Label

| TuiColorRole | Turbo Vision ref. | PC Tools color | Notes | Status |
|---|---|---|---|---|
| `LabelForeground` | `dpColor` fg | `#FFFFFF` | Label text | `implemented` |
| `LabelBackground` | `dpColor` bg | `#5555FF` | Transparent in practice — same as `DialogBackground` | `implemented` |

---

### 1.9 Controls — Input

| TuiColorRole | Turbo Vision ref. | PC Tools color | Notes | Status |
|---|---|---|---|---|
| `InputBackground` | `dpColor[6]` | `#FFFFFF` | Unfocused input field background | `implemented` |
| `InputForeground` | `dpColor[6]` | `#000000` | Text inside the field | `implemented` |
| `InputFocusBackground` | `dpColor[7]` | `#FFFFFF` | Focus is expressed with cursor, not background color | `implemented` |
| `InputFocusForeground` | `dpColor[7]` | `#000000` | Focused field text | `implemented` |
| `InputSelectionBackground` | `dpColor[8]` | `#000000` | Selected text background | `implemented` |
| `InputSelectionForeground` | `dpColor[8]` | `#FFFFFF` | Selected text foreground | `implemented` |
| `InputBorderBottom` | — | `#000000` | Bottom and right shadow/border of the field. Characteristic PC Tools visual pattern | `implemented` |

---

### 1.10 Controls — Button

| TuiColorRole | Turbo Vision ref. | PC Tools color | Notes | Status |
|---|---|---|---|---|
| `ButtonBackground` | `dpColor[9]` | `#FFFFFF` | Unfocused button background | `implemented` |
| `ButtonForeground` | `dpColor[9]` | `#000000` | Button label text | `implemented` |
| `ButtonAcceleratorForeground` | `dpColor[10]` Shortcut | `#AA0000` | Accelerator letter of an unfocused button | `implemented` |
| `ButtonFocusBackground` | `dpColor[11]` | `#FFFFFF` | Same as `ButtonBackground`; focus expressed by border | `implemented` |
| `ButtonFocusForeground` | `dpColor[11]` | `#000000` | Focused button label text | `implemented` |
| `ButtonFocusAcceleratorForeground` | `dpColor[12]` | `#AA0000` | Accelerator letter of a focused button | `implemented` |
| `ButtonShadow` | — | `#000000` | Bottom and right drop shadow | `implemented` |

---

### 1.11 Controls — CheckBox and RadioButton

| TuiColorRole | Turbo Vision ref. | PC Tools color | Notes | Status |
|---|---|---|---|---|
| `CheckBackground` | `dpColor` bg | `#5555FF` | Same as `DialogBackground` | `implemented` |
| `CheckForeground` | `dpColor` fg | `#FFFFFF` | Control label text | `implemented` |
| `CheckFocusBackground` | — | `#5555FF` | No visual differentiation from the normal state | `implemented` |
| `CheckFocusForeground` | — | `#FFFFFF` | Focused label text | `implemented` |
| `CheckMarkColor` | — | `#FFFFFF` | Mark glyph: `●` (RadioButton) and `✓` (CheckBox) | `implemented` |
| `CheckAcceleratorForeground` | — | `#FFFF55` | Accelerator letter in the control label | `implemented` |

---

### 1.12 Controls — ListBox

| TuiColorRole | Turbo Vision ref. | PC Tools color | Notes | Status |
|---|---|---|---|---|
| `ListBackground` | `dpColor` bg | `#5555FF` | Unselected item area background | `implemented` |
| `ListForeground` | `dpColor` fg | `#FFFFFF` | Normal item text | `implemented` |
| `ListSelectedBackground` | `dpColor` sel | `#000000` | Selected item background | `implemented` |
| `ListSelectedForeground` | `dpColor` sel | `#FFFFFF` | Selected item text | `implemented` |
| `ListHeaderBackground` | — | `#000000` | Header row background | `implemented` |
| `ListHeaderForeground` | — | `#FFFFFF` | Header row text | `implemented` |

---

### 1.13 Controls — ScrollBar

The PC Tools scroll bar has three distinct color zones: inactive track, active area and position thumb.

| TuiColorRole | Turbo Vision ref. | PC Tools color | Notes | Status |
|---|---|---|---|---|
| `ScrollBarBackground` | — | `#CACACA` | Inactive track (not yet scrolled) | `implemented` |
| `ScrollBarActiveBackground` | — | `#FFFFFF` | Active area (already scrolled) | `implemented` |
| `ScrollBarThumb` | — | `#696969` | Position indicator square | `implemented` |
| `ScrollBarArrow` | — | `#000000` | Arrow glyphs `▲▼◄►` | `implemented` |
| `ScrollBarArrowBackground` | — | `#CACACA` | Arrow button area background | `implemented` |

---

### 1.14 Deferred elements

The following roles are not implemented in this milestone. They will be added to `TuiColorRole` and `PcTools9Theme` in the corresponding widgets milestone.

| Element | Planned roles | Milestone |
|---|---|---|
| `ProgressBar` | `ProgressBarBackground`, `ProgressBarFill` | Widgets |
| `TreeView` | `TreeViewBackground`, `TreeViewForeground`, `TreeViewSelectedBackground`, `TreeViewSelectedForeground` | Widgets |

---

## 2. Table B — PC Tools 9.x Palette

Values obtained by direct visual behavior analysis of PC Tools 9.x (Central Point Software, ~1991) and verified by color measurement.

Origin legend:

| Tag | Meaning |
|---|---|
| `measured` | Value obtained by direct pixel measurement |
| `derived` | Logically deduced from measured values |

---

### 2.1 Full palette by TuiColorRole

| TuiColorRole | Hex | R | G | B | Origin |
|---|---|---|---|---|---|
| `DesktopBackground` | `#696969` | 105 | 105 | 105 | `measured` |
| `AppTitleBackground` | `#5555FF` | 85 | 85 | 255 | `measured` |
| `AppTitleForeground` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `AppTitleClockForeground` | `#FFFF55` | 255 | 255 | 85 | `measured` |
| `WindowBackground` | `#5555FF` | 85 | 85 | 255 | `measured` |
| `WindowForeground` | `#FFFFFF` | 255 | 255 | 255 | `derived` |
| `WindowBorder` | `#000000` | 0 | 0 | 0 | `derived` |
| `WindowTitleBackground` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `WindowTitleForeground` | `#000000` | 0 | 0 | 0 | `measured` |
| `WindowTitleInactiveBackground` | `#CACACA` | 202 | 202 | 202 | `measured` |
| `WindowTitleInactiveForeground` | `#696969` | 105 | 105 | 105 | `derived` |
| `WindowCloseButtonBackground` | `#FFFFFF` | 255 | 255 | 255 | `derived` — same pattern as `WindowTitleBackground` |
| `WindowCloseButtonForeground` | `#000000` | 0 | 0 | 0 | `derived` — same pattern as `WindowTitleForeground` |
| `WindowShadow` | `#000000` | 0 | 0 | 0 | `measured` — opacity via `TuiTheme.ShadowOpacity` |
| `DialogBackground` | `#5555FF` | 85 | 85 | 255 | `measured` — same as `WindowBackground` |
| `DialogForeground` | `#FFFFFF` | 255 | 255 | 255 | `derived` |
| `DialogBorder` | `#000000` | 0 | 0 | 0 | `derived` |
| `DialogTitleBackground` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `DialogTitleForeground` | `#000000` | 0 | 0 | 0 | `measured` |
| `DialogTitleWarningBackground` | `#FF5555` | 255 | 85 | 85 | `measured` |
| `DialogHighlightForeground` | `#FFFF55` | 255 | 255 | 85 | `measured` |
| `MenuBackground` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `MenuForeground` | `#000000` | 0 | 0 | 0 | `measured` |
| `MenuHotkeyForeground` | `#AA0000` | 170 | 0 | 0 | `measured` |
| `MenuSelectedBackground` | `#000000` | 0 | 0 | 0 | `derived` |
| `MenuSelectedForeground` | `#FFFFFF` | 255 | 255 | 255 | `derived` |
| `MenuDisabledForeground` | `#696969` | 105 | 105 | 105 | `derived` |
| `MenuSeparator` | `#000000` | 0 | 0 | 0 | `derived` |
| `MenuPopupBackground` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `MenuPopupForeground` | `#000000` | 0 | 0 | 0 | `measured` |
| `MenuPopupHotkeyForeground` | `#AA0000` | 170 | 0 | 0 | `measured` |
| `MenuPopupSelectedBackground` | `#000000` | 0 | 0 | 0 | `measured` |
| `MenuPopupSelectedForeground` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `MenuPopupSelectedHotkeyForeground` | `#AA0000` | 170 | 0 | 0 | `derived` |
| `MenuPopupSubMenuArrow` | `#FFFFFF` | 255 | 255 | 255 | `derived` |
| `MenuPopupBorder` | `#000000` | 0 | 0 | 0 | `derived` |
| `MenuPopupShadow` | `#000000` | 0 | 0 | 0 | `derived` |
| `StatusBackground` | `#696969` | 105 | 105 | 105 | `measured` — same as `DesktopBackground` |
| `StatusForeground` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `StatusKeyBackground` | `#696969` | 105 | 105 | 105 | `derived` |
| `StatusKeyForeground` | `#FFFF55` | 255 | 255 | 85 | `measured` |
| `LabelForeground` | `#FFFFFF` | 255 | 255 | 255 | `derived` |
| `LabelBackground` | `#5555FF` | 85 | 85 | 255 | `derived` — same as `DialogBackground` |
| `InputBackground` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `InputForeground` | `#000000` | 0 | 0 | 0 | `measured` |
| `InputFocusBackground` | `#FFFFFF` | 255 | 255 | 255 | `derived` — focus expressed with cursor |
| `InputFocusForeground` | `#000000` | 0 | 0 | 0 | `derived` |
| `InputSelectionBackground` | `#000000` | 0 | 0 | 0 | `derived` — inverted pattern |
| `InputSelectionForeground` | `#FFFFFF` | 255 | 255 | 255 | `derived` |
| `InputBorderBottom` | `#000000` | 0 | 0 | 0 | `measured` — same black as `ButtonShadow` |
| `ButtonBackground` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `ButtonForeground` | `#000000` | 0 | 0 | 0 | `derived` |
| `ButtonAcceleratorForeground` | `#AA0000` | 170 | 0 | 0 | `measured` |
| `ButtonFocusBackground` | `#FFFFFF` | 255 | 255 | 255 | `derived` — same as `ButtonBackground` |
| `ButtonFocusForeground` | `#000000` | 0 | 0 | 0 | `derived` |
| `ButtonFocusAcceleratorForeground` | `#AA0000` | 170 | 0 | 0 | `derived` |
| `ButtonShadow` | `#000000` | 0 | 0 | 0 | `measured` |
| `CheckBackground` | `#5555FF` | 85 | 85 | 255 | `derived` — same as `DialogBackground` |
| `CheckForeground` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `CheckFocusBackground` | `#5555FF` | 85 | 85 | 255 | `derived` |
| `CheckFocusForeground` | `#FFFFFF` | 255 | 255 | 255 | `derived` |
| `CheckMarkColor` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `CheckAcceleratorForeground` | `#FFFF55` | 255 | 255 | 85 | `measured` |
| `ListBackground` | `#5555FF` | 85 | 85 | 255 | `measured` |
| `ListForeground` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `ListSelectedBackground` | `#000000` | 0 | 0 | 0 | `measured` |
| `ListSelectedForeground` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `ListHeaderBackground` | `#000000` | 0 | 0 | 0 | `measured` |
| `ListHeaderForeground` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `ScrollBarBackground` | `#CACACA` | 202 | 202 | 202 | `measured` |
| `ScrollBarActiveBackground` | `#FFFFFF` | 255 | 255 | 255 | `measured` |
| `ScrollBarThumb` | `#696969` | 105 | 105 | 105 | `measured` |
| `ScrollBarArrow` | `#000000` | 0 | 0 | 0 | `measured` |
| `ScrollBarArrowBackground` | `#CACACA` | 202 | 202 | 202 | `measured` |

---

### 2.2 Unique colors in the palette

PC Tools 9.x uses only **8 distinct colors** across its entire interface:

| Hex | Name | EGA/VGA standard | Constant in `PcTools9Theme` | Primary roles |
|---|---|---|---|---|
| `#000000` | Pure black | Yes — color #0 | `Black` | Borders, shadows, text on light backgrounds, selections |
| `#5555FF` | EGA bright blue | Yes — color #9 | `EgaBrightBlue` | `AppTitle`, `Window`, `Dialog`, `Label`, `Check`, `List` backgrounds |
| `#696969` | PC Tools gray | **No** — custom value | `PcToolsGray` | `Desktop`, `Status`, `ScrollBarThumb`, `WindowTitleInactive` fg |
| `#AA0000` | EGA dark red | Yes — color #4 | `EgaDarkRed` | Accelerator letters in buttons and menus |
| `#CACACA` | PC Tools light gray | **No** — custom value | `PcToolsLightGray` | `ScrollBar` track/arrows, `WindowTitleInactive` bg |
| `#FF5555` | EGA bright red | Yes — color #12 | `EgaBrightRed` | `DialogTitleWarning` bg |
| `#FFFF55` | EGA bright yellow | Yes — color #14 | `EgaBrightYellow` | `StatusKey`, `CheckAccelerator`, `AppTitleClock`, `DialogHighlight` |
| `#FFFFFF` | Pure white | Yes — color #15 | `White` | General text, titles, buttons, inputs, `MenuBar`, `ScrollBar` active |

> **Note**: `#696969` (`PcToolsGray`) and `#CACACA` (`PcToolsLightGray`) are the only values
> that deviate from the standard 16-color EGA/VGA palette. PC Tools uses them for the system's
> "chrome" elements (desktop, bars, scrollbars), visually distinguishing them from application content.

---

*Sources: visual analysis of PC Tools 9.x (Central Point Software, ~1991), Turbo Vision Programming Guide (Borland, 1992).*