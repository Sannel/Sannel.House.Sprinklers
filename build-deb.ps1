#!/usr/bin/env pwsh
param(
	[Parameter(mandatory=$true)]
	[string]$version,
	[Parameter(mandatory=$true)]
	[int]$buildNumber
)

$rootPath = Join-Path -Path "build" -ChildPath "sannel.house.sprinklers_${version}-${buildNumber}_arm64"
New-Item -ItemType Directory $rootPath

$path = [System.IO.Path]::Combine($rootPath,"var","lib","Sannel","House","Sprinklers")
New-Item -ItemType Directory -Path $path

$path = [System.IO.Path]::Combine($rootPath,"etc","Sannel","House")
New-Item -ItemType Directory -Path $path

$path = [System.IO.Path]::Combine($rootPath,"etc","systemd","system")
New-Item -ItemType Directory -Path $path

Copy-Item ([System.IO.Path]::Combine("src","Sannel.House.Sprinklers","appsettings.etc.json")) ([System.IO.Path]::Combine($rootPath,"etc","Sannel","House","sprinklers.json"))

Copy-Item "Sannel.House.Sprinklers.service" ([System.IO.Path]::Combine($rootPath,"etc","systemd","system","Sannel.House.Sprinklers.service"))

$path = [System.IO.Path]::Combine($rootPath,"usr","share","Sannel","House","Sprinklers")
New-Item -ItemType Directory -Path $path

dotnet publish -r linux-arm64 -c Release --sc -o $path /p:PublishSingleFile=true src/Sannel.House.Sprinklers/Sannel.House.Sprinklers.csproj

$content = "Package: sannel.house.sprinklers
Version: $version
Maintainer: Adam Holt <holtsoftware@outlook.com>
Architecture: arm64
Homepage: https://github.com/Sannel/Sannel.House.Sprinklers
Description: Firmware to run a sprinkler system off of a raspberry pi (Currently only supports OSRPI)"

$path = [System.IO.Path]::Combine($rootPath,"DEBIAN")
New-Item -ItemType Directory -Path $path
$path = Join-Path -Path $path -ChildPath "control"
Set-Content -Path $path -Value $content

dpkg-deb --build --root-owner-group -Zgzip $rootPath
