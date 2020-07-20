'use strict';

/* global ko:true */
/* global _:true */
/* global moment:true */
/* global emoticons:true */
/* global model:true */

var model;

function ConsoleApp(agentSessionGuid, mediaSupport, pageTrackerUrl)
{

  function setupTextcomplete()
  {
    var textCompleteStart = '\\\\'; // string is encoded and regexp encoded

    $('.emoji-wysiwyg-editor').textcomplete(
        [
          {
            id: 'shortcut',
            debug: true,
            match: new RegExp(textCompleteStart + '([\\w]*)'),
            search: function (term, callback)
            {
              if (!model || !model.cannedMessageStorage)
                callback(null);
              else
                callback(model.cannedMessageStorage.lookup(term));
            },

            template: function (value)
            {
              return '<p>'
                + escapeHtml(textCompleteStart)
                + escapeHtml(value.key())
                + ' - '
                + escapeHtml(value.value())
                + '</p>';
            },
            replace: function (value)
            {
              return value.value();
            },
            index: 1
          }
        ],
        {
          onKeydown: function (e, commands)
          {
            if (e.ctrlKey && e.keyCode === 74)
            { // CTRL-J
              return commands.KEY_ENTER;
            }
          },
          appendTo: '#session-flow-container',
          placement: 'absleft:top',
        }
      );
  };

  this.start = function ()
  {
    var validationOptions = {
        insertMessages: true,
        decorateElement: true,
        errorElementClass: 'error',
        messagesOnModified: false,
        //debug: true,
        grouping: {
            deep: true,
            observable: false //Needed so added objects AFTER the initial setup get included
          },
      };
    ko.validation.init(validationOptions);

    setupMoment();
    setupSessionTabs();

    var hub = new AgentConsoleHubProxy(agentSessionGuid);

    model = new ConsoleModel(hub, mediaSupport, pageTrackerUrl);
    ko.applyBindings(model, document.getElementById('all-body'));

    setupTextcomplete();
  };


  function setupMoment()
  {
    var locale = window.navigator.userLanguage || window.navigator.language;
    console.log('set moment.js locale to ', locale);
    moment.locale(locale);
  }

  function setupSessionTabs()
  {
    $('#session-tab-buttons a[data-toggle="tab"]').click(
      function (e)
      {
        console.log('session tab', e);
        var target = $(e.currentTarget).attr('href');
        var active = $(target).hasClass('active');
        console.log(active);

        if (active)        
          model.sessionActionsPanel.toggle();
        else
          model.sessionActionsPanel.show();        
      });
  }
}