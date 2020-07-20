
$chatSrvName        = "O2BionicsChat"
$featureSrvName     = "O2BionicsFeatureService"

Function Main
{
    # start services
    Write-Host "`nStarting $chatSrvName service..." -ForegroundColor Green
    Start-Service $chatSrvName
    Write-Host "`nStarting $featureSrvName service..." -ForegroundColor Green
    Start-Service $featureSrvName
    iex "iisreset /start"
}

Main