# this file should be runned in the root folder of the solution

$chatSrvName        = "O2BionicsChat"
$featureSrvName     = "O2BionicsFeatureService"
$config             = "Debug"

$msbuildPath        = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin"
$msbuild            = "$msbuildPath\MSBuild.exe"
$buildArgs          = @("O2Bionics.sln", "/t:Rebuild", "/m", "/fileLogger", "/noconsolelogger", "/p:Configuration=$config")

$nugetExe           = "C:\Program Files (x86)\NuGet\nuget.exe"

$cd                 = Get-Location

Function Main
{
    Write-Host "`nWill build $config in $cd" -ForegroundColor Cyan 
    
    Write-Host "`nStopping services..." -ForegroundColor Green
    iex "iisreset /stop"
    SafeStopService $chatSrvName "Com.O2Bionics.ChatService.Host"
    SafeStopService $featureSrvName "Com.O2Bionics.FeatureService.SelfHostWeb"

    Write-Host "`nBuilding solution..." -ForegroundColor Green
    # you can download nuget.exe from https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
    # the file expected to be located in C:\Program Files (x86)\NuGet\
    & $nugetExe restore -verbosity quiet -MsbuildPath "$msbuildPath"
    if ($LASTEXITCODE -gt 0)
    {
        Write-Host "Nuget restore failed with code $LASTEXITCODE"
        exit $LASTEXITCODE
    }

    pushd .\src\web\com.o2bionics.chat.app\
    npm install
    .\node_modules\.bin\bower install
    popd

    #  & $msbuild $buildArgs
    #  if ($LASTEXITCODE -gt 0)
    #  {
    #      Write-Host "Build failed with code $LASTEXITCODE"
    #      exit $LASTEXITCODE
    #  }

    SafeInstallService $chatSrvName "$cd\src\chat\Com.O2Bionics.ChatService.Host\bin\$config\Com.O2Bionics.ChatService.Host.exe"
    SafeInstallService $featureSrvName "$cd\src\featureService\Com.O2Bionics.FeatureService.SelfHostWeb\bin\$config\Com.O2Bionics.FeatureService.SelfHostWeb.exe"
}

Function SafeStopService([string] $ServiceName, [string] $processName) 
{
    if (Get-Service "$ServiceName" -ErrorAction SilentlyContinue) 
    {
        Write-Host "`nStopping $ServiceName service..." -ForegroundColor Green
        Stop-Service $ServiceName 
        While ((Get-Service $ServiceName).Status -ne "Stopped") 
        {
            Write-Host "Waiting for $ServiceName service to stop..." -ForegroundColor Yellow
            Sleep 2
        }

        while ($true)
        {
            $proc = Get-Process $processName -ErrorAction SilentlyContinue
            if (!$proc -or $proc.HasExited) { break }
            Write-Host "waiting for $ServiceName process $processName to stop" -ForegroundColor Yellow
            Sleep 2
        }

        Write-Host "$ServiceName service stopped." -ForegroundColor Green
    }
    else
    {
        Write-Host "`nService $ServiceName isn`'t installed." -ForegroundColor Green
    }
}

Function SafeInstallService([string] $ServiceName, [string]$executablePath)
{
    $install = $false
    if (Get-Service "$ServiceName" -ErrorAction SilentlyContinue) 
    {
        Write-Host "Service $ServiceName is already installed." -ForegroundColor Green

        $path = (Get-WmiObject win32_service | ?{$_.Name -eq $ServiceName } | select  @{Name="Path"; Expression={$_.PathName.split('"')[1]}}).Path
        if (-not ($path -eq $executablePath))
        {
            Write-Host "service path $path doesn`'t equal to $executablePath" -ForegroundColor Red
            Write-Host "Uninstalling $ServiceName service at $path ..." -ForegroundColor Yellow
            iex "$path uninstall"
            if ($LASTEXITCODE -gt 0)
            {
                Write-Host "Uninstall service $ServiceName failed with code $LASTEXITCODE" -ForegroundColor Red
                exit $LASTEXITCODE
            }
            $install = $true
        }
    }
    else
    {
        $install = $true
    }

    if ($install)
    {
        Write-Host "Installing $ServiceName service to $executablePath ..." -ForegroundColor Yellow
        iex "$executablePath install"
        if ($LASTEXITCODE -gt 0)
        {
            Write-Host "Install service $ServiceName failed with code $LASTEXITCODE" -ForegroundColor Red
            exit $LASTEXITCODE
        }
        Write-Host "Service $ServiceName installed" -ForegroundColor Green
    }
}

Main