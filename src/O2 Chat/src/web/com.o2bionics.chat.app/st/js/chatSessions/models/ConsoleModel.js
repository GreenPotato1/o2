'use strict';

function ConsoleModel(hub, mediaSupport, pageTrackerUrl)
{
  var self = this;

  this.hub = hub;
  this.pageTrackerUrl = pageTrackerUrl;
  this.settings = null;

  this.currentSession = ko.observable();
  this.newSessionsUnreadCount = ko.observable(0);
  this.activeSessionsUnreadCount = ko.observable(0);

  this.agent = ko.observable();
  this.agents = ko.observableArray();
  this.departments = ko.observableArray();

  this.agentStatusModel = new AgentStatusModel(this);

  this.visibleAgents = ko.pureComputed(
    function () { return _.filter(self.agents(), function (x) { return !x.isDeleted(); }); },
    this);

  this.visibleDepartments = ko.pureComputed(
    function () { return _.filter(self.departments(), function (x) { return !x.isDeleted(); }); },
    this);
 
  this.sessions = ko.observableArray([]);
  this.newSessions = ko.pureComputed(
    function () { return _.filter(self.sessions(), function (x) { return x.isNewSession() && !x.isCompleted(); }); },
    this);
  this.activeSessions = ko.pureComputed(
    function () { return _.filter(self.sessions(), function (x) { return !x.isNewSession() && !x.isCompleted(); }); },
    this);

  this.visitorStorage = new VisitorStorage();
  this.agentStorage = new AgentStorage();
  this.departmentStorage = new DepartmentStorage();
  this.cannedMessageStorage = new CannedMessageStorage();

  this.sendToAgentsOnly = ko.observable(false);
  this.messageText = ko.observable('');
  this.messageTextDisable = ko.observable(false);

 

  this.mediaSupport = mediaSupport;
  this.mediaSupportVideo = this.mediaSupport >= Enums.MediaSupport.Video;
  this.mediaSupportAudio = this.mediaSupport >= Enums.MediaSupport.Audio;

  this.rtcChannel = ko.observable(null);
  this.mediaCallSession = ko.observable(null);
  this.isMediaCallHasAgentVideo = ko.observable(null);
  this.mediaCallTimeTillTimeout = ko.observable(null);
  this.mediaCallTimeoutMoment = null;
  this.mediaCallTimeoutReason = null;
  this.mediaCallUpdateTimer = null;

  this.isMediaCallInProgress =
    ko.pureComputed(function () { return self.mediaCallSession() != null }, this)
    .subscribe(function (newValue)
    {      
      var elt = $('#media-call');
      if (newValue)
        elt.show();
      else
        elt.hide();
    });
  this.isMediaCallAudioPaused = ko.observable(false);
  this.isMediaCallVideoPaused = ko.observable(false);

  this.mediaCallStatusText = ko.pureComputed(
    function ()
    {
      var mcs = self.mediaCallSession();
      if (!mcs) return '';
      switch (mcs.sessionInfo().MediaCallStatus)
      {
      case Enums.MediaCallStatus.ProposedByAgent:
        return 'Call was proposed by agent.';
      case Enums.MediaCallStatus.AcceptedByVisitor:
        return 'Call was accepted by the visitor.';
      case Enums.MediaCallStatus.Established:
        return 'Connection is established.';
      default:
        return '';
      }
    },
    this);

  this.enterPressHandler = function () {
    sendMessage(model);
  }

  this.dialogSmileModel = new DialogSmileModel();
  this.emojiAreaModel = new EmojiAreaModel('#message-reply', self.enterPressHandler);

  this.smileModelShow = function ()
  {
    this.dialogSmileModel.open(
      function (emoji)
      {
        var smileIcon = document.createElement('img');
        smileIcon.src = emoji.image;
        smileIcon.alt = emoji.text;

        self.emojiAreaModel.insertValueAtCursorPosition(smileIcon);
      });
  };
  
  this.dialogStartSessionModel = new DialogStartSessionModel();
  this.dialogSessionInviteModel = new DialogSessionInviteModel();
  this.dialogSessionTranscriptWasntSentModel = new DialogSessionTranscriptWasntSentModel();
  this.sessionActionsPanel = new PanelModel('#session-actions-panel');
  this.sessionInfoPanel = new PanelModel('#session-panel-info');
  this.dialogEditCannedMessage = new DialogEditCannedMessageModel();
  this.dialogCannedMessages = new DialogCannedMessagesModel(
    Enums.CannedMessageType.Personal,
    this.dialogEditCannedMessage);
  this.cannedMessageCursorPosition = 0;
  this.selection = null;

  this.dialogCannedMessagesShow = function ()
  {
    self.cannedMessageStorage.loadUserMessages(
      function ()
      {
        self.dialogCannedMessages.open(
          null,
          self.cannedMessageStorage,
          function (cm)
          {
            if (cm)
            {
              var cannedMessage = cm.value();

              self.emojiAreaModel.insertValueAtCursorPosition(cannedMessage);

              return;
            }
          });
      });
  }
 
 
  // hub events
  this.hub.onSessionUpdate(
    function (reason, sessionInfo, messagesInfo, visitorInfo)
    {
      var html;
      switch (reason)
      {
      case 'VisitorSessionCreated':
        var deptName = self.departmentStorage.get(sessionInfo.Invites[0].DepartmentId).name();

        html = '<p>Department: ' +
          escapeHtml(deptName) +
          '</p>' +
          '<p>' +
          escapeHtml(messagesInfo[0].Text) +
          '</p>' +
          '<div class="button" onclick="model.setCurrentSessionBySkey(' +
          sessionInfo.Skey +
          ', this); return false;">нажмите здесь, чтобы активировать</div>';
        showAlert('New Session', html, 'chat.visitor.session.new');
        break;
      case 'VisitorMessage':
        _.forEach(
          messagesInfo,
          function (x)
          {
            html = '<p>' +
              escapeHtml(x.Text) +
              '</p>' +
              ' <div class="button" onclick="model.setCurrentSessionBySkey(' +
              sessionInfo.Skey +
              ', this); return false;">нажмите здесь, чтобы активировать</div>';
            showAlert(x.SenderAgentName, html, 'chat.visitor.message.new');
          });
        break;
      case 'AgentClosedSession':
        _.forEach(
          messagesInfo,
          function (x)
          {
            html = '<p>' +
              escapeHtml(x.Text) +
              '</p>' +
              ' <div class="button" onclick="model.setCurrentSessionBySkey(' +
              sessionInfo.Skey +
              ', this); return false;">нажмите здесь, чтобы активировать</div>';
            showAlert(x.SenderAgentName, html, 'chat.visitor.session.ended');
          });
        break;
      }

      addOrUpdateSession(this, sessionInfo.Skey, sessionInfo, messagesInfo, visitorInfo);
    }.bind(this));
  this.hub.onSessionTranscriptSentToVisitor(
    function (sessionInfo, messagesInfo)
    {
      addOrUpdateSession(this, sessionInfo.Skey, sessionInfo, messagesInfo);
    }.bind(this));
  this.hub.onVisitorInfoChanged(
    function (visitorId, wasRemoved, newName, newEmail, newPhone, newTranscriptMode, sessionSkey, messages)
    {
        console.log('visitorInfoChanged', visitorId, wasRemoved, newName, newEmail, newPhone, newTranscriptMode, sessionSkey, messages);
      var visitor = self.visitorStorage.get(visitorId);
      if (visitor != null)
          visitor.updateInfo2(wasRemoved, newName, newEmail, newPhone, newTranscriptMode);
      addOrUpdateSession(this, sessionSkey, null, messages);
    }.bind(this));
  this.hub.onMediaCallProposal(
    function (sessionInfo, messagesInfo)
    {
      addOrUpdateSession(this, sessionInfo.Skey, sessionInfo, messagesInfo);
    }.bind(this));
  this.hub.onVisitorRejectedMediaCallProposal(
    function (sessionInfo, messagesInfo)
    {
      self.mediaCallClearTimeout();
      var mcs = self.mediaCallSession();
      if (mcs && mcs.skey === sessionInfo.Skey)
      {
        self.mediaCallSession(null);
        self.isMediaCallHasAgentVideo(null);
      }
      addOrUpdateSession(this, sessionInfo.Skey, sessionInfo, messagesInfo);
    }.bind(this));
  this.hub.onVisitorAcceptedMediaCallProposal(
    function (sessionInfo, messagesInfo)
    {
      console.log('onVisitorAcceptedMediaCallProposal', sessionInfo);

      self.mediaCallScheduleTimeout(
        moment().add(self.settings.mediaCallConnectTimeoutMs, 'ms'),
        'Media call was cancelled because of connect timeout.');

      addOrUpdateSession(this, sessionInfo.Skey, sessionInfo, messagesInfo);
    }.bind(this));
  this.hub.onAgentStoppedMediaCall(
    function (sessionInfo, messagesInfo)
    {
      addOrUpdateSession(this, sessionInfo.Skey, sessionInfo, messagesInfo);
    }.bind(this));
  this.hub.onVisitorStoppedMediaCall(
    function (sessionInfo, messagesInfo)
    {
      self.mediaCallClearTimeout();
      addOrUpdateSession(this, sessionInfo.Skey, sessionInfo, messagesInfo);

      self.mediaCallSession(null);
      self.isMediaCallHasAgentVideo(null);

      var rtcChannel = self.rtcChannel();
      if (rtcChannel)
      {
        rtcChannel.dispose();
        self.rtcChannel(null);
      }
      $('#session-flow-container').find('#video-call-container').hide();
    }.bind(this));
  this.hub.onMediaCallVisitorConnectionIdSet(
    function (sessionInfo)
    {
      console.log('onMediaCallVisitorConnectionIdSet', sessionInfo);

      addOrUpdateSession(this, sessionInfo.Skey, sessionInfo);

      var mcs = self.mediaCallSession();
      if (mcs && mcs.skey !== sessionInfo.Skey) return;

      var rtcChannel = self.rtcChannel();
      if (rtcChannel)
      {
        rtcChannel.dispose();
        self.rtcChannel(null);
      }

      self.isMediaCallAudioPaused(false);
      self.isMediaCallVideoPaused(false);

      if (sessionInfo.MediaCallVisitorHasVideo === true)
        $('#session-flow-container').find('#video-call-container').show();
      self.scrollToLastMessage();

      rtcChannel = new RtcChannel(
        self.hub,
        sessionInfo.MediaCallVisitorConnectionId,
        document.getElementById('localVideo'),
        document.getElementById('remoteVideo'));

      rtcChannel.onConnected(function ()
      {
        console.log('channel.onConnected called');
        self.mediaCallClearTimeout();
      }.bind(this));

      rtcChannel.initiateCall(self.isMediaCallHasAgentVideo())
        .then(function ()
        {
          self.rtcChannel(rtcChannel);
        }.bind(this))
        .fail(function (reason)
        {
          self.hub.mediaCallStop(sessionInfo.Skey, reason);
          self.mediaCallSession(null);
          self.isMediaCallHasAgentVideo(null);
          rtcChannel.dispose();
          rtcChannel = null;
        }.bind(this));
    }.bind(this));
  hub.onDepartmentStateChanged(
    function (onlineStatusInfo)
    {
      console.log(onlineStatusInfo);

      _.each(onlineStatusInfo,
        function (x)
        {
          var d = self.departmentStorage.get(x.Id);
          if (d)
          {
            console.log(d.name(), d);
            d.updateOnlineStatus(x.IsOnline);
          }
          else
            console.log('unknown department ' + onlineStatusInfo.Id);
        }.bind(this));
    }.bind(this));
  hub.onDepartmentCreated(
   function (departmentInfo)
   {
     var d = new DepartmentModel(departmentInfo);
     self.departmentStorage.add(d);
     self.departments.push(d);
   }.bind(this));
  hub.onDepartmentUpdated(
    function (departmentInfo)
    {
      console.log(departmentInfo);
      var d = self.departmentStorage.get(departmentInfo.Id);
      if (d)
        d.update(departmentInfo);
      else
        self.departmentStorage.add(new DepartmentModel(departmentInfo));
    }.bind(this));
  hub.onDepartmentRemoved(
   function (departmentId)
   {
     self.departmentStorage.markDeleted(departmentId);
   }.bind(this));
  hub.onUserCreated(
    function (userInfo)
    {
      var agent = new AgentModel(userInfo);
      self.agentStorage.add(agent);
      self.agents.push(agent);
    }.bind(this));
  hub.onUserUpdated(
    function (userInfo)
    {
      console.log(userInfo);
      var agent = self.agentStorage.get(userInfo.Id);
      if (agent)
        agent.update(userInfo);
      else
        self.agentStorage.add(new AgentModel(userInfo));
    }.bind(this));
  hub.onUserRemoved(
    function (userId)
    {
      self.agentStorage.markDeleted(userId);
    }.bind(this));

  // model events
  this.onMessageTextKeyPress = function (model, e)
  {
    if (e.ctrlKey && (e.keyCode === 10 || e.keyCode === 13)) // ^enter
    {
      sendMessage(model);
      return false;
    }
    return true;
  }
  this.onClickSendButton = function (model)
  {
    //throw Error("Test.ErrorService.Workspace");
    sendMessage(model);
    return false;
  }

  this.canInvite = ko.pureComputed(function ()
    {
      var cs = self.currentSession();
      if (!cs) return false;
      return cs.isAgentParticipating(model.agent().id);
    },
    this);
  this.onClickInvite = function ()
  {
    if (!this.canInvite()) return;

    this.dialogSessionInviteModel.show(
      this.currentSession(),
      this.agents(),
      this.departments(),
      function (session, target, actOnBehalfOfInvitor, message)
      {
        console.log('adding invite:', session, target, actOnBehalfOfInvitor, message);
        if (target instanceof AgentModel)
          self.hub.inviteAgentToChatSession(session.skey, target.id, actOnBehalfOfInvitor, message);
        else if (target instanceof DepartmentModel)
          self.hub.inviteDepartmentToChatSession(session.skey, target.id, actOnBehalfOfInvitor, message);
        else console.log('invalid invite target', target);
      }.bind(this));
  }

  this.canSendTranscript = ko.pureComputed(function ()
    {
      var cs = self.currentSession();
      if (!cs) return false;
      if (!cs.visitor) return false;
      if (!cs.visitor.email()) return false;
      return cs.isAgentParticipating(model.agent().id);
    },
    this);
  this.onClickSendTranscript = function ()
  {
    if (!this.canSendTranscript()) return;

    var cs = this.currentSession();
    var timeZoneOffset = cs.visitor.timeZoneOffset();
    this.hub.sendTranscriptToVisitor(cs.skey, timeZoneOffset ? timeZoneOffset : 0);
  }

  this.canExitSession = ko.pureComputed(function ()
    {
      var cs = self.currentSession();
      if (!cs) return false;
      if (!cs.isAgentParticipating(model.agent().id)) return false;
      var mcs = self.mediaCallSession();
      if (mcs != null && mcs.skey === cs.skey) return false;
      return true;
    },
    this);
  this.onClickExitSession = function ()
  {
    if (!this.canExitSession()) return;

    this.hub.exitSession(this.currentSession().skey, '');
  }

  this.canCloseSession = ko.pureComputed(function ()
    {
      var cs = self.currentSession();
      if (!cs) return false;
      if (!cs.isAgentParticipating(model.agent().id)) return false;
      var mcs = self.mediaCallSession();
      if (mcs != null && mcs.skey === cs.skey) return false;
      return true;
    },
    this);
  this.onClickCloseSession = function ()
  {
    if (!this.canCloseSession()) return;

    var cs = this.currentSession();
    if (!cs.isOnline() && !cs.transcriptSentTime())
    {
        this.dialogSessionTranscriptWasntSentModel.show(
        function ()
        {
          var timeZoneOffset = cs.visitor.timeZoneOffset();
          self.hub.sendTranscriptToVisitor(cs.skey, timeZoneOffset ? timeZoneOffset : 0)
            .then(
              function ()
              {
                self.hub.closeSession(cs.skey, '');
                self.currentSession(null);
              }.bind(this));
        }.bind(this),
        function ()
        {
          self.hub.closeSession(cs.skey, '');
          self.currentSession(null);
        }.bind(this));
    }
    else
    {
      this.hub.closeSession(cs.skey, '');
      this.currentSession(null);
    }
  }

  this.onClickNewSession = function ()
  {
    this.dialogStartSessionModel.show(
      this.agents(),
      this.departments(),
      function (target, message)
      {
        console.log('new session', target, message);
        if (target instanceof AgentModel)
          self.hub.startChatSessionToAgent(target.id, message);
        else if (target instanceof DepartmentModel)
          self.hub.startChatSessionToDepartment(target.id, message);
        else console.log('invalid session target', target);
      }.bind(this));
  };
  
  this.onClickAcceptSession = function ()
  {
    this.acceptCurrentSession()
      .then(function ()
      {
        self.scrollToLastMessage();
        $('#active-sessions-tab-header').tab('show');
      }.bind(this));
  }

  this.canAcceptSessionWithVideo = ko.pureComputed(function ()
    {
      var cs = self.currentSession();
      if (!cs) return false;
      return self.mediaSupportVideo && cs.canAccept() && cs.visitorHasVideo();
    },
    this);
  this.onClickAcceptSessionWithVideo = function ()
  {
    var cs = this.currentSession();
    this.acceptCurrentSession()
      .then(function ()
      {
        self.scrollToLastMessage();

        $('#active-sessions-tab-header').tab('show');

        self.mediaCallSession(cs);
        self.isMediaCallHasAgentVideo(true);

        self.hub.mediaCallProposal(cs.skey, true);
      }.bind(this));
  }

  this.canStartVideoCall = ko.pureComputed(function ()
    {
      if (!self.mediaSupportVideo) return false;
      var mcs = self.mediaCallSession();
      if (mcs) return false;
      var cs = self.currentSession();
      return cs != null && cs.visitorHasVideo() && cs.isAgentParticipating(self.agent().id);
    },
    this);
  this.onClickStartVideoCall = function ()
  {
    if (!this.canStartVideoCall()) return;

    var cs = this.currentSession();
    this.hub.mediaCallProposal(cs.skey, true)
      .then(function ()
      {
        self.mediaCallSession(cs);
        self.isMediaCallHasAgentVideo(true);

        self.mediaCallScheduleTimeout(
          moment().add(self.settings.mediaCallProposalTimeoutMs, 'ms'),
          'Media call proposal was cancelled because of timeout.');
      }.bind(this));
  }
  this.mediaCallScheduleTimeout = function (m, reason)
  {
    console.debug('schedule timeout', m, reason);

    this.mediaCallClearTimeout();

    this.mediaCallTimeoutMoment = m;
    this.mediaCallTimeoutReason = reason;
    this.mediaCallUpdateTimer = window.setTimeout(this.mediaCallUpdate.bind(this), 500);
  }
  this.mediaCallClearTimeout = function ()
  {
    console.debug('clear timeout');

    if (this.mediaCallUpdateTimer)
    {
      window.clearTimeout(this.mediaCallUpdateTimer);
      this.mediaCallUpdateTimer = null;
    }
    this.mediaCallTimeoutMoment = null;
    this.mediaCallTimeoutReason = null;
    this.mediaCallTimeTillTimeout(null);
  }
  this.mediaCallUpdate = function ()
  {
    if (!this.mediaCallTimeoutMoment) return;

    var diff = this.mediaCallTimeoutMoment.diff(moment(), 's');
    if (diff < 0)
      this.mediaCallStop(this.mediaCallTimeoutReason);
    else
    {
      this.mediaCallTimeTillTimeout(diff + 's.');
      this.mediaCallUpdateTimer = window.setTimeout(this.mediaCallUpdate.bind(this), 500);
    }
  };

  this.canStartAudioCall = ko.pureComputed(function ()
    {
      if (!self.mediaSupportAudio) return false;
      var mcs = self.mediaCallSession();
      if (mcs) return false;
      var cs = self.currentSession();
      return cs != null && cs.visitorHasAudio() && cs.isAgentParticipating(self.agent().id);
    },
    this);
  this.onClickStartAudioCall = function ()
  {
    if (!this.canStartAudioCall()) return;

    var cs = this.currentSession();
    this.hub.mediaCallProposal(cs.skey, false)
      .then(function ()
      {
        self.mediaCallSession(cs);
        self.isMediaCallHasAgentVideo(false);

        self.mediaCallScheduleTimeout(
          moment().add(self.settings.mediaCallProposalTimeoutMs, 'ms'),
          'Media call proposal was cancelled because of timeout.');
      }.bind(this));
  }

  this.isNotInMediaCallSession = ko.pureComputed(function ()
  {
    var cs = self.currentSession();
    var mcs = self.mediaCallSession();
    return !cs || !mcs || cs.skey !== mcs.skey;
  });
  this.onClickMediaCallOpenSession = function ()
  {
    var mcs = this.mediaCallSession();
    if (!mcs) return;
    mcs.onSelectSession();
  };
  this.onClickMediaCallPauseAudio = function ()
  {
    var rtcChannel = this.rtcChannel();
    if (!rtcChannel) return;
    var newState = !this.isMediaCallAudioPaused();
    this.isMediaCallAudioPaused(newState);
    rtcChannel.pauseAudio(!newState);
  };
  this.onClickMediaCallPauseVideo = function ()
  {
    var rtcChannel = this.rtcChannel();
    if (!rtcChannel) return;
    var newState = !this.isMediaCallVideoPaused();
    this.isMediaCallVideoPaused(newState);
    rtcChannel.pauseVideo(!newState);
  };
  this.onClickMediaCallStop = function ()
  {
    this.mediaCallStop('Media call has been stopped by Agent ' + this.agent().name());
  };

  this.mediaCallStop = function (reason)
  {
    this.mediaCallClearTimeout();

    var mcs = this.mediaCallSession();
    if (mcs)
      this.hub.mediaCallStop(mcs.skey, reason);
    this.mediaCallSession(null);
    this.isMediaCallHasAgentVideo(null);

    var rtcChannel = this.rtcChannel();
    if (rtcChannel)
    {
      rtcChannel.dispose();
      this.rtcChannel(null);
    }
    $('#session-flow-container').find('#video-call-container').hide();
  }

  // methods
  this.setConsoleInfo = function (consoleInfo)
  {
    var model = this;

    this.customerId = consoleInfo.CustomerId;

    this.settings = {
        mediaCallProposalTimeoutMs: consoleInfo.Settings.MediaCallProposalTimeoutMs,
        mediaCallConnectTimeoutMs: consoleInfo.Settings.MediaCallConnectTimeoutMs
      };

    this.departmentStorage.addList(consoleInfo.Departments, consoleInfo.OnlineDepartments);
    this.agentStorage.addList(consoleInfo.Users, consoleInfo.OnlineAgents);
    this.visitorStorage.addList(consoleInfo.Visitors);

    var agent = this.agentStorage.get(consoleInfo.AgentId);
    agent.setDepartments(consoleInfo.AgentDepartments);
    this.agent(agent);

    this.agents(this.agentStorage.getAllBesides([this.agent().id]));
    this.departments(this.departmentStorage.getAll());

    this.sessions(_.map(consoleInfo.Sessions, function (x) { return new ChatSessionModel(x, model); }));
  };

  this.setCurrentSessionBySkey = function (skey, selectedElement)
  {
    var session = _.find(this.sessions(), { skey: skey });
    if (_.isUndefined(session)) return;
    
    session.onSelectSession();
    if ($(selectedElement).parents('.gritter-custom').length)
    {
      var nameClass = $(selectedElement).parents('.gritter-custom').get(0).id;
      $.gritter.remove(nameClass.substring(nameClass.lastIndexOf('-') + 1, nameClass.length));
    }

  }

  this.scrollToLastMessage = function ()
  {
    $('#messages-container').scrollTo($('#messages-container li:last'), 50);
  }

  this.acceptCurrentSession = function ()
  {
    var model = this;
    var cs = model.currentSession();
    if (cs == null) return $.Deferred().reject().promise();

    var agentInvite = cs.findAgentInvite(model.agent());
    if (agentInvite != null) return model.hub.acceptSessionAsAgent(cs.skey);

    var departmentInvite = cs.findDepartmentInvite(model.agent());
    if (departmentInvite != null) return model.hub.acceptSessionAsDepartment(cs.skey, departmentInvite.DepartmentId);

    return $.Deferred().reject().promise();
  }
  

  function sendMessage(model)
  {
    var cs = model.currentSession();
    if (!cs) return;

    var text = model.messageText();
    if (text.trim().length === 0) return;

    model.messageTextDisable(true);
    model.hub.sendMessage(cs.skey, model.sendToAgentsOnly(), text)
      .done(function ()
      {
        model.messageText('');
        model.messageTextDisable(false);

        $(self.emojiAreaModel.divContainerSelector).html('').focus();
      });
  }

  function addOrUpdateSession(model, sessionSkey, sessionInfo, messagesInfo, visitorInfo)
  {
    if (visitorInfo != null)
      model.visitorStorage.addOrUpdate(visitorInfo);

    var session = _.find(model.sessions(), { skey: sessionSkey });
    if (_.isUndefined(session))
    {
      if (!sessionInfo) return;
      session = addSession(model, sessionInfo);
    }
    session.update(sessionInfo, messagesInfo);

    var cs = model.currentSession();
    if (cs && cs.skey === session.skey)
    {
      // ??? activate session tab
      model.scrollToLastMessage();
    }
  }

  function addSession(model, sessionInfo)
  {
    var session = new ChatSessionModel(sessionInfo, model);
    model.sessions.push(session);

    if (session.isNewSession())
      model.newSessionsUnreadCount(model.newSessionsUnreadCount() + 1);
    else
      model.activeSessionsUnreadCount(model.activeSessionsUnreadCount() + 1);
    return session;
  }

  

  // initialization

  this.cannedMessageStorage.loadUserMessages();

  this.hub.getConsoleInfo()
    .done(function (consoleInfo)
    {
      console.log(consoleInfo);
      self.setConsoleInfo(consoleInfo);

      var sessions = self.sessions();
      var hash = _.trimStart(window.location.hash, '#');
      var session = _.find(sessions, function (s) { return ('' + s.skey) === hash; });
      if (!session) session = _.find(sessions, function (s) { return !s.isNewSession() && !s.isCompleted(); });
      if (!session) session = _.find(sessions, function (s) { return s.isNewSession(); });
      if (session)
        session.onSelectSession();
      else
        $('a#new-sessions-tab-header').tab('show');
    }.bind(this));
}