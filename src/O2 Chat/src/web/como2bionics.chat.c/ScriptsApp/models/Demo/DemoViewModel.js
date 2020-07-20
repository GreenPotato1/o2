'use strict';

function DemoViewModel(appearance, windowMessageHandler)
{
  var self = this;

  this.debugMode = false;

  this.iframeMode = true;

  this.isMinimized = ko.observable();
  this.dialog = ko.observable(Dialogs.chat);

  this.poweredByVisibleFlag = ko.observable(appearance.poweredByVisible.value);

  this.isStartChatDialogVisible =
    ko.pureComputed(function () { return !self.isMinimized() && self.dialog() === Dialogs.startChat; });
  this.isChatDialogVisible =
    ko.pureComputed(function () { return !self.isMinimized() && self.dialog() === Dialogs.chat; });
  this.isOfflineMessageSentDialogVisible =
    ko.pureComputed(function () { return !self.isMinimized() && self.dialog() === Dialogs.offlineMessageSent; });
  this.isEditVisitorInfoDialogVisible =
    ko.pureComputed(function () { return !self.isMinimized() && self.dialog() === Dialogs.editVisitorInfo; });
  this.isLoadingDialogVisible =
    ko.pureComputed(function () { return !self.isMinimized() && self.dialog() === Dialogs.loading; });
  this.isTranscriptProposalDialogVisible =
    ko.pureComputed(function () { return !self.isMinimized() && self.dialog() === Dialogs.transcriptProposal; });
  this.isPoweredByVisible =
    ko.pureComputed(function () { return !self.isMinimized() && self.poweredByVisibleFlag() });

  this.startChatDialog = new DemoStartChatDialogViewModel(this, ko);
  this.chatDialog = new DemoChatDialogViewModel(this, ko);
  this.offlineMessageSentDialog = new DemoOfflineMessageSentDialogViewModel(this);
  this.editVisitorInfoDialog = new DemoEditVisitorInfoDialogViewModel(this);
  this.transcriptProposalDialog = new TranscriptProposalDialogViewModel(this, null);

  this.connectionState = ko.observable(Enums.ConnectionState.initial);

  var internalMinimizedTitleText = ko.observable(
    appearance.minimizedTitleText.value
    ? appearance.minimizedTitleText.value
    : Strings.windowTitleOfflineMessage);

  this.minimizedTitleText = ko.pureComputed(
    {
      read: function ()
      {
        return internalMinimizedTitleText();
      },
      write: function (value)
      {
        value ? internalMinimizedTitleText(value) : internalMinimizedTitleText(Strings.windowTitleOfflineMessage);
      },
      owner: this
    });

  this.windowTitleText = ko.pureComputed(
    function ()
    {
      switch (self.connectionState())
      {
      case Enums.ConnectionState.disconnected:
        return Strings.windowTitleDisconnectedMessage;
      case Enums.ConnectionState.reconnecting:
        return Strings.windowTitleReconnectingMessage;
      default:
        return self.startChatDialog.isSelectedDepartmentOffline()
                 ? Strings.windowTitleOfflineMessage
                 : Strings.windowTitleOnlineMessage;
      }
    });
  this.windowTitleClass = ko.pureComputed(
    function ()
    {
      return '';
    },
    this);

  windowMessageHandler.onAppearance(function (appearance)
  {
      if (appearance.theme)
      {
          $('#themeStyleLink').attr('href', 'https://' + window.location.host + '/themes/maximized/' + appearance.theme.value + '/styles.css');

          if (this.isMinimized())
              this.onClickTitleBar();
      }

    if (appearance.themeMin)
    {
      $('#themeMinStyleLink').attr('href', 'https://' + window.location.host + '/themes/minimized/' + appearance.themeMin.value + '/styles.css');

      $.ajax({
          url: 'https://' + window.location.host + '/themes/minimized/' + appearance.themeMin.value + "/min.html",
          cache: false,
          success: function (result)
          {
            $("#collapsed-state").html(result);
            refreshMinWidgetBindings();
            self.adjustIframeSize();
          },
          error: function (xhr, textStatus, errorThrown)
          {
            $("#collapsed-state").load('https://' + window.location.host + '/themes/minimized/default/min.html', refreshMinWidgetBindings);
          }
      });

      if (!this.isMinimized())
          this.onClickTitleBar();
    }

    function refreshMinWidgetBindings()
    {
      var elem = $("#collapsed-state").get(0);
      ko.cleanNode(elem);
      ko.applyBindings(self, elem);
    }

    if (appearance.minimizedTitleText)
    {
      self.minimizedTitleText(appearance.minimizedTitleText.value);
      self.adjustIframeSize();
    }

    if (appearance.poweredByVisible)
    {
      self.poweredByVisibleFlag(appearance.poweredByVisible.value);
      self.adjustIframeSize();
    }

    if (appearance.positioning)
    {
      windowMessageHandler.postSetPosition(appearance.positioning);
    }
  }.bind(this));


  windowMessageHandler.onHeaderClick(function ()
  {
    $('#collapsed-state .chat-title-bar').click();
  }.bind(this));

  this.onClickTitleBar = function ()
  {
    var minimized = !this.isMinimized();
    this.isMinimized(minimized);
    this.adjustIframeSize();
  };

  this.onClickPopoutButton = function (model, event)
  {
    event.stopPropagation();
  };

  // methods
  this.adjustIframeSize = function ()
  {
      var height = Metrics.titleHeight;
      var width = Metrics.dialogWidth;

      if (!this.isMinimized()) {
          height += Metrics.dialogHeight;

          if (this.isPoweredByVisible())
              height += Metrics.poweredByHeight;
      }
      else {
          var bar = $("#collapsed-state .chat-title-bar");

          var widthProvidedInStyles = bar.get(0).style.width;

          if (widthProvidedInStyles)
              width = bar.width();

          var titlePosRight = $(".title-text").css("right"),
              titlePaddingRight = $(".title-text").css("padding-right"),
              titlePaddingLeft = $(".title-text").css("padding-left"),
              titleMarginRight = $(".title-text").css("margin-right"),
              titleMarginLeft = $(".title-text").css("margin-left"),
              titleWidth = $(".title-text").width() + "px";
          var calcBarNewWidth = parseInt(titlePosRight) + parseInt(titlePaddingRight) + parseInt(titlePaddingLeft) + parseInt(titleWidth) + parseInt(titleMarginRight) + parseInt(titleMarginLeft) + 1;

          var barNewWidth = (calcBarNewWidth ? calcBarNewWidth : width) + "px";

          if (!widthProvidedInStyles || width < calcBarNewWidth) {
              bar.css("width", barNewWidth);
              width = bar.width()
          }

          if (bar.get(0).style.height)
              height = bar.height();
      }
      
      if ($(".chat-title-img").css('display') !== 'none') {
          var elem = $(".chat-title-img");
          width = Math.max(width, elem.width());
          height = Math.max(height, elem.height());
      }

      windowMessageHandler.postShow(width, height, this.isMinimized());
  };

  this.initialize = function ()
  {
    $('.title-buttons').removeAttr('data-bind');
    $('button:not(.back)').attr('disabled', true);
    $('textarea, input').attr('readonly', true);

    this.isMinimized(false);

    windowMessageHandler.postSetPosition(appearance.positioning);

    this.adjustIframeSize();
  }
}