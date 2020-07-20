Set-StrictMode -Version Latest

Import-Module WebAdministration

. "$PSScriptRoot\common.ps1"

Write-Host @"
  recreating sites in $sitesRoot
  for $serverKind server
  using $zone zone
"@ -ForegroundColor Green

$customerSiteCert = GetMyCert("$(Suffix).$testZone")
$chatSiteCert = GetMyCert("$(Suffix 'chat').$zone")
$chatSitesCert = GetMyCert("*.$(Suffix 'chat').$zone")

CreateWebSite $sitesRoot "net.customer"                     "$(Suffix).$testZone" $customerSiteCert

CreateWebSite $sitesRoot "o2bionics.com.chat"                "$(Suffix 'chat').$zone" $chatSiteCert
CreateWebSite $sitesRoot "o2bionics.com.chat.www"        "www.$(Suffix 'chat').$zone" $chatSitesCert
CreateWebSite $sitesRoot "o2bionics.com.chat.www-st"  "www-st.$(Suffix 'chat').$zone" $chatSitesCert
CreateWebSite $sitesRoot "o2bionics.com.chat.app"        "app.$(Suffix 'chat').$zone" $chatSitesCert
CreateWebSite $sitesRoot "o2bionics.com.chat.app-st"  "app-st.$(Suffix 'chat').$zone" $chatSitesCert
CreateWebSite $sitesRoot "o2bionics.com.chat.c"            "c.$(Suffix 'chat').$zone" $chatSitesCert
CreateWebSite $sitesRoot "o2bionics.com.chat.c-st"      "c-st.$(Suffix 'chat').$zone" $chatSitesCert
CreateWebSite $sitesRoot "o2bionics.com.chat.fs"          "fs.$(Suffix 'chat').$zone" $chatSitesCert
CreateWebSite $sitesRoot "o2bionics.com.chat.tr"          "tr.$(Suffix 'chat').$zone" $chatSitesCert
CreateWebSite $sitesRoot "o2bionics.com.chat.ats"        "ats.$(Suffix 'chat').$zone" $chatSitesCert
CreateWebSite $sitesRoot "o2bionics.com.chat.mailer"  "mailer.$(Suffix 'chat').$zone" $chatSitesCert
