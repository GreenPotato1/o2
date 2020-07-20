import * as Contract from '../contract/Contract';
import * as AuditTrailContract from '../contract/AuditTrailContract';
import * as AuditTrailFormat from './AuditTrailFormat';
import * as Controllers from '../contract/Controllers';
import * as DateRangeOptionsFactory from '../components/DateRangeOptionsFactory';

import UserStorage from '../models/UserStorage';
import DepartmentStorage from '../models/DepartmentStorage';
import VisitorStorage from '../models/VisitorStorage';
import UserModel from '../models/UserModel';
import DepartmentModel from '../models/DepartmentModel';
import AppBase from '../AppBase';
import MainModelBase from '../MainModelBase';
import Dt from '../Dt';
import * as moment from '../typings/moment/moment';
import * as _ from 'lodash';
import ErrorModel from '../models/ErrorModel';

export default class AuditTrailApp extends AppBase {

  private constructor(currentUserId: number, public formKind: string, public maxDays: number)
  {
    super(
      currentUserId,
      new UserStorage(currentUserId, info => new UserModel(info)),
      new DepartmentStorage(),
      new VisitorStorage());
  }

  public createModel(): MainModelBase
  {
    return new AuditTrailMainModel(this);
  }
}

class AuditTrailMainModel extends MainModelBase {
  public readonly dateRange = {
      start: ko.observable<moment.Moment>(moment()),
      end: ko.observable<moment.Moment>(moment())
    };
  public readonly errorModel = new ErrorModel();
  public readonly dateRangeOptions: any;

  public readonly operations = ko.observableArray<AuditTrailContract.Facet2>([]);
  public readonly operationsSelected = ko.observableArray<string>([]);

  public readonly statuses = ko.observableArray<AuditTrailContract.Facet2>([]);
  public readonly statusesSelected = ko.observableArray<string>([]);

  public readonly authors = ko.observableArray<AuditTrailContract.Facet2>([]);
  public readonly authorsSelected = ko.observableArray<string>([]);

  public readonly changedOnly = ko.observable<boolean>(true);
  public readonly wordSearch = ko.observable<string>('');

  public readonly records = ko.observableArray<AuditTrailContract.AuditRecord>([]);
  public readonly hasMore = ko.observable<boolean>(false);

  private static readonly defaultPageSize: number = 10;
  private lastPageLoaded: number = 0;
  private positionInfo: Contract.SearchPositionInfo | null;
  private readonly auditTrailMaps = new AuditTrailContract.AuditTrailMaps();
  private readonly auditTrailFormatter = new AuditTrailFormat.AuditTrailFormatter(this.auditTrailMaps);
  private readonly uniqueIds = new Set<string>();

  public constructor(public readonly app: AuditTrailApp)
  {
    super(app);
    this.dateRangeOptions = DateRangeOptionsFactory.DateRangeOptionsFactory.createDateRangeOptions();
    if (0 < app.maxDays)
    {
      const date = new Date();
      date.setDate(date.getDate() - this.app.maxDays);
      this.dateRangeOptions.minDate = date;
    }
  }

  public init(): void
  {
    this.parseHash();
    this.startSearch();
  }

  public run(): void
  {}

  private startSearch()
  {
    this.lastPageLoaded = 0;
    this.positionInfo = null;
    this.records([]);
    this.uniqueIds.clear();
    this.hasMore(false);
    this.updateHash();
    this.loadMore();
  }

  public async loadMore(): Promise<void>
  {
    const oldPage = this.lastPageLoaded;
    const isFirstPage = 0 === oldPage;
    const filter = this.getFilter(isFirstPage);
    const facetResponse = await this.fetchFacets(filter) as AuditTrailContract.FacetResponse;
    this.processResponse(facetResponse, isFirstPage);
    this.lastPageLoaded = oldPage + 1;
  }

  private updateHash(): void
  {
    const startDate = Dt.asDateHashText(this.dateRange.start().toDate());
    const endDate = Dt.asDateHashText(this.dateRange.end().toDate());

    const ops = this.operationsSelected().join(',');
    const statuses = this.statusesSelected().join(',');
    const authors = this.authorsSelected().join(',');
    const changed = this.changedOnly();
    const word = encodeURIComponent(this.wordSearch());

    window.location.hash = `#sd:${startDate}|ed:${endDate}|o:${ops}|s:${statuses}|a:${authors}|c:${changed}|w:${word}`;
  }

  private parseHash(): void
  {
    const hash = _.trimStart(window.location.hash, '#');
    for (let part of hash.split('|'))
    {
      if (_.startsWith(part, 'sd:'))
      {
        part = _.trimStart(part, 'sd:');
        this.dateRange.start(moment(Dt.fromDateHashText(part)));
      }
      else if (_.startsWith(part, 'ed:'))
      {
        part = _.trimStart(part, 'ed:');
        this.dateRange.end(moment(Dt.fromDateHashText(part)));
      }
      else if (_.startsWith(part, 'o:'))
      {
        part = _.trimStart(part, 'o:');
        this.operationsSelected(this.parseStringArray(part));
      }
      else if (_.startsWith(part, 's:'))
      {
        part = _.trimStart(part, 's:');
        this.statusesSelected(this.parseStringArray(part));
      }
      else if (_.startsWith(part, 'a:'))
      {
        part = _.trimStart(part, 'a:');
        this.authorsSelected(this.parseStringArray(part));
      }
      else if (_.startsWith(part, 'c:'))
      {
        part = _.trimStart(part, 'c:');
        const isChanged = 'true' === part;
        this.changedOnly(isChanged);
      }
      else if (_.startsWith(part, 'w:'))
      {
        part = _.trimStart(part, 'w:');
        this.wordSearch(part);
      }
    }
  }

  private parseStringArray(part: string): Array<string>
  {
    const result = part.split(',').map(x => x).filter(x => null != x && 0 < x.length);
    return result;
  }

  private getFilter(isFirstPage: boolean): Contract.Filter
  {
    //ProductCode, CustomerId are set on the server.
    const filter = new Contract.Filter();
    filter.PageSize = AuditTrailMainModel.defaultPageSize;
    filter.ChangedOnly = this.changedOnly();

    const str = ('' + this.wordSearch()).trim();
    if (0 < str.length)
      filter.Substring = str;

    filter.FromTimeStr = Dt.asDateRequestText(this.dateRange.start().toDate());
    filter.ToTimeStr = Dt.asDateRequestText(this.dateRange.end().toDate());

    {
      const selected = this.operationsSelected();
      if (null != selected && 0 < selected.length)
        filter.Operations = selected;
    }
    {
      const selected = this.statusesSelected();
      if (null != selected && 0 < selected.length)
        filter.Statuses = selected;
    }
    {
      const selected = this.authorsSelected();
      if (null != selected && 0 < selected.length)
        filter.AuthorIds = selected;
    }

    if (!isFirstPage)
    {
      if (this.positionInfo !== null)
      {
        filter.SearchPosition = this.positionInfo;
      }
    }

    return filter;
  }

  private async fetchFacets(filter: Contract.Filter): Promise<AuditTrailContract.FacetResponse | null>
  {
    this.errorModel.errorMessages.removeAll();

    let result: AuditTrailContract.FacetResponse;
    try
    {
      const auditKind = 'AuditTrail', loginKind = 'Login';
      switch (this.app.formKind)
      {
      case auditKind:
        result =
          (await new Controllers.AuditTrailController(this.app).selectAuditTrailEvents(filter)) as AuditTrailContract.
          FacetResponse;
        break;
      case loginKind:
        result =
          (await new Controllers.AuditTrailController(this.app).selectLoginEvents(filter)) as AuditTrailContract.
          FacetResponse;
        break;
      default:
        throw new Error(`Unknown audit form kind="${this.app.formKind}".`);
      }

      if (null == result)
        return null;
    }
    catch (x)
    {
      this.errorModel.wrapException(x);
      return null;
    }

    this.fixDisplayNames(result.Authors);
    this.fixDisplayNameById(result.Operations, this.auditTrailMaps.operationKindNames);
    this.fixDisplayNameById(result.Statuses, this.auditTrailMaps.operationStatusNames);

    const lines = result.RawDocuments;
    if (null != lines && 0 < lines.length)
    {
      result.Documents = [];
      for (let i = 0; i < lines.length; i++)
      {
        try
        {
          if (null == lines[i] || 0 === lines[i].length)
            continue;

          const changeHistory = JSON.parse(lines[i]) as AuditTrailContract.AuditEvent;
          if (null == changeHistory || null == changeHistory.Id)
            continue;

          const field = 'FieldChanges';
          if ('undefined' !== (changeHistory as any)[field])
          {
            delete (changeHistory as any)[field];
          }

          result.Documents.push(changeHistory);
        }
        catch (e)
        {
          this.errorModel.logErrorSafe(e, 'AuditTrail.fetchFacets');
        }
      }
    }

    return result;
  }

  private fixDisplayNames(values: Array<AuditTrailContract.Facet2>): void
  {
    if (null == values || 0 === values.length)
      return;

    for (let item of values)
    {
      this.fixDisplayName(item);
    }
  }

  private fixDisplayName(facet: AuditTrailContract.Facet2): void
  {
    const name = null != facet.Name && 0 < facet.Name.length ? facet.Name : facet.Id;
    const count = null != facet.Count && 'number' === typeof(facet.Count) ? facet.Count : 0;
    facet.DisplayName = name + ' (' + count + ')';
  }

  private fixDisplayNameById(values: Array<AuditTrailContract.Facet2>, map1: Contract.IdToNameMap): void
  {
    if (null == values || 0 === values.length)
      return;

    for (let item of values)
    {
      let name = map1.get(item.Id);
      if (null == name || 0 === name.length)
        name = item.Id;

      item.DisplayName = name + ' (' + item.Count + ')';
    }
  }

  private getNewDocuments(raw: Array<AuditTrailContract.AuditEvent> | null): Array<AuditTrailContract.AuditRecord>
  {
    const documents = raw as Array<AuditTrailContract.AuditEvent>;

    const newData: Array<AuditTrailContract.AuditRecord> = [];
    if (null != documents)
    {
      for (let i = 0; i < documents.length; i++)
      {
        const history = documents[i];
        if (this.uniqueIds.has(history.Id))
          continue;

        const record = new AuditTrailContract.AuditRecord();
        this.auditTrailFormatter.format(history, record);

        const details = ko.observable<string>(null);
        record.Details = details;
        const isDetailsVisible = ko.observable<boolean>(false);
        record.IsDetailsVisible = isDetailsVisible;

        record.lazyFormDetails = () =>
        {
          if (!details())
          {
            const history2 = record.History as AuditTrailContract.AuditEvent;
            if (null == history2)
              return;

            const str = this.auditTrailFormatter.buildAuditDetails(history2);
            details(str);
            delete record.History;
          }
          isDetailsVisible(!isDetailsVisible());
        };

        this.uniqueIds.add(history.Id);
        newData.push(record);
      }
    }

    return newData;
  }

  private stringToDate(s: string): Date
  {
    const result = new Date(s);
    return result;
  }

  private dateToTimestampMs(d: Date): number
  {
    const result = d.valueOf();
    return result;
  }

  private processResponse(rawResponse: AuditTrailContract.FacetResponse | null, isFirstPage: boolean): void
  {
    const response = rawResponse as AuditTrailContract.FacetResponse;
    let hasdata = false;
    if (null != response && null != response.Documents && 0 < response.Documents.length)
    {
      const last = response.Documents[response.Documents.length - 1];
      const date = this.stringToDate(last.Timestamp);
      // TODO: searchPosition should be delivered from server
      this.positionInfo = { Values: ['' + this.dateToTimestampMs(date), last.Id] };

      const newData = this.getNewDocuments(response.Documents);
      hasdata = null != newData && 0 < newData.length;
      if (hasdata)
      {
        hasdata = null != newData && AuditTrailMainModel.defaultPageSize === newData.length;
        this.records.push.apply(this.records, newData);
      }
    }
    this.hasMore(hasdata);

    this.applyFacetValues(
      this.operations,
      null == response ? null : response.Operations,
      this.operationsSelected,
      isFirstPage);

    this.applyFacetValues(
      this.statuses,
      null == response ? null : response.Statuses,
      this.statusesSelected,
      isFirstPage);

    this.applyFacetValues(this.authors, null == response ? null : response.Authors, this.authorsSelected, isFirstPage);
  }

  private applyFacetValues(
    observableArr: KnockoutObservableArray<AuditTrailContract.Facet2>,
    rawNewValues: Array<AuditTrailContract.Facet2> | null,
    observableSelected: KnockoutObservableArray<string>,
    isFirstPage: boolean): void
  {
    const oldValues = observableArr();
    const newValues = null != rawNewValues ? rawNewValues as Array<AuditTrailContract.Facet2> : [];
    if (!isFirstPage)
      this.mergeFacets(oldValues, newValues);

    const selected = observableSelected();
    observableArr(newValues);
    observableSelected(selected);
  }

  private mergeFacets(oldValues: AuditTrailContract.Facet2[], newValues: AuditTrailContract.Facet2[]): void
  {
    if (null == oldValues || 0 === oldValues.length)
      return;

    const set1 = new Set<string>();
    for (const value of newValues)
      set1.add(value.Id);

    for (const oldFacet of oldValues)
    {
      if (!set1.has(oldFacet.Id))
      {
        set1.add(oldFacet.Id);
        newValues.push(oldFacet);
      }
    }
  }
}