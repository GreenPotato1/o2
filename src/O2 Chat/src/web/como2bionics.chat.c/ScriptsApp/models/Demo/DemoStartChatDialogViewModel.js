'use strict';

function DemoStartChatDialogViewModel(parentModel, ko)
{
  this.selectedDepartmentId = ko.observable();
  this.departments = ko.observableArray([]);
  this.markOnlineDepartments = function (el, x)
  {
    $(el).attr('class', x.isOnline() ? 'online-department' : 'offline-department');
  };
  this.isSelectedDepartmentOffline = ko.pureComputed(
    function ()
    {
      var skey = this.selectedDepartmentId();
      var dep = ko.utils.arrayFirst(this.departments(), function (x) { return x.skey === skey; });
      if (dep == null) return true;
      return !dep.isOnline();
    },
    this);


  this.visitorName = ko.observable('Visitor Name');
  this.visitorEmail = ko.observable('fn.surname@xyz.com');
  this.visitorPhone = ko.observable('+X-XXX-XXXXXXX');
  this.isVisitorKnown = ko.pureComputed(function () { return this.visitorName() && this.visitorName().length > 0; },
    this);

  this.message = ko.observable()
    .extend({ required: { message: Strings.validationMessageTextIsRequired } });

  this.onClickEditVisitorInfo = function ()
  {
  };

  this.onClickDeleteVisitorInfo = function ()
  {
  };

  this.onClickSendOfflineMessage = function ()
  {
  };

  this.sendOfflineMessage = function ()
  {
  }

  this.onClickStartChat = function ()
  {
  };

  this.onClickT1 = function ()
  {
  }
}