'use strict';

function DemoChatMessage(id, sender, senderName, text, time)
{
  this.id = id;
  this.time = time;
  this.sender = sender;
  this.senderName = senderName;
  this.lines = emotify(escapeHtml(text)).split('\n');
  this.avatarUrl = AvatarConstants.toAvatarUrl();
}

function DemoChatDialogViewModel(parentModel, ko)
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

  this.messages.push(
    new DemoChatMessage(
      1,
      'self',
      null,
      '1st message',
      new Date('2016-12-14')));
  this.messages.push(
    new DemoChatMessage(
      2,
      'other',
      'John Doe',
      '2nd message',
      new Date('2016-12-15')));
  this.messages.push(
    new DemoChatMessage(
      3,
      'self',
      null,
      '3rd message',
      new Date('2016-12-16')));
  this.messages.push(
    new DemoChatMessage(
      4,
      'other',
      'John Doe',
      '4th message',
      new Date('2016-12-17')));

  this.text = ko.observable('');
  this.textFocus = ko.observable(true);
  this.textDisabled = ko.observable(false);

  this.mediaCallProposalHasVideo = ko.observable(true);
  this.mediaCallTypeText = ko.pureComputed(function () { return this.mediaCallProposalHasVideo() ? 'Video' : 'Voice'; },
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

  this.scrollToLastMessage = function ()
  {
    var last = $('#chat-container li:last');
    if (last.length > 0)
      $('#chat-container').scrollTo(last, 200);
  }

  this.reset = function ()
  {
    this.messages.removeAll();
    this.text('');
    this.textFocus(true);
  }

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
    return false;
  }

  this.back = function ()
  {
    parentModel.dialog(Dialogs.startChat);
  }

  this.onMediaCallAcceptedClick = function ()
  {
  }

  this.receiveMediaCall = function (isAgentProposedVideo, isVisitorAcceptedWithVideo)
  {
  }

  this.onClickMediaCallPauseAudio = function ()
  {
  }
  this.onClickMediaCallPauseVideo = function ()
  {
  }
  this.onClickMediaCallStop = function ()
  {
  }


  // TODO: remove
  this.onClickT1 = function ()
  {
  }
  // TODO: remove
  this.onClickT2 = function ()
  {
  }
}