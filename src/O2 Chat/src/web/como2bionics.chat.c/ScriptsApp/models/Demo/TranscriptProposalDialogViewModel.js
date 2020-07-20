'use strict';

function TranscriptProposalDialogViewModel(parentModel, chatHub)
{
  var self = this;

  this.transcriptMode = ko.observable(Enums.VisitorSendTranscriptMode.Ask);
  this.transcriptModes = [
      { id: Enums.VisitorSendTranscriptMode.Ask, text: 'Ask' },
      { id: Enums.VisitorSendTranscriptMode.Always, text: 'Always' },
      { id: Enums.VisitorSendTranscriptMode.Never, text: 'Never' }
    ];

  this.validateEmail = ko.pureComputed(function ()
  {
    return true;
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
    parentModel.dialog(Dialogs.transcriptProposal);
  }

  this.onClickOk = function ()
  {
  }

  this.onClickSkip = function ()
  {
  }
}