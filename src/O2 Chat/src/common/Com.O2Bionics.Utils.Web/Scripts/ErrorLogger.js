'use strict';

window.O2Bionics = window.O2Bionics || {};

(function ()
{
  function getStr(value)
  {
    return ('string' === typeof(value) && null != value && 0 < value.length) ? value : null;
  }

  function getErrorMessage(messageOrEvent, error)
  {
    var m = getStr(messageOrEvent);
    if (null != m)
      return m;

    if (null != error)
    {
      m = getStr(error.message);
      if (null != m)
        return m;
    }

    if (null != messageOrEvent && 'function' === typeof(messageOrEvent.toString))
    {
      m = getStr(messageOrEvent.toString());
    }
    return m;
  }

  function createErrorInfo(messageOrEvent, source, lineno, colno, error)
  {
    var errorInfo = {
        'ExceptionMessage': getErrorMessage(messageOrEvent, error),
        'Message': 'window.onerror'
      };
    if (null == errorInfo.ExceptionMessage)
      return null;

    var m;
    if (null != document.location)
    {
      m = getStr(document.location.href);
      if (null != m)
        errorInfo.Url = m;
    }

    //TODO: test in IE6 and 8. Should this information be in the "ExceptionStack"?
    //m = getStr(source);
    //if (null != m)
    //  errorInfo.ExceptionSource = 'source="' + m + '"';
    //if ('number' === typeof (lineno) && null != lineno)
    //  errorInfo.ExceptionSource += ', line=' + lineno.toString();
    //if ('number' === typeof (colno) && null != colno)
    //  errorInfo.ExceptionSource += ', col=' + colno.toString();

    if (null != error && null != error.stack && 'function' === typeof (error.stack.toString))
      errorInfo.ExceptionStack = error.stack.toString();

    return errorInfo;
  }

  window.O2Bionics.saveError = function (errorInfo)
  {
    try
    {
      if (null == errorInfo)
      {
        console.error('errorInfo must be not null.');
        return;
      }

      var app = window.application;
      if (null != app)
      {
        if ('number' === typeof (app.customerId) && 0 < app.customerId)
          errorInfo.CustomerId = app.customerId;
        if ('number' === typeof(app.visitorId) && 0 < app.visitorId)
          errorInfo.VisitorId = app.visitorId;
      }

      var d = new Date();
      errorInfo.TimeZoneOffset = -d.getTimezoneOffset();

      if ('function' === typeof (d.getTimezone))
      {
        var description = getStr(d.getTimezone().description);
        if (null != description && 0 < description.length)
          errorInfo.TimeZoneName = description;
      }

      var data = JSON.stringify(errorInfo);

      $.ajax(
        {
          url: '/postError',
          type: 'POST',
          data: data,
          error: function (xhr, status, error)
          {
            var msg = 'Error in saveError callback: ' + status;
            if (null != error)
            {
              msg += ', error= ' + error;
            }
            console.error(msg);
          }
        });
    }
    catch (e)
    {
      console.error('Error in saveError: ' + e);
    }
  }

  window.onerror = function (messageOrEvent, source, lineno, colno, error)
  {
    try
    {
      var errorInfo = createErrorInfo(messageOrEvent, source, lineno, colno, error);
      if (null != errorInfo) window.O2Bionics.saveError(errorInfo);
    }
    catch (e)
    {
      console.log(e);
    }
    return false;
  }
})();