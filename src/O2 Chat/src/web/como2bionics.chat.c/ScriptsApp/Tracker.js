'use strict';

function Tracker()
{
}

Tracker.add = function (url, customerId, visitorUniqueId, visitorExternalId, pageUrl, customText)
{
  var d = $.Deferred();
  var timezone = new Date().getTimezone();

  $.ajax({
        url: url + '/a',
        type: 'POST',
        crossDomain: true,
        dataType: 'json',
        data: {
            cid: customerId,
            vid: visitorUniqueId,
            veid: visitorExternalId,
            tzof: timezone.offset,
            tzde: timezone.description,
            u: pageUrl,
            ct: customText
          }
      })
    .done(function (r)
    {
      console.log(JSON.stringify(r));
      if (r != null && r.hasOwnProperty('error'))
        d.reject(r);
      else
        d.resolve(r);
    })
    .fail(function (r)
    {
      console.log('fail:', JSON.stringify(r));
      d.reject(r);
    });
  return d;
}