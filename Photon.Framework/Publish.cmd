if exist "PiServerLite\bin\Package\" rmdir /Q /S "PiServerLite\bin\Package"
nuget pack "PiServerLite\PiServerLite.csproj" -OutputDirectory "PiServerLite\bin\Package" -Build -Prop "Configuration=Release;Platform=AnyCPU"
nuget push "PiServerLite\bin\Package\*.nupkg" -Source "https://www.nuget.org/api/v2/package" -NonInteractive
pause
