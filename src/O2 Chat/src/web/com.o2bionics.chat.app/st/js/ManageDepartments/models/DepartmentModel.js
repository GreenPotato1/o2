
'use strict';

function DepartmentModel(data, parent)
{
  var self = this;

  this.parent = parent;
  this.source = data;

  this.Id = ko.observable();
  this.Status = ko.observable();
  this.IsPublic = ko.observable();
  this.Name = ko.observable()
    .extend({
        maxLength: 256,
        required: { message: 'Please enter department Name' }
      });
  this.Description = ko.observable()
    .extend({
        maxLength: 60,
        required: { message: 'Please enter Description' }
      });

  this.canEdit = ko.pureComputed(function ()
    {
      return this.parent.isOwner || !this.IsOwner();
    },
    this);


  this.serverMessages = ko.observableArray([]);

  this.departmentPropertiesErrors = ko.validation.group([
      this.Status,
      this.IsPublic,
      this.Name,
      this.Description
    ]);

  this.StatusText = ko.pureComputed(function ()
    {
      return this.Status() === 0
               ? 'Active'
               : this.Status() === 1
               ? 'Disabled'
               : this.Status() === 2
               ? 'Deleted'
               : 'Unkonwn';
    },
    this);

  this.IsDisabled = ko.pureComputed({
      read: function ()
      {
        return this.Status() === 1;
      },
      write: function (value)
      {
        var s = this.Status();
        if (value)
          this.Status(1);
        else
          this.Status(s === 1 ? 0 : s);
      },
      owner: this
    });

  this.reset();
}

DepartmentModel.prototype.update = function (data)
{
  this.source = data;
  this.reset();
}

DepartmentModel.prototype.reset = function ()
{
  var data = this.source;

  this.Id(data ? data.Id : 0);
  this.Status(data ? data.Status : 0);
  this.IsPublic(data ? data.IsPublic : true);
  this.Name(data ? data.Name : '');
  this.Description(data ? data.Description : '');

  this.serverMessages([]);
}

DepartmentModel.prototype.getData = function ()
{
  var department = {
      Id: this.Id(),
      Status: this.Status(),
      IsPublic: this.IsPublic(),
      Name: this.Name().trim(),
      Description: this.Description().trim(),
    };
  console.log(department);
  return department;
}