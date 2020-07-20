Set-StrictMode -Version Latest

$systemConfigFile = "C:\O2Bionics\ChatService.json"

$zone = $env:O2BIONICS_CHAT_ZONE
if (!$zone) { $zone = "o2bionics.com" }
$testZone = $env:O2BIONICS_CHAT_TEST_ZONE
if (!$testZone) { $testZone = "net.customer" }

$serverKind = $env:O2BIONICS_SERVER_KIND
if (!$serverKind) { $serverKind = "dev" }
$sitesRoot = $env:O2BIONICS_CHAT_SITES_PATH
if (!$sitesRoot) { $sitesRoot = "C:\Work\O2Bionics\src\web" }
$storageUser = $env:O2BIONICS_CHAT_STORAGE_USER
if (!$storageUser)
{
    if ($serverKind -eq "dev"){
        Throw "O2BIONICS_CHAT_STORAGE_USER environment variable MUST be configured for developer configuration"
    }
    $storageUser = $serverKind
}


Function Suffix([string] $text = $null)
{
    if (!$text) { return $serverKind; }
    return "$text-$serverKind"
}

Function GetMyCert([string] $name)
{
    return Get-ChildItem Cert:\LocalMachine\My | Where-Object {$_.SubjectName.Name -eq "CN=$name"}
}

Function CreateWebSite([string] $rootFolder, [string] $name, [string] $hostName, [object] $cert) 
{
    $sitePath = "IIS:\Sites\$name"
    
    $name2 = $name.Replace("o2bionics.com", "com.o2bionics")
    $name2 = $name2.Replace("net.customer", "com.customer")
    $path = "$rootFolder\$name2"
    $sslBindingPath = "IIS:\SslBindings\!443!$hostName"

    Write-Host "    creating site $hostName -> $path" -ForegroundColor Green

    if (Test-Path $sslBindingPath) 
    { 
        Get-Item $sslBindingPath | Remove-Item -Recurse 
    }
    if (Test-Path $sitePath -pathType container) 
    { 
        Get-Item $sitePath | Remove-Item -Recurse 
    }
    
    if(!(Test-Path -Path $path )){
        Write-Error "Folder must exist '$path'."
    }

    New-Website -Name $name -PhysicalPath $path -HostHeader $hostName
    New-WebBinding -Name $name -Protocol "https" -Port 443 -HostHeader $hostName -SslFlags 1
    New-Item -Path $sslBindingPath -Value $cert -SSLFlags 1
    Set-ItemProperty $sitePath -name applicationPool -value ".NET v4.5"
}
