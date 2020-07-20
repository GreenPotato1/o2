
function DialogSessionTranscriptWasntSentModel()
{
  this.onSendTranscriptAndCloseSessionCallback = null;
  this.onCloseSessionCallback = null;

  this.dialog = createDialogOptions(280, 430);

  this.onSendTranscriptAndCloseSession = function ()
  {
    this.dialog.isOpen(false);
    if (this.onSendTranscriptAndCloseSessionCallback) this.onSendTranscriptAndCloseSessionCallback();
  };

  this.onCloseSession = function ()
  {
    this.dialog.isOpen(false);
    if (this.onCloseSessionCallback) this.onCloseSessionCallback();
  };

  this.onCancel = function ()
  {
    this.dialog.isOpen(false);
  };

  this.show = function (onSendTranscriptAndCloseSessionCallback, onCloseSessionCallback)
  {
    this.onSendTranscriptAndCloseSessionCallback = onSendTranscriptAndCloseSessionCallback;
    this.onCloseSessionCallback = onCloseSessionCallback;
    this.dialog.isOpen(true);
  }
}