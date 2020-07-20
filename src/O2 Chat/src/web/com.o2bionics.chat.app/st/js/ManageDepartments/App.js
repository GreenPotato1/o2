'use strict';

/* global ko:true */
/* global _:true */
/* global moment:true */
/* global model:true */

var model;

function App(isOwner, currentUserId)
{
  this.start = function ()
  {
    ko.validation.init({
        errorElementClass: 'error',
        decorateElementOnModified: true,
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true
      },
      true);

    model = new ManageDepartmentsModel(isOwner, currentUserId);
    ko.applyBindings(model, document.getElementById('all-body'));
    model.load();
  };
}