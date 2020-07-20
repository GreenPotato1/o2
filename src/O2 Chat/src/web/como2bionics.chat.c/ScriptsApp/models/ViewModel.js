'use strict';

function ViewModel(mode,
    chatHub,
    isProactiveChatEnabled,
    isSessionStarted,
    customerId,
    historyId,
    mediaSupport,
    appearance,
    windowMessageHandler)
  {
    var self = this;

    this.debugMode = false;
    this.windowMode = mode;
    this.iframeMode = mode === Enums.WindowMode.iframe;
    this.popoutMode = mode === Enums.WindowMode.popout;
    this.customerId = customerId;
    this.sessionId = 0;

    this.isMinimized = ko.observable();
    this.dialog = ko.observable(Dialogs.loading);

    this.rtcChannel = null;

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

    this.startChatDialog = new StartChatDialogViewModel(this, chatHub, ko);
    this.startChatDialog.errors = ko.validation.group(this.startChatDialog);
    this.chatDialog = new ChatDialogViewModel(this, chatHub, ko);
    this.offlineMessageSentDialog = new OfflineMessageSentDialogViewModel(this);
    this.editVisitorInfoDialog = new EditVisitorInfoDialogViewModel(this, chatHub);
    this.editVisitorInfoDialog.errors = ko.validation.group(this.editVisitorInfoDialog);
    this.transcriptProposalDialog = new TranscriptProposalDialogViewModel(this, chatHub);

    this.connectionState = ko.observable(Enums.ConnectionState.initial);

    this.minimizedTitleText = ko.observable(
      appearance.minimizedTitleText.value
      ? appearance.minimizedTitleText.value
      : Strings.windowTitleOfflineMessage);

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
        switch (self.connectionState())
        {
        case Enums.ConnectionState.disconnected:
        case Enums.ConnectionState.reconnecting:
          return 'disconnected';
        default:
          return '';
        }
      });

    this.agentStorage = new AgentStorage();

    // connection events
    chatHub.onDisconnected(
      function ()
      {
        self.connectionState(Enums.ConnectionState.disconnected);
      });
    chatHub.onConnected(
      function ()
      {
        self.connectionState(Enums.ConnectionState.connected);
      });
    chatHub.onConnecting(
      function ()
      {
        self.connectionState(Enums.ConnectionState.reconnecting);
      });


    // chat events
    chatHub.onVisitorSessionCreated(
      function (sessionId, messages)
      {
        console.log('onVisitorSessionCreated', sessionId, messages);
        self.sessionId = sessionId;

        self.chatDialog.messages([]);
        self.chatDialog.addMessages(messages);
        self.chatDialog.isSessionClosed(false);
        self.chatDialog.textDisabled(false);
        self.dialog(Dialogs.chat);
        $(window).resize();
      });
    chatHub.onVisitorMessage(
      function (messages)
      {
        self.chatDialog.addMessages(messages);
      });
    chatHub.onVisitorClosedSession(
      function ()
      {
        if (self.dialog() === Dialogs.chat)
          self.dialog(Dialogs.startChat);
      });

    // model events
    var isOpened = false;

    windowMessageHandler.onHeaderClick(function ()
    {
      $('#collapsed-state .chat-title-bar').click();
    }.bind(this));

    this.onClickTitleBar = function ()
    {
      var minimized = !this.isMinimized();
      this.isMinimized(minimized);
      this.adjustIframeSize();

      if (minimized)
        windowMessageHandler.postSetPosition(appearance.positioning);

      if (!minimized && !isOpened)
        this.connect();
    };

    this.onClickPopoutButton = function (model, event)
    {
      event.stopPropagation();

      var nw = window.open(PopoutWindowUrl + customerId, PopoutWindowName, PopoutWindowOptions);
      if (nw && typeof (nw.focus) === 'function') nw.focus();

      this.isMinimized(true);
      this.adjustIframeSize();
    };


    // methods
    this.adjustIframeSize = function ()
    {
      var height = Metrics.titleHeight;
      var width = Metrics.dialogWidth;

      if (!this.isMinimized())
      {
        height += Metrics.dialogHeight;

        if (this.isPoweredByVisible())
          height += Metrics.poweredByHeight;
      }
        else
            {
                var bar = $("#collapsed-state .chat-title-bar");

          if (bar.get(0).style.width) {
             width = bar.width(); 
          } else {
              var titlePosRight = $(".title-text").css("right"),
                  titlePaddingRight = $(".title-text").css("padding-right"),
                  titlePaddingLeft = $(".title-text").css("padding-left"),
                  titleMarginRight = $(".title-text").css("margin-right"),
                  titleMarginLeft = $(".title-text").css("margin-left"),
                  titleWidth = $(".title-text").width() + "px";                  
                  var barNewWidth = parseInt(titlePosRight) + parseInt(titlePaddingRight) + parseInt(titlePaddingLeft) + parseInt(titleWidth) + parseInt(titleMarginRight) + parseInt(titleMarginLeft) + parseInt(titleMarginRight) + "px";  
                  
              bar.css("width", barNewWidth);     
              width = bar.width();
          }
              

          if (bar.get(0).style.height)
              height = bar.height();
            }
      
        if ($(".chat-title-img").css('display') !== 'none')
      {
          var elem = $(".chat-title-img");
        width = Math.max(width, elem.width());
        height = Math.max(height, elem.height());
      }

      windowMessageHandler.postShow(width, height, this.isMinimized());
    };

    this.connect = function ()
    {
      this.dialog(Dialogs.loading);
      chatHub.chatWindowOpen(mediaSupport, window.location.hash)
        .then(function (chatWindowOpenResult)
        {
          self.agentStorage.addList(chatWindowOpenResult.Agents);
          self.startChatDialog.setData(chatWindowOpenResult);

          if (chatWindowOpenResult.HasActiveSession)
          {
            console.log('connect', chatWindowOpenResult);
            self.sessionId = chatWindowOpenResult.Session.Skey;

            if (chatWindowOpenResult.Session.Agents.length > 1)
            {
              //var sessionAgentSkeys = _.map(chatWindowOpenResult.Session.Agents, function (x) { return x.AgentSkey; });
              //self.chatDialog.agents(self.agentStorage.getList(sessionAgentSkeys));
              self.chatDialog.updateSessionAgents(chatWindowOpenResult.Session.Agents);
            }

            self.chatDialog.addMessages(chatWindowOpenResult.SessionMessages);

            self.chatDialog.isSessionClosed(false);
            self.chatDialog.textDisabled(false);
            self.dialog(Dialogs.chat);

            $(window).resize();

            var mediaCallMode = self.mediaCallMode();
            if (mediaCallMode.isDefined &&
              chatWindowOpenResult.Session &&
              chatWindowOpenResult.Session.MediaCallAgentConnectionId)
            {
              self.chatDialog.mediaCallAgentConnectionId = chatWindowOpenResult.Session.MediaCallAgentConnectionId;

              if (mediaCallMode.wasCallAccepted)
              {
                chatHub.mediaCallProposalAccepted(mediaCallMode.isVisitorAcceptedWithVideo)
                  .then(function ()
                  {
                    window.location.hash = self
                      .buildMediaCallModeHash(mediaCallMode.isAgentProposedVideo,
                        mediaCallMode.isVisitorAcceptedWithVideo);
                    self.chatDialog.receiveMediaCall(mediaCallMode.isAgentProposedVideo,
                      mediaCallMode.isVisitorAcceptedWithVideo);
                  });
              } else
              {
                self.chatDialog.receiveMediaCall(mediaCallMode.isAgentProposedVideo,
                  mediaCallMode.isVisitorAcceptedWithVideo);
              }
            }

            if (!mediaCallMode.isDefined && hasMediaCallProposal(chatWindowOpenResult.Session))
              self.chatDialog.showMediaCallProposal(chatWindowOpenResult.Session);

            self.chatDialog.scrollToLastMessage();
          } else
            self.dialog(Dialogs.startChat);
          isOpened = true;
        });
    }

    this.initialize = function ()
    {
      var isMinimized = !isSessionStarted && this.iframeMode;
      this.isMinimized(isMinimized);

      windowMessageHandler.postSetPosition(appearance.positioning);

      if (!isMinimized || isProactiveChatEnabled) this.connect();
      this.adjustIframeSize();
    }

    this.buildMediaCallModeHash = function (isAgentProposedVideo, isVisitorAcceptedWithVideo, wasCallAccepted)
    {
      return '#' +
        (isAgentProposedVideo ? 'v' : 'a') +
        (isVisitorAcceptedWithVideo ? 'v' : 'a') +
        (wasCallAccepted ? 'a' : 's');
    }

    this.mediaCallMode = function ()
    {
      var h = window.location.hash;
      return {
          isDefined: h.length === 4,
          isAgentProposedVideo: h.substr(1, 1) === 'v',
          isVisitorAcceptedWithVideo: h.substr(2, 1) === 'v',
          wasCallAccepted: h.substr(3, 1) === 'a'
        };
    };

    function hasMediaCallProposal(sessionInfo)
    {
      return sessionInfo && sessionInfo.MediaCallStatus === Enums.MediaCallStatus.ProposedByAgent;
    }
  }