
var entityMap = {
    "&": '&amp;',
    "<": '&lt;',
    ">": '&gt;',
    '"': '&quot;',
    "'": '&#39;',
    "/": '&#x2F;'
  };

function escapeHtml(string)
{
  return String(string)
    .replace(/[&<>"'\/]/g,
      function (s)
      {
        return entityMap[s];
      });
}


function showAlert(title, messageHtml, audioId)
{
  $.gritter.add({
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
    if (elt && typeof (elt.play) === 'function') elt.play();
  }
}


// ReSharper disable once NativeTypePrototypeExtending
Date.prototype.toMessageTimeString = function ()
{
  return moment(this).format('llll');
};

// ReSharper disable once NativeTypePrototypeExtending
Date.prototype.toVisitorDateTimeString = function ()
{
  return moment(this).format('llll');
};

// ReSharper disable once NativeTypePrototypeExtending
Date.prototype.toVisitorTimeString = function ()
{
  return moment(this).format('hh:mm a');
};

// ReSharper disable once NativeTypePrototypeExtending
Date.prototype.asUtc = function ()
{
  return new Date(
      this.getUTCFullYear(),
      this.getUTCMonth(),
      this.getUTCDate(),
      this.getUTCHours(),
      this.getUTCMinutes(),
      this.getUTCSeconds()
    );
};
Date.fromJson = function (v)
{
  if (!v) return null;
  var d = new Date(v);
//  console.log('date', v, d);
  return d;
};

function scrollToTheEnd(id)
{
  var el = document.getElementById(id);
  if (el) el.scrollTop = el.scrollHeight;
}

function createDialogOptions(height, width, isChildDialog, position)
{
  return {
      modal: true,
      autoOpen: false,
      height: (height == null ? 'auto' : height),
      width: (width == null ? 'auto' : width),
      resizable: false,
      draggable: true,
      closeOnEscape: true,
      position: position ? position : undefined,
      isOpen: ko.observable(false),
      create: function (event)
      {
        $(event.target).parent().css('position', 'fixed');
      },
      open: isChildDialog
              ? undefined
              : function ()
              {
                $('.ui-dialog').css('z-index', 7001);
                $('.ui-widget-overlay').css('z-index', 7000);
              }
    };
}

var selectionUtils = {};

selectionUtils.saveSelection = (function () {
  if (window.getSelection) {
    return function () {
      var sel = window.getSelection(), ranges = [];
      if (sel.rangeCount) {
        for (var i = 0, len = sel.rangeCount; i < len; ++i) {
          ranges.push(sel.getRangeAt(i));
        }
      }
      return ranges;
    };
  } else if (document.selection && document.selection.createRange) {
    return function () {
      var sel = document.selection;
      return (sel.type.toLowerCase() !== 'none') ? sel.createRange() : null;
    };
  }
})();

selectionUtils.restoreSelection = (function () {
  if (window.getSelection) {
    return function (savedSelection) {
      var sel = window.getSelection();
      sel.removeAllRanges();
      for (var i = 0, len = savedSelection.length; i < len; ++i) {
        sel.addRange(savedSelection[i]);
      }
    };
  } else if (document.selection && document.selection.createRange) {
    return function (savedSelection) {
      if (savedSelection) {
        savedSelection.select();
      }
    };
  }
})();

selectionUtils.replaceSelection = (function () {
  if (window.getSelection) {
    return function (content) {
      var range, sel = window.getSelection();
      var node = typeof content === 'string' ? document.createTextNode(content) : content;
      if (sel.getRangeAt && sel.rangeCount) {
        range = sel.getRangeAt(0);
        range.deleteContents();
        range.insertNode(document.createTextNode(' '));
        range.insertNode(node);
        range.insertNode(document.createTextNode(' '));
        range.setStart(node, 0);

        window.setTimeout(function () {
          range = document.createRange();
          range.setStartAfter(node);
          range.collapse(true);
          sel.removeAllRanges();
          sel.addRange(range);
        }, 0);
      }
    }
  } else if (document.selection && document.selection.createRange) {
    return function (content) {
      var range = document.selection.createRange();
      if (typeof content === 'string') {
        range.text = content;
      } else {
        range.pasteHTML(content.outerHTML);
      }
    }
  }
})();

(function ($)
{
  $.fn.getCursorPosition = function ()
  {
    var el = $(this).get(0);
    var pos = 0;
    if ('selectionStart' in el)
    {
      pos = el.selectionStart;
    }
    else if ('selection' in document)
    {
      el.focus();
      var sel = document.selection.createRange();
      var selLength = document.selection.createRange().text.length;
      sel.moveStart('character', -el.value.length);
      pos = sel.text.length - selLength;
    }
    return pos;
  }

  $.fn.setCursorPosition = function (pos)
  {
    var el = $(this).get(0);
    if (el.setSelectionRange)
    {
      el.setSelectionRange(pos, pos);
    }
    else if (el.createTextRange)
    {
      var range = el.createTextRange();
      range.collapse(true);
      if (pos < 0)
      {
        pos = $(el).val().length + pos;
      }
      range.moveEnd('character', pos);
      range.moveStart('character', pos);
      range.select();
    }
  }

  $.fn.insertTextAtCursorPosition = function (text)
  {
    var e = $(this);

    var position = e.getCursorPosition();
    var value = e.val();
    value = [value.slice(0, position), text, value.slice(position)].join('');
    e.val(value).change();
    e.setCursorPosition(position + text.length);
    return e;
  }
})(jQuery);

