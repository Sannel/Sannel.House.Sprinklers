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

$longVersion = "$version.0"

dotnet publish -r linux-arm64 -c Release --sc -o $path /p:PublishSingleFile=true /p:VersionPrefix=$version /p:Version=$longVersion src/Sannel.House.Sprinklers/Sannel.House.Sprinklers.csproj

$content = "Package: sannel.house.sprinklers
Version: $version-$buildNumber
Maintainer: Adam Holt <holtsoftware@outlook.com>
Architecture: arm64
Homepage: https://github.com/Sannel/Sannel.House.Sprinklers
Description: Firmware to run a sprinkler system off of a raspberry pi (Currently only supports OSRPI)
"

$path = [System.IO.Path]::Combine($rootPath,"DEBIAN")
New-Item -ItemType Directory -Path $path
$debianDir = $path
$path = Join-Path -Path $path -ChildPath "control"
Set-Content -Path $path -Value $content

$content = "/etc/Sannel/House/sprinklers.json"
$path = Join-Path -Path $debianDir -ChildPath "conffiles"
Set-Content -Path $path -Value $content

$content = "#!/bin/bash
useradd --system sprinkler -G gpio,i2c
if [ ! -d `"`$DIRECTORY`" ]; then
	mkdir /var/lib/Sannel/House/Sprinklers/data
fi
chown -R sprinkler:sprinkler /var/lib/Sannel/House/Sprinklers
chmod 700 /var/lib/Sannel/House/Sprinklers/data
" -replace "`r",""
$path = Join-Path -Path $debianDir -ChildPath "postinst"
Set-Content -Path $path -Value $content
chmod 0775 $path


dpkg-deb --build --root-owner-group -Zgzip $rootPath

aptly repo add sannel-repository "$rootPath.deb"
