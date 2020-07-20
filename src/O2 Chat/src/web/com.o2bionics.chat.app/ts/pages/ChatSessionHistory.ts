import * as Contract from '../contract/Contract';
import { UserController, SessionController } from '../contract/Controllers';
import * as DateRangeOptionsFactory from '../components/DateRangeOptionsFactory';

import EditUserModel from '../models/EditUserModel';
import UserStorage from '../models/UserStorage';
import DepartmentStorage from '../models/DepartmentStorage';
import VisitorStorage from '../models/VisitorStorage';
import UserModel from '../models/UserModel';
import DepartmentModel from '../models/DepartmentModel';
import SessionBriefModel from '../models/SessionBriefModel';

import AppBase from '../AppBase';

import SelectAvatarDialog from '../dialogs/SelectAvatarDialog';
import MainModelBase from '../MainModelBase';

import Dt from '../Dt';

import * as _ from 'lodash';
import * as moment from '../typings/moment/moment';

export default class ChatSessionHistoryApp extends AppBase {

  private constructor(currentUserId: number)
  {
    super(
      currentUserId,
      new UserStorage(currentUserId, info => new UserModel(info)),
      new DepartmentStorage(),
      new VisitorStorage());
  }

  public createModel(): MainModelBase
  {
    return new ChatSessionHistoryMainModel(this);
  }
}

class ChatSessionHistoryMainModel extends MainModelBase {


  private readonly sessionHistoryPageSize: number = 5;


  public readonly agents = ko.observableArray<UserModel>([]);
  public readonly agentsSelected = ko.observableArray<number>([]);
  public readonly dateRange = {
      start: ko.observable<moment.Moment>(moment()),
      end: ko.observable<moment.Moment>(moment())
    };
  public readonly dateRangeOptions: any;

  public readonly wordSearch = ko.observable<string>('');

  private lastPageLoaded: number = 0;
  public readonly sessions = ko.observableArray<SessionBriefModel>([]);
  public readonly hasMoreSessions = ko.observable<boolean>(false);

  public constructor(private readonly app: ChatSessionHistoryApp)
  {
    super(app);
    this.dateRangeOptions = DateRangeOptionsFactory.DateRangeOptionsFactory.createDateRangeOptions();
  }

  public init(): void
  {
    this.loadFiltersData()
      .then(
        () =>
        {
          this.parseHash();
          this.startSearch();
        });
  }

  public run(): void
  {}


  public start(): void
  {}

  private startSearch()
  {
    this.lastPageLoaded = 0;
    this.sessions([]);
    this.hasMoreSessions(false);
    this.updateHash();

    this.loadMoreSessions();
  }

  private updateHash(): void
  {
    const startDate = Dt.asDateHashText(this.dateRange.start().toDate());
    const endDate = Dt.asDateHashText(this.dateRange.end().toDate());
    const word = encodeURIComponent(this.wordSearch());
    const agents = this.agentsSelected().join(',');
    window.location.hash = `#sd:${startDate}|ed:${endDate}|w:${word}|a:${agents}`;
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
      else if (_.startsWith(part, 'w:'))
      {
        part = _.trimStart(part, 'w:');
        this.wordSearch(part);
      }
      else if (_.startsWith(part, 'a:'))
      {
        part = _.trimStart(part, 'a:');
        const ids = part.split(',').map(x => parseInt(x, 10)).filter(x => !isNaN(x));
        this.agentsSelected(ids);
      }
    }
  }

  public async loadFiltersData(): Promise<void>
  {
    const r = await new UserController(this.app).getAll();

    this.app.departmentStorage.load(r.Departments);
    this.app.userStorage.load(r.Users);

    this.agents(this.app.userStorage.visible());
  }

  public async loadMoreSessions(): Promise<void>
  {
    const page = this.lastPageLoaded + 1;

    const r = await new SessionController(this.app)
      .search(
        Dt.asDateRequestText(this.dateRange.start().toDate()),
        Dt.asDateRequestText(this.dateRange.end().toDate()),
        this.wordSearch(),
        this.agentsSelected(),
        this.sessionHistoryPageSize,
        page,
      );

    this.app.visitorStorage.update(r.Visitors);

    const sm = r.Items.map(x => new SessionBriefModel(x, this.app));
    this.sessions.push.apply(this.sessions, sm);

    this.hasMoreSessions(r.HasMore);
    this.lastPageLoaded = page;
  }
}