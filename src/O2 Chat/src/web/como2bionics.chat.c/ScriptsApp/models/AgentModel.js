function AgentModel(agentInfo)
{
  this.skey = agentInfo.Skey;
  this.firstName = ko.observable(agentInfo.FirstName);
  this.lastName = ko.observable(agentInfo.LastName);
  this.avatar = ko.observable(agentInfo.Avatar);

  this.avatarUrl = ko.pureComputed(
    function () { return AvatarConstants.toAvatarUrl(this.avatar()); },
    this);
  this.name = ko.pureComputed(
    function () { return (this.firstName() + ' ' + this.lastName()).trim(); },
    this);
}