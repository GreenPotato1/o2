using System;
using Com.O2Bionics.ChatService.Contract.Settings;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils.JsonSettings;
using Com.O2Bionics.Utils.Web.Settings;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Widget
{
    [SettingsRoot("widget")]
    [UsedImplicitly]
    public sealed class WidgetSettings
    {
        [Required]
        [IntRange(1)]
        public int ChatServiceEventReceiverPort { get; set; }

        [Required]
        public Uri WorkspaceUrl { get; set; }

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
    }
}