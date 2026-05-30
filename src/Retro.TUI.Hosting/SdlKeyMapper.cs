// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SdlKeyMapper.cs" company="OscarNET-SOFTware">
// ···
//      Retro.TUI.Framework - A modern C-Sharp .NET multiplatform TUI framework
//      powered by SkiaSharp, inspired by Turbo Vision with a PC Tools 9.x retro aesthetic.
// ···
//      Copyright (c) 2026 Oscar Fernandez Gonzalez a.k.a. Osc@rNET and Contributors
//      Licensed under the MIT License. See the 'LICENSE.md' file for details.
// ···
//      Third-party components are used in this project. For full license texts,
//      see the 'licenses' folder and the 'THIRD-PARTY-NOTICES.md' file.
// ···
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

using Retro.TUI.Events;

using Silk.NET.SDL;

namespace Retro.TUI.Hosting;

/// <summary>
/// Maps SDL2 <see cref="KeyCode"/> values to framework <see cref="TuiKey"/> values.
/// </summary>
/// <remarks>
/// This class is intentionally <c>internal</c>: SDL2 types must not leak beyond
/// the Hosting layer. The static dictionary is initialised once and shared across
/// all <c>SdlHost</c> instances for the process lifetime.
/// <para/>
/// Keys not present in the map are treated as <see cref="TuiKey.None"/>, meaning
/// the event carries a printable character via <see cref="TuiKeyEvent.KeyChar"/>
/// rather than a special key code.
/// </remarks>
internal static class SdlKeyMapper
{
    // Initialised once; read-only after static construction.
    private static readonly Dictionary<int, TuiKey> s_map = BuildMap();

    /// <summary>
    /// Translates an SDL <see cref="KeyCode"/> to the corresponding <see cref="TuiKey"/>.
    /// </summary>
    /// <param name="sdlKey">The raw SDL key code from the keyboard event.</param>
    /// <returns>
    /// The matching <see cref="TuiKey"/>, or <see cref="TuiKey.None"/> when the key
    /// produces a printable character or is not mapped.
    /// </returns>
    internal static TuiKey ToTuiKey(int sdlKey)
        => s_map.GetValueOrDefault(sdlKey, TuiKey.None);

    /// <summary>
    /// Translates SDL modifier flags to <see cref="TuiModifiers"/>.
    /// </summary>
    /// <param name="sdlMod">The SDL modifier bitmask from the keyboard event.</param>
    /// <returns>The equivalent <see cref="TuiModifiers"/> flags.</returns>
    internal static TuiModifiers ToTuiModifiers(ushort sdlMod)
    {
        var result = TuiModifiers.None;

        // SDL uses separate bits for left/right variants; collapse each pair.
        if ((sdlMod & (ushort)(Keymod.Lshift | Keymod.Rshift)) != 0)
        {
            result |= TuiModifiers.Shift;
        }

        if ((sdlMod & (ushort)(Keymod.Lctrl | Keymod.Rctrl)) != 0)
        {
            result |= TuiModifiers.Control;
        }

        if ((sdlMod & (ushort)(Keymod.Lalt | Keymod.Ralt)) != 0)
        {
            result |= TuiModifiers.Alt;
        }

        return result;
    }

    // ── Private ───────────────────────────────────────────────────────────

    private static Dictionary<int, TuiKey> BuildMap() =>
        new()
        {
            // ── Navigation ────────────────────────────────────────────────
            { (int)KeyCode.KReturn,   TuiKey.Enter     },
            { (int)KeyCode.KEscape,   TuiKey.Escape    },
            { (int)KeyCode.KTab,      TuiKey.Tab       },
            { (int)KeyCode.KBackspace,TuiKey.BackSpace },
            { (int)KeyCode.KDelete,   TuiKey.Delete    },
            { (int)KeyCode.KInsert,   TuiKey.Insert    },
            { (int)KeyCode.KHome,     TuiKey.Home      },
            { (int)KeyCode.KEnd,      TuiKey.End       },
            { (int)KeyCode.KPageup,   TuiKey.PageUp    },
            { (int)KeyCode.KPagedown, TuiKey.PageDown  },

            // ── Arrow keys ────────────────────────────────────────────────
            { (int)KeyCode.KLeft,     TuiKey.Left      },
            { (int)KeyCode.KRight,    TuiKey.Right     },
            { (int)KeyCode.KUp,       TuiKey.Up        },
            { (int)KeyCode.KDown,     TuiKey.Down      },

            // ── Function keys ─────────────────────────────────────────────
            { (int)KeyCode.KF1,       TuiKey.F1        },
            { (int)KeyCode.KF2,       TuiKey.F2        },
            { (int)KeyCode.KF3,       TuiKey.F3        },
            { (int)KeyCode.KF4,       TuiKey.F4        },
            { (int)KeyCode.KF5,       TuiKey.F5        },
            { (int)KeyCode.KF6,       TuiKey.F6        },
            { (int)KeyCode.KF7,       TuiKey.F7        },
            { (int)KeyCode.KF8,       TuiKey.F8        },
            { (int)KeyCode.KF9,       TuiKey.F9        },
            { (int)KeyCode.KF10,      TuiKey.F10       },
            { (int)KeyCode.KF11,      TuiKey.F11       },
            { (int)KeyCode.KF12,      TuiKey.F12       },
        };
}
