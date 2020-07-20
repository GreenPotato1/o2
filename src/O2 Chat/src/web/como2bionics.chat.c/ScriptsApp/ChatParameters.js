'use strict';

function ChatParameters(data)
{
  this.visitorExternalId = data.visitorExternalId;
  this.pageUrl = data.pageUrl;
  this.customText = data.customText;
}

ChatParameters.load = function (windowMessageHandler)
{
  var d = $.Deferred();

  var handler = windowMessageHandler.onParams(
    function (data)
    {
      try
      {
        var cp = new ChatParameters(data);
        d.resolve(cp);
      }
      catch (err)
      {
        console.log('ChatParameters', 'failed creating instance', data, err);
        d.reject(err);
      }
      handler.remove();
    });

  windowMessageHandler.postGetParams();
  return d;
}