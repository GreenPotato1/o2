

function VisitorStorage()
{
  var storage = {};

  this.addList = function (list)
  {
    var models = _.map(list, function (x) { return new VisitorModel(x); });
    _.each(models, function (x) { storage[x.uniqueId] = x; });
  }

  this.get = function (uniqueId)
  {
    if (!uniqueId) return null;

    var v = storage[uniqueId];
    if (typeof v === 'undefined') return null;
    return v;
  }

  this.addOrUpdate = function (visitorInfo)
  {
    var v = storage[visitorInfo.UniqueId];
    if (typeof v === 'undefined' || v === null)
      storage[visitorInfo.UniqueId] = new VisitorModel(visitorInfo);
    else
      v.updateInfo(visitorInfo);
  }
}