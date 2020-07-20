import * as Contract from '../contract/Contract';

import SessionModel from './SessionModel';

import * as Common from '../Common';
import dateFromJson = Common.dateFromJson;
import escapeHtml = Common.escapeHtml;

import AppBase from '../AppBase';

import AvatarHelper from '../AvatarHelper';

export default class SessionMessageModel {

  public readonly id: number;
  public readonly timestamp: Date;
  public readonly lines: Array<string>;
  public readonly senderName: string;
  public readonly senderClass: string;
  public readonly senderAvatarUrl: string | null;
  public readonly iconHtml: string | null;

  constructor(x: Contract.ChatSessionMessageInfo, session: SessionModel, app: AppBase)
  {
    this.id = x.Id;
    this.timestamp = dateFromJson(x.TimestampUtc!)!;
    //this.lines = emotify(escapeHtml(x.Text)).split('\n');

    this.lines = emotify(Common.escapeHtmlLight(x.Text)).split('\n');

    for (var i = 0; i < this.lines.length; i++)
      this.lines[i] = Common.parseUrls(this.lines[i]);

    //                                      class               name                                        avatar
    // system                               system
    // visitor                              visitor             visitor.name ?? Visitor                     
    // agent
    //      me                              me                  agent-me.fullName                           agent-me.avatar
    //      me not visible to visitor       me private          agent-me.fullName                           agent-me.avatar
    //      me as other                     me as-other         agent-me.fullName as agent-1.fullName       agent-me.avatar
    //      other                           other               agent-1.fullName                            agent-1.avatar
    //      other not visible               other private       agent-1.fullName                            agent-1.avatar
    //      other as me                     other as-me         agent-1.fullName as agent-me.fullName       agent-1.avatar
    //      other as other2                 other as-other      agent-1.fullName as agent-2.fullName        agent-1.avatar
    // 

    this.senderName = '';
    this.senderClass = '';
    this.iconHtml = null;
    this.senderAvatarUrl = null;

    switch (x.Sender)
    {
    case Contract.ChatMessageSender.System:
      this.senderClass = 'by-system';
      break;

    case Contract.ChatMessageSender.Visitor:
      this.senderClass = 'by-visitor';
      this.senderName = app.visitorStorage.name(session.visitorId!);
      break;

    case Contract.ChatMessageSender.Agent:
      this.senderClass = x.SenderAgentId === app.currentUserId ? 'by-me' : 'by-other';
      if (x.IsToAgentsOnly)
      {
        this.senderClass += ' private';
        this.iconHtml = '<i class="material-icons">&#xE8F5;</i>';
      }
      else if (x.OnBehalfOfId)
      {
        this.senderClass += x.OnBehalfOfId === app.currentUserId ? ' as-me' : ' as-other';
        this.iconHtml = '<i class="material-icons">&#xE915;</i>';
      }

      this.senderAvatarUrl = AvatarHelper.toAvatarUrl(app.userStorage.avatar(x.SenderAgentId!));

      this.senderName = app.userStorage.name(x.SenderAgentId!);
      if (x.OnBehalfOfId && !x.IsToAgentsOnly) this.senderName += ' as ' + app.userStorage.name(x.OnBehalfOfId);

      break;
    }
  }
}