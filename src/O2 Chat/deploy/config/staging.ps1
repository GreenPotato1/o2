. "$PSScriptRoot\.config-defaults.ps1";

# Overwrite the default values
$add_buffer_size = 100
$add_buffer_flush_timeout = "0:0:30"

. "$PSScriptRoot\.config-template.ps1";