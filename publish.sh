#!/usr/bin/env bash
dotnet build
dotnet pack
echo "Set NUGETAPIKEY environment variable !!!!!!!"
dotnet nuget push ./HDRThumbnail/bin/Debug/HDRThumbnail.1.0.5.nupkg --api-key ${NUGETAPIKEY} --source https://api.nuget.org/v3/index.json