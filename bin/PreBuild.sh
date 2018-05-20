#!/bin/bash

nuget.exe restore "./bin/Photon.sln"

msbuild.exe "./Photon.Publishing/Photon.Publishing.csproj" /t:Rebuild /p:Configuration="Debug" /p:Platform="Any CPU" /p:OutputPath="bin\Debug" /v:m
