# TqkLibrary.Scrcpy

A .NET library to mirror the screen and control Android devices, by reusing the [scrcpy](https://github.com/Genymobile/scrcpy) server.

It pushes the scrcpy server to the device over ADB, then exposes the video, audio and control streams as a managed API. Video is decoded through a small native helper (`TqkLibrary.ScrcpyNative`) together with FFmpeg.

## Features

- Video stream (H.264 / H.265 / AV1 / VP8 / VP9)
- Audio stream (Opus / AAC / FLAC / raw)
- Device control: touch, key, text injection, scroll, clipboard (copy / paste, autosync)
- Camera mirroring
- Query device capabilities: list encoders, displays and cameras

## Supported scrcpy server versions

Each package bundles a matching `scrcpy-server.jar` — the server must match the client protocol. The bundled version is exposed by `Constant.ScrcpyServerVersion`. Pick the package whose version matches the scrcpy server you want to run.

| Package branch | scrcpy server |
|----------------|---------------|
| 2.4 | 2.4 |
| 2.5 | 2.5 |
| 2.6 | 2.6.1 |
| 2.7 | 2.7 |
| 3.0 | 3.0 |
| 3.1 | 3.1 |
| 3.2 | 3.2 |
| 3.3 | 3.3.4 |
| 4.0 | 4.0 |
| 4.1 | 4.1 |

## Platform

- Windows x64 / x86 (the native helper `TqkLibrary.ScrcpyNative` ships for `win-x64` and `win-x86`).
- Targets: .NET Framework 4.6.2, .NET 6 (Windows), .NET 8 (Windows).

## Credits

The bundled `scrcpy-server.jar` is taken, unmodified, from the [scrcpy](https://github.com/Genymobile/scrcpy) project by [Genymobile](https://github.com/Genymobile) (Romain Vimont and contributors). scrcpy is licensed under the **Apache License 2.0**. All credit for the server goes to the scrcpy authors.

## License

`TqkLibrary.Scrcpy` is licensed under the **MIT License** — see [LICENSE](LICENSE). Your application does **not** have to be open source to use it.

This software uses libraries from **FFmpeg** (https://ffmpeg.org) licensed under the [LGPL-2.1-or-later](https://www.gnu.org/licenses/old-licenses/lgpl-2.1.html); the build is configured without `--enable-gpl` and contains no x264/x265, because this library only ever *decodes* (the device does the encoding). Build scripts and sources: https://github.com/tqk2811/FFmpegBuild.

FFmpeg is linked dynamically as separate DLLs, so LGPL-2.1 §6 is satisfied and you may swap in your own FFmpeg build. If you redistribute your application, keep the FFmpeg notice and the LGPL-2.1 text ([licenses/LGPL-2.1.txt](licenses/LGPL-2.1.txt)) with it.

See [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) for the full attribution of FFmpeg and the scrcpy server.

> Versions released **before** this change were licensed under GPL-3.0-or-later (they bundled a GPL FFmpeg build); that license still applies to those older packages.

The bundled scrcpy server (`scrcpy-server.jar`) is a separate program by Genymobile, executed on the device (not linked into this library), licensed under the **Apache License 2.0**; see the [scrcpy repository](https://github.com/Genymobile/scrcpy) for its terms.
