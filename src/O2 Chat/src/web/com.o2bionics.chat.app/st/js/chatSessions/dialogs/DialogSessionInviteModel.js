

function DialogSessionInviteModel()
{
  this.session = null;
  this.selected = ko.observable();

  this.dialog = createDialogOptions(570, 430);

  this.targets = ko.observableArray([]);

  this.actOnBehalfOfInvitor = ko.observable();
  this.message = ko.observable('');

  this.okEnabled = ko.pureComputed(
    function ()
    {
      return this.selected() && this.message().length > 0;
    },
    this);

  this.okCallback = null;

  this.onOk = function ()
  {
    this.dialog.isOpen(false);

    if (this.okCallback)
      this.okCallback(this.session, this.selected(), this.actOnBehalfOfInvitor() === true, this.message());
  };

  this.onCancel = function ()
  {
    console.log(this.actOnBehalfOfInvitor());
    this.dialog.isOpen(false);
  };

  this.show = function (session, agents, departments, selectedCallback)
  {
    var a = _.map(agents, function (x) { return new ChatSessionTargetModel(this, x, true); }.bind(this));
    a = _.concat(a, _.map(departments, function (x) { return new ChatSessionTargetModel(this, x, true); }.bind(this)));

    console.log(session.sessionInfo());

    this.session = session;
    this.targets(a);
    this.selected(null);
    this.actOnBehalfOfInvitor(null);
    this.message('');
    this.okCallback = selectedCallback;
    this.dialog.isOpen(true);
  }
}