#!/usr/bin/env pwsh
param(
	[Parameter(Mandatory=$true)]
	$major=0,
	[Parameter(Mandatory=$true)]
	$minor=1,
	[Parameter(Mandatory=$true)]
	$patch=0
)

$RID = "linux-x64"

if ($IsLinux -eq $true)
{
	$ARCH = uname -m
	if($ARCH -eq "aarm64" -or $ARCH -eq "arm64")
	{
		$RID = "linux-arm64"
	}
}
elseif ($IsWindows -eq $true)
{
	$RID = "win-x64"
}
else
{
	Write-Host "Unsupported OS"
	return
}

docker build . -f src/Sannel.House.Sprinklers/Dockerfile --build-arg MAJOR=$major --build-arg MINOR=$minor --build-arg PATCH=$patch --build-arg RID=$RID -t sannel/house.sprinklers:$RID-$major.$minor.$patch
docker push sannel/house.sprinklers:$RID-$major.$minor.$patch
