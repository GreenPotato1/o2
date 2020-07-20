'use strict';

// global ko:true

var Dialogs = {
    startChat: 1,
    chat: 2,
    offlineMessageSent: 3,
    editVisitorInfo: 4,
    loading: 5,
    transcriptProposal: 6
};

var emoticonsPath = '../Content/emotify/gerty/';
var emoticons = {
    ':-)': ['happy.png', 'happy', ':)', '(:', '^_^', ')))'],
    ':-(': ['sad.png', 'sad', ':(', '=(', '=-(', '):'],
    ':D': ['grin.png', 'grin', 'LOL'],
    ':|': ['neutral.png', 'neutral', '|:'],
    ':o': ['suprised.png', 'suprised', 'o:', '0_0'],
    ':&#39;(': ['cry.png', 'cry', ':-((', ':(('],
    ':?': ['worried.png', 'worried'],
    ':&#x2F;': ['puzzled.png', 'puzzled', '&#x2F;:']
  };

function ChatApplication(appParams, window, console, $, ko, windowMessageHandler)
/// { mode, customerId, visitorId, isSessionStarted, pageTrackerUrl, isChatEnabled, isProactiveChatEnabled }
///     mode: 'popout' | 'iframe'
{
  var app = this;

  this.windowMessageHandler = windowMessageHandler;
  this.model = null;
  this.customerId = appParams.customerId;
  this.visitorId = appParams.visitorId;
  this.pageTrackerUrl = appParams.pageTrackerUrl;
  this.isProactiveChatEnabled = appParams.isProactiveChatEnabled;
  this.isSessionStarted = appParams.isSessionStarted;
  this.isDemo = appParams.isDemo;

  this.start = function ()
  {
    if (!appParams.isChatEnabled)
    {
      console.log('Sorry, O2Chat is disabled now for the customer ' + app.customerId);
      return;
    }

    if (app.isDemo)
    {
      startModel(null, Enums.MediaSupport.NotSupported);
    }
    else if (appParams.mode === Enums.WindowMode.iframe)
    {
      $.when(
          ChatParameters.load(app.windowMessageHandler),
          DetectRTCHelper.load())
        .then(function (chatParameters, mediaSupport)
        {
          Tracker.add(
              app.pageTrackerUrl,
              app.customerId,
              app.visitorId,
              chatParameters.visitorExternalId,
              chatParameters.pageUrl,
              chatParameters.customText)
            .done(function (trackerResult)
            {
              app.visitorId = trackerResult.vid;

              var vid = Cookies.get('v_' + app.customerId);
              console.log('old vid', vid);
              var exp = new Date();
              exp.setDate(exp.getDate() + 10 * 365);
              Cookies.set('v_' + app.customerId, '' + trackerResult.vid, exp, '/Client/');

              startModel(trackerResult.hid, mediaSupport);
            });
        });
    }
    else if (appParams.mode === Enums.WindowMode.popout)
    {
      DetectRTCHelper.load()
        .then(function (mediaSupport)
        {
          startModel(null, mediaSupport);
        });
    }
  }

  function startModel(hid, mediaSupport)
  {
    setupLayoutManager();
    setupEmotify();
    setupKoValidation();

    if (app.isDemo)
    {
      app.model = new DemoViewModel(
        appParams.appearance,
        windowMessageHandler);
    }
    else
    {
      var hub = new VisitorChatHubProxy(app.customerId, app.visitorId, hid, $, console, window);

      app.model = new ViewModel(
        appParams.mode,
        hub,
        app.isProactiveChatEnabled,
        app.isSessionStarted,
        app.customerId,
        hid,
        mediaSupport,
        appParams.appearance,
        windowMessageHandler);
    }

    ko.applyBindings(app.model);
    app.model.initialize();
  }

  function setupLayoutManager()
  {
    var divVisitorInfoBanner = document.getElementById('enter-visitor-info-banner');
    var divVideo = document.getElementById('rtc-call-video');
    var divMediaControls = document.getElementById('rtc-call-controls');
    var divChat = document.getElementById('chat-container');
    var divVideoProposal = document.getElementById('rtc-call-notification');
    var divTextInput = document.getElementById('chat-message-text');
    var divButtons = document.getElementById('chat-buttons');
    var videoLocal = document.getElementById('video-local');
    var divPoweredBy = document.getElementById('poweredBy');

    var heightTitle = (appParams.mode === Enums.WindowMode.iframe) ? Metrics.titleHeight : 0;
    var heightVisitorInfoBanner = Metrics.visitorInfoBanner;
    var heightMediaControls = Metrics.chatMediaControlsHeight;
    var heightChat = Metrics.chatMessagesAreaHeight; // used if video is visible
    var heightVideoProposal = Metrics.chatVideoProposalHeight;
    var heightTextInput = Metrics.chatTextInputHeight;
    var heightButtons = Metrics.chatButtonsHeight;
    var poweredByHeight = Metrics.poweredByHeight;

    $(".dialog").height(Metrics.dialogHeight);

    function resizeChatDialog()
    {
      var h = $(window).height();
      var w = $(window).width();

      console.log('resize',
        w,
        h,
        divVideo.style.display,
        divMediaControls.style.display,
        divVideoProposal.style.display);

      var isVisibleVisitorInfoBanner = divVisitorInfoBanner.style.display !== 'none';
      var isVisibleVideo = divVideo.style.display !== 'none';
      var isVisibleMediaControls = divMediaControls.style.display !== 'none';
      var isVisibleVideoProposal = divVideoProposal.style.display !== 'none';
      var isVisiblePoweredBy = divPoweredBy.style.display !== 'none';

      //
      // title?
      // visitorInfoBanner?
      // video?
      // media controls?
      // chat
      // video proposal?
      // textInput
      // buttons
      //

      var actualHeightVisitorInfoBanner = isVisibleVisitorInfoBanner ? heightVisitorInfoBanner : 0;
      var actualHeightVideoProposal = isVisibleVideoProposal ? heightVideoProposal : 0;
      var actualHeightMediaControls = isVisibleMediaControls ? heightMediaControls : 0;
      var actualHeightAllExceptVideo = heightTitle +
        actualHeightVisitorInfoBanner +
        actualHeightMediaControls +
        actualHeightVideoProposal +
          heightTextInput;

      var actualHeightChat = isVisibleVideo ? heightChat : h - (actualHeightAllExceptVideo);

      if (isVisiblePoweredBy)
        actualHeightChat -= poweredByHeight;

      var top = heightTitle;

      if (isVisibleVisitorInfoBanner)
      {
        divVisitorInfoBanner.style.top = top + 'px';
        divVisitorInfoBanner.style.height = actualHeightVisitorInfoBanner + 'px';
        top += actualHeightVisitorInfoBanner;
      }

      if (isVisibleVideo)
      {
        var heightVideo = h -
          (heightTitle +
            actualHeightVisitorInfoBanner +
            actualHeightMediaControls +
            actualHeightChat +
            actualHeightVideoProposal +
              heightTextInput);
        divVideo.style.top = top + 'px';
        divVideo.style.height = heightVideo + 'px';
        top += heightVideo;

        videoLocal.style.left = (w - $(videoLocal).width() - 20) + 'px';
        videoLocal.style.top = (heightVideo - $(videoLocal).height() - 10) + 'px';
      }

      if (isVisibleMediaControls)
      {
        divMediaControls.style.top = top + 'px';
        divMediaControls.style.height = actualHeightMediaControls + 'px';
        divMediaControls.style.lineHeight = actualHeightMediaControls + 'px';
        top += actualHeightMediaControls;
      }

      divChat.style.top = top + 'px';
      divChat.style.height = actualHeightChat + 'px';
      top += actualHeightChat;

      if (isVisibleVideoProposal)
      {
        divVideoProposal.style.top = top + 'px';
        divVideoProposal.style.height = actualHeightVideoProposal + 'px';
        top += actualHeightVideoProposal;
      }

      divTextInput.style.top = top + 'px';
      divTextInput.style.height = heightTextInput + 'px';


        divButtons.style.top = 0 + 'px';
      divButtons.style.height = heightButtons + 'px';
      divButtons.style.lineHeight = heightButtons + 'px';
    }

    $('#video-local')
      .on('playing',
        function (evt)
        {
          var e = $(evt.target);
          var ew = e.width();
          var eh = e.height();
          var vw = evt.target.videoWidth;
          var vh = evt.target.videoHeight;

          console.debug('playing', ew, eh, vw, vh);
          if (ew === 0 || eh === 0 || vw === 0 || vh === 0) return;

          var a = vw / vh;
          if (eh * a < ew)
            e.width(eh * a);
          else if (ew / a < eh)
            e.height(ew / a);

          $(window).resize();
        });

    $(window).on('resize', function () { resizeChatDialog(); });

    resizeChatDialog();
  }

  function setupEmotify()
  {
    emotify.emoticons(emoticonsPath, true, emoticons);
  }

  function setupKoValidation()
  {
    ko.validation.init({
        errorElementClass: 'error',
        decorateElementOnModified: true,
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null
      },
      true);
  }
}