'use strict';

// see http://stackoverflow.com/questions/14573223/set-cookie-and-get-cookie-with-javascript

var Cookies =
  {
    get: function (sKey)
    {
      return decodeURIComponent(
          document.cookie.replace(
            new RegExp(
              '(?:(?:^|.*;)\\s*'
              + encodeURIComponent(sKey).replace(/[\-\.\+\*]/g, '\\$&')
              + '\\s*\\=\\s*([^;]*).*$)|^.*$'),
            '$1'))
        || null;
    },
    set: function (sKey, sValue, vEnd, sPath, sDomain, bSecure)
    {
      if (!sKey || /^(?:expires|max\-age|path|domain|secure)$/i.test(sKey))
      {
        return false;
      }
      var sExpires = '';
      if (vEnd)
      {
        switch (vEnd.constructor)
        {
        case Number:
          sExpires = vEnd === Infinity ? '; expires=Fri, 31 Dec 9999 23:59:59 GMT' : '; max-age=' + vEnd;
          break;
        case String:
          sExpires = '; expires=' + vEnd;
          break;
        case Date:
          sExpires = '; expires=' + vEnd.toUTCString();
          break;
        }
      }
      document.cookie = encodeURIComponent(sKey)
        + '='
        + encodeURIComponent(sValue)
        + sExpires
        + (sDomain ? '; domain=' + sDomain : '')
        + (sPath ? '; path=' + sPath : '')
        + (bSecure ? '; secure' : '');
      return true;
    },
    remove: function (sKey, sPath, sDomain)
    {
      if (!sKey || !this.hasItem(sKey))
      {
        return false;
      }
      document.cookie = encodeURIComponent(sKey)
        + '=; expires=Thu, 01 Jan 1970 00:00:00 GMT'
        + (sDomain ? '; domain=' + sDomain : '')
        + (sPath ? '; path=' + sPath : '');
      return true;
    },
    has: function (sKey)
    {
      return (new RegExp('(?:^|;\\s*)' + encodeURIComponent(sKey).replace(/[\-\.\+\*]/g, '\\$&') + '\\s*\\='))
        .test(document.cookie);
    },
    keys: /* optional method: you can safely remove it! */ function ()
    {
      var aKeys = document.cookie
        .replace(/((?:^|\s*;)[^\=]+)(?=;|$)|^\s*|\s*(?:\=[^;]*)?(?:\1|$)/g, '')
        .split(/\s*(?:\=[^;]*)?;\s*/);
      for (var i = 0; i < aKeys.length; i++)
      {
        aKeys[i] = decodeURIComponent(aKeys[i]);
      }
      return aKeys;
    }
  }