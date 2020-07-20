'use strict';

function DialogCannedMessagesModel(mode, editDialogModel)
{
  var self = this;

  self.mode = mode;
  self.editDialogModel = editDialogModel;

  self.filterString = ko.observable('');

  self.dialog = createDialogOptions(480, 500);

  self.departmentId = null;
  self.storage = null;
  self.list = ko.observableArray([]);

  self.onCloseCallback = null;
  self.onDialogEditCannedMessageCallBack = null;


  function getTrimmedFilterString()
  {
    return self.filterString().trim().toLowerCase();
  }

  self.filtered = ko.pureComputed(
    function ()
    {
      var fs = getTrimmedFilterString();
      return ko.utils.arrayFilter(
        self.list(),
        function (x)
        {
          return !fs || x.itemMatchFilter(fs);
        });
    });

  self.canEdit = function (item)
  {
    return self.mode === item.type;
  }

  self.canInsertMessageText = function ()
  {
    return self.mode === Enums.CannedMessageType.Personal;
  }

  self.open = function (departmentId, storage, aCloseCallback)
  {
    self.onCloseCallback = aCloseCallback;
    self.departmentId = departmentId;
    self.storage = storage;
    self.list(storage.items);
    self.dialog.isOpen(true);
  }


  self.onResetSearch = function ()
  {
    self.filterString('');
    $('#dialog-canned-messages-search').trigger('change');
  }

  self.onCloseDialog = function ()
  {
    self.dialog.isOpen(false);

    if (self.onCloseCallback)
      self.onCloseCallback(null);
  }

  self.onInsertMessage = function (item)
  {
    self.dialog.isOpen(false);

    if (self.onCloseCallback)
      self.onCloseCallback(item);
  }

  self.onDelete = function (item)
  {
    self.storage.delete(
      item.id,
      function ()
      {
        self.list.remove(item);
      });
  }

  self.addNewItem = function ()
  {
    var item = new CannedMessageModel(0, '', '', self.departmentId);
    self.editDialogModel.open(
      item,
      self.storage,
      function (x)
      {
        if (!x) return;

        var fs = getTrimmedFilterString();
        if (fs && !x.itemMatchFilter(fs))
          self.onResetSearch();

        self.list.push(x);
        scrollToTheEnd('dialog-canned-messages-list-container');
      });
  }

  self.onEdit = function (item)
  {
    self.editDialogModel.open(item, self.storage);
  }
}