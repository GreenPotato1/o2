import * as Contract from '../contract/Contract';
import * as Common from '../Common';
import AppBase from '../AppBase';
import dateFromJson = Common.dateFromJson;

export default class SessionBriefModel {

  public readonly source = ko.observable<Contract.ChatSessionInfo>();

  public readonly skey: number = 0;
  public readonly addTimestamp: Date | null = null;
  public readonly isOffline: boolean = false;
  public readonly visitorId: number | null = null;

  public readonly status = ko.observable<Contract.ChatSessionStatus>();
  public readonly answerTimestamp = ko.observable<Date>();
  public readonly endTimestamp = ko.observable<Date>();
  public readonly visitorMessageCount = ko.observable<number>();
  public readonly agentMessageCount = ko.observable<number>();

  public readonly creatorHtml = ko.pureComputed(
    () =>
    this.getSessionCreatorHtml(this.source()));
  public readonly targetHtml = ko.pureComputed(
    () =>
    this.getSessionTargetHtml(this.source()));
  public readonly agentsHtml = ko.pureComputed(
    () =>
    this.getSessionAgentsHtml(this.source()));

  public statusText = ko.pureComputed(
    () =>
    this.status() === undefined ? '' : Contract.ChatSessionStatus[this.status()]);
  public messageCountText = ko.pureComputed(
    () =>
    (this.visitorMessageCount() + '/' + this.agentMessageCount()));

  public constructor(
    source: Contract.ChatSessionInfo,
    private readonly app: AppBase)
  {
    if (source)
    {
      this.skey = source.Skey;
      this.addTimestamp = new Date(source.AddTimestampUtc);
      this.isOffline = source.IsOffline;
      this.visitorId = source.VisitorId;

      this.update(source);
    }
    else
    {
      this.addTimestamp = new Date();
    }
  }

  public update(si: Contract.ChatSessionInfo): void
  {
    this.status(si.Status);
    this.answerTimestamp(dateFromJson(si.AnswerTimestampUtc));
    this.endTimestamp(dateFromJson(si.EndTimestampUtc));
    this.visitorMessageCount(si.VisitorMessageCount);
    this.agentMessageCount(si.AgentMessageCount);

    this.source(si);
  }

  private getSessionCreatorHtml(si: Contract.ChatSessionInfo): string
  {
    if (si.VisitorId !== null)
    {
      return this.app.visitorHtml(si.VisitorId);
    }

    const userId = this.getSessionStarterAgent(si);
    if (userId !== null)
    {
      return this.app.userHtml(userId);
    }

    return 'n/a';
  }

  private getSessionStarterAgent(si: Contract.ChatSessionInfo): number | null
  {
    if (si.Invites.length < 1) return null;
    return si.Invites[0].CreatorAgentId; // can be null!
  }

  private getSessionTargetHtml(si: Contract.ChatSessionInfo): string
  {
    if (si.Invites.length < 1) return '';
    const invite = si.Invites[0];
    if (invite.InviteType === Contract.ChatSessionInviteType.Department)
    {
      const id = (invite as Contract.ChatSessionDepartmentInviteInfo).DepartmentId;
      return this.app.departmentHtml(id);
    }
    else
    {
      const id = (invite as Contract.ChatSessionAgentInviteInfo).AgentId;
      return this.app.userHtml(id);
    }
  }

  private getSessionAgentsHtml(si: Contract.ChatSessionInfo): string
  {
    return si.AgentsInvolved
      .map(x => this.app.userHtml(x))
      .join(', ');
  }
}