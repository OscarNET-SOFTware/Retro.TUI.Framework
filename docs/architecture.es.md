# Retro.TUI.Framework — Arquitectura Técnica

> Documento de diseño técnico detallado.  
> Complementa el [README.es.md](../README.es.md) con los contratos, jerarquías y
> comportamientos de cada capa.

---

## Índice

1. [Capa Events](#1-capa-events)
2. [Capa Theming](#2-capa-theming)
3. [Capa Hosting](#3-capa-hosting)
4. [Capa Rendering](#4-capa-rendering)
5. [Capa Core — Application y MessageLoop](#5-capa-core--application-y-messageloop)
6. [Capa Views — Árbol de vistas](#6-capa-views--árbol-de-vistas)
7. [Capa Windows](#7-capa-windows)
8. [Capa Widgets](#8-capa-widgets)
9. [Flujos transversales](#9-flujos-transversales)

---

## 1. Capa Events

**Proyecto**: `Retro.TUI.Events`  
**Namespace raíz**: `Retro.TUI.Events`  
**Sin dependencias de otras capas del framework.**

Esta capa define el lenguaje de comunicación entre todos los componentes del framework.
Ninguna otra capa puede prescindir de ella.

### 1.1 Jerarquía de TuiEvent

Todos los eventos son `record` inmutables que heredan de `TuiEvent`.
Esto garantiza inmutabilidad, igualdad estructural y soporte nativo de coincidencia de patrones.

```
TuiEvent  (abstract record)
├── TuiKeyEvent
├── TuiMouseEvent
├── TuiCommandEvent
├── TuiTimerEvent
└── TuiFocusEvent
```

**Contratos:**

```csharp
namespace Retro.TUI.Events;

/// <summary>Tipo base de todos los eventos del framework.</summary>
public abstract record TuiEvent;

/// <summary>Evento de teclado: tecla especial o carácter.</summary>
public sealed record TuiKeyEvent(
    TuiKey      Key,
    char        KeyChar,
    TuiModifiers Modifiers
) : TuiEvent;

/// <summary>Evento de ratón: movimiento, botón o rueda.</summary>
public sealed record TuiMouseEvent(
    TuiMouseAction Action,
    int            Col,
    int            Row,
    TuiMouseButton Button
) : TuiEvent;

/// <summary>
/// Evento de comando de alto nivel.
/// Permite comunicación desacoplada entre componentes.
/// </summary>
public sealed record TuiCommandEvent(
    TuiCommand Command,
    object?    Parameter = null
) : TuiEvent;

/// <summary>Evento de timer — emitido periódicamente por el message loop.</summary>
public sealed record TuiTimerEvent(
    TimeSpan Elapsed
) : TuiEvent;

/// <summary>Evento de cambio de foco.</summary>
public sealed record TuiFocusEvent(
    TuiFocusAction Action   // Gained | Lost
) : TuiEvent;
```

### 1.2 Enumeraciones de soporte

```csharp
/// <summary>Teclas especiales. Los caracteres imprimibles van en TuiKeyEvent.KeyChar.</summary>
public enum TuiKey
{
    None,
    Enter, Escape, Tab, BackSpace, Delete,
    Insert, Home, End, PageUp, PageDown,
    Left, Right, Up, Down,
    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12
}

/// <summary>Modificadores de teclado — combinables como flags.</summary>
[Flags]
public enum TuiModifiers
{
    None    = 0,
    Shift   = 1 << 0,
    Control = 1 << 1,
    Alt     = 1 << 2
}

/// <summary>Acciones de ratón.</summary>
public enum TuiMouseAction { Move, ButtonDown, ButtonUp, Click, DoubleClick, Wheel }

/// <summary>Botones de ratón.</summary>
public enum TuiMouseButton { None, Left, Right, Middle }

/// <summary>Acción de cambio de foco.</summary>
public enum TuiFocusAction { Gained, Lost }
```

### 1.3 TuiCommand

Los comandos son identificadores semánticos de alto nivel, independientes de la entrada de datos.
Permiten que un botón, una tecla de función y un elemento de menú activen la misma acción.

```csharp
/// <summary>
/// Comando de aplicación. Los valores del 0 al 99 están reservados por el framework.
/// Las aplicaciones deben usar valores desde 100 en adelante.
/// </summary>
public enum TuiCommand
{
    // Comandos de sistema (reservados)
    None         = 0,
    Quit         = 1,
    Close        = 2,
    Help         = 3,
    Ok           = 4,
    Cancel       = 5,
    Yes          = 6,
    No           = 7,

    // Comandos de ventana
    ZoomWindow   = 10,
    ResizeWindow = 11,
    MoveWindow   = 12,
    NextWindow   = 13,
    PrevWindow   = 14,

    // Rango libre para aplicaciones
    UserDefined  = 100
}
```

### 1.4 TuiEventQueue

```csharp
/// <summary>
/// Cola de eventos del framework.
/// El host publica eventos; el message loop los consume.
/// Implementada sobre System.Threading.Channels para soporte async nativo.
/// </summary>
public sealed class TuiEventQueue : IDisposable
{
    private readonly Channel<TuiEvent> _channel;

    public TuiEventQueue(int capacity = 256);

    /// <summary>Publica un evento en la cola (no bloqueante).</summary>
    public bool TryPost(TuiEvent evt);

    /// <summary>Envía un evento a la cola de forma asíncrona y espera si la cola está llena.</summary>
    /// <remarks>
    /// Prefiera <see cref="TryPost"/> del bucle de sondeo del host SDL2.
    /// Utilice <see cref="PostAsync"/> de los productores que puedan permitirse esperar,
    /// como las fuentes de temporizador internas o los ayudantes de prueba.
    /// </remarks>
    public async ValueTask PostAsync(TuiEvent evt, CancellationToken ct = default);

    /// <summary>Lee el siguiente evento de forma asíncrona.</summary>
    public ValueTask<TuiEvent> ReadAsync(CancellationToken ct = default);

    /// <summary>Intenta leer un evento sin bloquear.</summary>
    public bool TryRead(out TuiEvent? evt);

    public void Dispose();
}
```

---

## 2. Capa Theming

**Proyecto**: `Retro.TUI.Theming`  
**Namespace raíz**: `Retro.TUI.Theming`  
**Dependencias**: ninguna del framework (solo SkiaSharp para `SKColor`).

Esta capa define el esquema de color fijo y los parámetros de tipografía. Ningún
control ni vista tiene colores *hardcodeados*: siempre resuelven a través de
`TuiPalette.Resolve`.

### 2.1 TuiColorRole

Enum semántico que nombra cada rol visual del framework.
Independiente de cualquier valor de color concreto.

[mismo bloque de código que en architecture.md]

### 2.2 TuiEgaPalette

```csharp
/// <summary>
/// Paleta EGA estándar de 16 colores como constantes SKColor,
/// ordenadas por índice EGA canónico (0–15).
/// </summary>
public static class TuiEgaPalette { ... }
```

### 2.3 TuiPalette

```csharp
/// <summary>
/// Resuelve valores TuiColorRole a instancias SKColor concretas
/// usando el esquema de color fijo basado en EGA.
/// </summary>
public static class TuiPalette
{
    /// <summary>
    /// Devuelve el SKColor mapeado al rol dado.
    /// Lanza KeyNotFoundException si el rol no tiene entrada.
    /// </summary>
    public static SKColor Resolve(TuiColorRole role);
}
```

### 2.4 TuiTheme

```csharp
/// <summary>
/// Proporciona parámetros fijos de tipografía y sombra.
/// La tipografía IBM VGA 9x16 se carga desde un recurso embebido
/// en este ensamblado a través del constructor estático.
/// </summary>
public static class TuiTheme
{
    public static string FontFamily { get; }
    public static SKTypeface Typeface { get; }
    public static float FontSize { get; }
    public static float ShadowOpacity { get; }
    public static int ShadowOffsetX { get; }
    public static int ShadowOffsetY { get; }
}
```

---

## 3. Capa Hosting

**Proyecto**: `Retro.TUI.Hosting`  
**Namespace raíz**: `Retro.TUI.Hosting`  
**Dependencias**: `Retro.TUI.Events`

Esta capa es el único punto de contacto con SDL2 y el sistema operativo.
El framework no sabe nada de SDL2 fuera de esta capa.

### 3.1 ITuiHost

```csharp
/// <summary>
/// Contrato del host de ventana.
/// Abstrae SDL2 (u otro backend futuro) del resto del framework.
/// </summary>
public interface ITuiHost : IDisposable
{
    /// <summary>Ancho de la ventana en píxeles físicos.</summary>
    int PixelWidth { get; }

    /// <summary>Alto de la ventana en píxeles físicos.</summary>
    int PixelHeight { get; }

    /// <summary>Factor de escala DPI (1.0 en displays estándar, 2.0 en HiDPI).</summary>
    float DpiScale { get; }

    /// <summary>Título de la ventana.</summary>
    string Title { get; set; }

    /// <summary>
    /// Crea y muestra la ventana.
    /// Debe llamarse antes de cualquier otra operación.
    /// </summary>
    void Initialize(TuiHostOptions options);

    /// <summary>
    /// Procesa los eventos del sistema operativo pendientes y los publica
    /// en la cola de eventos del framework.
    /// Llamado en cada iteración del message loop.
    /// </summary>
    /// <returns>False si el host ha solicitado cierre.</returns>
    bool PollEvents(TuiEventQueue queue);

    /// <summary>
    /// Presenta el frame renderizado en pantalla.
    /// Llamado al final de cada iteración del message loop.
    /// </summary>
    void Present();

    /// <summary>Devuelve la superficie Skia para renderizar el frame actual.</summary>
    SKSurface AcquireRenderSurface();
}
```

### 3.2 TuiHostOptions

```csharp
public sealed class TuiHostOptions
{
    public required string Title { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public bool Resizable { get; init; } = false;
    public bool HideSystemCursor { get; init; } = true;
    public bool CenterOnScreen { get; init; } = true;

    /// <summary>
    /// Si true, la ventana no tiene decoraciones nativas (barra de título del SO).
    /// El framework dibuja su propia barra de título.
    /// </summary>
    public bool Borderless { get; init; } = true;
}
```

### 3.3 SdlHost

```csharp
/// <summary>
/// Implementación de ITuiHost sobre SDL2.
/// Es la única clase del framework que referencia SDL2 directamente.
/// </summary>
public sealed class SdlHost : ITuiHost
{
    // Implementación interna con SDL2.
    // Traduce eventos SDL a TuiEvent y los publica en TuiEventQueue.
    // Gestiona el contexto OpenGL para SkiaSharp.
}
```

**Mapeo de eventos SDL → TuiEvent:**

| Evento SDL | TuiEvent generado |
|---|---|
| `SDL_KEYDOWN` | `TuiKeyEvent` |
| `SDL_TEXTINPUT` | `TuiKeyEvent` (KeyChar) |
| `SDL_MOUSEMOTION` | `TuiMouseEvent(Move)` |
| `SDL_MOUSEBUTTONDOWN` | `TuiMouseEvent(ButtonDown)` |
| `SDL_MOUSEBUTTONUP` | `TuiMouseEvent(ButtonUp)` + `Click` |
| `SDL_QUIT` | `TuiCommandEvent(Quit)` |
| `SDL_WINDOWEVENT_CLOSE` | `TuiCommandEvent(Close)` |

---

## 4. Capa Rendering

**Proyecto**: `Retro.TUI.Rendering`  
**Namespace raíz**: `Retro.TUI.Rendering`  
**Dependencias**: `Retro.TUI.Theming`

Esta capa envuelve SkiaSharp y expone una API orientada a la cuadrícula de caracteres.
Ninguna vista ni control accede a SkiaSharp directamente: siempre pasan por `TuiRenderContext`.

### 4.1 TuiGrid

Métricas de la cuadrícula de caracteres. Se inicializa una vez con los valores reales
de la fuente cargada.

```csharp
/// <summary>
/// Métricas de la cuadrícula de caracteres.
/// Immutable tras Initialize. Accesible desde TuiRenderContext.
/// </summary>
public sealed class TuiGrid
{
    /// <summary>Ancho de celda en píxeles.</summary>
    public int CellWidth { get; private set; }

    /// <summary>Alto de celda en píxeles.</summary>
    public int CellHeight { get; private set; }

    /// <summary>Número de columnas visibles.</summary>
    public int Columns { get; private set; }

    /// <summary>Número de filas visibles.</summary>
    public int Rows { get; private set; }

    /// <summary>Ancho total de la pantalla en píxeles.</summary>
    public int ScreenWidth { get; private set; }

    /// <summary>Alto total de la pantalla en píxeles.</summary>
    public int ScreenHeight { get; private set; }

    // Conversión celda → píxel
    public float PixelX(int col)    => col * CellWidth;
    public float PixelY(int row)    => row * CellHeight;
    public float BaselineY(int row) => row * CellHeight + _ascent;

    // Conversión píxel → celda
    public int CellCol(float px) => (int)(px / CellWidth);
    public int CellRow(float py) => (int)(py / CellHeight);

    public void Initialize(int screenW, int screenH, SKFont font);
}
```

### 4.2 TuiFont

```csharp
/// <summary>
/// Gestión de la fuente del framework.
/// Carga la fuente desde recursos embebidos y expone el SKFont configurado.
/// </summary>
public sealed class TuiFont : IDisposable
{
    public SKFont SkFont { get; private set; } = null!;
    public SKFontMetrics Metrics { get; private set; }

    public void Load(SKTypeface typeface, float size);
    public void Dispose();
}
```

### 4.3 TuiRenderContext

La interfaz principal de renderizado. Cada frame, `TuiApplication` crea un contexto,
lo pasa al árbol de vistas para que dibujen, y luego lo finaliza.

```csharp
/// <summary>
/// Contexto de renderizado para un frame.
/// Opera en coordenadas de grid (col, row), no en píxeles.
/// </summary>
public sealed class TuiRenderContext
{
    public TuiGrid  Grid  { get; }
    public TuiFont  Font  { get; }
    public TuiTheme Theme { get; }

    // ── Fondo ────────────────────────────────────────────────────

    public void FillCell(int col, int row, TuiColorRole bg);
    public void FillRow(int row, TuiColorRole bg);
    public void FillRect(int col, int row, int width, int height, TuiColorRole bg);
    public void FillRect(int col, int row, int width, int height, SKColor color);

    // ── Texto ────────────────────────────────────────────────────

    public void DrawText(int col, int row, string text,
        TuiColorRole fg, TuiColorRole bg);

    public void DrawTextCentered(int col, int row, int width, string text,
        TuiColorRole fg, TuiColorRole bg);

    public void DrawTextClipped(int col, int row, int maxWidth, string text,
        TuiColorRole fg, TuiColorRole bg);

    // ── Bordes ───────────────────────────────────────────────────

    public void DrawBorder(int col, int row, int width, int height,
        TuiColorRole border);

    // ── Sombra ───────────────────────────────────────────────────

    public void DrawShadow(int col, int row, int width, int height);

    // ── Clipping ─────────────────────────────────────────────────

    /// <summary>
    /// Establece un clip rect en coordenadas de grid.
    /// Se apila: restaurar con PopClip.
    /// </summary>
    public void PushClip(int col, int row, int width, int height);
    public void PopClip();

    // ── Cursor personalizado ─────────────────────────────────────

    public void DrawCursor(float pixelX, float pixelY);
}
```

---

## 5. Capa Core — Application y MessageLoop

**Proyecto**: `Retro.TUI.Core`  
**Namespace raíz**: `Retro.TUI.Core`  
**Dependencias**: `Retro.TUI.Events`, `Retro.TUI.Theming`, `Retro.TUI.Rendering`, `Retro.TUI.Hosting`

El núcleo del framework: punto de entrada, bucle principal y gestión global.

### 5.1 TuiApplication

```csharp
/// <summary>
/// Clase raíz del framework. Punto de entrada de toda aplicación Retro.TUI.
/// Gestiona el ciclo de vida: inicialización, bucle de mensajes y cierre.
/// </summary>
public abstract class TuiApplication : IDisposable
{
    // ── Estado protegido ──────────────────────────────────────────

    /// <summary>Escritorio raíz de la jerarquía de vistas.</summary>
    protected TuiDesktop Desktop { get; private set; }

    /// <summary>Gestor de foco de teclado.</summary>
    protected TuiFocusManager FocusManager { get; private set; }

    // ── Ciclo de vida ─────────────────────────────────────────────

    /// <summary>
    /// Inicializa el framework, crea la ventana y arranca el bucle de mensajes.
    /// Bloquea hasta que la aplicación termina.
    /// </summary>
    public void Run(ITuiHost host, TuiTheme theme, TuiHostOptions options);

    /// <summary>Solicita el cierre de la aplicación en la siguiente iteración.</summary>
    public void RequestQuit();

    /// <summary>
    /// Abre <paramref name="dialog"/> como modal, ejecuta un bucle de eventos
    /// anidado hasta que se llama a <see cref="TuiDialog.Close"/>, y limpia
    /// automáticamente. Devuelve el TuiCommand pasado a Close().
    /// </summary>
    protected TuiCommand RunModal(IModalDialog dialog, TuiDesktop desktop);

    public void Dispose();

    // ── Métodos sobreescribibles ──────────────────────────────────

    /// <summary>
    /// Inicialización de la aplicación: construir el Desktop y añadir vistas.
    /// Se llama tras inicializar el host y el renderizado, antes del primer frame.
    /// </summary>
    protected abstract void OnInitialize();

    /// <summary>
    /// Manejador de comandos a nivel de aplicación. Se invoca antes de que el
    /// evento llegue al árbol de vistas. La implementación base gestiona
    /// TuiCommand.Quit. Suprimido mientras hay un modal activo — los comandos
    /// van al modal en su lugar.
    /// </summary>
    protected virtual void OnCommand(TuiCommandEvent ev);
}
```

### 5.2 TuiMessageLoop

```csharp
/// <summary>
/// Bucle principal del framework. Orquesta el ciclo sondeo → despacho →
/// renderizado → presentación. Interno a TuiApplication. El código de
/// aplicación nunca llama a esto directamente.
/// </summary>
internal sealed class TuiMessageLoop
{
    /// <summary>
    /// Ejecuta el bucle hasta que el host señala el cierre o se cancela ct.
    /// Cada iteración delega en RunIteration().
    /// </summary>
    public void Run(
        ITuiHost          host,
        TuiEventQueue     queue,
        TuiDesktop        desktop,
        TuiFocusManager   focusManager,
        TuiRenderContext  renderCtx,
        CancellationToken ct,
        Action<TuiCommandEvent>? onCommand = null);

    /// <summary>
    /// Ejecuta una iteración sondeo → despacho → renderizado → presentación.
    /// Usado por TuiApplication.RunModal para conducir el bucle modal anidado
    /// sin duplicar la lógica del ciclo.
    /// </summary>
    internal bool RunIteration(
        ITuiHost          host,
        TuiEventQueue     queue,
        TuiDesktop        desktop,
        TuiFocusManager   focusManager,
        TuiRenderContext  renderCtx,
        Action<TuiCommandEvent>? onCommand = null);

    // ── Captura de ratón ──────────────────────────────────────────
    // En ButtonDown, el primer ancestro en la cadena de burbuja con
    // HasCustomMouseHandling=true reclama la entrada exclusiva de ratón
    // (_capturedView). Move y ButtonUp se entregan directamente a la
    // vista capturada, saltando FindAt. Se libera incondicionalmente
    // en ButtonUp. Permite el arrastre correcto cuando el cursor sale
    // del chrome de la ventana.

    // ── Enrutado modal de eventos ─────────────────────────────────
    // Cuando TuiDesktop.HasModal es true:
    //   - Los eventos de ratón van directamente a ActiveModal (FindAt omitido).
    //   - Los eventos de teclado (incluido Escape/Cancel) van a ActiveModal.
    //   - Los TuiCommandEvent van a ActiveModal; onCommand queda suprimido.
}
```

### 5.3 TuiFocusManager

```csharp
/// <summary>
/// Gestiona el foco de teclado entre las vistas enfocables.
/// Mantiene un orden de tabulación y la vista actualmente enfocada.
/// </summary>
public sealed class TuiFocusManager
{
    public TuiView? Current { get; private set; }

    public void Register(TuiView view);
    public void Unregister(TuiView view);

    /// <summary>Establece el foco en una vista específica.</summary>
    public void SetFocus(TuiView view);

    /// <summary>Mueve el foco al siguiente en el orden de tabulación.</summary>
    public void FocusNext();

    /// <summary>Mueve el foco al anterior en el orden de tabulación.</summary>
    public void FocusPrevious();

    public bool IsFocused(TuiView view) => Current == view;

    /// <summary>
    /// Limpia todo el estado de foco.
    /// Llamado cuando se cierra un diálogo modal.
    /// </summary>
    public void Clear();
}
```

---

## 6. Capa Views — Árbol de vistas

**Proyecto**: `Retro.TUI.Views`  
**Namespace raíz**: `Retro.TUI.Views`  
**Dependencias**: `Retro.TUI.Core`, `Retro.TUI.Rendering`

### 6.1 TuiView

La clase base de todo el árbol visual.

```csharp
/// <summary>
/// Clase base de todos los elementos visuales del framework.
/// Gestiona el árbol de composición, coordenadas relativas,
/// ciclo de vida e invalidación.
/// </summary>
public abstract class TuiView
{
    // ── Posición y tamaño (en celdas de grid) ─────────────────────

    public int Col    { get; set; }
    public int Row    { get; set; }
    public int Width  { get; set; }
    public int Height { get; set; }

    // ── Estado ────────────────────────────────────────────────────

    public bool Visible  { get; set; } = true;
    public bool Enabled  { get; set; } = true;
    public bool Focusable { get; set; } = false;

    // ── Árbol ─────────────────────────────────────────────────────

    public TuiView?         Parent   { get; internal set; }
    protected IReadOnlyList<TuiView> Children { get; }

    // ── Coordenadas absolutas ─────────────────────────────────────

    public int AbsCol => (Parent?.AbsCol ?? 0) + Col;
    public int AbsRow => (Parent?.AbsRow ?? 0) + Row;

    // ── Gestión del árbol ─────────────────────────────────────────

    public    void AddChild(TuiView child);
    public    void RemoveChild(TuiView child);
    protected void ClearDirty();

    // ── Invalidación ─────────────────────────────────────────────

    /// <summary>
    /// Marca esta vista como necesitada de redibujado.
    /// Sube la invalidación al padre si es necesario.
    /// </summary>
    public void Invalidate();

    protected bool IsDirty { get; private set; }

    // ── Ciclo de vida ─────────────────────────────────────────────

    /// <summary>Llamado cuando la vista se añade a su padre.</summary>
    protected virtual void OnAdded() { }

    /// <summary>Llamado cuando la vista se elimina de su padre.</summary>
    protected virtual void OnRemoved() { }

    // ── Rendering ─────────────────────────────────────────────────

    /// <summary>
    /// Dibuja esta vista y sus hijos.
    /// Las subclases sobreescriben Draw() para su apariencia propia,
    /// llamando a base.Draw() al final para dibujar los hijos.
    /// </summary>
    public virtual void Draw(TuiRenderContext ctx)
    {
        if (!Visible) return;
        foreach (var child in Children)
            child.Draw(ctx);
        IsDirty = false;
    }

    // ── Despacho de eventos ───────────────────────────────────────

    /// <summary>
    /// Despacha un evento a esta vista.
    /// Devuelve true si el evento fue consumido (no debe propagarse más).
    /// </summary>
    public virtual bool HandleEvent(TuiEvent evt) => false;

    // ── Hit testing ───────────────────────────────────────────────

    public bool HitTest(int col, int row);

    /// <summary>
    /// Busca la vista más profunda del árbol que contiene (col, row).
    /// </summary>
    public TuiView? FindAt(int col, int row);

    // ── Eventos de ciclo de vida de foco ─────────────────────────

    public virtual void OnGotFocus()  { }
    public virtual void OnLostFocus() { }
}
```

### 6.2 TuiGroup

```csharp
/// <summary>
/// Vista contenedora con despacho de eventos a los hijos.
/// Equivalente al TGroup de Turbo Vision.
/// </summary>
public class TuiGroup : TuiView
{
    public TuiGroup(int col, int row, int width, int height)
        : base(col, row, width, height) { }

    /// <summary>
    /// Despacha el evento a los hijos en orden inverso (z-order)
    /// hasta que uno lo consuma.
    /// </summary>
    public override bool HandleEvent(TuiEvent evt)
    {
        // Despacho inverso — el hijo de mayor z-order tiene prioridad
        for (int i = Children.Count - 1; i >= 0; i--)
            if (Children[i].HandleEvent(evt)) return true;
        return base.HandleEvent(evt);
    }
}
```

### 6.3 TuiDesktop

```csharp
/// <summary>
/// Contenedor raíz de la aplicación. Ocupa toda la pantalla con el patrón
/// de fondo del tema activo y actúa como contenedor de nivel superior para
/// todas las ventanas, diálogos y superposiciones.
/// </summary>
public sealed class TuiDesktop : TuiGroup
{
    // ── Pila modal ────────────────────────────────────────────────

    /// <summary>
    /// Introduce modal en la pila y lo añade como hijo para que participe
    /// en el renderizado. Los eventos de teclado y ratón solo llegan a la
    /// vista modal activa mientras la pila no esté vacía.
    /// </summary>
    public void PushModal(TuiView modal);

    /// <summary>
    /// Elimina el modal superior de la pila y de la colección de hijos.
    /// Devuelve la vista eliminada, o null si la pila estaba vacía.
    /// </summary>
    public TuiView? PopModal();

    /// <summary>True cuando hay una o más vistas modales activas.</summary>
    public bool HasModal { get; }

    /// <summary>La vista modal superior, o null cuando la pila está vacía.</summary>
    public TuiView? ActiveModal { get; }

    // ── Renderizado ───────────────────────────────────────────────

    public override void Draw(TuiRenderContext ctx);
}
```

---

## 7. Capa Windows

**Proyecto**: `Retro.TUI.Windows`  
**Namespace raíz**: `Retro.TUI.Windows`  
**Dependencias**: `Retro.TUI.Views`

### 7.1 TuiWindow

```csharp
/// <summary>
/// Ventana con borde, barra de título, sombra y soporte de arrastre.
/// </summary>
public class TuiWindow : TuiGroup
{
    public string Title     { get; set; }
    public bool   Movable   { get; set; } = true;
    public bool   ShowShadow { get; set; } = true;
    public bool   ShowTitle  { get; set; } = true;

    public TuiWindow(string title, int col, int row, int width, int height);

    public override void Draw(TuiRenderContext ctx);
    public override bool HandleEvent(TuiEvent evt);

    // Área interior (excluye borde y título)
    public int InnerCol    => AbsCol + 1;
    public int InnerRow    => AbsRow + 2;   // título + borde
    public int InnerWidth  => Width - 2;
    public int InnerHeight => Height - 3;
}
```

### 7.2 IModalDialog

```csharp
/// <summary>
/// Contrato mínimo que una vista modal debe satisfacer para que
/// TuiApplication.RunModal pueda conducir y observar su ciclo de vida
/// sin depender de Retro.TUI.Windows.
/// Los implementadores deben derivar también de TuiView.
/// </summary>
/// <remarks>Definido en Retro.TUI.Views.</remarks>
public interface IModalDialog
{
    /// <summary>
    /// True tras llamar a Close(). Sondeado en cada iteración por
    /// TuiApplication.RunModal para detectar que el bucle anidado debe detenerse.
    /// </summary>
    bool CloseRequested { get; }

    /// <summary>
    /// El resultado de comando que el diálogo devuelve a RunModal.
    /// Por defecto TuiCommand.Cancel antes de llamar a Close().
    /// </summary>
    TuiCommand Result { get; }
}
```

### 7.3 TuiDialog

```csharp
/// <summary>
/// Diálogo modal. Extiende TuiWindow e implementa IModalDialog.
/// No se abre directamente — usar TuiApplication.RunModal.
/// </summary>
public class TuiDialog(string title, int col, int row, int width, int height)
    : TuiWindow(title, col, row, width, height), IModalDialog
{
    /// <inheritdoc/>
    public bool CloseRequested { get; private set; }

    /// <inheritdoc/>
    public TuiCommand Result { get; private set; }

    /// <summary>
    /// Señala que el diálogo debe cerrarse, devolviendo result al llamador
    /// de TuiApplication.RunModal. Idempotente: las llamadas posteriores
    /// a la primera son no-ops.
    /// </summary>
    public void Close(TuiCommand result = TuiCommand.Cancel);
}
```

---

## 8. Capa Widgets

**Proyecto**: `Retro.TUI.Widgets`  
**Namespace raíz**: `Retro.TUI.Widgets`  
**Dependencias**: `Retro.TUI.Windows`

Relación de controles planificados con sus responsabilidades:

| Widget | Descripción |
|---|---|
| `TuiMenuBar` | Barra de menú superior con items y hover |
| `TuiMenu` | Menú desplegable asociado a un item del MenuBar |
| `TuiMenuItem` | Item individual de un menú desplegable |
| `TuiStatusBar` | Barra inferior con atajos de teclas de función |
| `TuiButton` | Botón pulsable con foco y Enter/Space |
| `TuiLabel` | Texto estático, sin foco |
| `TuiInputLine` | Campo de texto con cursor, selección y edición |
| `TuiCheckBox` | Casilla de verificación |
| `TuiRadioButton` | Botón de opción con grupos exclusivos |
| `TuiListBox` | Lista de items con selección y scroll |
| `TuiScrollBar` | Barra de desplazamiento (vertical/horizontal) |
| `TuiAppTitleBar` | Barra de título de la aplicación (fija, no de ventana) |

Todos los widgets siguen el mismo contrato:

- Heredan de `TuiView` (los simples) o `TuiGroup` (los compuestos).
- Sin colores *hardcodeados*: se resuelven mediante `TuiPalette.Resolve(TuiColorRole)`.
- Implementan `HandleEvent` para responder a teclado y ratón.
- Llaman a `Invalidate()` cuando su estado cambia y necesitan redibujado.
- Exponen eventos .NET estándar para notificar al código de la aplicación:
  `EventHandler`, `EventHandler<T>`.

---

## 9. Flujos transversales

### 9.1 Ciclo de vida de la aplicación

```
TuiApplication.Run(host, options)
    │
    ├── host.Initialize(options)                            // Crea ventana SDL2
    ├── TuiFont.Load(TuiTheme.Typeface, TuiTheme.FontSize)  // Fuente IBM VGA desde TuiTheme
    ├── TuiFont.Load(...)                                   // Carga fuente IBM VGA
    ├── TuiGrid.Initialize(...)                             // Calcula métricas de grid
    ├── TuiRenderContext.Create(...)                        // Prepara contexto Skia
    ├── Desktop = new TuiDesktop()
    ├── Desktop.RebuildBackground(ctx)                      // Pre-renderiza fondo
    ├── OnInitialize()                                      // La app construye su árbol de vistas
    │
    └── TuiMessageLoop.Run(...)                             // Bucle principal
            │
            ├── host.PollEvents(queue)                      // SDL → TuiEvent → queue
            ├── DispatchEvents(queue)                       // queue → árbol de vistas
            ├── UpdateTimers()                              // TuiTimerEvent si procede
            ├── Desktop.Draw(ctx)                           // Dibuja árbol completo
            └── host.Present()                              // Muestra el frame
```

### 9.2 Flujo de un evento de ratón

```
SDL_MOUSEBUTTONDOWN
    │
    └── SdlHost.PollEvents()
            │
            └── TuiEventQueue.TryPost(
                    new TuiMouseEvent(ButtonDown, col, row, Left))
                    │
                    └── TuiMessageLoop.DispatchEvents()
                            │
                            ├── Desktop.HasModal?
                            │       └── Sí → despacha solo a la vista modal activa
                            │
                            └── No → Desktop.FindAt(col, row)
                                    │
                                    └── target.HandleEvent(mouseEvent)
                                            │
                                            └── Consumed? → stop
                                            └── No → propaga al padre
```

### 9.3 Flujo de un evento de teclado

```
SDL_KEYDOWN
    │
    └── SdlHost → TuiKeyEvent → TuiEventQueue
            │
            └── TuiMessageLoop.DispatchEvents()
                    │
                    ├── ¿Es Tab / Shift+Tab?
                    │       └── TuiFocusManager.FocusNext() / FocusPrevious()
                    │
                    ├── ¿Es Escape? → TuiCommandEvent(Cancel) → modal activo
                    │
                    └── TuiFocusManager.Current?.HandleEvent(keyEvent)
```

### 9.4 Invalidación y redibujado

En la primera iteración del milestone 1 se usa **redraw completo**: en cada frame
se redibuja el árbol entero desde el Desktop.

El modelo de `Invalidate()` está diseñado desde el principio para soportar
**dirty regions** en una iteración futura:

1. `view.Invalidate()` marca `IsDirty = true` en la vista y sube la invalidación al padre.
2. En el render pass, solo se redibujan las vistas con `IsDirty = true`.
3. Las regiones limpias se copian del frame anterior.

Esto se activará como optimización en un milestone posterior, sin cambios en la API pública.

---

*Documento vivo — se actualiza con cada milestone completado.*
