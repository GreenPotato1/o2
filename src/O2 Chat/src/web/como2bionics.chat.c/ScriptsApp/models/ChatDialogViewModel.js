'use strict';

function ChatMessage(id, sender, senderName, text, time, avatarUrl)
{
  this.id = id;
  this.time = time;
  this.sender = sender;
  this.senderName = senderName;
  this.lines = emotify(escapeHtmlLight(text)).split('\n');

  for (var i = 0; i < this.lines.length; i++)
    this.lines[i] = parseUrls(this.lines[i]);

  this.avatarUrl = avatarUrl;
}

function ChatDialogViewModel(parentModel, chatHub, ko)
{
  var self = this;

  this.pageUniqueId = parentModel.pageUniqueId;

  this.visitorIconClass = ko.pureComputed(
    function ()
    {
      var name = parentModel.startChatDialog.visitorName();
      return (name && name.length !== 0) ? 'known' : '';
    },
    this);

  this.visitorTitle = ko.pureComputed(
    function ()
    {
      var name = parentModel.startChatDialog.visitorName();
      var email = parentModel.startChatDialog.visitorEmail();
      return '' + name + ' <' + email + '>';
    },
    this);

  this.messages = ko.observableArray([]);
  this.text = ko.observable('');
  this.textFocus = ko.observable(true);
  this.textDisabled = ko.observable(false);

  this.mediaCallProposalHasVideo = ko.observable(true);
  this.mediaCallTypeText = ko.pureComputed(function () { return self.mediaCallProposalHasVideo() ? 'Video' : 'Voice'; },
    this);
  this.mediaAnswerType = ko.observable('');
  this.isCallNotificationVisible = ko.observable(false);
  this.mediaCallAgentConnectionId = null;

  this.isMediaCallAudioPaused = ko.observable(false);
  this.isMediaCallVideoPaused = ko.observable(false);

  this.isMediaCallVideoVisible = ko.observable(false);
  this.isMediaCallVisitorVideoVisible = ko.observable(false);
  this.isMediaControlsVisible = ko.observable(false);

  this.isSessionClosed = ko.observable(false);

  this.agents = ko.observableArray([]);

  // chat events
  chatHub.onChatEvent(
    function (sessionInfo, agent, messages)
    {
      parentModel.agentStorage.addOrUpdate(new AgentModel(agent));
      self.updateSessionAgents(sessionInfo.Agents);
      self.addMessages(messages);
    }.bind(this));

  chatHub.onAgentClosedSession(
    function (sessionInfo, agent, messages)
    {
      self.textDisabled(true);
      self.addMessages(messages);
      self.isSessionClosed(true);

      var transcriptMode = parentModel.startChatDialog.transcriptMode();
      if (transcriptMode === null) transcriptMode = Enums.VisitorSendTranscriptMode.Ask;
      switch (transcriptMode)
      {
      case Enums.VisitorSendTranscriptMode.Always:
        sendTranscript();
        break;
      case Enums.VisitorSendTranscriptMode.Ask:
        {
          parentModel.transcriptProposalDialog.show(
            function ()
            {
              sendTranscript();
            },
            function ()
            {
              parentModel.dialog(Dialogs.chat);
              self.scrollToLastMessage();
            });
        }
        break;
      }
    }.bind(this));

  function sendTranscript()
  {
    parentModel.dialog(Dialogs.loading);
    chatHub.sendTranscript(parentModel.sessionId, new Date().getTimezoneOffset())
      .done(function (r)
      {
        console.log('sendTranscript result', r);
        parentModel.dialog(Dialogs.chat);
        self.scrollToLastMessage();
      }.bind(this))
      .fail(function ()
      {
        parentModel.dialog(Dialogs.chat);
        self.scrollToLastMessage();
      }.bind(this));
  }
  

  chatHub.onVisitorRequestedTranscriptSent(
    function (sessionInfo, messages)
    {
      self.addMessages(messages);
      parentModel.dialog(Dialogs.chat);
      self.scrollToLastMessage();
    }.bind(this));

  chatHub.onMediaCallProposal(
    function (sessionInfo, agent, messages, hasVideo)
    {
      this.addMessages(messages);
      this.showMediaCallProposal(sessionInfo);
    }.bind(this));

  chatHub.onVisitorAcceptedMediaCallProposal(
    function ()
    {
      this.hideMediaCallProposal();
    }.bind(this));

  chatHub.onVisitorRejectedMediaCallProposal(
    function ()
    {
      this.hideMediaCallProposal();
    }.bind(this));

  chatHub.onAgentStoppedMediaCall(
    function (sessionInfo, agent, messages)
    {
      this.addMessages(messages);

      this.hideMediaCallProposal();
      this.hideMediaCallVideo();
      this.hideMediaCallControls();
      if (parentModel.rtcChannel)
      {
        parentModel.rtcChannel.dispose();
        parentModel.rtcChannel = null;
      }
      if (parentModel.popoutMode)
        window.location.hash = '';
    }.bind(this));


  // model events
  this.onClickEnterVisitorInfoBanner = function ()
  {
    parentModel.editVisitorInfoDialog.show(
      Strings.saveAndReturnToChat,
      Strings.messageEnterMissingVisitorDetailsChat,
      function ()
      {
        parentModel.dialog(Dialogs.chat);
        $(window).resize();
        self.scrollToLastMessage();
      }.bind(this),
      function ()
      {
        parentModel.dialog(Dialogs.chat);
        $(window).resize();
        self.scrollToLastMessage();
      }.bind(this));
  }

  this.enterSend = function (model, e)
  {
    if (e.ctrlKey && (e.keyCode === 10 || e.keyCode === 13)) // ^enter
    {
      this.send();
      return false;
    }
    return true;
  }

  this.send = function ()
  {
    //throw Error("Test.ErrorService.Widget");
    var text = this.text();
    if (text.length === 0) return;
   
    this.textDisabled(true);
    chatHub.sendMessage(text)
      .done(function ()
      {
        this.text('');
        this.textDisabled(false);
        this.textFocus(true);
        this.scrollToLastMessage();
      }.bind(this))
      .fail(function ()
      {
        this.textDisabled(false);
        this.textFocus(true);
      }.bind(this));
  }

  this.back = function ()
  {
    chatHub.endChat()
      .done(function ()
      {
        parentModel.dialog(Dialogs.startChat);
      });
  }

  this.onMediaCallAcceptedClick = function ()
  {
    console.log('onMediaCallAcceptedClick');

    this.hideMediaCallProposal();

    var mediaAnswerType = this.mediaAnswerType();

    if (mediaAnswerType === 'text')
      chatHub.mediaCallProposalRejected();
    else
    {
      var isAgentProposedVideo = this.mediaCallProposalHasVideo();
      var isVisitorAcceptedWithVideo = mediaAnswerType === 'video';

      if (parentModel.windowMode === Enums.WindowMode.popout)
      {
        chatHub.mediaCallProposalAccepted(isVisitorAcceptedWithVideo)
          .then(function ()
          {
            window.location.hash = parentModel.buildMediaCallModeHash(isAgentProposedVideo, isVisitorAcceptedWithVideo);
            this.receiveMediaCall(isAgentProposedVideo, isVisitorAcceptedWithVideo);
          }.bind(this));
      } else
      {
        var modeHash = parentModel.buildMediaCallModeHash(isAgentProposedVideo, isVisitorAcceptedWithVideo, true);
        var nw = window.open(PopoutWindowUrl + parentModel.customerId + modeHash,
          PopoutWindowName,
          PopoutWindowOptions);
        if (nw && typeof (nw.focus) === 'function') nw.focus();
      }
    }
  }

  this.receiveMediaCall = function (isAgentProposedVideo, isVisitorAcceptedWithVideo)
  {
    this.isMediaCallAudioPaused(false);
    this.isMediaCallVideoPaused(false);

    this.showMediaCallControls();
    if (isAgentProposedVideo)
      this.showMediaCallVideo();
    this.isMediaCallVisitorVideoVisible(isVisitorAcceptedWithVideo);

    var rtcChannel = new RtcChannel(
      chatHub,
      this.mediaCallAgentConnectionId,
      document.getElementById('video-local'),
      document.getElementById('video-remote'));

    rtcChannel.receiveCall(isAgentProposedVideo, isVisitorAcceptedWithVideo)
      .then(function ()
      {
        chatHub.mediaCallSetConnectionId()
          .then(function ()
          {
            parentModel.rtcChannel = rtcChannel;
          }.bind(this));
      }.bind(this))
      .fail(function (reason)
      {
        chatHub.mediaCallStop(reason);

        this.hideMediaCallControls();
        this.hideMediaCallVideo();

        window.location.hash = '';

        rtcChannel.dispose();
        rtcChannel = null;
      }.bind(this));
  }

  this.onClickMediaCallPauseAudio = function ()
  {
    var rtcChannel = parentModel.rtcChannel;
    if (!rtcChannel) return;
    var newState = !this.isMediaCallAudioPaused();
    this.isMediaCallAudioPaused(newState);
    rtcChannel.pauseAudio(!newState);
  }
  this.onClickMediaCallPauseVideo = function ()
  {
    var rtcChannel = parentModel.rtcChannel;
    if (!rtcChannel) return;
    var newState = !this.isMediaCallVideoPaused();
    this.isMediaCallVideoPaused(newState);
    rtcChannel.pauseVideo(!newState);
  }
  this.onClickMediaCallStop = function ()
  {
    chatHub.mediaCallStop('Media call has been stopped by the Visitor.')
      .then(function ()
      {
        this.hideMediaCallProposal();
        this.hideMediaCallVideo();
        this.hideMediaCallControls();

        var rtcChannel = parentModel.rtcChannel;
        if (rtcChannel)
        {
          rtcChannel.dispose();
          parentModel.rtcChannel = null;
        };

        if (parentModel.popoutMode)
          window.location.hash = '';
      }.bind(this));
  }


  // TODO: remove
  this.onClickT1 = function ()
  {
    if (this.isMediaControlsVisible())
    {
      this.isMediaControlsVisible(false);
      console.log('change', document.getElementById('rtc-call-controls').style.display);
      $(window).resize();
    } else
    {
      this.isMediaControlsVisible(true);
      console.log('change', document.getElementById('rtc-call-controls').style.display);
      $(window).resize();
    }
  }
  // TODO: remove
  this.onClickT2 = function ()
  {
    if (this.isMediaCallVideoVisible())
    {
      this.isMediaCallVideoVisible(false);
      $(window).resize();
      this.scrollToLastMessage();
    } else
    {
      this.isMediaCallVideoVisible(true);
      $(window).resize();
      this.scrollToLastMessage();
    }
  }


  this.scrollToLastMessage = function ()
  {
    var last = $('#message-flow-end');
    if (last.length > 0)
      $('#chat-container').scrollTo(last, 100);
  }

  this.addMessages = function (messages)
  {
    console.log('add messages', messages);
    var model = this;
    $.each(messages,
      function (_, x)
      {
        var mm = model.messages();
        var pm = ko.utils.arrayFirst(mm, function (y) { return y.id === x.Id });
        if (pm == null)
        {
          var sender = '';
          var avatarUrl = '';
          switch (x.Sender)
          {
          case Enums.ChatMessageSender.Agent:
            sender = 'other';
            if (parentModel.chatDialog.agents().length > 1)
            {
              var agent = parentModel.agentStorage.get(x.SenderAgentSkey);
              if (agent)
                avatarUrl = agent.avatarUrl();
            }
            break;
          case Enums.ChatMessageSender.Visitor:
            sender = 'self';
            break;
          case Enums.ChatMessageSender.System:
            sender = 'system';
            break;
          }
          var senderName = x.Sender === Enums.ChatMessageSender.Agent ? x.SenderAgentName : null;

          model.messages.push(
            new ChatMessage(
              x.Id,
              sender,
              senderName,
              x.Text,
              new Date(x.TimestampUtc),
              avatarUrl));
        }
      });
    this.scrollToLastMessage();
  }

  this.reset = function ()
  {
    this.messages.removeAll();
    this.text('');
    this.textFocus(true);
  }

  this.showMediaCallProposal = function (sessionInfo)
  {
    this.mediaCallAgentConnectionId = sessionInfo.MediaCallAgentConnectionId;

    this.mediaCallProposalHasVideo(sessionInfo.MediaCallAgentHasVideo === true);
    this.mediaAnswerType('');
    this.isCallNotificationVisible(true);
    $(window).resize();
  }
  this.hideMediaCallProposal = function ()
  {
    this.isCallNotificationVisible(false);
    $(window).resize();
  }

  this.showMediaCallVideo = function ()
  {
    this.isMediaCallVideoVisible(true);
    $(window).resize();
  }
  this.hideMediaCallVideo = function ()
  {
    this.isMediaCallVideoVisible(false);
    $(window).resize();
  }

  this.showMediaCallControls = function ()
  {
    this.isMediaControlsVisible(true);
    $(window).resize();
  }
  this.hideMediaCallControls = function ()
  {
    this.isMediaControlsVisible(false);
    $(window).resize();
  }

  this.updateSessionAgents = function (sessionAgents)
  {
    var sessionAgentSkeys = _.map(sessionAgents, function (x) { return x.AgentSkey; });
    self.agents(parentModel.agentStorage.getList(sessionAgentSkeys));
  }
}