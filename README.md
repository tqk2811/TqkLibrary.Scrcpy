# TqkLibrary.Scrcpy

A .NET library to mirror the screen and control Android devices, by reusing the [scrcpy](https://github.com/Genymobile/scrcpy) server.

It pushes the scrcpy server to the device over ADB, then exposes the video, audio and control streams as a managed API. Video is decoded through a small native helper (`TqkLibrary.ScrcpyNative`) together with FFmpeg.

## Features

- Video stream (H.264 / H.265 / AV1)
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

## Platform

- Windows x64 / x86 (the native helper `TqkLibrary.ScrcpyNative` ships for `win-x64` and `win-x86`).
- Targets: .NET Framework 4.6.2, .NET 6 (Windows), .NET 8 (Windows).

## Credits

The bundled `scrcpy-server.jar` is taken, unmodified, from the [scrcpy](https://github.com/Genymobile/scrcpy) project by [Genymobile](https://github.com/Genymobile) (Romain Vimont and contributors). scrcpy is licensed under the **Apache License 2.0**. All credit for the server goes to the scrcpy authors.

## License

`TqkLibrary.Scrcpy` is licensed under the **MIT License**.

The bundled scrcpy server is a separate work licensed under the **Apache License 2.0** by Genymobile; see the [scrcpy repository](https://github.com/Genymobile/scrcpy) for its license terms.
