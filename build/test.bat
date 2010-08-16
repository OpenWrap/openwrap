rd ..\wraps\cache\OpenWrap-1.0.42 /S /Q
del ..\wraps\OpenWrap-1.0.42.wrap /F /Q

copy /Y ..\scratch\bin\OpenWrap-Debug-AnyCPU\*.* ..\wraps\openwrap\bin-net35
copy /Y ..\scratch\bin\OpenWrap.Build.Tasks-Debug-AnyCPU\*.* ..\wraps\openwrap\build
copy /Y ..\scratch\bin\OpenWrap.Commands-Debug-AnyCPU\*.* ..\wraps\openwrap\commands
pause
msbuild self-build.test