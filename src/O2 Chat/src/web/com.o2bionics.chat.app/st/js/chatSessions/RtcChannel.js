// ReSharper disable InconsistentNaming

'use strict';

// hub:
//  onRtcSendIceCandidate(callback(json))
//  rtcSendIceCandidate(remoteConnectionId, json)
//  ?onRtcSendCallAnswer(callback(json))
//  ?rtcSendCallAnswer(remoteConnectionId, json)
//  ?onRtcSendCallOffer(callback(json))
//  ?rtcSendCallOffer(remoteConnectionId, json)

function RtcChannel(
  hub,
  remoteConnectionId,
  localVideoElement,
  remoteVideoElement)
{
  var localStream = null;
  var remoteStream = null;
  var pc = null;


  var connectionConfig = {
    'iceServers': [
      { 'urls': 'stun:stun.services.mozilla.com' },
      { 'urls': 'stun:stun.l.google.com:19302' }
    ]
  };

  var connectionConstraints = {
    'optional': [
      { 'DtlsSrtpKeyAgreement': true },
      { 'RtpDataChannels': true }
    ]
  };
  var sdpConstraints = null;


  var rtcSendIceCandidateHandler = hub.onRtcSendIceCandidate(function (json)
  {
    //debug('remote ice candidate:', json);
    if (!pc)
    {
      error('peer connection is null in onRtcSendIceCandidate');
      return;
    }
    var x = JSON.parse(json);
    pc.addIceCandidate(new RTCIceCandidate({ sdpMLineIndex: x.label, candidate: x.candidate }));
  });

  var rtcSendCallAnswerHandler = null;
  if (hub.onRtcSendCallAnswer)
  {
    rtcSendCallAnswerHandler = hub.onRtcSendCallAnswer(function (json)
    {
      //debug('received remote call answer', json);
      if (!pc)
      {
        error('peer connection is null in onRtcSendCallAnswer');
        return;
      }
      var x = JSON.parse(json);
      pc.setRemoteDescription(new RTCSessionDescription(x.sdp));
    });
  }

  var rtcSendCallOfferHandler = null;
  if (hub.onRtcSendCallOffer)
  {
    rtcSendCallOfferHandler = hub.onRtcSendCallOffer(function (json)
    {
      //debug('received remote call offer', json);
      if (!pc)
      {
        error('peer connection is null in onRtcSendCallOffer');
        return;
      }

      var x = JSON.parse(json);
      pc.setRemoteDescription(new RTCSessionDescription(x.sdp));

      pc.createAnswer(
        function (sessionDescription)
        {
          sessionDescription.sdp = preferOpus(sessionDescription.sdp);
          pc.setLocalDescription(sessionDescription);
          //debug('sending answer', sessionDescription);
          hub.rtcSendCallAnswer(remoteConnectionId, JSON.stringify({ sdp: sessionDescription }));
        },
        function (e)
        {
          error('createAnswer() error', e);
          // TODO?
        },
        sdpConstraints);
    });
  }

  var events = new utils.Events();

  this.onConnected = function (callback)
  {
    return events.on('connected', callback);
  }

  function emitConnected()
  {
    debug('emit connected event');
    events.emit('connected');
  }

  this.onDisconnected = function (callback)
  {
    return events.on('disconnected', callback);
  }

  function emitDisconnected()
  {
    debug('emit disconnected event');
    events.emit('disconnected');
  }


  this.dispose = function ()
  {
    stopVideo();

    if (rtcSendIceCandidateHandler)
    {
      rtcSendIceCandidateHandler.remove();
      rtcSendIceCandidateHandler = null;
    }
    if (rtcSendCallAnswerHandler)
    {
      rtcSendCallAnswerHandler.remove();
      rtcSendCallAnswerHandler = null;
    }
    if (rtcSendCallOfferHandler)
    {
      rtcSendCallOfferHandler.remove();
      rtcSendCallOfferHandler = null;
    }
  };

  this.initiateCall = function (enableVideo)
  {
    var r = $.Deferred();
    attachLocalMedia(enableVideo)
      .then(function ()
      {
        pc = createPeerConnection(
          remoteConnectionId,
          function ()
          {
            r.reject('create connection failed');
          });

        debug('creating offer');
        pc.createOffer(
          function (sessionDescription)
          {
            sessionDescription.sdp = preferOpus(sessionDescription.sdp);
            pc.setLocalDescription(sessionDescription);
            //debug('sending offer message', sessionDescription);
            hub.rtcSendCallOffer(remoteConnectionId, JSON.stringify({ sdp: sessionDescription }));
            r.resolve();
          },
          function (e)
          {
            error('createOffer() failed', e);
            r.reject('create offer failed');
          });
      })
      .fail(function (reason)
      {
        r.reject(reason);
      });

    return r;
  }

  this.receiveCall = function (enableRemoteVideo, enableLocalVideo)
  {
    sdpConstraints = {
      'mandatory': {
        'OfferToReceiveAudio': true,
        'OfferToReceiveVideo': enableRemoteVideo
      }
    };

    var r = $.Deferred();
    attachLocalMedia(enableLocalVideo)
      .then(function ()
      {
        pc = createPeerConnection(
          function ()
          {
            r.reject('create connection failed');
          });

        r.resolve();
      })
      .fail(function (reason)
      {
        r.reject(reason);
      });
    return r;
  }

  this.pauseAudio = function (enabled)
  {
    if (!localStream) return;

    var tracks = localStream.getAudioTracks();
    for (var i = 0; i < tracks.length; i++) tracks[i].enabled = enabled;
  }

  this.pauseVideo = function (enabled)
  {
    if (!localStream) return;

    var tracks = localStream.getVideoTracks();
    for (var i = 0; i < tracks.length; i++) tracks[i].enabled = enabled;
  }

  this.stop = function ()
  {
    stopVideo();
  }

  function stopVideo()
  {
    remoteVideoElement.src = '';
    if (remoteStream != null)
    {
      stopStream(remoteStream);
      remoteStream = null;
    }
    if (pc != null)
    {
      pc.close();
      pc = null;
    }

    localVideoElement.src = '';
    if (localStream != null)
    {
      stopStream(localStream);
      localStream = null;
    }
  }

  function stopStream(s)
  {
    var tracks = s.getTracks();
    for (var i = 0; i < tracks.length; i++)
    {
      var x = tracks[i];
      if (typeof (x.stop) === 'function') x.stop();
    }
    if (typeof (s.stop) === 'function') s.stop();
  }

  function attachLocalMedia(enableVideo)
  {
    var mediaConstraints = {
      video: enableVideo,
      audio: true
    };

    debug('getUserMedia()', mediaConstraints);
    var r = $.Deferred();
    navigator.getUserMedia(
      mediaConstraints,
      function (stream)
      {
        debug('getUserMedia() succeeded', stream);
        localStream = stream;
        localVideoElement.srcObject = stream;
        r.resolve();
      },
      function (e)
      {
        var reason;
        if (e.name === 'DevicesNotFoundError')
          reason = 'Can\'t start media call: no media devices found.';
        else if (e.name === 'PermissionDeniedError')
          reason = 'Can\'t start media call: media device use was prohibited by the user.';
        else reason = 'Can\'t start media call: unexpected error \'' + e.name + '\'';

        error('getUserMedia() failure', reason, e);
        r.reject(reason);
      });
    return r;
  }

  function createPeerConnection(fnFailure)
  {
    var connection;
    try
    {
      connection = new RTCPeerConnection(connectionConfig, connectionConstraints);
      debug('RTCPeerConnnection created');
    }
    catch (e)
    {
      error('RTCPeerConnection creation failed', e);
      fnFailure(e);
      return null;
    }

    connection.onicecandidate = function (event)
    {
      if (!connection) return;
      //debug('onicecandidate', event);
      if (event.candidate)
      {
        var x = JSON.stringify(
          {
            label: event.candidate.sdpMLineIndex,
            candidate: event.candidate.candidate
          });
        hub.rtcSendIceCandidate(remoteConnectionId, x);
      } else
      {
        //debug('end of local IceCandidates');
      }
    };
    connection.oniceconnectionstatechange = function ()
    {
      debug('oniceconnectionstatechange', connection.iceConnectionState);
      switch (connection.iceConnectionState)
      {
      case 'completed':
        emitConnected();
        break;
      case 'disconnected':
        emitDisconnected();
        break;
      }
    };
    connection.onconnectionstatechange = function (a)
    {
      error('onconnectionstatechange', a, connection.connectionState);
    }
    connection.onsignalingstatechange = function ()
    {
      debug('onsignalingstatechange', connection.signalingState);
    };
    connection.onaddstream = function (event)
    {
      debug('onaddstream', event);
      remoteStream = event.stream;

//      attachMediaStream(remoteVideoElement, remoteStream);
      remoteVideoElement.srcObject = remoteStream;

      debug('remote stream added');
    };
    connection.onremovestream = function (event)
    {
      debug('onremovestream', event);
    };

    connection.addStream(localStream);

    return connection;
  }

  function debug()
  {
    var args = Array.prototype.slice.call(arguments, 0);
    console.debug.apply(
      console,
      ['[' + new Date().toLogTimestampString() + '] RtcChannel:'].concat(args));
  }

  function error()
  {
    var args = Array.prototype.slice.call(arguments, 0);
    console.error.apply(
      console,
      ['[' + new Date().toLogTimestampString() + '] RtcChannel:'].concat(args));
  }


  function preferOpus(sdp)
  {
    var sdpLines = sdp.split('\r\n');
    var mLineIndex = null;
    // Search for m line.
    for (var i = 0; i < sdpLines.length; i++)
    {
      if (sdpLines[i].search('m=audio') !== -1)
      {
        mLineIndex = i;
        break;
      }
    }
    if (mLineIndex === null) return sdp;

    // If Opus is available, set it as the default in m line.
    for (var j = 0; j < sdpLines.length; j++)
    {
      if (sdpLines[j].search('opus/48000') !== -1)
      {
        var opusPayload = extractSdp(sdpLines[j], /:(\d+) opus\/48000/i);
        if (opusPayload) sdpLines[mLineIndex] = setDefaultCodec(sdpLines[mLineIndex], opusPayload);
        break;
      }
    }

    // Remove CN in m line and sdp.
    sdpLines = removeCn(sdpLines, mLineIndex);

    sdp = sdpLines.join('\r\n');
    return sdp;
  }

  function extractSdp(sdpLine, pattern)
  {
    var result = sdpLine.match(pattern);
    return result && result.length === 2 ? result[1] : null;
  }

// Set the selected codec to the first in m line.
  function setDefaultCodec(mLine, payload)
  {
    var elements = mLine.split(' ');
    var newLine = [];
    var index = 0;
    for (var i = 0; i < elements.length; i++)
    {
      if (index === 3)
      { // Format of media starts from the fourth.
        newLine[index++] = payload; // Put target payload to the first.
      }
      if (elements[i] !== payload) newLine[index++] = elements[i];
    }
    return newLine.join(' ');
  }

// Strip CN from sdp before CN constraints is ready.
  function removeCn(sdpLines, mLineIndex)
  {
    var line = sdpLines[mLineIndex];
    var mLineElements = line != undefined ? line.split(' ') : [];
    // Scan from end for the convenience of removing an item.
    for (var i = sdpLines.length - 1; i >= 0; i--)
    {
      var payload = extractSdp(sdpLines[i], /a=rtpmap:(\d+) CN\/\d+/i);
      if (payload)
      {
        var cnPos = mLineElements.indexOf(payload);
        if (cnPos !== -1)
        {
          // Remove CN payload from m line.
          mLineElements.splice(cnPos, 1);
        }
        // Remove CN line in sdp
        sdpLines.splice(i, 1);
      }
    }

    sdpLines[mLineIndex] = mLineElements.join(' ');
    return sdpLines;
  }
}