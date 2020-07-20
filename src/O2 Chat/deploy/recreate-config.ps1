Set-StrictMode -Version Latest

. "$PSScriptRoot\common.ps1"

$config = $serverKind

if (Test-Path "$PSScriptRoot\config\$storageUser.ps1")
{
	$config = $storageUser;
}

if (Test-Path $systemConfigFile)
{
	Write-Host "    copying existing $systemConfigFile to $systemConfigFile.prev"
	Copy-Item $systemConfigFile "$systemConfigFile.prev"
}

Write-Host "    creating $systemConfigFile using .\config\$config.ps1 template" -ForegroundColor Green
& "$PSScriptRoot\config\$config.ps1" > $systemConfigFile