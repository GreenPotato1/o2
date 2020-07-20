
function DialogStartSessionModel()
{
  var self = this;

  this.selected = ko.observable().extend({ required: true });

  this.dialog = createDialogOptions(570, 430);

  this.targets = ko.observableArray([]);

  this.message = ko.observable('').extend({ required: true });

  this.errors = ko.validation.group(self);

  this.okEnabled = ko.pureComputed(
    function ()
    {
      return self.errors().length === 0;
    });


  this.okCallback = null;

  this.onOk = function ()
  {
    self.dialog.isOpen(false);

    if (self.okCallback)
      self.okCallback(self.selected(), self.message());
  };

  this.onCancel = function ()
  {
    self.dialog.isOpen(false);
  };

  this.show = function (agents, departments, selectedCallback)
  {
    var a = _.map(agents, function (x) { return new ChatSessionTargetModel(self, x); });
    a = _.concat(a, _.map(departments, function (x) { return new ChatSessionTargetModel(self, x); }));

    self.targets(a);
    self.selected(null);
    self.message('');
    self.okCallback = selectedCallback;
    self.dialog.isOpen(true);
  }
}