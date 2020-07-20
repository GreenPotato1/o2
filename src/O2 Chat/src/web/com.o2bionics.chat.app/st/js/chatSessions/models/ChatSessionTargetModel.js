
function ChatSessionTargetModel(parent, obj, disableParticipants)
{
  this.parent = parent;
  this.value = obj;

  this.skey = obj.skey;

  this.isAgent = ko.pureComputed(function () { return obj instanceof AgentModel; });
  this.name = ko.pureComputed(function () { return this.value.name(); }, this);
  this.isOnline = ko.pureComputed(function () { return this.value.isOnline(); }, this);
  this.isDisabled = ko.pureComputed(function ()
    {
      if (!disableParticipants) return false;
      return this.isAgent()
               ? parent.session.isAgentParticipating(this.skey) || parent.session.isAgentInvited(this.skey)
               : parent.session.isDepartmentInvited(this.skey);
    },
    this);

  this.iconStyle = ko.pureComputed(function () { return { color: this.isOnline() ? 'green' : 'red' }; }, this);
  this.iconCss = { 'fa-user': this.isAgent(), 'fa-users': !this.isAgent() };

  this.css = ko.pureComputed(function ()
    {
      return {
          'selected': parent.selected() === this.value,
          'disabled': this.isDisabled(),
        };
    },
    this);

  this.attr = ko.pureComputed(function ()
    {
      if (!disableParticipants || !this.isDisabled()) return false;
      return { 'title': 'Already invited or participating' };
    },
    this);
}

ChatSessionTargetModel.prototype.onClick = function ()
{
  if (this.isDisabled()) return false;
  this.parent.selected(this.value);
  return true; // allows radiobutton check
}