@echo off

for /f "usebackq tokens=*" %%i in (`%~dp0\vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`) do (
  set InstallDir=%%i
)

if exist "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" (
  "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" %*
)

if %errorlevel% neq 0 exit /b %errorlevel%
