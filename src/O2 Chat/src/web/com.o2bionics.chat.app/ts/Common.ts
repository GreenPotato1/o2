import * as Autolinker from 'autolinker'

export function dateFromJson(s: string | null): Date | null
{
  return s === null ? null : new Date(s);
}


const entityMap = new Map<string, string>(
  [
    ['&', '&amp;'],
    ['<', '&lt;'],
    ['>', '&gt;'],
    ['"', '&quot;'],
    ["'", '&#39;'],
    ['/', '&#x2F;'],
  ]);

export function escapeHtml(s: string): string
{
  return s.replace(/[&<>"'\/]/g, (x: string) => entityMap.get(x)!);
}

export function escapeHtmlLight(s: string): string
{
  return s.replace(/[<>]/g, (x: string) => entityMap.get(x)!);
}

export function combineUrl(base: string, path: string)
{
  if (base.length === 0 || base[base.length - 1] !== '/') base += '/';
  if (path.length > 0 && path[0] === '/') path = path.slice(1);
  return base + path;
}

export function parseUrls(string: string): string
{
  return Autolinker.link(
    string,
    {
      email: true,
      truncate: { length: 50, location: 'end' }
    });
}

export function showAlert(title: string, messageHtml: string, audioId?: string)
{
  $.gritter.add(
    {
      // (string | mandatory) the heading of the notification
      title: title,
      // (string | mandatory) the text inside the notification
      text: messageHtml,
      // (string | optional) the image to display on the left
      image: '',
      // (bool | optional) if you want it to fade out on its own or just sit there
      sticky: false,
      // (int | optional) the time you want it to be alive for before fading out
      time: 5000,
      // (string | optional) the class name you want to apply to that specific message
      class_name: 'gritter-custom'
    });

  if (audioId != null)
  {
    var elt = document.getElementById(audioId);
    //if (elt && typeof elt.play === 'function') elt.play();
  }
}

export function scrollToTheEnd(id: string)
{
  var el = document.getElementById(id);
  if (el) el.scrollTop = el.scrollHeight;
}