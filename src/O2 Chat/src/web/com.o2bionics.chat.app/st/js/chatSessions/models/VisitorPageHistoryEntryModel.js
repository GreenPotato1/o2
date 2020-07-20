

function VisitorPageHistoryEntryModel(info, timeZone)
{
  this.id = info.Id;
  this.timestampStr = info.TimestampUtc;
  this.timestamp = new Date(info.TimestampUtc);
  this.url = info.Url;
  this.customText = info.CustomText;
    
  this.timestampText = Dt.default.asText(moment(this.timestamp));
  this.visitorZoneTimestamp = ko.pureComputed(function ()
  {
    var visitorTime = moment(info.TimestampUtc).utcOffset(timeZone.Offset);
    return Dt.default.asText(visitorTime.toDate()) + ', ' + timeZone.Description;
  });
    
  this.getUrlPass = getUrlPassFunc(this.url);    
    
  function getUrlPassFunc(u) {
      var curl = u;
      var curlNoHttp = curl.split('//');      
      var curlNoHttpSearch = curlNoHttp[1].split('?');
      var curlNoHttpSearchSlash = curlNoHttpSearch[0].split('/');
      if(curlNoHttpSearchSlash[1] == "") {
          return "default";
      } else {
          return curlNoHttpSearchSlash[1]; 
      }
           
  }  
    
}