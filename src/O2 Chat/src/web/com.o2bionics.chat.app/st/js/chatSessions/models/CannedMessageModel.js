'use strict';

function CannedMessageModel(id, key, value, departmentId)
{
  this.id = id;
  this.departmentId = departmentId;
  this.type = (!departmentId)
              ? Enums.CannedMessageType.Personal
              : Enums.CannedMessageType.Department;

  this.key = ko.observable(key);
  this.value = ko.observable(value);
}

CannedMessageModel.prototype.itemMatchFilter = function (fs)
{
  return this.key().toLowerCase().indexOf(fs) >= 0
    || this.value().toLowerCase().indexOf(fs) >= 0;
}

CannedMessageModel.prototype.update = function (info)
{
  if (this.id === 0)
    this.id = info.Id;
  else if (this.id !== info.Id)
    throw 'CannedMessage id mismatch ' + this.id + ' != ' + info.Id;

  this.key(info.Key);
  this.value(info.Value);
  this.departmentId = info.DepartmentId;
}

CannedMessageModel.prototype.asInfo = function ()
{
  return {
      Id: this.id,
      Key: this.key(),
      Value: this.value(),
      DepartmentId: this.departmentId
    };
}

function CannedMessageStorage()
{
  var self = this;

  this.items = [];

  this.lookup = function (term)
  {
    var matches = $.grep(
      this.items,
      function (x)
      {
        return x.key().toLowerCase().indexOf(term.toLowerCase()) === 0 ? x : null;
      });
    console.debug('search cm', term, this.items, matches);
    return matches;
  }

  this.delete = function (id, successCallback)
  {
    serverCall('POST', 'CannedMessages/Delete', { id: id })
      .done(
        function ()
        {
          if (successCallback) successCallback();
        })
      .fail(
        function ()
        {
          showAlert('Server error.', '<p>Server call failed.</p>');
        });
  }

  this.save = function (d, successCallback)
  {
    serverCall(
        'POST',
        d.Id === 0 ? 'CannedMessages/Create' : 'CannedMessages/Update',
        { data: d })
      .done(
        function (r)
        {
          if (successCallback) successCallback(r.CannedMessage);
        })
      .fail(
        function ()
        {
          showAlert('Server error.', '<p>Server call failed.</p>');
        });
  }

  function messagesFromInfo(infos)
  {
    return _.map(
      infos,
      function (x) { return new CannedMessageModel(x.Id, x.Key, x.Value, x.DepartmentId); });
  }

  this.loadUserMessages = function (successCallback)
  {
    serverCall(
        'GET',
        'CannedMessages/GetUserMessages',
        {})
      .done(
        function (r)
        {
          self.items = messagesFromInfo(r.CannedMessages);
          if (successCallback) successCallback();
        });
  }

  this.loadDepartmentsMessages = function (departmentId, successCallback)
  {
    serverCall(
        'GET',
        'CannedMessages/GetDepartmentMessages',
        { deptId: departmentId })
      .done(
        function (r)
        {
          self.items = messagesFromInfo(r.CannedMessages);
          if (successCallback) successCallback();
        });
  }

  function serverCall(method, url, data)
  {
    console.debug('call:', method, url, data);
    var d = $.Deferred();

    if (data && method !== 'GET') data = JSON.stringify(data);

    $.ajax(
        {
          type: method,
          url: url,
          data: data,
          contentType: method === 'GET' ? undefined : 'application/json; charset=utf-8',
          dataType: 'json'
        }).done(
        function (r)
        {
          if (r.Status !== undefined
            && r.Status.StatusCode !== undefined
            && r.Status.StatusCode !== Enums.CallResultStatus.Success)
          {
            console.error('call failed on the server', url, r);
            d.reject(r);
          }
          else
          {
            console.debug(r);
            d.resolve(r);
          }
        })
      .fail(
        function (r)
        {
          console.error(r);
          d.reject(r);
        });

    return d.promise();
  }
}