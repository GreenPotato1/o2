function AgentStorage()
{
  var storage = {};

  this.addList = function (agents)
  {
    var models = _.map(agents, function (x) { return new AgentModel(x) });
    _.each(models, function (x) { storage[x.skey] = x });
  }

  this.addOrUpdate = function (agentModel)
  {
    storage[agentModel.skey] = agentModel;
  }

  this.get = function (skey)
  {
    if (!skey) return null;

    var v = storage[skey];
    if (typeof v === 'undefined') return null;
    return v;
  }

  this.getAll = function ()
  {
    return _.values(storage);
  }

  this.getList = function (keys)
  {
    return _.filter(_.values(storage), function (x) { return _.indexOf(keys, x.skey) >= 0; });
  }
}