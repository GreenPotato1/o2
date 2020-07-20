Set-StrictMode -Version Latest

. "$PSScriptRoot\..\common.ps1"

$server_kind             =  $serverKind
$username                =  $storageUser
$emergency_log_directory = "C:\\O2Bionics\\O2Chat\\Logs"

# ports must be unique:
$chat_service_port       =  8523
$workspace_receiver_port =  8524
$widget_receiver_port    =  8525

$oracle_host =             "db00.o2bionics.com"
$oracle_instance =         "orcl"
$mysql_host =              "db00.o2bionics.com"
$elastic_url =             "http://es0[0].o2bionics.com:9200"
 
$kibana_url =              "http://es00.o2bionics.com:5601"
$smtp_host =               "127.0.0.1"

# Use small values for testing.
$add_buffer_size = 1
$add_buffer_flush_timeout = "0:0:1"
