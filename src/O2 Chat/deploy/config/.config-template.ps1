# default location is c:\O2Bionics\ChatService.json is set in the Web.config or App.config's app settings "ConfigFilePath"
# keep in mind that the ChatService.json should have access permissions set to allow IIS and Chat service to access it.

Write-Output @"
//
// `$chat_service_port =        $chat_service_port
// `$workspace_receiver_port =  $workspace_receiver_port
// `$widget_receiver_port =     $widget_receiver_port
// 
// `$emergency_log_directory =  '$emergency_log_directory'
//
// `$username =                 '$username'
// `$oracle_host =              '$oracle_host'
// `$oracle_instance =          '$oracle_instance'
// `$mysql_host =               '$mysql_host'
// `$elastic_url =              '$elastic_url'
// `$kibana_url =               '$kibana_url'
// `$server_kind =              '$server_kind'
// `$smtp_host =                '$smtp_host'
//
// The "pageLoadSettings" has small values to facilitate testing.
{
  "chatService": {
    "wcfBindPort": ${chat_service_port},
    "database": "User ID=${username}_chat;Password=1;Data Source=//${oracle_host}:1521/${oracle_instance};",
    "logSqlQuery": true,
    "cache": {
      "visitor": 1000,
      "session": 1000,
    },
    "workspaceUrl": "https://app.chat-${server_kind}.o2bionics.com",
  },

  "chatServiceClient": {
    "host": "localhost",
    "port": ${chat_service_port},
  },

  "mailerService": {
    "smtp": {
      "host": "${smtp_host}",
      "port": 25,
      "from": "no-respond@chat-${server_kind}.o2bionics.com",
    },
  },

  "errorTracker": {
    "elasticConnection": { "uris": ["${elastic_url}"] },
    "index": {
      "name": "${username}_error",
      "settings": {
        "index.number_of_shards": "5",
        "index.translog.durability": "async",
        "index.translog.sync_interval": "5s",
        "index.refresh_interval": "10s",
      },
    },
    "emergencyLogDirectory": "${emergency_log_directory}",
  },

  "kibana": {
    "kibanaUrl": "${kibana_url}",
  },

  "attachments": {
    "sizeLimit": 1024000,
    "amazon": {
      "servicesDomainUrl": "https://s3.amazonaws.com",
      "bucketName": "${username}-test",
      "attachmentsFolderName": "Attachments",
      "accessKey": "AKIAIPK5ANJLUEYZWYRA",
      "secretKey": "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",
    },
  },

  "workspace": {
    "chatServiceEventReceiverPort": ${workspace_receiver_port},
    "webSocket": {
      "connectionTimeout": "0:1:50",
      "disconnectTimeout": "0:1:30",
      "keepAlive": "0:0:30",
    },
    "widgetUrl": "https://c.chat-${server_kind}.o2bionics.com",
    "o2bionicsSite3DesKey": "YM0OllS0SE6afoFDvZKrlQ==",
  },

  "widget": {
    "chatServiceEventReceiverPort": ${widget_receiver_port},
    "webSocket": {
      "connectionTimeout": "0:1:50",
      "disconnectTimeout": "0:1:30",
      "keepAlive": "0:0:30",
    },
    "workspaceUrl": "https://app.chat-${server_kind}.o2bionics.com",
  },

  "pageTracker": {
    "database": "server=${mysql_host};user=${username};database=${username}_pagetrack;port=3306;password=1;",
    "logSqlQuery": true,

    "elasticConnection": { "uris": ["${elastic_url}"] },
    "pageVisitIndex": {
      "name": "${username}_track",
      "settings": {
        "index.number_of_shards": "5",
        "index.translog.durability": "async",
        "index.translog.sync_interval": "5s",
        "index.refresh_interval": "10s",
      }
    },
    "idStorageIndex": {
      "name": "${username}_track_ids",
      "settings": {
        "index.number_of_shards": "1",
      }
    },
    "idStorageBlockSize": 1000,

    "maxMindGeoIpDatabasePath": "C:\\O2Bionics\\O2Chat\\geoip\\GeoLite2-City.mmdb",
    "widgetUrl": "https://c.chat-${server_kind}.o2bionics.com",
    "workspaceUrl": "https://app.chat-${server_kind}.o2bionics.com",
    "featureCacheTimeToLive": "0:1:0",
    "addBufferSize": ${add_buffer_size},
    "addBufferFlushTimeout": "${add_buffer_flush_timeout}",
  },

  "pageTrackerClient": {
    "url": "https://tr.chat-${server_kind}.o2bionics.com",
  },

  "featureService": {
    "selfHostWebBindUri": "http://*:8080",
    "databases": {
      "chat": "User ID=${username}_feature;Password=1;Data Source=//${oracle_host}:1521/${oracle_instance};",
      "test": "User ID=${username}_feature_test;Password=1;Data Source=//${oracle_host}:1521/${oracle_instance};",
    },
    "logSqlQuery": true,
    "logProcessing": true,
    "timeToLive": "0:1:0",
    "cache": {
      "memoryLimitMegabytes": 0,
      "physicalMemoryLimitPercentage": 0,
      "memoryPollingInterval": "0:1:0",
    },
  },

  "featureServiceClient": {
    "productCode": "chat",
    "urls": [ "http://fs.chat-${server_kind}.o2bionics.com/" ],
    "timeout": "0:0:20",
    "localCacheTimeToLiveSeconds": 10,
  },

  "auditTrailClient": {
    "urls": [ "http://ats.chat-${server_kind}.o2bionics.com/" ],
  },

  "auditTrailService": {
    "elasticConnection": { "uris": ["${elastic_url}"] },
    "index": {
      "name": "${username}_ats",
      "settings": {
        "index.number_of_shards": "5",
        "index.translog.durability": "async",
        "index.translog.sync_interval": "5s",
        "index.refresh_interval": "10s",
      },
    },
    "productCodes": [ 
	  "chat",
	],
  },

  "widgetLoadLimiter": {
    "countersDbUpdateDelta": 10,
    "countersDbUpdateMinimumIntervalSeconds": 5,
  },

  "mailerServiceClient": {
    "urls": [ "http://mailer.chat-${server_kind}.o2bionics.com/" ],
  },

  "testCustomerSite": {},

  "test": {
    "chatServiceDatabase": "User ID=${username}_chat_test;Password=1;Data Source=//${oracle_host}:1521/${oracle_instance};",
    "featureServiceDatabase": "User ID=${username}_feature_test;Password=1;Data Source=//${oracle_host}:1521/${oracle_instance};",
    "pageTrackerServiceDatabase": "server=${mysql_host};user=${username};database=${username}_pagetrack_test;port=3306;password=1;",
    "errorTracker": {
      "elasticConnection": { "uris": ["${elastic_url}"]},
      "index": {
          "name": "${username}_error_test",
          "settings": {
            "index.number_of_shards": "5",
            "index.translog.durability": "async",
            "index.translog.sync_interval": "5s",
            "index.refresh_interval": "10s",
          },
      },
      "emergencyLogDirectory": "${emergency_log_directory}",
    },
  },
}
"@