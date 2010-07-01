rd ..\wraps\cache\OpenWrap-1.0.42 /S /Q
del ..\wraps\OpenWrap-1.0.42.wrap /F /Q

copy /Y ..\scratch\bin\OpenWrap-Debug-AnyCPU\*.* ..\wraps\cache\openwrap-1.0.0\bin-net35
copy /Y ..\scratch\bin\OpenWrap.Build.Tasks-Debug-AnyCPU\*.* ..\wraps\cache\openwrap-1.0.0\build
copy /Y ..\scratch\bin\OpenWrap.Commands-Debug-AnyCPU\*.* ..\wraps\cache\openwrap-1.0.0\commands

msbuild self-build.test