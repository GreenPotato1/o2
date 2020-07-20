'use strict';

function DemoOfflineMessageSentDialogViewModel(parentModel)
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