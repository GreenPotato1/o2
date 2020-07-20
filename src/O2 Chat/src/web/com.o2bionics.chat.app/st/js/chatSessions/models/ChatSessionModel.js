'use strict';

function ChatSessionModel(sessionInfo, model)
{
  var self = this;

  this.skey = sessionInfo.Skey;
  this.addTimestamp = new Date(sessionInfo.AddTimestampUtc);

  this.sessionInfo = ko.observable(sessionInfo);
  this.visitor = sessionInfo.VisitorId ? model.visitorStorage.get(sessionInfo.VisitorId) : null;
  this.morePageHistoryButtonText = 'Load More';
  this.transcriptSentTime = ko.observable();

  this.otherAgents = ko.pureComputed(
    function ()
    {
      var ids = _.map(self.sessionInfo().Agents, function (x) { return x.AgentId; });
      var currentAgentId = model.agent().id;
      var otherAgentIds = _.filter(skeys, function (x) { return x !== currentAgentId; });
      return model.agentStorage.getList(otherAgentIds);
    },
    this);

  this.invites = ko.pureComputed(
    function ()
    {
      return _.map(
        self.sessionInfo().Invites,
        function (x)
        {
          return {
              createdTime:
                new Date(x.CreatedTimestampUtc),
              createdBy:
                x.CreatorAgentId
                  ? 'Agent ' + model.agentStorage.get(x.CreatorAgentId).name()
                  : 'Visitor',
              createdFor:
                x.InviteType === Enums.ChatSessionInviteType.Agent
                  ? ('Agent ' +
                    model.agentStorage.get(x.AgentId).name() +
                    (x.ActOnBehalfOfAgentId
                       ? ('(' + model.agentStorage.get(x.ActOnBehalfOfAgentId).name() + ')')
                       : ''))
                  : 'Department ' + model.departmentStorage.get(x.DepartmentId).name(),
              status:
                x.AcceptedByAgentId
                  ? 'accepted by ' + model.agentStorage.get(x.AcceptedByAgentId).name()
                  : x.CanceledByAgentId
                  ? 'canceled by ' + model.agentStorage.get(x.CanceledByAgentId).name()
                  : 'now pending',
              statusChangeTime:
                x.AcceptedByAgentId
                  ? new Date(x.AcceptedTimestampUtc)
                  : x.CanceledByAgentId
                  ? new Date(x.CanceledTimestampUtc)
                  : null,
            };
        });
    },
    this);

  this.agents = ko.pureComputed(
    function ()
    {
      return _.map(
        self.sessionInfo().Agents,
        function (x)
        {
          return model.agentStorage.get(x.AgentId).name() +
            (x.ActsOnBehalfOfAgentId ? ('(' + model.agentStorage.get(x.ActsOnBehalfOfAgentId).name() + ')') : '');
        });
    },
    this);

  this.isNewSession = ko.pureComputed(
    function ()
    {
      var si = self.sessionInfo();
      if (si.Status === Enums.ChatSessionStatus.Completed) return false;
      var currentAgentId = model.agent().id;
      return _.every(si.Agents, function (x) { return x.AgentId !== currentAgentId; });
    },
    this);

  this.isCompleted = ko.pureComputed(
    function ()
    {
      return self.sessionInfo().Status === Enums.ChatSessionStatus.Completed;
    },
    this);

  this.isOnline = ko.pureComputed(
    function ()
    {
      var si = self.sessionInfo();
      return !si.IsOffline;
    },
    this);

  this.title = ko.pureComputed(
    function ()
    {
      var title = self.skey;
      var si = self.sessionInfo();

      if (self.isNewSession())
      {
        var lastPendingInvite = _.last(si.Invites, function (x) { return isInvitePending(x); });
        if (lastPendingInvite)
        {
          if (lastPendingInvite.CreatorAgentId)
            title += ' - ' + model.agentStorage.get(lastPendingInvite.CreatorAgentId).name();
          else if (self.visitor)
            title += ' - ' + self.visitor.title();
        }
      } else
      {
        var participants = [];
        if (self.visitor) participants.push(self.visitor.title());
        var currentAgentId = model.agent().id;
        _.forEachRight(
          si.Agents,
          function (x)
          {
            if (x.AgentId !== currentAgentId) participants.push(model.agentStorage.get(x.AgentId).name());
          });
        _.forEachRight(
          si.Invites,
          function (x)
          {
            if (isInvitePending(x))
            {
              if (x.InviteType === Enums.ChatSessionInviteType.Agent)
                participants.push(model.agentStorage.get(x.AgentId).name());
              else if (x.InviteType === Enums.ChatSessionInviteType.Department)
                participants.push(model.departmentStorage.get(x.DepartmentId).name());
            }
          });
        if (participants.length > 0)
          title += ' - ' + participants.join(', ');
      }

      return title;
    },
    this);

  this.starter = ko.pureComputed(
    function ()
    {
      if (self.visitor !== null)
        return 'Visitor ' + self.visitor.title();

      var agents = self.sessionInfo().Agents;
      if (agents.length > 0)
      {
        var agent = model.agentStorage.get(agents[0].AgentId);
        return 'Agent ' + (agent ? agent.name() : 'Unknown');
      }
      return 'Unknown';
    },
    this);

  this.isStartedOnline = ko.pureComputed(
    function ()
    {
      return !self.sessionInfo().IsOffline;
    },
    this);

  this.target = ko.pureComputed(
    function ()
    {
      var invite = self.sessionInfo().Invites[0];
      if (invite.InviteType === Enums.ChatSessionInviteType.Department)
        return 'To Department ' + model.departmentStorage.get(invite.DepartmentId).name();
      else if (invite.InviteType === Enums.ChatSessionInviteType.Agent)
        return 'To Agent ' + model.agentStorage.get(invite.AgentId).name();
      else return 'Unknown';
    },
    this);

  this.isAgentParticipating = function (agentId)
  {
    return _.some(this.sessionInfo().Agents, function (x) { return x.AgentId === agentId; });
  };

  function isInvitePending(x)
  {
    return x.AcceptedByAgentId === null && x.CanceledByAgentId === null;
  }

  function isAgentInvited(x, agentId)
  {
    return x.InviteType === Enums.ChatSessionInviteType.Agent && x.AgentId === agentId;
  }

  function isDepartmentInvited(x, idOrAgent)
  {
    if (x.InviteType !== Enums.ChatSessionInviteType.Department) return false;
    if (idOrAgent instanceof AgentModel)
      return idOrAgent.belongsToDepartment(x.DepartmentId);
    else
      return x.DepartmentId === idOrAgent;
  }

  this.isAgentInvited = function (id)
  {
    return _.some(
      this.sessionInfo().Invites,
      function (x) { return isInvitePending(x) && isAgentInvited(x, id); });
  }

  this.isDepartmentInvited = function (id)
  {
    return _.some(
      this.sessionInfo().Invites,
      function (x) { return isInvitePending(x) && isDepartmentInvited(x, id); });
  }

  this.findAgentInvite = function (agent)
  {
    return _.find(
      this.sessionInfo().Invites,
      function (x) { return isInvitePending(x) && isAgentInvited(x, agent.id); });
  }

  this.findDepartmentInvite = function (agent)
  {
    return _.find(
      this.sessionInfo().Invites,
      function (x) { return isInvitePending(x) && isDepartmentInvited(x, agent); });
  }

  this.canWrite = ko.pureComputed(
    function ()
    {
      var currentAgentId = model.agent().id;
      var si = self.sessionInfo();
      return self.isAgentParticipating(currentAgentId) && si.Status !== Enums.ChatSessionStatus.Completed;
    },
    this);

  this.visitorHasVideo = ko.pureComputed(
    function ()
    {
      return self.visitor && self.visitor.mediaSupportVideo() && !self.sessionInfo().IsOffline;
    },
    this);
  this.visitorHasAudio = ko.pureComputed(
    function ()
    {
      return self.visitor && self.visitor.mediaSupportAudio() && !self.sessionInfo().IsOffline;
    },
    this);

  this.canAccept = ko.pureComputed(
    function ()
    {
      var currentAgentId = model.agent().id;
      var si = self.sessionInfo();
      return !self.isAgentParticipating(currentAgentId) && si.Status !== Enums.ChatSessionStatus.Completed;
    },
    this);

  this.messages = ko.observableArray([]);
  this.unreadMessagesCount = ko.observable(0);


  this.update = function (sessionInfo, messages)
  {
    if (sessionInfo)
    {
      if (this.skey !== sessionInfo.Skey)
        throw 'Invalid update session.Skey=' + this.skey + ' with sessionInfo.Skey=' + sessionInfo.Skey;

      this.sessionInfo(sessionInfo);

      if (sessionInfo.Messages)
      {
        var m1 = _.map(sessionInfo.Messages,
          function (messageInfo) { return new ChatMessageModel(model, messageInfo); }.bind(this));
        var m2 = _.sortBy(m1, 'id');
        this.messages(m2);
      }

      this.transcriptSentTime(sessionInfo.VisitorTranscriptTimestampUtc
                              ? new Date(sessionInfo.VisitorTranscriptTimestampUtc)
                              : null);
    }

    if (messages)
    {
      var cm = this.messages();
      _.each(messages,
        function (messageInfo)
        {
          var m = new ChatMessageModel(model, messageInfo);
          var index = _.findIndex(cm, { id: m.id });
          if (index >= 0)
            cm[index] = m;
          else
          {
            var index1 = _.sortedIndexBy(cm, m.id, function (x) { return x.id; });
            cm.splice(index1, 0, m);
          }
        }.bind(this));
      this.messages.valueHasMutated();

      var cs = model.currentSession();
      if (cs !== null && cs.skey === this.Skey)
        model.scrollToLastMessage();
      else
        this.unreadMessagesCount(this.unreadMessagesCount() + messages.length);
    }
  }

  this.activateSessionTab = function ()
  {
    var tab = self.isNewSession() ? 'new' : 'active';
    var headerId = 'a#' + tab + '-sessions-tab-header';
    if (!$(headerId).parent().hasClass('active'))
      $(headerId).tab('show');
  }

  this.onSelectSession = function ()
  {
    console.log(this);
    console.log(this.sessionInfo());

    //onselectsession - call to select session  update - accordion 1 - check status and update only when needed 
    model.hub.getFullChatSessionInfo(this.skey)
      .done(function (sessionInfo)
      {
        self.update(sessionInfo);

        self.unreadMessagesCount(0);

        if (self.visitor && !self.visitor.isTrackerInfoPolpulated)
          self.visitor.loadTrackerInfo();

        var activeSessionInfoTab = $('#current-session-info-tab-headers li.active a').attr('id');

        model.currentSession(this);
        model.messageText('');
        model.messageTextDisable(false);
        $(model.emojiAreaModel.divContainerSelector).html('').focus();

        model.scrollToLastMessage();

        self.activateSessionTab();

        if (_.isUndefined(activeSessionInfoTab)) activeSessionInfoTab = 'session-info-tab-header';
        $('#' + activeSessionInfoTab).tab('show');

        window.location.hash = '' + self.skey;
      }.bind(this));
  };
}