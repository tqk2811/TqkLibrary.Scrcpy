d3dconfig apps --remove %cd%\TestConsole\bin\x64\Debug\net462\TestConsole.exe
d3dconfig apps --remove %cd%\TestRenderWpf\bin\x64\Debug\net462\TestRenderWpf.exe
d3dconfig debug-layer debug-layer-mode=off
d3dconfig message-break allow-debug-breaks=false
pause