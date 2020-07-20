'use strict';

function DialogSmileModel()
{
  var self = this;

  var emoticonsPath = 'st/i/emotify/';
  var emoticons = {
      ':-)': ['happy.png', 'happy', ':)', '(:', '^_^', ')))'],
      ':-(': ['sad.png', 'sad', ':(', '=(', '=-(', '):'],
      ':D': ['grin.png', 'grin', 'LOL'],
      ':|': ['neutral.png', 'neutral', '|:'],
      ':o': ['suprised.png', 'suprised', 'o:', '0_0'],
      ':&#39;(': ['cry.png', 'cry', ':-((', ':(('],
      ':?': ['worried.png', 'worried'],
      ':&#x2F;': ['puzzled.png', 'puzzled', '&#x2F;:']
    };
  var dialogRowLength = 4;

  self.smiles = ko.observableArray(buildEmoticonsList());

  self.dialog = createDialogOptions(
    null,
    300,
    null,
    { my: 'bottom', at: 'bottom', of: $('#smilesButton') });


  self.rows = ko.pureComputed(
    function ()
    {
      var items = self.smiles();
      $.emojiarea.path = emoticonsPath;
      $.emojiarea.icons = emoticons;

      return [].concat.apply(
          [],
          ko.utils.arrayMap(
            items,
            function (elem, i)
            {
              return i % dialogRowLength ? [] : [items.slice(i, i + dialogRowLength)];
            })
        );
    },
    this);

  function buildEmoticonsList()
  {
    emotify.emoticons(emoticonsPath, true, emoticons);
    return _.map(
      emoticons,
      function (v, key)
      {
        return {
            text: key,
            image: v[0],
            desc: v[1],
            onClick: function (x)
            {
              self.onSelectItem(x);
              return;
            }
          }
      });
  }

  self.onSelectItem = function (item)
  {
    self.dialog.isOpen(false);

    if (self.onCloseCallback)
      self.onCloseCallback(item);
  }

  self.open = function (aCloseCallback)
  {
    self.onCloseCallback = aCloseCallback;
    self.dialog.isOpen(true);
  }
}