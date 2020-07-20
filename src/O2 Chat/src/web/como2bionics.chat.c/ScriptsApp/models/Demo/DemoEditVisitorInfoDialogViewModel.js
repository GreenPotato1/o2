'use strict';

function DemoEditVisitorInfoDialogViewModel(parentModel)
{
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

  this.visitorEmail = ko.observable()
    .extend({
        email: { message: Strings.validationEmailIsRequired },
        required: { message: Strings.validationEmailIsRequired }
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

    this.saveButtonText(saveButtonText);
    this.messageText(messageText);

    saveCallback = aSaveCallback;
    cancelCallback = aCancelCallback;

    parentModel.dialog(Dialogs.editVisitorInfo);
    this.hasFocusVisitorName(true);
  }

  this.onClickSave = function ()
  {
    return false;
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