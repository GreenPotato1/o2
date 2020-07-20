'use strict';

function DetectRTCHelper()
{
}

DetectRTCHelper.load = function ()
{
  var d = $.Deferred();
  var detectRtcLoadCounter = 0;
  DetectRTC.load(function ()
  {
    console.log('DetectRTC.load', detectRtcLoadCounter, DetectRTC);

    if (detectRtcLoadCounter > 0) return;
    detectRtcLoadCounter++;

    var mediaSupport = Enums.MediaSupport.NotSupported;
    if (DetectRTC.isWebRTCSupported && DetectRTC.hasMicrophone)
      mediaSupport = DetectRTC.hasWebcam ? Enums.MediaSupport.Video : Enums.MediaSupport.Audio;

    console.log(mediaSupport);
    d.resolve(mediaSupport);
  });
  return d;
}