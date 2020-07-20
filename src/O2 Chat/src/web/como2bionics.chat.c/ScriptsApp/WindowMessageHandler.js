'use strict';

window.O2Bionics = window.O2Bionics || {};

function WindowMessageHandler()
{
  var events = new utils.Events();

  this.postGetParams = function ()
  {
    window.parent.postMessage('get-parameters', '*');
  }

  this.postShow = function (width, height, isMinimized)
  {
    parent.postMessage('show,' + width + ',' + height + ',' + (isMinimized ? '1' : '0'), '*');
  }

  this.postSetPosition = function (p)
  {
    var location = p.location;
    var offsetX = p.offsetX;
    var offsetY = p.offsetY;

    window.parent.postMessage('set-default-position,' + location + ',' + offsetX + ',' + offsetY, '*');
  };

  //
  // event handler can be unsubscribed by calling .remove()
  //
  //  var wmr = new WindowMessageReceiver()
  //  var h = wmr.onParams(function(pp) { ... });
  //  h.remove(); // unsubscribe
  //

  this.onParams = function (callback)
  {
    /// <signature><param name="callback(params)" type="Function">Callback.</param></signature>
    return events.on('params', callback);
  };

  function callParams(args)
  {
    events.emit('params', [args]);
  }

  this.onHeaderClick = function (callback)
  {
    /// <signature><param name="callback()" type="Function">Callback.</param></signature>
    return events.on('hdrclk', callback);
  };

  function callHeaderClick(args)
  {
    events.emit('hdrclk', [args]);
  }

  this.onAppearance = function (callback)
  {
    /// <signature><param name="callback(params)" type="Function">Callback.</param></signature>
    return events.on('appearance', callback);
  };

  function callAppearance(args)
  {
    events.emit('appearance', [args]);
  }


  $(window).on(
    'message',
    function (e)
    {
      console.log('WindowMessageHandler', 'received', e);

      if (typeof (e) === 'undefined') return;
      if (typeof (e.originalEvent) === 'undefined') return;
      if (typeof (e.originalEvent.data) === 'undefined') return;
      var data = e.originalEvent.data;
      if (!data) return;
      try
      {
        data = JSON.parse(e.originalEvent.data);
      }
      catch (err)
      {
        console.error('WindowMessageHandler', 'parse failed', err);
        return;
      }

      if (typeof (data.params) !== 'undefined')
      {
        console.log('WindowMessageHandler', 'params received', data.params);
        callParams(data.params);
      }
      if (typeof(data.hdrclck) !== 'undefined')
      {
        console.log('WindowMessageHandler', 'headerClick received', data.hdrclck);
        callHeaderClick(data.hdrclck);
      }
      if (typeof (data.appearance) !== 'undefined')
      {
        console.log('WindowMessageHandler', 'appearance received', data.appearance);
        callAppearance(data.appearance);
      }
      else if (typeof (data.error) !== 'undefined')
      {
        console.log('WindowMessageHandler', 'error received', data.error);
        if ('function' === typeof(window.O2Bionics.saveError))
          window.O2Bionics.saveError(data.error);
      }
    });
}