'use strict';

var VisitorChatHubProxy = function (customerId, visitorId, historyId, $, console, window)
{
  var hubPath = '';
  var hubName = 'visitorChatHub';
  var reconnectInterval = 30000;

  var hub = createHub(hubPath, hubName, customerId, visitorId, historyId);
  var reconnectAttempt = 0;
  var isReconnectScheduled = false;
  var isClosing = false;

  var events = new utils.Events();

  // at least one subscription must be added before connect to receive events
  hub.on('ping', function () {});

  hub.connection.starting(function ()
  {
    console.log('hub.connection: starting');
    callConnectingHandler();
  });
  hub.connection.connectionSlow(function ()
  {
    console.log('hub.connection: connectionSlow');
  });
  hub.connection.reconnecting(function ()
  {
    console.log('hub.connection: reconnecting');
    callConnectingHandler();
  });
  hub.connection.reconnected(function ()
  {
    console.log('hub.connection: reconnected');
    callConnectedHandler();
  });
  hub.connection.disconnected(function ()
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
    .unload(function ()
    {
      isClosing = true;
      if (hub.connection.state !== $.signalR.connectionState.disconnected)
        hub.connection.stop(false, true);
    });

  function createHub(hubPath, hubName, customerId, visitorId, historyId)
  {
    var connection = $.hubConnection(hubPath);
    connection.logging = true;
    connection.log = function (m)
    {
      console.debug('[' + new Date().toLogTimestampString() + '] SignalR:', m);
    }
    connection.qs = connection.qs || {};
    connection.qs.c = customerId;
    connection.qs.v = visitorId;
    connection.qs.h = historyId;
    return connection.createHubProxy(hubName);
  }

  function invoke()
  {
    var args = Array.prototype.slice.call(arguments);
    var connectionState = hub.connection.state;
    return hub.connection.start({ pingInterval: null })
      .then(
        function ()
        {
          if (connectionState !== $.signalR.connectionState.connected) callConnectedHandler();
          return hub.invoke.apply(hub, args)
            .fail(function (x) { console.log('hub.connection: method ' + args[0] + ' call error: ' + x); });
        },
        function (x)
        {
          console.log('hub.connection: connection start error: ' + x);
        });
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
          console.log('hub.connection: reconnect attempt ' +
            n +
            ' failure: ' +
            x +
            '; scheduling reconnection in ' +
            reconnectInterval +
            'ms.');
          setTimeout(tryReconnect, reconnectInterval);
        });
  }

  // connection events
  this.onDisconnected = function (callback)
  {
    /// <signature><param name="callback()" type="Function">Callback.</param></signature>
    return events.on('Disconnect', callback);
  };

  function callDisconnectHandler()
  {
    events.emit('Disconnect');
  }

  this.onConnected = function (callback)
  {
    /// <signature><param name="callback()" type="Function">Callback.</param></signature>
    return events.on('Connected', callback);
  };

  function callConnectedHandler()
  {
    events.emit('Connected');
  }

  this.onConnecting = function (callback)
  {
    /// <signature><param name="callback()" type="Function">Callback.</param></signature>
    return events.on('Connecting', callback);
  };

  function callConnectingHandler()
  {
    events.emit('Connecting');
  }

  // hub events
  function handleHubEvent(signalName, eventName)
  {
    hub.on(signalName,
      function () { events.emit(eventName ? eventName : signalName, Array.prototype.slice.call(arguments)); });
  }

  handleHubEvent('DepartmentStateChanged');
  this.onDepartmentStateChanged = function (callback)
  {
    /// <signature><param name="callback(departmentInfo)" type="Function">Callback.</param></signature>
    return events.on('DepartmentStateChanged', callback);
  }

  handleHubEvent('AgentSessionAccepted', 'ChatEvent');
  handleHubEvent('DepartmentSessionAccepted', 'ChatEvent');
  handleHubEvent('AgentMessage', 'ChatEvent');
  handleHubEvent('AgentLeftSession', 'ChatEvent');
  this.onChatEvent = function (callback)
  {
    /// <signature><param name="callback(sessionInfo, agentInfo, messagesInfo)" type="Function">Callback.</param></signature>
    return events.on('ChatEvent', callback);
  }

  handleHubEvent('AgentClosedSession');
  this.onAgentClosedSession = function (callback)
  {
    /// <signature><param name="callback(sessionInfo, agentInfo, messagesInfo)" type="Function">Callback.</param></signature>
    return events.on('AgentClosedSession', callback);
  }

  handleHubEvent('MediaCallProposal');
  this.onMediaCallProposal = function (callback)
  {
    /// <signature><param name="callback(sessionInfo, agentInfo, messagesInfo, hasVideo)" type="Function">Callback.</param></signature>
    return events.on('MediaCallProposal', callback);
  }

  handleHubEvent('VisitorAcceptedMediaCallProposal');
  this.onVisitorAcceptedMediaCallProposal = function (callback)
  {
    /// <signature><param name="callback()" type="Function">Callback.</param></signature>
    return events.on('VisitorAcceptedMediaCallProposal', callback);
  }

  handleHubEvent('VisitorRejectedMediaCallProposal');
  this.onVisitorRejectedMediaCallProposal = function (callback)
  {
    /// <signature><param name="callback()" type="Function">Callback.</param></signature>
    return events.on('VisitorRejectedMediaCallProposal', callback);
  }

  handleHubEvent('VisitorStoppedMediaCall');
  this.onVisitorStoppedMediaCall = function (callback)
  {
    /// <signature><param name="callback(sessionInfo, messages)" type="Function">Callback.</param></signature>
    return events.on('VisitorStoppedMediaCall', callback);
  }

  handleHubEvent('AgentStoppedMediaCall');
  this.onAgentStoppedMediaCall = function (callback)
  {
    /// <signature><param name="callback(sessionInfo, agentInfo, messagesInfo)" type="Function">Callback.</param></signature>
    return events.on('AgentStoppedMediaCall', callback);
  }

  handleHubEvent('RtcSendIceCandidate');
  this.onRtcSendIceCandidate = function (callback)
  {
    /// <signature><param name="callback(candidate)" type="Function">Callback.</param></signature>
    return events.on('RtcSendIceCandidate', callback);
  }

  handleHubEvent('RtcSendCallOffer');
  this.onRtcSendCallOffer = function (callback)
  {
    /// <signature><param name="callback(sdp)" type="Function">Callback.</param></signature>
    return events.on('RtcSendCallOffer', callback);
  }

  handleHubEvent('VisitorInfoChanged');
  this.onVisitorInfoChanged = function (callback)
  {
    /// <signature><param name="callback(wasRemoved, newName, newEmail, newPhone, newTranscriptMode)" type="Function">Callback.</param></signature>
    return events.on('VisitorInfoChanged', callback);
  }

  handleHubEvent('VisitorSessionCreated');
  this.onVisitorSessionCreated = function (callback)
  {
    /// <signature><param name="callback(sessionId, messages)" type="Function">Callback.</param></signature>
    return events.on('VisitorSessionCreated', callback);
  }

  handleHubEvent('VisitorMessage');
  this.onVisitorMessage = function (callback)
  {
    /// <signature><param name="callback(sdp)" type="Function">Callback.</param></signature>
    return events.on('VisitorMessage', callback);
  }

  handleHubEvent('VisitorClosedSession');
  this.onVisitorClosedSession = function (callback)
  {
    /// <signature><param name="callback(sdp)" type="Function">Callback.</param></signature>
    return events.on('VisitorClosedSession', callback);
  }

  handleHubEvent('VisitorRequestedTranscriptSent');
  this.onVisitorRequestedTranscriptSent = function (callback)
  {
    /// <signature><param name="callback(sessionInfo, messagesInfo)" type="Function">Callback.</param></signature>
    return events.on('VisitorRequestedTranscriptSent', callback);
  }

  // hub methods
  this.chatWindowOpen = function (mediaSupported, isVideoPopout)
  {
    return invoke('chatWindowOpen', mediaSupported, isVideoPopout);
  };
  this.updateVisitorInfo = function (actualVisitorInfo)
  {
    return invoke('updateVisitorInfo', actualVisitorInfo);
  };
  this.clearVisitorInfo = function ()
  {
    return invoke('clearVisitorInfo');
  };
  this.sendOfflineMessage = function (selectedDepartmentId, text)
  {
    return invoke('sendOfflineMessage', selectedDepartmentId, text);
  };
  this.startChat = function (selectedDepartmentId, text)
  {
    return invoke('startChat', selectedDepartmentId, text);
  };
  this.sendMessage = function (text)
  {
    return invoke('sendMessage', text);
  };
  this.endChat = function ()
  {
    return invoke('endChat');
  };
  this.mediaCallProposalRejected = function ()
  {
    return invoke('mediaCallProposalRejected');
  }
  this.mediaCallProposalAccepted = function (hasVideo)
  {
    return invoke('mediaCallProposalAccepted', hasVideo);
  }
  this.mediaCallSetConnectionId = function ()
  {
    return invoke('mediaCallSetConnectionId');
  }
  this.mediaCallStop = function (reason)
  {
    return invoke('mediaCallStop', reason);
  }
  this.rtcSendIceCandidate = function (agentConnectionId, x)
  {
    return invoke('rtcSendIceCandidate', agentConnectionId, x);
  }
  this.rtcSendCallAnswer = function (agentConnectionId, x)
  {
    return invoke('rtcSendCallAnswer', agentConnectionId, x);
  }

  this.sendTranscript = function (sessionId, visitorTimezoneOffsetMinutes) {
    return invoke('sendTranscript', sessionId, visitorTimezoneOffsetMinutes);
  }

  // TODO: remove
  this.dumpConnections = function ()
  {
    return invoke('dumpConnections');
  }
};