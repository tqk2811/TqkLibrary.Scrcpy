d3dconfig apps --add %cd%\TestConsole\bin\x64\Debug\net462\TestConsole.exe
d3dconfig apps --add %cd%\TestRenderWpf\bin\x64\Debug\net462\TestRenderWpf.exe
d3dconfig debug-layer debug-layer-mode=force-on
d3dconfig message-break allow-debug-breaks=true
pause