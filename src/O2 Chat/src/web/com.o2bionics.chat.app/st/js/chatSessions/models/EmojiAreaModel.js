'use strict';

function EmojiAreaModel(textAreaSelector, enterPressHandler)
{
  var self = this;

  this.divContainerSelector = '.emoji-wysiwyg-editor';

  this.selection = null;
  this.enterPressHandler = enterPressHandler;

  //trick to showing custom emoji selector
  this.wysiwyg = $(textAreaSelector).emojiarea({ button: '#smilesButton2' });

  $(self.divContainerSelector).attr('tabindex', 1);

  attachEvents();

  this.insertValueAtCursorPosition = function (value)
  {
    $(self.divContainerSelector).focus();

    if (self.selection) {
      selectionUtils.restoreSelection(self.selection);
    }

    try {
      selectionUtils.replaceSelection(value);
    }
    catch (e) {
      console.log(e);
    }

    $(self.wysiwyg[0]).change();   
  }

  function attachEvents() {
    $(self.wysiwyg).on('change', function () {
      $(this).val(getEmojiTxtValue());
    });

    //triks for FF handle selection to insert smile and canned message in correct place

    $(self.divContainerSelector).mouseup(function (e) {
      self.selection = selectionUtils.saveSelection();
    });

    $(self.divContainerSelector).keyup(function (e) {
      self.selection = selectionUtils.saveSelection();
    });
    // end tricks

    $(self.divContainerSelector).keypress(function (e) {
    if (e.ctrlKey && (e.keyCode === 10 || e.keyCode === 13)) // ^enter
    {
      if (self.enterPressHandler)
        self.enterPressHandler();
    }
  });
  }

  //copy with small modifications from jquery.emojiarea.js
  function getEmojiTxtValue() {
    var lines = [];
    var line = [];

    var flush = function () {
      lines.push(line.join(''));
      line = [];
    };

    var sanitizeNode = function (node) {

      var TEXT_NODE = 3;
      var ELEMENT_NODE = 1;
      var TAGS_BLOCK = ['p', 'div', 'pre', 'form'];

      if (node.nodeType === TEXT_NODE) {
        line.push(node.nodeValue);
      } else if (node.nodeType === ELEMENT_NODE) {
        var tagName = node.tagName.toLowerCase();
        var isBlock = TAGS_BLOCK.indexOf(tagName) !== -1;

        if (isBlock && line.length) flush();

        if (tagName === 'img') {
          var alt = node.getAttribute('alt') || '';
          if (alt) line.push(alt + ' ');
          return;
        } else if (tagName === 'br') {
          flush();
        }

        var children = node.childNodes;
        for (var i = 0; i < children.length; i++) {
          sanitizeNode(children[i]);
        }

        if (isBlock && line.length) flush();
      }
    };

    var children = $(self.divContainerSelector)[0].childNodes;
    for (var i = 0; i < children.length; i++) {
      sanitizeNode(children[i]);
    }

    if (line.length) flush();

    return lines.join('\n');
  };
}