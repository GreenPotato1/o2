# this file should be runned in the root folder of the solution
# 
# $env:O2BIONICS_PATH="c:\Work\chat-deploy" (default: "d:\work\chat\deploy")
# $env:O2BIONICS_SERVER_KIND="staging"   (default: dev; "prod" for production)

$chatSrvName 		= "O2BionicsChat"
$featureSrvName 	= "O2BionicsFeatureService"
$config 			= "Debug"

$deploymentFolder 	= $env:O2BIONICS_PATH
$deploymentFolder 	= if ($deploymentFolder -eq $null) { "d:\work\chat\deploy" } else { $deploymentFolder }

$serverKind         = $env:O2BIONICS_SERVER_KIND
$serverKind         = if ($serverKind -eq $null) { "dev" } else { $serverKind }
$serverKind         = if ($serverKind -eq "prod") { "" } else { $serverKind }
$serverKindSep      = if ($serverKind.Length -eq 0) { "" } else { "-" }

$msbuildPath 		= "c:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin"
if (!(Test-Path $msbuildPath))
{ 
	$msbuildPath	= "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin"
}

$msbuild 			= "$msbuildPath\MSBuild.exe"
$buildArgs			= @("O2Bionics.sln", "/t:Rebuild", "/m", "/fileLogger", "/noconsolelogger", "/p:Configuration=$config")

$nugetExe			= "C:\Program Files (x86)\NuGet\nuget.exe"

$cd 				= Get-Location

Function Main
{
	Write-Host "`nWill build $config in $cd" -ForegroundColor Cyan 
	Write-Host "Will deploy to $deploymentFolder" -ForegroundColor Cyan

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

	& $msbuild $buildArgs
	if ($LASTEXITCODE -gt 0)
	{
		Write-Host "Build failed with code $LASTEXITCODE"
		exit $LASTEXITCODE
	}

	# chat service
	SafeStopService $chatSrvName "Com.O2Bionics.ChatService.Host"
	RewriteFolder "$cd\src\chat\Com.O2Bionics.ChatService.Host\bin\$config" "$deploymentFolder\chatService"
	SafeInstallService $chatSrvName "$deploymentFolder\chatService\Com.O2Bionics.ChatService.Host.exe"

	# feature service
	SafeStopService $featureSrvName "Com.O2Bionics.FeatureService.SelfHostWeb"
	RewriteFolder "$cd\src\featureService\Com.O2Bionics.FeatureService.SelfHostWeb\bin\$config" "$deploymentFolder\featureService"
	SafeInstallService $featureSrvName "$deploymentFolder\featureService\Com.O2Bionics.FeatureService.SelfHostWeb.exe"

	# web
	Write-Host
	RewriteFolder "$cd\src\web" "$deploymentFolder\web" { 
			$folder = if ($_ -is [System.IO.DirectoryInfo]) { $_.FullName } else { $_.DirectoryName }
			if ($folder.Contains("\obj\") -or $folder.EndsWith("\obj")) { $false } else	{ $true	}
		}

    
    $hostPlaceholder = "chat.o2bionics.com"
    $actualHost = "chat$($serverKindSep + $serverKind).o2bionics.com"
    
    ReplaceInFile $hostPlaceholder $actualHost "$deploymentFolder\web\com.o2bionics.chat.app-st\web.config"
    ReplaceInFile $hostPlaceholder $actualHost "$deploymentFolder\web\com.o2bionics.chat.c-st\web.config"
    ReplaceInFile $hostPlaceholder $actualHost "$deploymentFolder\web\com.o2bionics.chat.c\test-track.html"
    ReplaceInFile $hostPlaceholder $actualHost "$deploymentFolder\web\com.o2bionics.chat.www-st\web.config"
    ReplaceInFile $hostPlaceholder $actualHost "$deploymentFolder\web\com.o2bionics.chat.www\Web.config"
    ReplaceInFile $hostPlaceholder $actualHost "$deploymentFolder\web\com.o2bionics.chat.www\Views\Shared\_Layout.cshtml"
    ReplaceInFile $hostPlaceholder $actualHost "$deploymentFolder\web\com.o2bionics.chat\Web.config"
    ReplaceInFile $hostPlaceholder $actualHost "$deploymentFolder\web\com.customer\Web.config"
    

	# start services
	Write-Host "`nStarting $chatSrvName service..." -ForegroundColor Green
	Start-Service $chatSrvName
	Write-Host "`nStarting $featureSrvName service..." -ForegroundColor Green
	Start-Service $featureSrvName

	# restart IIS
	Write-Host "`nRestarting IIS..." -ForegroundColor Green
	iex "iisreset"
}

Function ReplaceInFile([string] $s1, [string] $s2, [string] $filePath)
{
    (Get-Content $filePath) -replace $s1, $s2 | Set-Content $filePath
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

		Sleep 5
		# while ($true)
		# {
		# 	$proc = Get-Process $processName -ErrorAction SilentlyContinue
		# 	if (!$proc -or $proc.HasExited) { break }
		# 	Write-Host "waiting for $ServiceName process $processName to stop" -ForegroundColor Yellow
		# 	Sleep 2
		# }

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

Function RewriteFolder([string] $source, [string] $target, [ScriptBlock] $filter = { $true })
{
	Write-Host "`Updating $target..." -ForegroundColor Green
	if (Test-Path $target)
	{
		Get-ChildItem "$target" -Filter *.* -Recurse |  Remove-Item -Force -Recurse -ErrorAction Stop
		Remove-Item "$target" -Force -ErrorAction Stop
	}
	
	New-Item -Path "$target" -ItemType Directory > $null
	Get-ChildItem "$source" -Filter * -Recurse `
		| Where-Object -FilterScript $filter `
		| ForEach-Object { 
				$item = $_

				$destDir = Split-Path ($item.FullName -Replace [regex]::Escape($source), $target)
		        
		        if (!(Test-Path $destDir))
		        {
		            New-Item -ItemType directory $destDir | Out-Null
		        }

		       	if ($item -is [System.IO.FileInfo])
		        {
		        	Copy-Item $item.FullName -Destination $destDir -ErrorAction Stop
		        }
		        elseif ($item -is [System.IO.DirectoryInfo])
		        {
		        	$dir = $destDir + "\" + $item.Name
		        	if (!(Test-Path $dir))
		         	{
		        		New-Item -ItemType directory $dir | Out-Null
		         	}
		        }
			}
	Write-Host "$target files updated..." -ForegroundColor Green
}

Main