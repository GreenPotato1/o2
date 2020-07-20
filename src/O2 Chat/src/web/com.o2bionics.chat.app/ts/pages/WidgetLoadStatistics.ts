import * as Contract from '../contract/Contract';
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

  private constructor(public customerId: number, currentUserId: number)
  {
    super(
      currentUserId,
      new UserStorage(currentUserId, info => new UserModel(info)),
      new DepartmentStorage(),
      new VisitorStorage());
  }

  public createModel(): MainModelBase
  {
    return new WidgetLoadStatisticsModel(this);
  }
}

class WidgetLoadStatisticsModel extends MainModelBase {
  public readonly dateRange = {
      start: ko.observable<moment.Moment>(moment()),
      end: ko.observable<moment.Moment>(moment())
    };
  public readonly errorModel = new ErrorModel();
  public readonly dateRangeOptions: any;
  public readonly records = ko.observableArray<Contract.WidgetViewStatisticsEntry>([]);

  public constructor(public readonly app: AuditTrailApp)
  {
    super(app);
    this.dateRangeOptions = DateRangeOptionsFactory.DateRangeOptionsFactory.createDateRangeOptions();
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
    this.records([]);
    this.updateHash();
    this.loadMore();
  }

  public async loadMore(): Promise<void>
  {
    const request = this.buildRequest();
    const response = await this.fetchLoads(request) as Array<Contract.WidgetViewStatisticsEntry>;
    this.processResponse(response);
  }

  private updateHash(): void
  {
    const startDate = Dt.asDateHashText(this.dateRange.start().toDate());
    const endDate = Dt.asDateHashText(this.dateRange.end().toDate());

    window.location.hash = `#sd:${startDate}|ed:${endDate}`;
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
    }
  }

  private buildRequest(): Contract.WidgetLoadRequest
  {
    const result = new Contract.WidgetLoadRequest();
    result.BeginDateStr = Dt.asDateRequestText(this.dateRange.start().toDate());
    result.EndDateStr = Dt.asDateRequestText(this.dateRange.end().toDate());
    return result;
  }

  private async fetchLoads(request: Contract.WidgetLoadRequest):
    Promise<Array<Contract.WidgetViewStatisticsEntry> | null>
  {
    this.errorModel.errorMessages.removeAll();

    let result: Array<Contract.WidgetViewStatisticsEntry>;
    try
    {
      result = await new Controllers.WidgetController(this.app).selectWidgetLoads(request) as Array<
        Contract.WidgetViewStatisticsEntry>;
      if (null == result || 0 === result.length)
        return null;
    }
    catch (x)
    {
      this.errorModel.wrapException(x);
      return null;
    }

    return result;
  }

  private processResponse(rawResponse: Array<Contract.WidgetViewStatisticsEntry> | null): void
  {
    const response = rawResponse as Array<Contract.WidgetViewStatisticsEntry>;
    if (null != response && 0 < response.length)
    {
      for (let i = 0; i < response.length; i++)
      {
        const load: any = response[i];
        const isYes = load.IsOverload ? 'Yes' : 'No';
        load['Overload'] = isYes;
      }
      this.records(response);
    }
  }
}