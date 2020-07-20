'use strict';

function DepartmentModel(info)
{
  this.id = info.Id;
  this.isOnline = ko.observable(info.IsOnline);
  this.isPublic = ko.observable(info.IsPublic);
  this.name = ko.observable(info.Name);
  this.description = ko.observable(info.Description);
  this.isDeleted = ko.observable(false);
  this.isActive = ko.observable(info.Status === 0);
}

DepartmentModel.prototype.update = function (info)
{
  if (this.id === 0)
    this.id = info.Id;
  else if (this.id !== info.Id)
    throw 'Department id mismatch ' + this.id + ' != ' + info.Id;

  this.isOnline(info.IsOnline);
  this.isPublic(info.IsPublic);
  this.name(info.Name);
  this.description(info.Description);
  this.isActive(info.Status === 0);
}

DepartmentModel.prototype.updateOnlineStatus = function (isOnline)
{
  this.isOnline(isOnline);
}

function DepartmentStorage()
{
  var storage = {};

  this.addList = function (list, onlineList)
  {
    _.each(list,
      function (x)
      {
        x.IsOnline = onlineList.indexOf(x.Id) >= 0;
      });

    var models = _.map(list, function (x) { return new DepartmentModel(x); });
    _.each(models, function (x) { storage[x.id] = x; });
  }

  this.add = function (departmentModel)
  {
    storage[departmentModel.id] = departmentModel;
  }

  this.get = function (id)
  {
    if (!id) return null;

    var v = storage[id];
    if (typeof v === 'undefined') return null;
    return v;
  }

  this.getAll = function ()
  {
    return _.values(storage);
  }

  this.markDeleted = function (id)
  {
    var dep = storage[id];
    if (dep) dep.isDeleted(true);
  }
}