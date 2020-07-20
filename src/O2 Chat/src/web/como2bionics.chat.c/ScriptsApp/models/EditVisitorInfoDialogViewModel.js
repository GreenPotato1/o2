'use strict';

function EditVisitorInfoDialogViewModel(parentModel, chatHub)
{
  var self = this;

  var saveCallback = null;
  var cancelCallback = null;

  this.visitorName = ko.observable()
    .extend({ required: { message: Strings.validationNameIsRequired } });

  this.transcriptMode = ko.observable(Enums.VisitorSendTranscriptMode.Ask);
  this.transcriptModes = [
      { id: Enums.VisitorSendTranscriptMode.Ask, text: 'Ask' },
      { id: Enums.VisitorSendTranscriptMode.Always, text: 'Always' },
      { id: Enums.VisitorSendTranscriptMode.Never, text: 'Never' }
    ];

  this.validateEmail = ko.pureComputed(function ()
  {
    return self.transcriptMode() === Enums.VisitorSendTranscriptMode.Always;
  });
  this.visitorEmail = ko.observable()
    .extend({
        email: { message: Strings.validationEmailIsInvalid },
        required: {
            message: Strings.validationEmailIsRequired,
            onlyIf: self.validateEmail
          }
      });
  this.visitorPhone = ko.observable();
 
  this.hasFocusVisitorName = ko.observable();

  this.saveButtonText = ko.observable();
  this.messageText = ko.observable();

  this.show = function (saveButtonText, messageText, aSaveCallback, aCancelCallback)
  {
    this.visitorName(parentModel.startChatDialog.visitorName());
    this.visitorEmail(parentModel.startChatDialog.visitorEmail());
    this.visitorPhone(parentModel.startChatDialog.visitorPhone());
    this.transcriptMode(parentModel.startChatDialog.transcriptMode());

    this.saveButtonText(saveButtonText);
    this.messageText(messageText);

    saveCallback = aSaveCallback;
    cancelCallback = aCancelCallback;

    parentModel.dialog(Dialogs.editVisitorInfo);
    this.hasFocusVisitorName(true);
  }

  this.onClickSave = function ()
  {
    if (this.errors().length > 0)
    {
      this.errors.showAllMessages();
      return;
    }

    var actualVisitorInfo = {
        Name: this.visitorName(),
        Email: this.visitorEmail(),
        Phone: this.visitorPhone(),
        TranscriptMode: this.transcriptMode()
      };

    parentModel.dialog(Dialogs.loading);
    chatHub.updateVisitorInfo(actualVisitorInfo)
      .then(function ()
      {
        parentModel.startChatDialog.visitorName( self.visitorName());
        parentModel.startChatDialog.visitorEmail(self.visitorEmail());
        parentModel.startChatDialog.visitorPhone(self.visitorPhone());
        parentModel.startChatDialog.transcriptMode(self.transcriptMode());
        saveCallback();
      }.bind(this))
      .fail(function ()
      {
        parentModel.dialog(Dialogs.editVisitorInfo);
      });
  };

  this.onClickBack = function ()
  {
    cancelCallback();
  };

  this.onKeyPressed = function (model, e)
  {
    if (e.keyCode === 10 || e.keyCode === 13) // enter
    {
      this.onClickSave();
      return false;
    } else if (e.keyCode === 27) // esc
    {
      this.onClickBack();
      return false;
    }
    return true;
  };
}