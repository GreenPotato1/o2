import * as Contract from '../contract/Contract';
import { UserController, SessionController } from '../contract/Controllers';

import UserStorage from '../models/UserStorage';
import DepartmentStorage from '../models/DepartmentStorage';
import VisitorStorage from '../models/VisitorStorage';
import UserModel from '../models/UserModel';
import DepartmentModel from '../models/DepartmentModel';
import SessionModel from '../models/SessionModel';

import OffsetClockComponent from '../components/OffsetClockComponent';

import AppBase from '../AppBase';

import MainModelBase from '../MainModelBase';

import * as _ from 'lodash';

import * as Common from '../Common';
import dateFromJson = Common.dateFromJson;
import escapeHtml = Common.escapeHtml;

export default class ChatSessionDetailsApp extends AppBase {

  private constructor(
    currentUserId: number,
    public readonly customerId: number,
    private readonly sessionSkey: number,
    private readonly trackerUrl: string)
  {
    super(
      currentUserId,
      new UserStorage(currentUserId, info => new UserModel(info)),
      new DepartmentStorage(),
      new VisitorStorage());
  }

  public createModel(): MainModelBase
  {
    return new ChatSessionDetailsMainModel(this, this.sessionSkey);
  }

  public getTrackerUrl(): string { return this.trackerUrl; }

  public registerComponents(): void
  {
    super.registerComponents();

    OffsetClockComponent.register();
  }
}


class ChatSessionDetailsMainModel extends MainModelBase {

  public loading = ko.observable(true);
  public notFound = ko.observable(false);
  public session = ko.observable<SessionModel>(null);

  public ready = ko.pureComputed(() => !this.loading() && !this.notFound());

  public constructor(
    private readonly app: ChatSessionDetailsApp,
    private readonly sessionSkey: number)
  {
    super(app);
  }

  public init(): void
  {
    this.loadSession(this.sessionSkey);
  }

  public run(): void
  {}

  private async loadSession(skey: number): Promise<void>
  {
    const r = await new SessionController(this.app)
      .get(skey, SessionModel.messagesPageSize);

    this.app.userStorage.update(r.Users);
    this.app.departmentStorage.update(...r.Departments);

    if (r.Session)
    {
      const sessionModel = new SessionModel(r.Session, r.Messages, this.app, this.app.customerId);
      if (r.Visitor)
      {
        this.app.visitorStorage.update([r.Visitor]);
        const v = this.app.visitorStorage.get(r.Visitor.UniqueId);
        if (v && !v.isTrackerInfoLoaded())
        {
          sessionModel.loadPageHistory();
        }
      }

      this.session(sessionModel);
    }
    else
    {
      this.notFound(true);
    }
    this.loading(false);
    if (this.session())
    {
      $('#session-info-tab-header').tab('show');
    }
  }
}