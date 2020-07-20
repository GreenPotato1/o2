

function AgentStorage()
{
  var storage = {};

  this.addList = function (list, onlineList)
  {
    _.each(list,
      function (x)
      {
        x.IsOnline = onlineList.indexOf(x.Id) >= 0;
      });

    var models = _.map(list, function (x) { return new AgentModel(x); });
    _.each(models, function (x) { storage[x.id] = x; });
  }

  this.add = function (agentModel)
  {
    storage[agentModel.id] = agentModel;
  }

  this.get = function (id)
  {
    if (!id) return null;

    var v = storage[id];
    if (typeof v === 'undefined') return null;
    return v;
  }

  this.getList = function (ids)
  {
    return _.filter(_.values(storage), function (x) { return _.indexOf(ids, x.id) >= 0; });
  }

  this.getAllBesides = function (ids)
  {
    return _.filter(_.values(storage), function (x) { return _.indexOf(ids, x.id) < 0; });
  }

  this.markDeleted = function (id)
  {
    var agent = storage[id];
    if (agent) agent.isDeleted(true);
  }
}