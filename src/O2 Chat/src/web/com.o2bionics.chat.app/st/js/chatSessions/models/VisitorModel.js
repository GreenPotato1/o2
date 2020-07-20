
function VisitorModel(visitorInfo)
{
  this.uniqueId = visitorInfo.UniqueId;
  this.addTimestamp = new Date(visitorInfo.AddTimestampUtc);
  this.timestamp = toTimestamp(this.addTimestamp);

  this.name = ko.observable(visitorInfo.Name);
  this.email = ko.observable(visitorInfo.Email);
  this.phone = ko.observable(visitorInfo.Phone);

  this.transcriptMode = ko.observable(visitorInfo.TranscriptMode);

  this.mediaSupport = ko.observable(visitorInfo.MediaSupport);
  this.mediaSupportVideo = ko.pureComputed(function () { return this.mediaSupport() >= Enums.MediaSupport.Video; },
    this);
  this.mediaSupportAudio = ko.pureComputed(function () { return this.mediaSupport() >= Enums.MediaSupport.Audio; },
    this);


  this.isTrackerInfoPolpulated = false;

  this.externalId = ko.observable('Loading..');
  this.timeZoneName = ko.observable(null);
  this.timeZoneOffset = ko.observable(null);
  this.location = ko.observable('Loading..');
  this.userAgent = ko.observable('Loading..');
  this.userAgentText = ko.observable('Loading..');

  this.pageHistory = ko.observableArray();
  this.hasMorePageHistory = ko.observable(false);
  this.pageHistorySearchPosition = null;

  function toTimestamp(d)
  {
    return d.getTime();
  }
}


VisitorModel.prototype.title = function ()
{
  var head = this.name();
  if (!head) head = '';
  var tail = this.email();
  tail = tail ? ' <' + tail + '>' : ' #' + this.uniqueId;
  return head + tail;
}

VisitorModel.prototype.loadTrackerInfo = function ()
{
  // TODO: move request to Tracker class
  var url = combineUrl(model.pageTrackerUrl, '/g');

  var pageHistoryPageSize = 20;
  var req = {
      cid: model.customerId,
      vid: this.uniqueId,
      sz: pageHistoryPageSize,
      sp: this.pageHistorySearchPosition,
    };
  
  $.ajax({
        url: url,
        type: 'GET',
        crossDomain: true,
        dataType: 'json',
        data: req
      })
    .done(function (r)
    {
      console.log(JSON.stringify(r));
      var v = r.Visitor;
      if (v)
      {
        this.externalId(v.VisitorExternalId);
        this.timeZoneName(v.TimeZone.Description);
        this.timeZoneOffset(v.TimeZone.Offset);

        var userAgent = '';
        var userAgentText = '';
        if (v.UserAgent)
        {
          userAgent = v.UserAgent.UserAgent + ' on ' + v.UserAgent.Os;
          userAgentText = v.UserAgent.UserAgentString;
        }
        this.userAgent(userAgent);
        this.userAgentText(userAgentText);

        var location = '';
        if (v.IpLocation)
        {
          if (v.IpLocation.City) location += v.IpLocation.City;
          if (v.IpLocation.Country) location += (location.length > 0 ? ', ' : '') + v.IpLocation.Country;
        }
        this.location(location);

        var history = this.pageHistory();
        if (r.Items){
          _.each(r.Items, function (x) { history.push(new VisitorPageHistoryEntryModel(x, v.TimeZone)); });
          this.pageHistory.valueHasMutated();
          console.log("pageHistory.len=" + this.pageHistory().length);
        }
      }

      $('#pageHistoryEnd')[0].scrollIntoView();

      this.pageHistorySearchPosition = r.SearchPosition ? r.SearchPosition.Values.join('|') : null;
      this.hasMorePageHistory(r.HasMore);

      this.isTrackerInfoPolpulated = true; //probably need to remove so that update info if subscription plan is changed
    }.bind(this))
    .fail(function (r)
    {
      console.log('error:', JSON.stringify(r));
    });
}

VisitorModel.prototype.updateInfo = function (visitorInfo)
{
  this.name(visitorInfo.Name);
  this.email(visitorInfo.Email);
  this.phone(visitorInfo.Phone);
  this.transcriptMode(visitorInfo.TranscriptMode);
  this.mediaSupport(visitorInfo.MediaSupport);
}

VisitorModel.prototype.updateInfo2 = function (wasRemoved, newName, newEmail, newPhone, newTrancriptMode)
{
  if (wasRemoved)
  {
    this.name(null);
    this.email(null);
    this.phone(null);
    this.transcriptMode(null);
  } else
  {
    if (newName) this.name(newName);
    if (newEmail) this.email(newEmail);
    if (newPhone) this.phone(newPhone);
    if (newTrancriptMode) this.transcriptMode(newTrancriptMode);
  }
}


VisitorModel.prototype.updateTrackerInfo = function (trackerInfo)
{
}