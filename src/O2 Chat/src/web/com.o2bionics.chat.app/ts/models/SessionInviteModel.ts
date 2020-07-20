import * as Contract from '../contract/Contract';
import AppBase from '../AppBase';


export default class SessionInviteModel {
  public readonly createdTime: Date;
  public readonly createdByHtml: string;
  public readonly createdToHtml: string;
  public readonly createdForName: string;
  public readonly status: string;
  public readonly statusChangeTime: Date | null;
  public readonly isPending: boolean;

  constructor(
      x: Contract.ChatSessionInviteInfo,
      app: AppBase,
    )
  {
    this.createdTime = new Date(x.CreatedTimestampUtc);
    this.createdByHtml = x.CreatorAgentId
                         ? app.userHtml(x.CreatorAgentId)
                         : 'Visitor';
    if (x.InviteType === Contract.ChatSessionInviteType.Agent)
    {
      const invite = x as Contract.ChatSessionAgentInviteInfo;
      this.createdForName = app.userStorage.name(invite.AgentId);
      this.createdToHtml = app.userHtml(invite.AgentId);
      if (invite.ActOnBehalfOfAgentId)
        this.createdToHtml += ` (as ${app.userHtml(invite.ActOnBehalfOfAgentId)})`;
    }
    else
    {
      const invite = x as Contract.ChatSessionDepartmentInviteInfo;
      const deptName = app.departmentStorage.name(invite.DepartmentId);
      this.createdForName = deptName;
      this.createdToHtml = app.departmentHtml(invite.DepartmentId);
      if (invite.ActOnBehalfOfAgentId)
        this.createdToHtml += ` (as ${app.userHtml(invite.ActOnBehalfOfAgentId)})`;
    }

    this.status =
      x.AcceptedByAgentId
      ? `accepted by ${app.userHtml(x.AcceptedByAgentId)}`
      : x.CanceledByAgentId
      ? `canceled by ${app.userHtml(x.CanceledByAgentId)}`
      : 'now pending';
    this.statusChangeTime =
      x.AcceptedByAgentId
      ? new Date(x.AcceptedTimestampUtc!)
      : x.CanceledByAgentId
      ? new Date(x.CanceledTimestampUtc!)
      : null;

    this.isPending = x.AcceptedByAgentId == null && x.CanceledByAgentId == null;
  }
}