// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="OscarNET-SOFTware">
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

using Retro.TUI.Hosting;
using Retro.TUI.Sample.Basic;
using Retro.TUI.Theme.PcTools9;

// Force the theme assembly to load and register the IBM VGA font with
// SKFontManager before TuiApplication.Run resolves the typeface.
_ = PcTools9Theme.Instance;
 
using var app = new BasicApp();

app.Run(
    host: new SdlHost(),
    theme: PcTools9Theme.Instance,
    options: new TuiHostOptions
    {
    Title   = "Retro.TUI.Framework — Basic Sample",
        Width   = 720,
        Height  = 400,
    });
