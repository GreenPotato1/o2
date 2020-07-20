import * as Contract from '../contract/Contract';
import * as TrackerContract from '../contract/TrackerContract';
import Dt from '../Dt';
import * as moment from '../typings/moment/moment';

class VisitorPageHistoryEntryModel {

  public id: string;
  public timestamp: Date;
  public timestampStr: string;
  public url: string;
  public customText: string;
  public visitorZoneTimestamp: string;

  public constructor(info: TrackerContract.PageHistoryRecord, timeZone: TrackerContract.TimeZoneDescription)
  {
    this.id = info.Id;
    this.timestampStr = info.TimestampUtc;
    this.timestamp = new Date(info.TimestampUtc);
    this.url = info.Url;
    this.customText = info.CustomText;

    var visitorTime = moment(info.TimestampUtc).utcOffset(timeZone.Offset);
    this.visitorZoneTimestamp = Dt.asText(visitorTime.toDate()) + ', ' + timeZone.Description;
  }
}

export default class VisitorModel {
  public readonly uniqueId: number = 0;
  public readonly addTimestamp: Date | undefined = undefined;

  public readonly name = ko.observable<string>();
  public readonly email = ko.observable<string>();
  public readonly phone = ko.observable<string>();
  public readonly mediaSupport = ko.observable<Contract.MediaSupport>();
  public readonly transcriptMode = ko.observable<Contract.VisitorSendTranscriptMode>();

  public readonly mediaSupportVideo = ko.pureComputed(() => this.mediaSupport() >= Contract.MediaSupport.Video);
  public readonly mediaSupportAudio = ko.pureComputed(() => this.mediaSupport() >= Contract.MediaSupport.Audio);

  // tracker info
  public readonly isTrackerInfoLoaded = ko.observable(false);
  public readonly externalId = ko.observable<string>(null);
  public readonly timeZoneName = ko.observable<string>(null);
  public readonly timeZoneOffset = ko.observable<number>(null);
  public readonly ip = ko.observable<string>(null);
  public readonly location = ko.observable<string>(null);
  public readonly userAgent = ko.observable<string>(null);
  public readonly userAgentText = ko.observable<string>(null);
  public readonly pageHistory = ko.observableArray<VisitorPageHistoryEntryModel>();

  public constructor(source: Contract.VisitorInfo)
  {
    this.uniqueId = source.UniqueId;
    this.addTimestamp = new Date(source.AddTimestampUtc);

    this.update(source);
  }

  public update(source: Contract.VisitorInfo): void
  {
    this.name(source.Name);
    this.email(source.Email);
    this.phone(source.Phone);
    this.mediaSupport(source.MediaSupport);
    this.transcriptMode(source.TranscriptMode);
  }

  public updateTrackerInfo(source: TrackerContract.GetHistoryResult)
  {
    this.isTrackerInfoLoaded(true);
    const v = source.Visitor;
    if (v)
    {
      this.externalId(v.VisitorExternalId);
      this.ip(v.Ip);
      if (v.TimeZone)
      {
        this.timeZoneName(v.TimeZone.Description);
        this.timeZoneOffset(v.TimeZone.Offset);
      }
      if (v.IpLocation)
      {
        let location = '';
        if (v.IpLocation.City) location += v.IpLocation.City;
        if (v.IpLocation.Country) location += (location.length > 0 ? ', ' : '') + v.IpLocation.Country;
        this.location(location);
      }
      let userAgent = '';
      let userAgentText = '';
      if (v.UserAgent)
      {
        userAgent = v.UserAgent.UserAgent + ' on ' + v.UserAgent.Os;
        userAgentText = v.UserAgent.UserAgentString;
      }
      this.userAgent(userAgent);
      this.userAgentText(userAgentText);
    }

    if (source.Items)
    {
      const pp = source.Items.map(x => new VisitorPageHistoryEntryModel(x, v.TimeZone));
      this.pageHistory.push.apply(this.pageHistory, pp);
      console.log("pageHistory.len=" + this.pageHistory().length);
    }
  }
}