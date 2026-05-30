# Retro.TUI.Framework — Referencia de Theming

> Documento de referencia del sistema de temas.  
> Complementa [`architecture.es.md`](architecture.es.md) con el inventario completo
> de roles de color y la paleta del tema `Retro.TUI.Theme.PcTools9`.

---

## Índice

1. [Tabla A — Inventario de TuiColorRole](#1-tabla-a--inventario-de-tuicolorrole)
   - [1.1 Escritorio](#11-escritorio)
   - [1.2 Barra de título de aplicación](#12-barra-de-título-de-aplicación)
   - [1.3 Ventana](#13-ventana)
   - [1.4 Diálogo](#14-diálogo)
   - [1.5 Barra de menú](#15-barra-de-menú)
   - [1.6 Menú desplegable (popup)](#16-menú-desplegable-popup)
   - [1.7 Barra de estado](#17-barra-de-estado)
   - [1.8 Controles — Label](#18-controles--label)
   - [1.9 Controles — Input](#19-controles--input)
   - [1.10 Controles — Button](#110-controles--button)
   - [1.11 Controles — CheckBox y RadioButton](#111-controles--checkbox-y-radiobutton)
   - [1.12 Controles — ListBox](#112-controles--listbox)
   - [1.13 Controles — ScrollBar](#113-controles--scrollbar)
   - [1.14 Elementos diferidos](#114-elementos-diferidos)
2. [Tabla B — Paleta PC Tools 9.x](#2-tabla-b--paleta-pc-tools-9x)
   - [2.1 Paleta completa por TuiColorRole](#21-paleta-completa-por-tuicolorrole)
   - [2.2 Colores únicos de la paleta](#22-colores-únicos-de-la-paleta)

---

## 1. Tabla A — Inventario de TuiColorRole

Cruce de dos fuentes de referencia:

- **Turbo Vision** — entradas de paleta del _Programming Guide_ (Borland, 1992).
- **PC Tools 9.x** — análisis visual del comportamiento de color de Central Point Software (~1991).

Leyenda de estado:

| Etiqueta | Significado |
|---|---|
| `implementado` | Presente en `TuiColorRole` y con valor asignado en `PcTools9Theme` |
| `diferido` | Reservado para un milestone posterior |

---

### 1.1 Escritorio

| TuiColorRole | Ref. Turbo Vision | Color PC Tools | Notas | Estado |
|---|---|---|---|---|
| `DesktopBackground` | `apColor[0]` bg | `#696969` | Color de relleno del escritorio sin patrón | `implementado` |
| `DesktopPatternDot` | — | `#696969` | Para el modo `DotGrid`. Igual a `DesktopBackground` en el tema PC Tools | `implementado` |

---

### 1.2 Barra de título de aplicación

| TuiColorRole | Ref. Turbo Vision | Color PC Tools | Notas | Estado |
|---|---|---|---|---|
| `AppTitleBackground` | `apColor[1]` | `#5555FF` | Mismo azul que ventanas y diálogos | `implementado` |
| `AppTitleForeground` | `apColor[2]` | `#FFFFFF` | Texto centrado del título | `implementado` |
| `AppTitleClockForeground` | — | `#FFFF55` | El reloj usa color propio, independiente de `AppTitleForeground` | `implementado` |

---

### 1.3 Ventana

| TuiColorRole | Ref. Turbo Vision | Color PC Tools | Notas | Estado |
|---|---|---|---|---|
| `WindowBackground` | `wpColor[0]` | `#5555FF` | Color de relleno del área cliente | `implementado` |
| `WindowForeground` | `wpColor[1]` | `#FFFFFF` | Texto por defecto dentro de la ventana | `implementado` |
| `WindowBorder` | `wpColor[4]` | `#000000` | Borde exterior de la ventana activa | `implementado` |
| `WindowTitleBackground` | `wpColor[2]` | `#FFFFFF` | Barra de título de la ventana activa | `implementado` |
| `WindowTitleForeground` | `wpColor[3]` | `#000000` | Texto de la barra de título activa | `implementado` |
| `WindowTitleInactiveBackground` | `wpColor[6]*` | `#CACACA` | Barra de título de la ventana en segundo plano | `implementado` |
| `WindowTitleInactiveForeground` | `wpColor[7]*` | `#696969` | Texto de la barra de título inactiva | `implementado` |
| `WindowCloseButtonBackground` | — | `#FFFFFF` | Fondo del botón de cerrar/restaurar ventana | `implementado` |
| `WindowCloseButtonForeground` | — | `#000000` | Glifo del botón de cerrar/restaurar ventana | `implementado` |
| `WindowShadow` | `wpColor[5]` | `#000000` | Negro puro; opacidad controlada por `TuiTheme.ShadowOpacity` | `implementado` |

---

### 1.4 Diálogo

| TuiColorRole | Ref. Turbo Vision | Color PC Tools | Notas | Estado |
|---|---|---|---|---|
| `DialogBackground` | `dpColor[0]` | `#5555FF` | Color de relleno del área cliente del diálogo | `implementado` |
| `DialogForeground` | `dpColor[1]` | `#FFFFFF` | Texto por defecto dentro del diálogo | `implementado` |
| `DialogBorder` | `dpColor[4]` | `#000000` | Borde exterior del diálogo | `implementado` |
| `DialogTitleBackground` | `dpColor[2]` | `#FFFFFF` | Barra de título de diálogos normales | `implementado` |
| `DialogTitleForeground` | `dpColor[3]` | `#000000` | Texto de la barra de título normal | `implementado` |
| `DialogTitleWarningBackground` | — | `#FF5555` | Barra de título de diálogos de advertencia o error | `implementado` |
| `DialogHighlightForeground` | — | `#FFFF55` | Texto de énfasis dentro del diálogo, distinto del foreground normal | `implementado` |

---

### 1.5 Barra de menú

| TuiColorRole | Ref. Turbo Vision | Color PC Tools | Notas | Estado |
|---|---|---|---|---|
| `MenuBackground` | `CMenuView[1]` | `#FFFFFF` | Fondo de la barra horizontal de menú | `implementado` |
| `MenuForeground` | `CMenuView[1]` | `#000000` | Texto de los ítems normales de la barra | `implementado` |
| `MenuHotkeyForeground` | `CMenuView[3]` Shortcut | `#AA0000` | Letra aceleradora en los ítems de la barra. Turbo Vision: _Shortcut color_ | `implementado` |
| `MenuSelectedBackground` | `CMenuView[2]` | `#000000` | Fondo del ítem activo en la barra | `implementado` |
| `MenuSelectedForeground` | `CMenuView[2]` | `#FFFFFF` | Texto del ítem activo en la barra | `implementado` |
| `MenuDisabledForeground` | `CMenuView[5]` | `#696969` | Texto de ítems deshabilitados | `implementado` |
| `MenuSeparator` | `CMenuView[6]` | `#000000` | Línea horizontal separadora en el popup | `implementado` |

---

### 1.6 Menú desplegable (popup)

La barra de menú y el popup comparten el mismo fondo blanco con texto negro. El popup añade borde y sombra propios.

| TuiColorRole | Ref. Turbo Vision | Color PC Tools | Notas | Estado |
|---|---|---|---|---|
| `MenuPopupBackground` | `CMenuView[1]` | `#FFFFFF` | Fondo del panel desplegable | `implementado` |
| `MenuPopupForeground` | `CMenuView[1]` | `#000000` | Texto de ítems normales en el popup | `implementado` |
| `MenuPopupHotkeyForeground` | `CMenuView[3]` | `#AA0000` | Letra aceleradora de un ítem no seleccionado | `implementado` |
| `MenuPopupSelectedBackground` | `CMenuView[2]` | `#000000` | Fondo del ítem seleccionado en el popup | `implementado` |
| `MenuPopupSelectedForeground` | `CMenuView[2]` | `#FFFFFF` | Texto del ítem seleccionado | `implementado` |
| `MenuPopupSelectedHotkeyForeground` | `CMenuView[4]` Selected Shortcut | `#AA0000` | Letra aceleradora del ítem seleccionado | `implementado` |
| `MenuPopupSubMenuArrow` | — | `#FFFFFF` | Indicador `▶` de submenú anidado | `implementado` |
| `MenuPopupBorder` | — | `#000000` | Borde del rectángulo del popup | `implementado` |
| `MenuPopupShadow` | — | `#000000` | Sombra del popup; opacidad vía `TuiTheme.ShadowOpacity` | `implementado` |

---

### 1.7 Barra de estado

| TuiColorRole | Ref. Turbo Vision | Color PC Tools | Notas | Estado |
|---|---|---|---|---|
| `StatusBackground` | `CStatusLine[1]` | `#696969` | Mismo valor que `DesktopBackground` | `implementado` |
| `StatusForeground` | `CStatusLine[1]` | `#FFFFFF` | Texto de los ítems normales | `implementado` |
| `StatusKeyBackground` | `CStatusLine[2]` | `#696969` | Mismo fondo; sin diferenciación visual respecto a `StatusBackground` | `implementado` |
| `StatusKeyForeground` | `CStatusLine[2]` | `#FFFF55` | Número de tecla de función (F1→F10) | `implementado` |

---

### 1.8 Controles — Label

| TuiColorRole | Ref. Turbo Vision | Color PC Tools | Notas | Estado |
|---|---|---|---|---|
| `LabelForeground` | `dpColor` fg | `#FFFFFF` | Texto de la etiqueta | `implementado` |
| `LabelBackground` | `dpColor` bg | `#5555FF` | Transparente en la práctica — igual que `DialogBackground` | `implementado` |

---

### 1.9 Controles — Input

| TuiColorRole | Ref. Turbo Vision | Color PC Tools | Notas | Estado |
|---|---|---|---|---|
| `InputBackground` | `dpColor[6]` | `#FFFFFF` | Campo sin foco | `implementado` |
| `InputForeground` | `dpColor[6]` | `#000000` | Texto dentro del campo | `implementado` |
| `InputFocusBackground` | `dpColor[7]` | `#FFFFFF` | El foco se expresa con cursor, no con color de fondo | `implementado` |
| `InputFocusForeground` | `dpColor[7]` | `#000000` | Texto con foco | `implementado` |
| `InputSelectionBackground` | `dpColor[8]` | `#000000` | Fondo del texto seleccionado | `implementado` |
| `InputSelectionForeground` | `dpColor[8]` | `#FFFFFF` | Texto seleccionado | `implementado` |
| `InputBorderBottom` | — | `#000000` | Sombra/borde inferior y derecho del campo. Patrón visual característico de PC Tools | `implementado` |

---

### 1.10 Controles — Button

| TuiColorRole | Ref. Turbo Vision | Color PC Tools | Notas | Estado |
|---|---|---|---|---|
| `ButtonBackground` | `dpColor[9]` | `#FFFFFF` | Fondo del botón sin foco | `implementado` |
| `ButtonForeground` | `dpColor[9]` | `#000000` | Texto del botón | `implementado` |
| `ButtonAcceleratorForeground` | `dpColor[10]` Shortcut | `#AA0000` | Letra aceleradora del botón sin foco | `implementado` |
| `ButtonFocusBackground` | `dpColor[11]` | `#FFFFFF` | Igual que `ButtonBackground`; el foco se expresa con borde | `implementado` |
| `ButtonFocusForeground` | `dpColor[11]` | `#000000` | Texto del botón con foco | `implementado` |
| `ButtonFocusAcceleratorForeground` | `dpColor[12]` | `#AA0000` | Letra aceleradora del botón con foco | `implementado` |
| `ButtonShadow` | — | `#000000` | Sombra inferior y derecha del botón | `implementado` |

---

### 1.11 Controles — CheckBox y RadioButton

| TuiColorRole | Ref. Turbo Vision | Color PC Tools | Notas | Estado |
|---|---|---|---|---|
| `CheckBackground` | `dpColor` bg | `#5555FF` | Mismo que `DialogBackground` | `implementado` |
| `CheckForeground` | `dpColor` fg | `#FFFFFF` | Texto de la etiqueta del control | `implementado` |
| `CheckFocusBackground` | — | `#5555FF` | Sin diferenciación visual respecto al estado normal | `implementado` |
| `CheckFocusForeground` | — | `#FFFFFF` | Texto con foco | `implementado` |
| `CheckMarkColor` | — | `#FFFFFF` | Glifo de marca: `●` (RadioButton) y `✓` (CheckBox) | `implementado` |
| `CheckAcceleratorForeground` | — | `#FFFF55` | Letra aceleradora en la etiqueta del control | `implementado` |

---

### 1.12 Controles — ListBox

| TuiColorRole | Ref. Turbo Vision | Color PC Tools | Notas | Estado |
|---|---|---|---|---|
| `ListBackground` | `dpColor` bg | `#5555FF` | Fondo de los ítems no seleccionados | `implementado` |
| `ListForeground` | `dpColor` fg | `#FFFFFF` | Texto de los ítems normales | `implementado` |
| `ListSelectedBackground` | `dpColor` sel | `#000000` | Fondo del ítem seleccionado | `implementado` |
| `ListSelectedForeground` | `dpColor` sel | `#FFFFFF` | Texto del ítem seleccionado | `implementado` |
| `ListHeaderBackground` | — | `#000000` | Fondo de la fila de cabecera | `implementado` |
| `ListHeaderForeground` | — | `#FFFFFF` | Texto de la fila de cabecera | `implementado` |

---

### 1.13 Controles — ScrollBar

El scrollbar de PC Tools tiene tres zonas de color diferenciadas: track inactivo, zona activa y thumb de posición.

| TuiColorRole | Ref. Turbo Vision | Color PC Tools | Notas | Estado |
|---|---|---|---|---|
| `ScrollBarBackground` | — | `#CACACA` | Track inactivo (zona aún no desplazada) | `implementado` |
| `ScrollBarActiveBackground` | — | `#FFFFFF` | Zona activa (ya desplazada) | `implementado` |
| `ScrollBarThumb` | — | `#696969` | Cuadrado indicador de posición | `implementado` |
| `ScrollBarArrow` | — | `#000000` | Glifos de flecha `▲▼◄►` | `implementado` |
| `ScrollBarArrowBackground` | — | `#CACACA` | Fondo del área de los botones de flecha | `implementado` |

---

### 1.14 Elementos diferidos

Los siguientes roles no están implementados en este milestone. Se añadirán al enum `TuiColorRole` y a `PcTools9Theme` en el milestone de widgets correspondiente.

| Elemento | Roles previstos | Milestone |
|---|---|---|
| `ProgressBar` | `ProgressBarBackground`, `ProgressBarFill` | Widgets |
| `TreeView` | `TreeViewBackground`, `TreeViewForeground`, `TreeViewSelectedBackground`, `TreeViewSelectedForeground` | Widgets |

---

## 2. Tabla B — Paleta PC Tools 9.x

Valores obtenidos por análisis directo del comportamiento visual de PC Tools 9.x (Central Point Software, ~1991) y verificados mediante medición de color.

Leyenda de origen:

| Etiqueta | Significado |
|---|---|
| `medido` | Valor obtenido por medición directa de píxel |
| `derivado` | Deducido lógicamente de valores medidos |

---

### 2.1 Paleta completa por TuiColorRole

| TuiColorRole | Hex | R | G | B | Origen |
|---|---|---|---|---|---|
| `DesktopBackground` | `#696969` | 105 | 105 | 105 | `medido` |
| `DesktopPatternDot` | `#696969` | 105 | 105 | 105 | `derivado` — igual que `DesktopBackground` en el tema PC Tools |
| `AppTitleBackground` | `#5555FF` | 85 | 85 | 255 | `medido` |
| `AppTitleForeground` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `AppTitleClockForeground` | `#FFFF55` | 255 | 255 | 85 | `medido` |
| `WindowBackground` | `#5555FF` | 85 | 85 | 255 | `medido` |
| `WindowForeground` | `#FFFFFF` | 255 | 255 | 255 | `derivado` |
| `WindowBorder` | `#000000` | 0 | 0 | 0 | `derivado` |
| `WindowTitleBackground` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `WindowTitleForeground` | `#000000` | 0 | 0 | 0 | `medido` |
| `WindowTitleInactiveBackground` | `#CACACA` | 202 | 202 | 202 | `medido` |
| `WindowTitleInactiveForeground` | `#696969` | 105 | 105 | 105 | `derivado` |
| `WindowCloseButtonBackground` | `#FFFFFF` | 255 | 255 | 255 | `derivado` — mismo patrón que `WindowTitleBackground` |
| `WindowCloseButtonForeground` | `#000000` | 0 | 0 | 0 | `derivado` — mismo patrón que `WindowTitleForeground` |
| `WindowShadow` | `#000000` | 0 | 0 | 0 | `medido` — opacidad vía `TuiTheme.ShadowOpacity` |
| `DialogBackground` | `#5555FF` | 85 | 85 | 255 | `medido` — igual que `WindowBackground` |
| `DialogForeground` | `#FFFFFF` | 255 | 255 | 255 | `derivado` |
| `DialogBorder` | `#000000` | 0 | 0 | 0 | `derivado` |
| `DialogTitleBackground` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `DialogTitleForeground` | `#000000` | 0 | 0 | 0 | `medido` |
| `DialogTitleWarningBackground` | `#FF5555` | 255 | 85 | 85 | `medido` |
| `DialogHighlightForeground` | `#FFFF55` | 255 | 255 | 85 | `medido` |
| `MenuBackground` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `MenuForeground` | `#000000` | 0 | 0 | 0 | `medido` |
| `MenuHotkeyForeground` | `#AA0000` | 170 | 0 | 0 | `medido` |
| `MenuSelectedBackground` | `#000000` | 0 | 0 | 0 | `derivado` |
| `MenuSelectedForeground` | `#FFFFFF` | 255 | 255 | 255 | `derivado` |
| `MenuDisabledForeground` | `#696969` | 105 | 105 | 105 | `derivado` |
| `MenuSeparator` | `#000000` | 0 | 0 | 0 | `derivado` |
| `MenuPopupBackground` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `MenuPopupForeground` | `#000000` | 0 | 0 | 0 | `medido` |
| `MenuPopupHotkeyForeground` | `#AA0000` | 170 | 0 | 0 | `medido` |
| `MenuPopupSelectedBackground` | `#000000` | 0 | 0 | 0 | `medido` |
| `MenuPopupSelectedForeground` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `MenuPopupSelectedHotkeyForeground` | `#AA0000` | 170 | 0 | 0 | `derivado` |
| `MenuPopupSubMenuArrow` | `#FFFFFF` | 255 | 255 | 255 | `derivado` |
| `MenuPopupBorder` | `#000000` | 0 | 0 | 0 | `derivado` |
| `MenuPopupShadow` | `#000000` | 0 | 0 | 0 | `derivado` |
| `StatusBackground` | `#696969` | 105 | 105 | 105 | `medido` — igual que `DesktopBackground` |
| `StatusForeground` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `StatusKeyBackground` | `#696969` | 105 | 105 | 105 | `derivado` |
| `StatusKeyForeground` | `#FFFF55` | 255 | 255 | 85 | `medido` |
| `LabelForeground` | `#FFFFFF` | 255 | 255 | 255 | `derivado` |
| `LabelBackground` | `#5555FF` | 85 | 85 | 255 | `derivado` — igual que `DialogBackground` |
| `InputBackground` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `InputForeground` | `#000000` | 0 | 0 | 0 | `medido` |
| `InputFocusBackground` | `#FFFFFF` | 255 | 255 | 255 | `derivado` — foco expresado con cursor |
| `InputFocusForeground` | `#000000` | 0 | 0 | 0 | `derivado` |
| `InputSelectionBackground` | `#000000` | 0 | 0 | 0 | `derivado` — patrón invertido |
| `InputSelectionForeground` | `#FFFFFF` | 255 | 255 | 255 | `derivado` |
| `InputBorderBottom` | `#000000` | 0 | 0 | 0 | `medido` — mismo negro que `ButtonShadow` |
| `ButtonBackground` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `ButtonForeground` | `#000000` | 0 | 0 | 0 | `derivado` |
| `ButtonAcceleratorForeground` | `#AA0000` | 170 | 0 | 0 | `medido` |
| `ButtonFocusBackground` | `#FFFFFF` | 255 | 255 | 255 | `derivado` — igual que `ButtonBackground` |
| `ButtonFocusForeground` | `#000000` | 0 | 0 | 0 | `derivado` |
| `ButtonFocusAcceleratorForeground` | `#AA0000` | 170 | 0 | 0 | `derivado` |
| `ButtonShadow` | `#000000` | 0 | 0 | 0 | `medido` |
| `CheckBackground` | `#5555FF` | 85 | 85 | 255 | `derivado` — igual que `DialogBackground` |
| `CheckForeground` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `CheckFocusBackground` | `#5555FF` | 85 | 85 | 255 | `derivado` |
| `CheckFocusForeground` | `#FFFFFF` | 255 | 255 | 255 | `derivado` |
| `CheckMarkColor` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `CheckAcceleratorForeground` | `#FFFF55` | 255 | 255 | 85 | `medido` |
| `ListBackground` | `#5555FF` | 85 | 85 | 255 | `medido` |
| `ListForeground` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `ListSelectedBackground` | `#000000` | 0 | 0 | 0 | `medido` |
| `ListSelectedForeground` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `ListHeaderBackground` | `#000000` | 0 | 0 | 0 | `medido` |
| `ListHeaderForeground` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `ScrollBarBackground` | `#CACACA` | 202 | 202 | 202 | `medido` |
| `ScrollBarActiveBackground` | `#FFFFFF` | 255 | 255 | 255 | `medido` |
| `ScrollBarThumb` | `#696969` | 105 | 105 | 105 | `medido` |
| `ScrollBarArrow` | `#000000` | 0 | 0 | 0 | `medido` |
| `ScrollBarArrowBackground` | `#CACACA` | 202 | 202 | 202 | `medido` |

---

### 2.2 Colores únicos de la paleta

PC Tools 9.x utiliza solo **8 colores distintos** en toda su interfaz:

| Hex | Nombre | EGA/VGA estándar | Constante en `PcTools9Theme` | Roles principales |
|---|---|---|---|---|
| `#000000` | Negro puro | Sí — color #0 | `Black` | Bordes, sombras, texto sobre fondos claros, selecciones |
| `#5555FF` | Azul EGA brillante | Sí — color #9 | `EgaBrightBlue` | `AppTitle`, `Window`, `Dialog`, `Label`, `Check`, `List` backgrounds |
| `#696969` | Gris PC Tools | **No** — valor propio | `PcToolsGray` | `Desktop`, `Status`, `ScrollBarThumb`, `WindowTitleInactive` fg |
| `#AA0000` | Rojo EGA oscuro | Sí — color #4 | `EgaDarkRed` | Letras aceleradoras en botones y menús |
| `#CACACA` | Gris claro PC Tools | **No** — valor propio | `PcToolsLightGray` | `ScrollBar` track/flechas, `WindowTitleInactive` bg |
| `#FF5555` | Rojo EGA brillante | Sí — color #12 | `EgaBrightRed` | `DialogTitleWarning` bg |
| `#FFFF55` | Amarillo EGA brillante | Sí — color #14 | `EgaBrightYellow` | `StatusKey`, `CheckAccelerator`, `AppTitleClock`, `DialogHighlight` |
| `#FFFFFF` | Blanco puro | Sí — color #15 | `White` | Texto general, títulos, botones, inputs, `MenuBar`, `ScrollBar` activo |

> **Nota**: `#696969` (`PcToolsGray`) y `#CACACA` (`PcToolsLightGray`) son los únicos valores
> que se apartan de la paleta EGA/VGA de 16 colores estándar. PC Tools los utiliza para los
> elementos de "chrome" del sistema (escritorio, barras, scrollbars), diferenciándolos
> visualmente del contenido de la aplicación.

---

*Fuentes: análisis visual de PC Tools 9.x (Central Point Software, ~1991), Turbo Vision Programming Guide (Borland, 1992).*