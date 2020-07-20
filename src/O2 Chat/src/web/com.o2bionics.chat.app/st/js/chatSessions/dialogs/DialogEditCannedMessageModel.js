'use strict';

function DialogEditCannedMessageModel()
{
  var self = this;

  self.key = ko.observable();
  self.key
    .extend(
      {
        pattern: {
            message: 'The string must begin with a character and be at minimum 2 characters long',
            params: '^[a-zA-Z]{1}[a-zA-Z0-9]+$'
          }
      })
    .extend({ required: true });

  self.value = ko.observable();

  self.errors = ko.validation.group(self);

  self.editItem = null;
  self.storage = null;
  self.onCloseCallback = null;

  self.dialog = createDialogOptions(500, 300, true);


  self.open = function (item, storage, aCloseCallback)
  {
    self.onCloseCallback = aCloseCallback;
    self.storage = storage;
    self.editItem = item;
    self.key(item.key());
    self.value(item.value());

    $('#dialog-edit-canned-message-key').trigger('change');
    $('#dialog-edit-canned-message-value').trigger('change');

    self.dialog.isOpen(true);
    $('#dialog-edit-canned-message input')[0].focus();
  }

  function closeDialog(item)
  {
    self.dialog.isOpen(false);

    if (self.onCloseCallback)
      self.onCloseCallback(item);
  }

  self.onSave = function ()
  {
    if (self.errors().length > 0)
    {
      self.errors.showAllMessages();
      return;
    }

    var d = $.extend(
      self.editItem.asInfo(),
      {
        Key: self.key(),
        Value: self.value()
      });

    this.storage.save(
      d,
      function (update)
      {
        self.editItem.update(update);

        closeDialog(self.editItem);
      });
  }

  self.onCancel = function ()
  {
    closeDialog(null);
  }
}