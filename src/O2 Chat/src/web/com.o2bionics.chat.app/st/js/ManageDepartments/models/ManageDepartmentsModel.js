'use strict';

function ManageDepartmentsModel(isOwner, currentUserId)
{
  var self = this;
  self.isOwner = isOwner;
  self.currentUserId = currentUserId;

  this.editMode = ko.observable();
  this.editItem = ko.observable(null);
  this.list = ko.observableArray();
  this.listErrorMessages = ko.observableArray([]);
  this.maxDepartments = ko.observable();

  this.isEditing = ko.pureComputed(function () { return self.editItem() !== null; });
  this.count = ko.pureComputed(
    function ()
    {
      return _.filter(self.list(), function (x) { return x.Id() > 0; }).length;
    });
  this.showCountWarning = ko.pureComputed(function () { return self.count() >= self.maxDepartments() - 5; });
  this.canAdd = ko.pureComputed(function () { return !self.isEditing() && self.count() < self.maxDepartments(); });

  this.dialogEditCannedMessage = new DialogEditCannedMessageModel();
  this.dialogCannedMessages = new DialogCannedMessagesModel(
    Enums.CannedMessageType.Department,
    this.dialogEditCannedMessage);

  this.templateToUse = function (item)
  {
    return self.editItem() !== item
             ? 'rowTmpl'
             : ('row' + self.editMode() + 'Tmpl');
  }


  function blockUi()
  {
    $('#objectTable').block({ message: null });
  }

  function unblockUi()
  {
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

  this.showEditTemplate = function (item)
  {
    if (self.isEditing()) return;

    self.editMode('Edit');
    self.editItem(item);
  }

  this.showCreateTemplate = function ()
  {
    if (!self.canAdd()) return;

    var x = new DepartmentModel(null, self);
    self.list.unshift(x);

    self.editMode('Edit');
    self.editItem(x);
  }

  this.showDeleteTemplate = function (item)
  {
    if (self.isEditing()) return;

    self.editMode('Delete');
    self.editItem(item);
  }
  

  this.cancelEdit = function ()
  {
    var ei = self.editItem();
    if (ei.Id() === 0)
    {
      var index = self.list.indexOf(ei);
      if (index >= 0) self.list.splice(index, 1);
    }
    ei.reset();

    self.editItem(null);
  }

  this.dialogCannedMessagesShow = function (department)
  {
    var departmentId = department.Id();

    var cmStorage = new CannedMessageStorage();
    cmStorage.loadDepartmentsMessages(
      departmentId,
      function ()
      {
        self.dialogCannedMessages.open(departmentId, cmStorage, null);
      });
  }

  this.create = function ()
  {
    var item = self.editItem();

    if (item.departmentPropertiesErrors().length > 0)
    {
      item.departmentPropertiesErrors.showAllMessages();
      return;
    }

    serverCall(
      item,
      'Department/Create',
      { dept: item.getData() });
  }

  this.update = function ()
  {
    var item = self.editItem();

    if (item.departmentPropertiesErrors().length > 0)
    {
      item.departmentPropertiesErrors.showAllMessages();
      return;
    }

    serverCall(
      item,
      'Department/Update',
      { dept: item.getData() });
  }

  this.delete = function ()
  {
    var item = self.editItem();

    serverCall(
      item,
      'Department/Delete',
      { deptId: item.Id() },
      function (x)
      {
        var index = self.list.indexOf(x);
        if (index >= 0) self.list.splice(index, 1);
      });
  }


  function serverCall(item, url, data, successCallback)
  {
    item.serverMessages([]);
    blockUi();

    $.ajax({
          method: 'POST',
          url: url,
          data: JSON.stringify(data),
          contentType: 'application/json; charset=utf-8',
          dataType: 'json',
        })
      .done(function (result)
      {
        console.log(result);

        if (result.Status.StatusCode === Enums.CallResultStatus.Success)
        {
          self.editItem(null);

          if (successCallback) successCallback(item);
          else if (result.Department) item.update(result.Department);
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

  this.refresh = function ()
  {
    this.load();
  }

  this.load = function ()
  {
    blockUi();

    $.ajax({
          method: "GET",
          url: "Department/GetAll",
          dataType: "json"
        })
      .done(function (data)
      {
        console.log(data);

        if (data.Status.StatusCode === Enums.CallResultStatus.Success)
        {
          self.listErrorMessages([]);

          var items = _.map(data.Departments, function (x) { return new DepartmentModel(x, self); });
          self.list(items);
          self.maxDepartments(data.MaxDepartments);
        }
        else
        {
          self.listErrorMessages([]);
          _.each(data.Status.Messages, function (x) { self.listErrorMessages.push(x.Message); });
        }

        unblockUi();
      })
      .fail(function (result)
      {
        console.error(result);
        unblockUi();
        toast('Server error.', '<p>Server call failed.</p>');
      });
  };
}