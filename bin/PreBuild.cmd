echo "RESTORE"
nuget restore "%~dp0\..\Photon.sln"
echo "BUILD"
"%~dp0\msbuild.cmd" "%~dp0\..\Photon.Publishing\Photon.Publishing.csproj" /t:Rebuild /p:Configuration="Debug" /p:Platform="Any CPU" /p:OutputPath="bin\Debug" /v:m
