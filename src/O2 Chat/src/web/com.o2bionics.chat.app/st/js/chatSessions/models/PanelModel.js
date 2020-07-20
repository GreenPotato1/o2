

function PanelModel(id)
{
  var self = this;

  var isVisible = $(id).css('display') === 'block';
  console.log('panel', id, isVisible);
  this.visible = ko.observable(isVisible);

//  $(window).on('resize',
//    function ()
//    {
//      $(id).css("top", $(".m-tabs-row").height());
//      $(".active-xs").removeClass("active-xs");
//    });

  this.close = function ()
  {
    console.log('panel', id, 'close');
    self.visible(false);
//    $(id).removeClass("active-xs");
  }

  this.show = function ()
  {
    console.log('panel', id, 'show');

    self.visible(true);
//    $(id).toggleClass("active-xs");
  }

  this.toggle = function ()
  {
    console.log('panel', id, 'toggle');

    self.visible(!self.visible());
  }
}