function DialogSelectAvatar()
{
  var self = this;

  var okCallback = null;

  this.predefinedAvatars = ko.observableArray();
  this.selected = ko.observable();

  this.dialog = createDialogOptions(570, 430);

  this.onOk = function ()
  {
    self.dialog.isOpen(false);
    okCallback(self.selected());
  }

  this.onCancel = function ()
  {
    self.dialog.isOpen(false);
  }

  this.onDblClick = function ()
  {
    self.onOk();
  }

  this.show = function (currentAvatar, anOkCallback)
  {
    okCallback = anOkCallback;
    loadAvatars();
    self.selected(currentAvatar);
    self.dialog.isOpen(true);
  }

  this.selectAvatar = function (a)
  {
    self.selected(a);
  }

  function toast(title, messageHtml)
  {
    $.gritter.add({
        title: title,
        text: messageHtml,
        sticky: false,
        time: 7000,
        class_name: 'gritter-custom'
      });
  }

  function loadAvatars()
  {
    $.ajax({
          method: 'GET',
          url: 'Account/GetAvatars',
          dataType: 'json'
        })
      .done(function (data)
      {
        console.log(data);
        self.predefinedAvatars(data);
      })
      .fail(function (data)
      {
        console.error(data);
        toast('Server error.', '<p>Server call failed.</p>');
      });
  }
}