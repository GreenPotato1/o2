using System;
using Com.O2Bionics.AuditTrail.Client.Settings;
using Com.O2Bionics.ChatService.Contract.Settings;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.Utils.JsonSettings;
using Com.O2Bionics.Utils.Web.Settings;

namespace Com.O2Bionics.ChatService.Web.Console
{
    [SettingsRoot("workspace")]
    public class WorkspaceSettings
    {
        [Required]
        [IntRange(1)]
        public int ChatServiceEventReceiverPort { get; set; }

        [Required]
        public Uri WidgetUrl { get; set; }

        [Required]
        public string O2BionicsSite3DesKey { get; set; }

        public WebSocketSettings WebSocket { get; set; }

        [Required]
        [SettingsRoot("chatServiceClient")]
        public ChatServiceClientSettings ChatServiceClient { get; set; }

        [Required]
        [SettingsRoot("pageTrackerClient")]
        public PageTrackerClientSettings PageTrackerClient { get; set; }

        [Required]
        [SettingsRoot("attachments")]
        public AttachmentsSettings Attachments { get; set; }

        [Required]
        [SettingsRoot("errorTracker")]
        public ErrorTrackerSettings ErrorTracker { get; set; }

        [Required]
        [SettingsRoot(AuditTrailClientSettings.RootName)]
        public AuditTrailClientSettings AuditTrailClient { get; set; }

        [Required]
        [SettingsRoot("featureServiceClient")]
        public FeatureServiceClientSettings FeatureServiceClient { get; set; }
    }
}
