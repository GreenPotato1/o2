
'use strict';

function UserModel(data, parent)
{
  var self = this;

  this.parent = parent;
  this.source = data;

  this.Id = ko.observable();
  this.Status = ko.observable();
  this.Email = ko.observable()
    .extend({
        maxLength: 256,
        email: { message: 'Please enter valid Email' },
        required: { message: 'Please enter user Email' }
      });
  this.FirstName = ko.observable()
    .extend({
        maxLength: 60,
        required: { message: 'Please enter First Name' }
      });
  this.LastName = ko.observable()
    .extend({
        maxLength: 60,
      });
  this.Password = ko.observable()
    .extend({
        required: { message: 'Please enter Password' }
      });
  this.Password2 = ko.observable()
    .extend({
        equal: this.Password
    });
  this.Avatar = ko.observable();
  this.IsOwner = ko.observable();
  this.IsAdmin = ko.observable();
  this.AgentDepartments = ko.observableArray();
  this.SupervisorDepartments = ko.observableArray()
    .extend({
        validation: {
            validator: function (val)
            {
              // console.log(self.IsOwner(), self.IsAdmin(), self.AgentDepartments(), val);
              return self.IsOwner() || self.IsAdmin() || self.AgentDepartments().length > 0 || val.length > 0;
            },
            message: 'Please provide any of access rights',
          }
      });

  this.AvatarUrl = ko.pureComputed(function () { return AvatarConstants.toAvatarUrl(self.Avatar()); });

  this.canEdit = ko.pureComputed(function ()
    {
      return this.parent.isOwner || !this.IsOwner();
    },
    this);


  this.serverMessages = ko.observableArray([]);

  this.userPropertiesErrors = ko.validation.group([
      this.Status,
      this.Email,
      this.FirstName,
      this.LastName,
      this.IsOwner,
      this.IsAdmin,
      this.AgentDepartments,
      this.SupervisorDepartments,
    ]);
  this.userPasswordErrors = ko.validation.group([
      this.Password,
      this.Password2
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

  this.agentDepartmentNames = ko.pureComputed(
    function ()
    {
      var depts = this.parent.departments();
      var names = _.map(
        this.AgentDepartments(),
        function (x)
        {
          var dept = _.find(depts, function (y) { return y.Id() === x; });
          return typeof (dept) === 'undefined' ? '' : dept.Name();
        });
      return names.join('\n');
    },
    this);

  this.supervisorDepartmentNames = ko.pureComputed(
    function ()
    {
      var depts = this.parent.departments();
      var names = _.map(
        this.SupervisorDepartments(),
        function (x)
        {
          var dept = _.find(depts, function (y) { return y.Id() === x; });
          return typeof (dept) === 'undefined' ? '' : dept.Name();
        });
      return names.join('\n');
    },
    this);

  this.reset();
}

UserModel.prototype.update = function (data)
{
  this.source = data;
  this.reset();
}

UserModel.prototype.reset = function ()
{
  var data = this.source;

  this.Id(data ? data.Id : 0);
  this.Status(data ? data.Status : 0);
  this.Email(data ? data.Email : '');
  this.FirstName(data ? data.FirstName : '');
  this.LastName(data ? data.LastName : '');
  this.Password('');
  this.Password2('');
  this.IsOwner(data ? data.IsOwner : false);
  this.IsAdmin(data ? data.IsAdmin : false);
  this.AgentDepartments(data ? data.AgentDepartments.slice(0) : []);
  this.SupervisorDepartments(data ? data.SupervisorDepartments.slice(0) : []);
  this.Avatar(data ? data.Avatar : '');

  this.serverMessages([]);
}

UserModel.prototype.getData = function ()
{
  var user = {
      Id: this.Id(),
      Status: this.Status(),
      Email: this.Email().trim(),
      FirstName: this.FirstName().trim(),
      LastName: this.LastName().trim(),
      IsOwner: this.IsOwner(),
      IsAdmin: this.IsAdmin(),
      AgentDepartments: this.AgentDepartments().slice(0),
      SupervisorDepartments: this.SupervisorDepartments().slice(0),
      Avatar: this.Avatar(),
    };
  console.log(user);
  return user;
}

UserModel.prototype.getPassword = function ()
{
  return this.Password();
}