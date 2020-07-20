'use strict';

function TranscriptProposalDialogViewModel(parentModel, chatHub)
{
  var self = this;

  var okCallback = null;
  var skipCallback = null;

  this.transcriptMode = ko.observable(Enums.VisitorSendTranscriptMode.Ask);
  this.transcriptModes = [
      { id: Enums.VisitorSendTranscriptMode.Ask, text: 'Ask' },
      { id: Enums.VisitorSendTranscriptMode.Always, text: 'Always' },
      { id: Enums.VisitorSendTranscriptMode.Never, text: 'Never' }
    ];

  this.validateEmail = ko.pureComputed(function ()
  {
    return true; //self.transcriptMode() === Enums.VisitorSendTranscriptMode.Always;
  });
  this.visitorEmail = ko.observable()
    .extend({
        email: { message: Strings.validationEmailIsInvalid },
        required: {
            message: Strings.validationNameIsRequired,
            onlyIf: self.validateEmail
          }
      });

  this.errors = ko.validation.group(this);

  this.show = function (anOkCallback, aSkipCallback)
  {
    self.visitorEmail(parentModel.startChatDialog.visitorEmail());
    self.transcriptMode(parentModel.startChatDialog.transcriptMode());

    okCallback = anOkCallback;
    skipCallback = aSkipCallback;
    parentModel.dialog(Dialogs.transcriptProposal);
  }

  this.onClickOk = function ()
  {
    if (this.errors().length > 0)
    {
      this.errors.showAllMessages();
      return;
    }

    updateVisitorInfo(okCallback);
  }

  this.onClickSkip = function ()
  {
    if (this.errors().length === 0)
    {
      updateVisitorInfo(skipCallback);
    }
    else
    {
      skipCallback();
    }
  }

  function updateVisitorInfo(callback)
  {
    if (self.visitorEmail() !== parentModel.startChatDialog.visitorEmail() ||
      self.transcriptMode() !== parentModel.startChatDialog.transcriptMode())
    {
      var actualVisitorInfo = {
          Name: parentModel.startChatDialog.visitorName(),
          Email: self.visitorEmail(),
          Phone: parentModel.startChatDialog.visitorPhone(),
          TranscriptMode: self.transcriptMode()
        };

      parentModel.dialog(Dialogs.loading);
      chatHub.updateVisitorInfo(actualVisitorInfo)
        .then(function ()
        {
          parentModel.startChatDialog.visitorEmail(actualVisitorInfo.Email);
          parentModel.startChatDialog.transcriptMode(actualVisitorInfo.TranscriptMode);

          callback();
        })
        .fail(function ()
        {
          parentModel.dialog(Dialogs.transcriptProposal);
        });
    }
    else
    {
      callback();
    }
  }
}