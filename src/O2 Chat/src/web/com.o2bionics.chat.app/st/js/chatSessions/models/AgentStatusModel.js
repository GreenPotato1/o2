
function AgentStatusModel(parent)
{
  var self = this;
  var hub = parent.hub;

  this.connectionState = ko.observable($.signalR.connectionState.disconnected);

  this.agentName = ko.pureComputed(
    function ()
    {
      var agent = parent.agent();
      return agent ? agent.name() : '';
    },
    this);
  this.agentCheckboxSymbol = ko.pureComputed(
    function ()
    {
      var agent = parent.agent();
      var isOnline = agent && agent.isOnline();
      return isOnline ? '&#xE834;' : '&#xE835;';
    },
    this);
  this.agentStatusStyle = ko.pureComputed(
    function ()
    {
      var color;
      var cs = self.connectionState();
      if (cs === $.signalR.connectionState.disconnected) return { color: 'black' };
      else if (cs === $.signalR.connectionState.connecting) color = 'yellow';
      else color = (parent.agent() && parent.agent().isOnline()) ? '#43a047' : '#FF9800';
      return { color: color };
    },
    this);
  this.agentStatusText = ko.pureComputed(
    function ()
    {
      var cs = self.connectionState();
      if (cs === $.signalR.connectionState.disconnected) return 'Disconnected';
      else if (cs === $.signalR.connectionState.connecting) return 'Connecting...';
      else return (parent.agent() && parent.agent().isOnline()) ? 'В сети' : 'Нет на месте';
    },
    this);

  hub.onConnecting(function () { self.connectionState($.signalR.connectionState.connecting); }.bind(this));
  hub.onConnected(function () { self.connectionState($.signalR.connectionState.connected); }.bind(this));
  hub.onDisconnected(function () { self.connectionState($.signalR.connectionState.disconnected); }.bind(this));

  hub.onAgentStateChanged(
    function (onlineStatusInfo)
    {
      console.log(onlineStatusInfo);
      var agent = parent.agentStorage.get(onlineStatusInfo.Id);
      if (agent)
        agent.updateOnlineStatus(onlineStatusInfo);
      else
        console.log('unknown agent ' + onlineStatusInfo.Id);
    }.bind(this));

  this.onSwitchOnlineStatus = function ()
  {
    hub.sessionSetStatus(!parent.agent().isOnline());
  };
}