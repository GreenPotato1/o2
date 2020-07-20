Environment Variables

O2BIONICS_SERVER_KIND         - [dev|staging]; code assumes 'dev' if not set;
O2BIONICS_CHAT_STORAGE_USER   - contains username which will be used in the json configuration file
                            if a file .\config\$env:O2BIONICS_CHAT_STORAGE_USER.ps1 exists, it will be used for config generation
                            otherwise .\config\$O2BIONICS_SERVER_KIND.ps1 script will be used
                            MUST be set for dev server kind
O2BIONICS_CHAT_SITES_PATH     - path to root folder containing o2bionics sites; 'C:\Work\O2Bionics\src\web' is used if not set;
O2BIONICS_CHAT_ZONE           - the zone used for O2Bionics Chat sites. defaults to 'o2bionics.com'
O2BIONICS_CHAT_TEST_ZONE      - the zone used for the O2Bionics Chat widget test site. defaults to 'net.customer'

for a new system:
  0. enable powershell scripts execution (see .\bootstrap\enable-scripts.ps1)
  1. create certificates with .\bootstrap\create-dev-certificates.ps1
  3. create web sites with .\recreate-sites.ps1 
  4. build the solution
  5. run .\reset.ps1 to generate the json configuration file and initialize datasources

after breaking changes in a new build
  1. run .\reset.ps1 (all existing data in data sources will be lost!)
