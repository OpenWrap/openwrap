rd ..\wraps\cache\OpenWrap-1.0.42 /S /Q
del ..\wraps\OpenWrap-1.0.42.wrap /F /Q

copy /Y ..\scratch\bin\OpenWrap-Debug-AnyCPU\*.* ..\wraps\anchored\openwrap\bin-net35
copy /Y ..\scratch\bin\OpenWrap.Build.Tasks-Debug-AnyCPU\*.* ..\wraps\anchored\openwrap\build
copy /Y ..\scratch\bin\OpenWrap.Commands-Debug-AnyCPU\*.* ..\wraps\anchored\openwrap\commands
pause
msbuild self-build.test