'use strict';

function OfflineMessageSentDialogViewModel(parentModel)
{
  var cancelCallback = null;

  this.show = function (aCancelCallback)
  {
    cancelCallback = aCancelCallback;

    parentModel.dialog(Dialogs.offlineMessageSent);
  }

  this.cancel = function ()
  {
    cancelCallback();
  }
}