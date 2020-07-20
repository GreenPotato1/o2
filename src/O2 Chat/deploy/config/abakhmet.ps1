. "$PSScriptRoot\.config-defaults.ps1"

$oracle_host =             "db1.o2bionics.com"
$oracle_instance =         "ora11"
$mysql_host =              "db1.o2bionics.com"
$smtp_host =               "db1.o2bionics.com"
$elastic_url =             "http://192.168.41.[21,22]:9200"
$kibana_url =              "http://localhost:5601"

. "$PSScriptRoot\.config-template.ps1";