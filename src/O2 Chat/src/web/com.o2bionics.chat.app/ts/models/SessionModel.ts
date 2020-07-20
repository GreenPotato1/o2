import * as Contract from '../contract/Contract';
import { UserController, SessionController, PageTrackerController } from '../contract/Controllers';

import UserStorage from './UserStorage';
import DepartmentStorage from './DepartmentStorage';
import VisitorStorage from './VisitorStorage';
import UserModel from './UserModel';
import DepartmentModel from './DepartmentModel';

import SessionInviteModel from '../models/SessionInviteModel';
import SessionMessageModel from '../models/SessionMessageModel';
import VisitorModel from '../models/VisitorModel';

import AppBase from '../AppBase';

import * as Common from '../Common';
import dateFromJson = Common.dateFromJson;
import escapeHtml = Common.escapeHtml;

import * as _ from 'lodash';

export default class SessionModel {

  public static readonly messagesPageSize: number = 5;
  private static readonly pageHistoryPageSize: number = 20;

  private readonly source = ko.observable<Contract.ChatSessionInfo>(null);

  public readonly skey: number;
  public readonly addTimestamp: Date | null = null;
  public readonly isOffline: boolean = false;
  public readonly visitorId: number | null = null;

  public readonly starterHtml = ko.pureComputed(
    () =>
    this.getStarterHtml(this.source()));
  public readonly targetHtml = ko.pureComputed(
    () =>
    this.getTargetHtml(this.source()));

  public readonly visitor = ko.pureComputed(
    () =>
    this.visitorId ? this.app.visitorStorage.get(this.visitorId) : null);
  public readonly invites = ko.pureComputed(
    () =>
    this.source().Invites.map(x => new SessionInviteModel(x, this.app)));
  public readonly agents = ko.pureComputed(
    () =>
    this.source().Agents.map(x => this.getParticipatingUserText(x)));

  public readonly isOnline = ko.pureComputed(() => !this.isOffline);
  public readonly isNew = ko.pureComputed(
    () =>
    {
      const si = this.source();
      if (si.Status === Contract.ChatSessionStatus.Completed) return false;
      return si.Agents.every(x => x.AgentId !== this.app.currentUserId);
    });
  public readonly isCompleted = ko.pureComputed(
    () => this.source().Status === Contract.ChatSessionStatus.Completed);
  public readonly title = ko.pureComputed(
    () => this.getTitle());

  public readonly transcriptSentTime = ko.pureComputed(
    () =>
    {
      const ts = this.source().VisitorTranscriptTimestampUtc;
      return ts ? new Date(ts) : null;
    });

  public readonly hasMessagesPage = ko.observable<number>();
  public readonly hasMoreMessages = ko.observable<boolean>();
  public readonly messages = ko.observableArray<SessionMessageModel>([]);
  public readonly moreMessagesButtonText = ko.observable('More..');

  public readonly hasMorePageHistory = ko.observable<boolean>();
  public readonly morePageHistoryButtonText = ko.observable('Load More');
  public pageHistorySearchPosition: string | null = null;

  constructor(
    source: Contract.ChatSessionInfo,
    messages: Contract.GetSessionMessagesResult,
    private readonly app: AppBase,
    private readonly customerId: number)
  {
    this.skey = source.Skey;
    this.addTimestamp = new Date(source.AddTimestampUtc);
    this.isOffline = source.IsOffline;
    this.visitorId = source.VisitorId;

    this.source(source);

    this.setMessages(messages, 1);
  }

  public async loadMoreMessages(): Promise<void>
  {
    try
    {
      this.moreMessagesButtonText('Loading..');

      const page = this.hasMessagesPage() + 1;
      const r = await new SessionController(this.app)
        .messages(this.skey, SessionModel.messagesPageSize, page);
      this.setMessages(r, page);
    }
    finally
    {
      this.moreMessagesButtonText('More..');
      $('#sessionFlowEnd')[0].scrollIntoView();
    }
  }

  private setMessages(r: Contract.GetSessionMessagesResult, page: number): void
  {
    this.hasMessagesPage(page);
    this.hasMoreMessages(r.HasMore);
    const mm = r.Items.map(x => new SessionMessageModel(x, this, this.app));
    this.messages.push.apply(this.messages, mm);
  }

  private getParticipatingUserText(x: Contract.ChatSessionAgentInfo): string
  {
    let r = this.app.userStorage.name(x.AgentId);
    if (x.ActsOnBehalfOfAgentId)
      r += ` (as ${this.app.userStorage.name(x.ActsOnBehalfOfAgentId)})`;
    return r;
  }

  private getStarterHtml(x: Contract.ChatSessionInfo): string
  {
    if (x.VisitorId)
      return this.app.visitorHtml(x.VisitorId);
    else if (x.Agents.length > 0)
      return this.app.userHtml(x.Agents[0].AgentId);
    else return 'Unknown';
  }

  private getTargetHtml(x: Contract.ChatSessionInfo): string
  {
    if (x.Invites.length === 0) return 'Unknown';

    const invite = x.Invites[0];
    if (invite.InviteType === Contract.ChatSessionInviteType.Department)
    {
      const deptInvite = invite as Contract.ChatSessionDepartmentInviteInfo;
      return this.app.departmentHtml(deptInvite.DepartmentId);
    }
    else if (invite.InviteType === Contract.ChatSessionInviteType.Agent)
    {
      const agentInvite = (invite as Contract.ChatSessionAgentInviteInfo);
      return this.app.userHtml(agentInvite.AgentId);
    }
    else return 'Unknown';
  }

  private getTitle(): string
  {
    const si = this.source();

    let title = this.skey.toString();
    if (this.isNew())
    {
      const lastPendingInvite = _.findLast(this.invites(), (x: SessionInviteModel) => x.isPending);
      if (lastPendingInvite)
        title += ` - ${lastPendingInvite.createdByHtml}`;
    }
    else
    {
      const participants = new Array<string>();
      if (this.visitorId) participants.push(this.app.visitorStorage.name(this.visitorId));
      _.forEachRight(
        si.Agents,
        x =>
        {
          if (x.AgentId !== this.app.currentUserId)
            participants.push(this.app.userStorage.name(x.AgentId));
        });
      _.forEachRight(
        this.invites(),
        x =>
        {
          if (x.isPending) participants.push(x.createdForName);
        });
      if (participants.length > 0)
        title += ' - ' + participants.join(', ');
    }

    return title;
  }

  public async loadPageHistory(): Promise<void>
  {
    const source = this.source();
    const v = this.app.visitorStorage.get(source.VisitorId) as VisitorModel;
    this.hasMorePageHistory(false);
    v.pageHistory([]);
    await this.loadMorePageHistory();
  }

  public async loadMorePageHistory(): Promise<void>
  {
    try
    {
      const source = this.source();
      const v = this.app.visitorStorage.get(source.VisitorId) as VisitorModel;
      if (!v)
        return;
      this.morePageHistoryButtonText('Loading..');

      const r = await new PageTrackerController(this.app)
        .get(this.customerId, v.uniqueId, SessionModel.pageHistoryPageSize, this.pageHistorySearchPosition);
      v.updateTrackerInfo(r);
      $('#pageHistoryEnd')[0].scrollIntoView();

      this.pageHistorySearchPosition = r.SearchPosition ? r.SearchPosition.Values.join('|') : null;
      this.hasMorePageHistory(r.HasMore);
    }
    finally
    {
      this.morePageHistoryButtonText('Load More');
      $('#pageHistoryEnd')[0].scrollIntoView();
    }
  }
}