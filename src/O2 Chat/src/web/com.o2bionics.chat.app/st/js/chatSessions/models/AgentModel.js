
console.log('AgentModel1');

function AgentModel(agentInfo)
{
  this.id = agentInfo.Id;
  this.firstName = ko.observable(agentInfo.FirstName);
  this.lastName = ko.observable(agentInfo.LastName);
  this.email = ko.observable(agentInfo.Email);
  this.isOnline = ko.observable(agentInfo.IsOnline);
  this.isDeleted = ko.observable(false);
  this.isActive = ko.observable(agentInfo.Status === 0);
  this.departmentIds = [];
  this.avatar = ko.observable(agentInfo.Avatar);
  this.avatarUrl = ko.pureComputed(function () { return AvatarConstants.toAvatarUrl(this.avatar()); }, this);

  this.name = ko.pureComputed(
    function () { return this.firstName() + ' ' + this.lastName(); },
    this);
}

AgentModel.prototype.update = function (agentInfo, departmentIds)
{
  if (this.id === 0)
    this.id = agentInfo.Id;
  else if (this.id !== agentInfo.Id)
    throw 'Agent id mismatch ' + this.id + ' != ' + agentInfo.Id;

  this.firstName(agentInfo.FirstName);
  this.lastName(agentInfo.LastName);
  this.email = agentInfo.Email;
  this.isOnline(agentInfo.IsOnline);
  this.isActive(agentInfo.Status === 0);
  this.avatar(agentInfo.Avatar);
  
  if (departmentIds) this.departmentIds = departmentIds;
}

AgentModel.prototype.updateOnlineStatus = function (onlineInfo)
{
  if (this.id !== onlineInfo.Id)
    throw 'Agent id mismatch ' + this.id + ' != ' + onlineInfo.Id;

  this.isOnline(onlineInfo.IsOnline);
}

AgentModel.prototype.setDepartments = function (ids)
{
  this.departmentIds = ids;
}

AgentModel.prototype.belongsToDepartment = function (departmentId)
{
  return this.departmentIds.indexOf(departmentId) >= 0;
}

console.log('AgentModel 2');
