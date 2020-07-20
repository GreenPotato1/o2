'use strict';

function ManageUserViewModel()
{
  var self = this;

  this.userModel = ko.observable();
  this.listErrorMessages = ko.observableArray([]);

  this.dialogSelectAvatar = new DialogSelectAvatar();
  this.areAvatarsAllowed = ko.observable();


  this.update = function ()
  {
    var item = self.userModel();
    if (!item) return;

    item.serverMessages([]);

    blockUi();
    $.ajax({
          method: 'POST',
          url: 'Account/Update',
          dataType: 'json',
          contentType: 'application/json; charset=utf-8',
          data: JSON.stringify(item.getData()),
        })
      .done(function (result)
      {
        console.log(result);
        if (result.Status.StatusCode === Enums.CallResultStatus.Success)
        {
          setCurrentUserAvatar(item.Avatar());
          setCurrentUserName(item.FirstName() + item.LastName());
          toast('Updated');
        }
        else
        {
          var messages = _.map(result.Status.Messages, function (x) { return x.Field + ': ' + x.Message; });
          item.serverMessages(messages);
        }
        unblockUi();
      })
      .fail(function (result)
      {
        console.error(result);
        item.serverMessages(['Server call failed.']);
        unblockUi();
        toast('Server error.', '<p>Server call failed.</p>');
      });
  }

  function blockUi() {
    $('#objectTable').block({ message: null });
  }

  function unblockUi() {
    $('#objectTable').unblock();
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

  this.uploadAvatar = function ()
  {
    if (!self.areAvatarsAllowed())
    {
      showAvatarsWarning();
      return;
    }
  }

  this.showSelectAvatarDialog = function ()
  {
    if (!self.areAvatarsAllowed())
    {
      showAvatarsWarning();
      return;
    }

    var item = self.userModel();
    if (!item) return;

    self.dialogSelectAvatar.show(
      item.Avatar(),
      function (newAvatar) { item.Avatar(newAvatar); });
  }

  function showAvatarsWarning()
  {
    toast('Warning', '<p>Feature disabled. Please upgrade your plan for enable selecting avatars.</p>');
  }
  
  this.load = function ()
  {
    $.ajax({
          method: 'GET',
          url: 'Account/Get',
          dataType: 'json'
        })
      .done(function (data)
      {
        console.log(data);

        self.userModel(new UserModel(data.UserInfo, self));
        self.areAvatarsAllowed(data.AreAvatarsAllowed);
      })
      .fail(function (data)
      {
        console.error(data);
        //unblockUi();
        toast('Server error.', '<p>Server call failed.</p>');
      });
  }
}