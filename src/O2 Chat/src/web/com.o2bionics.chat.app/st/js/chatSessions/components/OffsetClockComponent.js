'use strict';


function OffsetClockComponent(params)
{
  this.currentTime = ko.pureComputed(function ()
    {
      var offset = params.offset();
      if (offset === null)
        return 'Loading...';
      return moment(new Date().asUtc()).add(offset, 'm').toDate().toVisitorTimeString();
    },
    this);

  this.timerHandle = window.setInterval(function ()
    {
      this.currentTime();
    }.bind(this),
    20000);
}

OffsetClockComponent.prototype.dispose = function ()
{
  window.clearInterval(this.timerHandle);
}

ko.components.register('offset-clock-component',
  {
    viewModel: OffsetClockComponent,
    template: '<span data-bind="text: currentTime()"></span>',
  });