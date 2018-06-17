"%~dp0nuget.exe" restore "%~dp0..\Photon.sln"
if not %errorlevel% == 0 exit %errorlevel%

set msbuild_exe="C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe"
%msbuild_exe% "%~dp0..\Photon.Publishing\Photon.Publishing.csproj" /t:Rebuild /p:Configuration="Debug" /p:Platform="Any CPU" /p:OutputPath="bin\Debug" /v:m
if not %errorlevel% == 0 exit %errorlevel%
