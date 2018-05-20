#!/bin/bash

mono "./bin/NuGet.exe" restore "./Photon.sln"

mono "msbuild.exe" "./Photon.Publishing/Photon.Publishing.csproj" /t:Rebuild /p:Configuration="Debug" /p:Platform="Any CPU" /p:OutputPath="bin\Debug" /v:m
