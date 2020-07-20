Set-StrictMode -Version Latest

. "$PSScriptRoot\common.ps1"

Write-Host @"
reset host configuration
  server kind:  $serverKind
  sites root:   $sitesRoot
  storage user: $storageUser
"@ -ForegroundColor Green

iisreset /stop

& "$PSScriptRoot\recreate-config.ps1"
& "$PSScriptRoot\recreate-sites.ps1"
& "$PSScriptRoot\storage\.recreate.ps1"

iisreset /start