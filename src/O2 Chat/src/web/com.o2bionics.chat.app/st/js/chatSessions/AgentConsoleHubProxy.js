
var AgentConsoleHubProxy = function (agentSessionGuid)
{
  var hubPath = '';
  var hubName = 'agentConsoleHub';
  var reconnectInterval = 30000;

  var hub = createHub(hubPath, hubName);
  var wasConnectionIdSet = false;
  var reconnectAttempt = 0;
  var isReconnectScheduled = false;
  var isClosing = false;

  var events = new utils.Events();

  // at least one subscription must be added before connect to receive events
  hub.on('ping', function () {});

  hub.connection.starting(
    function ()
    {
      console.log('hub.connection: starting');
      callConnectingHandler();
    });
  hub.connection.connectionSlow(
    function ()
    {
      console.log('hub.connection: connectionSlow');
    });
  hub.connection.reconnecting(
    function ()
    {
      console.log('hub.connection: reconnecting');
      callConnectingHandler();
    });
  hub.connection.reconnected(
    function ()
    {
      console.log('hub.connection: reconnected');
      callConnectedHandler();
    });
  hub.connection.disconnected(
    function ()
    {
      console.log('hub.connection: disconnected');
      if (!isClosing) callDisconnectHandler();
      if (!isReconnectScheduled && !isClosing)
      {
        console.log('hub.connection: scheduling reconnection in ' + reconnectInterval + 'ms.');
        setTimeout(tryReconnect, reconnectInterval);
        isReconnectScheduled = true;
      }
    });

  $(window)
    .unload(
      function ()
      {
        isClosing = true;
        if (hub.connection.state !== $.signalR.connectionState.disconnected)
          hub.connection.stop(false, true);
      });

  function createHub(hubPath, hubName, customerId)
  {
    var connection = $.hubConnection(hubPath);
    connection.logging = true;
    connection.log = function (m)
    {
      console.debug('[' + new Date().toLogTimestampString() + '] SignalR:', m);
    }
    connection.qs = connection.qs || {};
    connection.qs.cid = customerId;
    connection.qs.asgi = agentSessionGuid;
    return connection.createHubProxy(hubName);
  }

  function invoke()
  {
    var args = Array.prototype.slice.call(arguments);
    var connectionState = hub.connection.state;
    return hub.connection.start()
      .then(
        function ()
        {
          if (connectionState !== $.signalR.connectionState.connected) callConnectedHandler();
          if (!wasConnectionIdSet)
          {
            hub.connection.qs.coid = hub.connection.id;
            wasConnectionIdSet = true;
          }
          return hub.invoke.apply(hub, args)
            .fail(function (x) { console.log('hub.connection: method ' + args[0] + ' call error: ' + x); });
        },
        function (x)
        {
          console.log('hub.connection: connection start error: ' + x);
        });
  }

  function call(method, action, data)
  {
    var connectionState = hub.connection.state;
    return hub.connection.start().then(
      function ()
      {
        if (connectionState !== $.signalR.connectionState.connected) callConnectedHandler();
        if (!wasConnectionIdSet)
        {
          hub.connection.qs.coid = hub.connection.id;
          wasConnectionIdSet = true;
        }
        if (null == data) data = {};
        data.agentSessionId = agentSessionGuid;
        data.connectionId = hub.connection.id;
        return $.ajax(
            {
              method: method,
              url: '/Session/' + action,
              datatype: 'json',
              contentType: 'application/json; charset=utf-8',
              data: method === 'POST' ? JSON.stringify(data) : data
            })
          .fail(function (data) { console.error(data); });
      },
      function (x) { console.log('hub.connection: connection start error: ' + x); });
  }

  function get(action, data)
  {
    return call('GET', action, data);
  }

  function post(action, data)
  {
    return call('POST', action, data);
  }

  function tryReconnect()
  {
    var n = reconnectAttempt++;
    console.log('hub.connection: trying to reconnect after disconnect, attempt ' + n);
    callConnectingHandler();
    hub.connection.start()
      .then(
        function ()
        {
          console.log('hub.connection: reconnected successfully!');
          callConnectedHandler();
          isReconnectScheduled = false;
        },
        function (x)
        {
          console.log(
            'hub.connection: reconnect attempt '
            + n
            + ' failure: '
            + x
            + '; scheduling reconnection in '
            + reconnectInterval
            + 'ms.');
          setTimeout(tryReconnect, reconnectInterval);
        });
  }

  // connection events
  this.onDisconnected = function (callback)
  {
    /// <signature><param name="callback()" type="Function">Callback.</param></signature>
    return events.on('disconnect', callback);
  };

  function callDisconnectHandler()
  {
    events.emit('disconnect');
  }

  this.onConnected = function (callback)
  {
    /// <signature><param name="callback()" type="Function">Callback.</param></signature>
    return events.on('connected', callback);
  };

  function callConnectedHandler()
  {
    events.emit('connected');
  }

  this.onConnecting = function (callback)
  {
    /// <signature><param name="callback()" type="Function">Callback.</param></signature>
    return events.on('connecting', callback);
  };

  function callConnectingHandler()
  {
    events.emit('connecting');
  }

  // hub events
  function handleHubEvent(signalName, eventName)
  {
    hub.on(
      signalName,
      function () { events.emit(eventName ? eventName : signalName, Array.prototype.slice.call(arguments)); });
  }

  handleHubEvent('AgentStateChanged');
  this.onAgentStateChanged = function (callback)
  {
    /// <signature><param name="callback(OnlineStatusInfo)" type="Function">Callback.</param></signature>
    return events.on('AgentStateChanged', callback);
  }

  handleHubEvent('UserCreated');
  this.onUserCreated = function (callback)
  {
    /// <signature><param name="callback(userInfo)" type="Function">Callback.</param></signature>
    return events.on('UserCreated', callback);
  }

  handleHubEvent('UserUpdated');
  this.onUserUpdated = function (callback)
  {
    /// <signature><param name="callback(userInfo)" type="Function">Callback.</param></signature>
    return events.on('UserUpdated', callback);
  }

  handleHubEvent('UserRemoved');
  this.onUserRemoved = function (callback)
  {
    /// <signature><param name="callback(userId)" type="Function">Callback.</param></signature>
    return events.on('UserRemoved', callback);
  }

  handleHubEvent('DepartmentStateChanged');
  this.onDepartmentStateChanged = function (callback)
  {
    /// <signature><param name="callback(OnlineStatusInfo)" type="Function">Callback.</param></signature>
    return events.on('DepartmentStateChanged', callback);
  }

  handleHubEvent('DepartmentCreated');
  this.onDepartmentCreated = function (callback)
  {
    /// <signature><param name="callback(DepartmentInfo)" type="Function">Callback.</param></signature>
    return events.on('DepartmentCreated', callback);
  }

  handleHubEvent('DepartmentUpdated');
  this.onDepartmentUpdated = function (callback)
  {
    /// <signature><param name="callback(DepartmentInfo)" type="Function">Callback.</param></signature>
    return events.on('DepartmentUpdated', callback);
  }

  handleHubEvent('DepartmentRemoved');
  this.onDepartmentRemoved = function (callback)
  {
    /// <signature><param name="callback(DepartmentId)" type="Function">Callback.</param></signature>
    return events.on('DepartmentRemoved', callback);
  }

  _.forEach(
    [
      'VisitorSessionCreated',
      'AgentSessionCreated',
      'VisitorMessage',
      'VisitorLeftSession',
      'VisitorReconnected',
      'AgentMessage',
      'AgentLeftSession',
      'AgentClosedSession',
      'AgentSessionAccepted',
      'DepartmentSessionAccepted',
      'AgentSessionRejected',
      'AgentInvited',
      'DepartmentInvited',
      'AgentInvitationCanceled',
      'DepartmentInvitationCanceled',
    ],
    function (signalName)
    {
      hub.on(
        signalName,
        function ()
        {
          var args = Array.prototype.slice.call(arguments);
          args.unshift(signalName);
          events.emit('SessionUpdate', args);
        });
    });
  this.onSessionUpdate = function (callback)
  {
    /// <signature><param name="callback(reason, sessionInfo, messagesInfo, visitorInfo?)" type="Function">Callback.</param></signature>
    return events.on('SessionUpdate', callback);
  }

  handleHubEvent('MediaCallProposal');
  this.onMediaCallProposal = function (callback)
  {
    /// <signature><param name="callback(sessionInfo, messagesInfo)" type="Function">Callback.</param></signature>
    return events.on('MediaCallProposal', callback);
  }

  handleHubEvent('VisitorRejectedMediaCallProposal');
  this.onVisitorRejectedMediaCallProposal = function (callback)
  {
    /// <signature><param name="callback(sessionInfo, messagesInfo)" type="Function">Callback.</param></signature>
    return events.on('VisitorRejectedMediaCallProposal', callback);
  }

  handleHubEvent('VisitorAcceptedMediaCallProposal');
  this.onVisitorAcceptedMediaCallProposal = function (callback)
  {
    /// <signature><param name="callback(sessionInfo, messagesInfo)" type="Function">Callback.</param></signature>
    return events.on('VisitorAcceptedMediaCallProposal', callback);
  }

  handleHubEvent('VisitorStoppedMediaCall');
  this.onVisitorStoppedMediaCall = function (callback)
  {
    /// <signature><param name="callback(sessionInfo, messagesInfo)" type="Function">Callback.</param></signature>
    return events.on('VisitorStoppedMediaCall', callback);
  }

  handleHubEvent('AgentStoppedMediaCall');
  this.onAgentStoppedMediaCall = function (callback)
  {
    /// <signature><param name="callback(sessionInfo, messagesInfo)" type="Function">Callback.</param></signature>
    return events.on('AgentStoppedMediaCall', callback);
  }

  handleHubEvent('RtcSendIceCandidate');
  this.onRtcSendIceCandidate = function (callback)
  {
    /// <signature><param name="callback(candidate)" type="Function">Callback.</param></signature>
    return events.on('RtcSendIceCandidate', callback);
  }

  handleHubEvent('RtcSendCallAnswer');
  this.onRtcSendCallAnswer = function (callback)
  {
    /// <signature><param name="callback(sdp)" type="Function">Callback.</param></signature>
    return events.on('RtcSendCallAnswer', callback);
  }

  handleHubEvent('MediaCallVisitorConnectionIdSet');
  this.onMediaCallVisitorConnectionIdSet = function (callback)
  {
    /// <signature><param name="callback(sessionInfo)" type="Function">Callback.</param></signature>
    return events.on('MediaCallVisitorConnectionIdSet', callback);
  }

  handleHubEvent('VisitorInfoChanged');
  this.onVisitorInfoChanged = function (callback)
  {
    /// <signature><param name="callback(visitorId, wasRemoved, newName, newEmail, newPhone, sessionSkey, messages)" type="Function">Callback.</param></signature>
    return events.on('VisitorInfoChanged', callback);
  }

  handleHubEvent('SessionTranscriptSentToVisitor');
  this.onSessionTranscriptSentToVisitor = function (callback)
  {
    /// <signature><param name="callback(sessionInfo, messagesInfo)" type="Function">Callback.</param></signature>
    return events.on('SessionTranscriptSentToVisitor', callback);
  }


  // Get
  this.getConsoleInfo = function ()
  {
    return get(
      'GetConsoleInfo');
  }
  this.getFullChatSessionInfo = function (sessionSkey)
  {
    return get(
      'GetFullChatSessionInfo',
      {
        'chatSessionSkey': sessionSkey
      });
  }
  this.getVisitorInfo = function (visitorId)
  {
    return get(
      'GetVisitorInfo',
      {
        'visitorId': visitorId
      });
  }

  // Post
  this.acceptSessionAsAgent = function (sessionSkey)
  {
    return post(
      'AcceptSessionAsAgent',
      {
        'chatSessionSkey': sessionSkey
      });
  }
  this.acceptSessionAsDepartment = function (sessionSkey, departmentId)
  {
    return post(
      'AcceptSessionAsDepartment',
      {
        'chatSessionSkey': sessionSkey,
        'departmentId': departmentId
      });
  }
  this.exitSession = function (sessionSkey, message)
  {
    return post(
      'ExitSession',
      {
        'chatSessionSkey': sessionSkey,
        'text': message || ''
      });
  }
  this.closeSession = function (sessionSkey, message)
  {
    return post(
      'CloseSession',
      {
        'chatSessionSkey': sessionSkey,
        'text': message || ''
      });
  }
  this.sendMessage = function (sessionSkey, isToAgentsOnly, message)
  {
    return post(
      'SendMessage',
      {
        'chatSessionSkey': sessionSkey,
        'isToAgentsOnly': isToAgentsOnly,
        'text': message
      });
  }
  this.startChatSessionToAgent = function (targetAgentId, message)
  {
    return post(
      'StartChatSessionToAgent',
      {
        'targetAgentId': targetAgentId,
        'message': message
      });
  }
  this.startChatSessionToDepartment = function (targetDepartmentId, message)
  {
    return post(
      'StartChatSessionToDepartment',
      {
        'targetDepartmentId': targetDepartmentId,
        'message': message
      });
  }
  this.inviteAgentToChatSession = function (chatSessionSkey, invitedAgentId, actOnBehalfOfInvitor, text)
  {
    return post(
      'InviteAgentToChatSession',
      {
        'chatSessionSkey': chatSessionSkey,
        'invitedAgentId': invitedAgentId,
        'actOnBehalfOfInvitor': actOnBehalfOfInvitor,
        'text': text
      });
  }
  this.cancelAgentInvitationToChatSession = function (chatSessionSkey, invitedAgentId, text)
  {
    return post(
      'CancelAgentInvitationToChatSession',
      {
        'chatSessionSkey': chatSessionSkey,
        'invitedAgentId': invitedAgentId,
        'text': text
      });
  }
  this.inviteDepartmentToChatSession = function (chatSessionSkey, invitedDepartmentId, actOnBehalfOfInvitor, text)
  {
    return post(
      'InviteDepartmentToChatSession',
      {
        'chatSessionSkey': chatSessionSkey,
        'invitedDepartmentId': invitedDepartmentId,
        'actOnBehalfOfInvitor': actOnBehalfOfInvitor,
        'text': text
      });
  }
  this.cancelDepartmentInvitationToChatSession = function (chatSessionSkey, invitedDepartmentId, text)
  {
    return post(
      'CancelDepartmentInvitationToChatSession',
      {
        'chatSessionSkey': chatSessionSkey,
        'invitedDepartmentId': invitedDepartmentId,
        'text': text
      });
  }
  this.sendTranscriptToVisitor = function (chatSessionSkey, visitorTimezoneOffsetMinutes)
  {
    return post(
      'SendTranscriptToVisitor',
      {
        'chatSessionSkey': chatSessionSkey,
        'visitorTimezoneOffsetMinutes': visitorTimezoneOffsetMinutes
      });
  }
  this.mediaCallProposal = function (chatSessionSkey, hasVideo)
  {
    return post(
      'MediaCallProposal',
      {
        'chatSessionSkey': chatSessionSkey,
        'hasVideo': hasVideo
      });
  }
  this.mediaCallStop = function (chatSessionSkey, reason)
  {
    return post(
      'MediaCallStop',
      {
        'chatSessionSkey': chatSessionSkey,
        'reason': reason
      });
  }
  this.rtcSendIceCandidate = function (visitorConnectionId, candidate)
  {
    return post(
      'RtcSendIceCandidate',
      {
        'visitorConnectionId': visitorConnectionId,
        'candidate': candidate
      });
  }
  this.rtcSendCallOffer = function (visitorConnectionId, sdp)
  {
    return post(
      'RtcSendCallOffer',
      {
        'visitorConnectionId': visitorConnectionId,
        'sdp': sdp
      });
  }
  this.sessionSetStatus = function (isOnline)
  {
    return post(
      'SessionSetStatus',
      {
        'isOnline': isOnline
      });
  }
};