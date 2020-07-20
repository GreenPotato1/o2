'use strict';

function StartChatDialogViewModel(parentModel, chatHub, ko)
{
  var self = this;

  this.selectedDepartmentId = ko.observable();
  this.departments = ko.observableArray([]);
  this.markOnlineDepartments = function (el, x)
  {
    $(el).attr('class', x.isOnline() ? 'online-department' : 'offline-department');
  };
  this.isSelectedDepartmentOffline = ko.pureComputed(
    function ()
    {
      var id = self.selectedDepartmentId();
      var dep = ko.utils.arrayFirst(self.departments(), function (x) { return x.id === id; });
      if (dep == null) return true;
      return !dep.isOnline();
    },
    this);


  this.visitorName = ko.observable();
  this.visitorEmail = ko.observable();
  this.visitorPhone = ko.observable();
  this.transcriptMode = ko.observable();

  this.isVisitorKnown = ko.pureComputed(function () { return self.visitorName() && self.visitorName().length > 0; },
    this);

  this.message = ko.observable()
    .extend({ required: { message: Strings.validationMessageTextIsRequired } });


  // chat events
  chatHub.onDepartmentStateChanged(
    function (changedStatusList)
    {
      console.log(changedStatusList);
      self.updateDepartmentsStatus(changedStatusList);
    }.bind(this));
  chatHub.onVisitorInfoChanged(
    function (wasRemoved, newName, newEmail, newPhone, newTranscriptMode)
    {
      if (wasRemoved)
      {
        self.visitorName('');
        self.visitorEmail('');
        self.visitorPhone('');
        self.transcriptMode(Enums.VisitorSendTranscriptMode.Ask);
      } else
      {
        if (newName) self.visitorName(newName);
        if (newEmail) self.visitorEmail(newEmail);
        if (newPhone) self.visitorPhone(newPhone);
        if (newTranscriptMode) self.transcriptMode(newTranscriptMode);
      }
    }.bind(this));


  // model events
  this.onClickEditVisitorInfo = function ()
  {
    parentModel.editVisitorInfoDialog.show(
        Strings.save,
        null,
        function ()
        {
          parentModel.dialog(Dialogs.startChat);
        }.bind(this),
        function ()
        {
          parentModel.dialog(Dialogs.startChat);
        }.bind(this)
      );
  };

  this.onClickDeleteVisitorInfo = function ()
  {
    parentModel.dialog(Dialogs.loading);
    chatHub.clearVisitorInfo()
      .then(function ()
      {
        self.visitorName('');
        self.visitorEmail('');
        self.visitorPhone('');
        self.transcriptMode(Enums.VisitorSendTranscriptMode.Ask);
        parentModel.dialog(Dialogs.startChat);
      }.bind(this))
      .fail(function ()
      {
        parentModel.dialog(Dialogs.startChat);
      });
  };

  this.onClickSendOfflineMessage = function ()
  {
    console.log(this.errors().length);
    if (this.errors().length > 0) return;


    if (!this.isVisitorKnown())
    {
      parentModel.editVisitorInfoDialog.show(
        'Send Message',
        Strings.messageEnterMissingVisitorDetailsOfflineMessage,
        function ()
        {
          self.sendOfflineMessage();
        }.bind(this),
        function ()
        {
          parentModel.dialog(Dialogs.startChat);
        }.bind(this));
    } else
      this.sendOfflineMessage();
  };

  this.sendOfflineMessage = function ()
  {
    parentModel.dialog(Dialogs.loading);

    var text = this.message();

    chatHub.sendOfflineMessage(
        this.selectedDepartmentId(),
        text)
      .done(function ()
      {
        self.message('');
        parentModel.offlineMessageSentDialog.show(
          function ()
          {
            parentModel.dialog(Dialogs.startChat);
          }.bind(this));
      }.bind(this))
      .fail(function ()
      {
        parentModel.dialog(Dialogs.startChat);
      }.bind(this));
  }

  this.onClickStartChat = function ()
  {
    
    console.log(this.errors().length);
    if (this.errors().length > 0) return;

    parentModel.dialog(Dialogs.loading);

    var text = this.message();
    chatHub.startChat(
        this.selectedDepartmentId(),
        text)
      .done(function ()
      {
        self.message('');
      }.bind(this))
      .fail(function ()
      {
        parentModel.dialog(Dialogs.startChat);
      }.bind(this));
  };

  this.onClickT1 = function ()
  {
    parentModel.dialog(Dialogs.offlineMessageSent);
  }


  // methods
  this.setData = function (serverData)
  {
    var modelDepartments = this.departments();
    modelDepartments.splice(0, modelDepartments.length);
    this.updateDepartments(serverData.Departments, serverData.OnlineDepartments);

    this.visitorName(serverData.Visitor.Name);
    this.visitorEmail(serverData.Visitor.Email);
    this.visitorPhone(serverData.Visitor.Phone);
    this.transcriptMode(serverData.Visitor.TranscriptMode);

    this.message('');
  };

  this.updateDepartments = function (deps, onlineDeps)
  {
    var modelDepartments = this.departments();

    $.each(deps,
      function (_, dept)
      {
        var current = ko.utils.arrayFirst(modelDepartments, function (x) { return x.id === dept.Id; });
        if (current != null)
        {
          current.name(dept.Name);
          current.isOnline(onlineDeps.indexOf(dept.Id) >= 0);
        } else
        {
          var d = {
              id: dept.Id,
              name: ko.observable(dept.Name),
              isOnline: ko.observable(onlineDeps.indexOf(dept.Id) >= 0)
            };
          d.title = ko.pureComputed(function ()
            {
              return this.name() + ' (' + (this.isOnline() ? Strings.online : Strings.offline) + ')';
            },
            d);
          modelDepartments.push(d);
        }
      });

    this.departments.valueHasMutated();
  };

  this.updateDepartmentsStatus = function (changedStatusList)
  {
    var modelDepartments = this.departments();

    $.each(changedStatusList,
      function (_, dept)
      {
        var current = ko.utils.arrayFirst(modelDepartments, function (x) { return x.id === dept.Id; });
        if (current != null)
          current.isOnline(dept.IsOnline);
      });

    this.departments.valueHasMutated();
  };
}