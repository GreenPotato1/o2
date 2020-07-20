
var entityMap = {
    '&': '&amp;',
    "<": '&lt;',
    ">": '&gt;',
    '"': '&quot;',
    "'": '&#39;',
    "/": '&#x2F;'
  };

function escapeHtml(s)
{
  return String(s).replace(/[&<>"'\/]/g,
    function (x)
    {
      return entityMap[x];
    });
}

var entityUnmap = {
    '&amp;': '&',
    '&lt;': '<',
    '&gt;': '>',
    '&quot;': '"',
    '&#39;': "'",
    '&#x2F;': '/'
  };

function unescapeHtml(s)
{
  return String(s).replace(/&amp;|&lt;|&gt;|&quot;|&#39;|&#x2F;/gi,
    function (x)
    {
      return entityUnmap[x];
    });
}

function escapeHtmlLight(string) {
    return String(string)
        .replace(/[<>]/g,
        function (s) {
            return entityMap[s];
        });
}

function combineUrl(base, path)
{
  base = base || '';
  path = path || '';
  if (base.length === 0 || base[base.length - 1] !== '/') base += '/';
  if (path.length > 0 && path[0] === '/') path = path.slice(1);
  return base + path;
}

function parseUrls(string) {
    return Autolinker.link(string, {
        email: true,
        truncate: { length: 50, location: 'end' }
    });
}

// ReSharper disable once NativeTypePrototypeExtending


Date.prototype.toMessageTimeString = function ()
{
  var h = this.getHours();
  var m = this.getMinutes();
  var suffix = (h >= 12) ? 'pm' : 'am';
  h = (h > 12) ? h - 12 : h;
  h = (h === 0) ? 12 : h;
  m = m < 10 ? ('0' + m) : ('' + m);
  return h + ':' + m + ' ' + suffix;
};

// ReSharper disable once NativeTypePrototypeExtending
Date.prototype.getTimezone = function ()
{
  var ts = this.toTimeString();
  var start = ts.indexOf('GMT');
  if (start < 0) start = ts.indexOf('UTC');
  return { offset: -this.getTimezoneOffset(), description: start >= 0 ? ts.substr(start) : '' };
};

// ReSharper disable once NativeTypePrototypeExtending
Date.prototype.toLogTimestampString = function ()
{
  var h = this.getHours();
  var m = this.getMinutes();
  var s = this.getSeconds();
  var f = this.getMilliseconds();

  h = (h < 10 ? '0' : '') + h;
  m = (m < 10 ? '0' : '') + m;
  s = (s < 10 ? '0' : '') + s;
  f = '' + f;
  while (f.length < 3) f = '0' + f;
  return h + ':' + m + ':' + s + '.' + f;
}