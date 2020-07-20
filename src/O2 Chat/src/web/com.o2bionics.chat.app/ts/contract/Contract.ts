// This file was auto-generated. If necessary, make changes only to the source .t4 files.

// ReSharper disable InconsistentNaming

export class IdToNameMap {
  protected readonly idNameMap = new Map<string, string>();

  public get(value: string): string
  {
    const result = this.idNameMap.get(value) as string;
    if (null == result)
      return '';
    return result;
  }
}

export interface INamed {
  Name: string;
}

export interface IdName<T> extends INamed {
  Id: T;
  Name: string;
}

export interface ILimit {
  Limit: number;
}

export enum CallResultStatusCode {
  Success = 0,
  AccessDenied = 1,
  Warning = 2,
  Failure = 3,
  NotFound = 4,
  ValidationFailed = 5,
}

export enum MediaCallStatus {
  None = 0,
  ProposedByAgent = 1,
  AcceptedByVisitor = 2,
  Established = 3,
}

export enum MediaSupport {
  NotSupported = 0,
  Audio = 1,
  Video = 2,
}

export enum ObjectStatus {
  Active = 0,
  Disabled = 1,
  Deleted = 2,
  NotConfirmed = 3,
}

export class ObjectStatusNames extends IdToNameMap {
  constructor()
  {
    super();

    const m = this.idNameMap;

    m.set('0', 'Active');
    m.set('1', 'Disabled');
    m.set('2', 'Deleted');
    m.set('3', 'Not confirmed');
  }
}

export enum ChatSessionStatus {
  Queued = 0,
  Active = 1,
  Completed = 2,
}

export enum ChatMessageSender {
  System = 1,
  Visitor = 2,
  Agent = 3,
}

export enum ChatSessionInviteType {
  Department = 1,
  Agent = 2,
}

export enum VisitorSendTranscriptMode {
  Ask = 0,
  Always = 1,
  Never = 2,
}

export enum ChatWidgetLocation {
  TopLeft = 1,
  TopRight = 2,
  BottomLeft = 3,
  BottomRight = 4,
}


export interface ValidationMessage {
  Field: string; // System.String
  Message: string; // System.String
}

export interface CallResultStatus {
  StatusCode: CallResultStatusCode; // enum Com.O2Bionics.ChatService.Contract.CallResultStatusCode
  Messages: ValidationMessage[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.ValidationMessage]
}

export interface UserInfo {
  Id: number; // System.UInt32
  CustomerId: number; // System.UInt32
  AddTimestampUtc: string; // System.DateTime
  UpdateTimestampUtc: string; // System.DateTime
  Status: ObjectStatus; // enum Com.O2Bionics.ChatService.Contract.ObjectStatus
  FirstName: string; // System.String
  LastName: string; // System.String
  Email: string; // System.String
  IsOwner: boolean; // System.Boolean
  IsAdmin: boolean; // System.Boolean
  AgentDepartments: number[]; // System.Collections.Generic.HashSet`1[System.UInt32]
  SupervisorDepartments: number[]; // System.Collections.Generic.HashSet`1[System.UInt32]
  Avatar: string; // System.String
}

export interface VisitorInfo {
  UniqueId: number; // System.UInt64
  AddTimestampUtc: string; // System.DateTime
  Name: string; // System.String
  Email: string; // System.String
  Phone: string; // System.String
  MediaSupport: MediaSupport; // enum Com.O2Bionics.ChatService.Contract.MediaSupport
  TranscriptMode: VisitorSendTranscriptMode | null; // System.Nullable`1[Com.O2Bionics.ChatService.Contract.VisitorSendTranscriptMode]
}

export interface DepartmentInfo extends INamed {
  Id: number; // System.UInt32
  CustomerId: number; // System.UInt32
  Status: ObjectStatus; // enum Com.O2Bionics.ChatService.Contract.ObjectStatus
  IsPublic: boolean; // System.Boolean
  Name: string; // System.String
  Description: string; // System.String
}

export interface ChatSessionInviteInfo {
  CreatedTimestampUtc: string; // System.DateTime
  CreatorAgentId: number | null; // System.Nullable`1[System.UInt32]
  InviteType: ChatSessionInviteType; // enum Com.O2Bionics.ChatService.Contract.ChatSessionInviteType
  ActOnBehalfOfAgentId: number | null; // System.Nullable`1[System.UInt32]
  AcceptedTimestampUtc: string | null; // System.Nullable`1[System.DateTime]
  AcceptedByAgentId: number | null; // System.Nullable`1[System.UInt32]
  CanceledTimestampUtc: string | null; // System.Nullable`1[System.DateTime]
  CanceledByAgentId: number | null; // System.Nullable`1[System.UInt32]
  IsAccepted: boolean; // System.Boolean
  IsCanceled: boolean; // System.Boolean
  IsPending: boolean; // System.Boolean
}

export interface ChatSessionAgentInviteInfo {
  AgentId: number; // System.UInt32
  CreatedTimestampUtc: string; // System.DateTime
  CreatorAgentId: number | null; // System.Nullable`1[System.UInt32]
  InviteType: ChatSessionInviteType; // enum Com.O2Bionics.ChatService.Contract.ChatSessionInviteType
  ActOnBehalfOfAgentId: number | null; // System.Nullable`1[System.UInt32]
  AcceptedTimestampUtc: string | null; // System.Nullable`1[System.DateTime]
  AcceptedByAgentId: number | null; // System.Nullable`1[System.UInt32]
  CanceledTimestampUtc: string | null; // System.Nullable`1[System.DateTime]
  CanceledByAgentId: number | null; // System.Nullable`1[System.UInt32]
  IsAccepted: boolean; // System.Boolean
  IsCanceled: boolean; // System.Boolean
  IsPending: boolean; // System.Boolean
}

export interface ChatSessionDepartmentInviteInfo {
  DepartmentId: number; // System.UInt32
  CreatedTimestampUtc: string; // System.DateTime
  CreatorAgentId: number | null; // System.Nullable`1[System.UInt32]
  InviteType: ChatSessionInviteType; // enum Com.O2Bionics.ChatService.Contract.ChatSessionInviteType
  ActOnBehalfOfAgentId: number | null; // System.Nullable`1[System.UInt32]
  AcceptedTimestampUtc: string | null; // System.Nullable`1[System.DateTime]
  AcceptedByAgentId: number | null; // System.Nullable`1[System.UInt32]
  CanceledTimestampUtc: string | null; // System.Nullable`1[System.DateTime]
  CanceledByAgentId: number | null; // System.Nullable`1[System.UInt32]
  IsAccepted: boolean; // System.Boolean
  IsCanceled: boolean; // System.Boolean
  IsPending: boolean; // System.Boolean
}

export interface ChatSessionAgentInfo {
  AgentId: number; // System.UInt32
  ActsOnBehalfOfAgentId: number | null; // System.Nullable`1[System.UInt32]
}

export interface ChatSessionMessageInfo {
  Id: number; // System.Int32
  EventId: number; // System.Int64
  TimestampUtc: string; // System.DateTime
  Sender: ChatMessageSender; // enum Com.O2Bionics.ChatService.Contract.ChatMessageSender
  SenderAgentName: string; // System.String
  SenderAgentId: number | null; // System.Nullable`1[System.UInt32]
  OnBehalfOfName: string; // System.String
  OnBehalfOfId: number | null; // System.Nullable`1[System.UInt32]
  IsToAgentsOnly: boolean; // System.Boolean
  Text: string; // System.String
}

export interface ChatSessionInfo {
  Skey: number; // System.Int64
  Status: ChatSessionStatus; // enum Com.O2Bionics.ChatService.Contract.ChatSessionStatus
  IsOffline: boolean; // System.Boolean
  IsVisitorConnected: boolean; // System.Boolean
  VisitorTranscriptLastEvent: number | null; // System.Nullable`1[System.Int64]
  VisitorTranscriptTimestampUtc: string | null; // System.Nullable`1[System.DateTime]
  AddTimestampUtc: string; // System.DateTime
  AnswerTimestampUtc: string | null; // System.Nullable`1[System.DateTime]
  EndTimestampUtc: string | null; // System.Nullable`1[System.DateTime]
  LastEventTimestampUtc: string; // System.DateTime
  MediaCallStatus: MediaCallStatus; // enum Com.O2Bionics.ChatService.Contract.MediaCallStatus
  MediaCallAgentId: number; // System.UInt32
  MediaCallAgentHasVideo: boolean | null; // System.Nullable`1[System.Boolean]
  MediaCallVisitorHasVideo: boolean | null; // System.Nullable`1[System.Boolean]
  MediaCallAgentConnectionId: string; // System.String
  MediaCallVisitorConnectionId: string; // System.String
  VisitorId: number | null; // System.Nullable`1[System.UInt64]
  Invites: ChatSessionInviteInfo[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.ChatSessionInviteInfo]
  Agents: ChatSessionAgentInfo[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.ChatSessionAgentInfo]
  AgentsInvolved: number[]; // System.Collections.Generic.HashSet`1[System.Decimal]
  DepartmentsInvolved: number[]; // System.Collections.Generic.HashSet`1[System.UInt32]
  VisitorMessageCount: number; // System.Int32
  AgentMessageCount: number; // System.Int32
}

export interface FullChatSessionInfo {
  Messages: ChatSessionMessageInfo[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.ChatSessionMessageInfo]
  Skey: number; // System.Int64
  Status: ChatSessionStatus; // enum Com.O2Bionics.ChatService.Contract.ChatSessionStatus
  IsOffline: boolean; // System.Boolean
  IsVisitorConnected: boolean; // System.Boolean
  VisitorTranscriptLastEvent: number | null; // System.Nullable`1[System.Int64]
  VisitorTranscriptTimestampUtc: string | null; // System.Nullable`1[System.DateTime]
  AddTimestampUtc: string; // System.DateTime
  AnswerTimestampUtc: string | null; // System.Nullable`1[System.DateTime]
  EndTimestampUtc: string | null; // System.Nullable`1[System.DateTime]
  LastEventTimestampUtc: string; // System.DateTime
  MediaCallStatus: MediaCallStatus; // enum Com.O2Bionics.ChatService.Contract.MediaCallStatus
  MediaCallAgentId: number; // System.UInt32
  MediaCallAgentHasVideo: boolean | null; // System.Nullable`1[System.Boolean]
  MediaCallVisitorHasVideo: boolean | null; // System.Nullable`1[System.Boolean]
  MediaCallAgentConnectionId: string; // System.String
  MediaCallVisitorConnectionId: string; // System.String
  VisitorId: number | null; // System.Nullable`1[System.UInt64]
  Invites: ChatSessionInviteInfo[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.ChatSessionInviteInfo]
  Agents: ChatSessionAgentInfo[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.ChatSessionAgentInfo]
  AgentsInvolved: number[]; // System.Collections.Generic.HashSet`1[System.Decimal]
  DepartmentsInvolved: number[]; // System.Collections.Generic.HashSet`1[System.UInt32]
  VisitorMessageCount: number; // System.Int32
  AgentMessageCount: number; // System.Int32
}

export interface SessionSearchResult {
  Status: CallResultStatus; // Com.O2Bionics.ChatService.Contract.CallResultStatus
  Items: ChatSessionInfo[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.ChatSessionInfo]
  HasMore: boolean; // System.Boolean
  Visitors: VisitorInfo[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.VisitorInfo]
}

export interface GetSessionMessagesResult {
  Status: CallResultStatus; // Com.O2Bionics.ChatService.Contract.CallResultStatus
  Items: ChatSessionMessageInfo[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.ChatSessionMessageInfo]
  HasMore: boolean; // System.Boolean
}

export interface GetSessionResult {
  Status: CallResultStatus; // Com.O2Bionics.ChatService.Contract.CallResultStatus
  Session: ChatSessionInfo; // Com.O2Bionics.ChatService.Contract.ChatSessionInfo
  Messages: GetSessionMessagesResult; // Com.O2Bionics.ChatService.Contract.GetSessionMessagesResult
  Visitor: VisitorInfo; // Com.O2Bionics.ChatService.Contract.VisitorInfo
  Users: UserInfo[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.UserInfo]
  Departments: DepartmentInfo[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.DepartmentInfo]
}

export interface GetUsersResult {
  Status: CallResultStatus; // Com.O2Bionics.ChatService.Contract.CallResultStatus
  Users: UserInfo[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.UserInfo]
  Departments: DepartmentInfo[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.DepartmentInfo]
  AreAvatarsAllowed: boolean; // System.Boolean
  MaxUsers: number; // System.Int32
}

export interface UpdateUserResult {
  Status: CallResultStatus; // Com.O2Bionics.ChatService.Contract.CallResultStatus
  User: UserInfo; // Com.O2Bionics.ChatService.Contract.UserInfo
}


export interface GetDepartmentsResult {
  Status: CallResultStatus; // Com.O2Bionics.ChatService.Contract.CallResultStatus
  Departments: DepartmentInfo[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.DepartmentInfo]
  MaxDepartments: number; // System.Int32
}

export interface UpdateDepartmentResult {
  Status: CallResultStatus; // Com.O2Bionics.ChatService.Contract.CallResultStatus
  Department: DepartmentInfo; // Com.O2Bionics.ChatService.Contract.DepartmentInfo
}

export interface CannedMessageInfo {
  Id: number; // System.UInt32
  Key: string; // System.String
  Value: string; // System.String
  UserId: number | null; // System.Nullable`1[System.UInt32]
  DepartmentId: number | null; // System.Nullable`1[System.UInt32]
  AddTimestampUtc: string; // System.DateTime
  UpdateTimestampUtc: string; // System.DateTime
}

export interface UpdateCannedMessageResult {
  CannedMessage: CannedMessageInfo; // Com.O2Bionics.ChatService.Contract.CannedMessageInfo
  Status: CallResultStatus; // Com.O2Bionics.ChatService.Contract.CallResultStatus
}

export interface GetCannedMessagesResult {
  CannedMessages: CannedMessageInfo[]; // System.Collections.Generic.List`1[Com.O2Bionics.ChatService.Contract.CannedMessageInfo]
  Status: CallResultStatus; // Com.O2Bionics.ChatService.Contract.CallResultStatus
}


export interface ChatWidgetAppearance {
  themeId: string; // System.String
  themeMinId: string; // System.String
  location: ChatWidgetLocation; // enum Com.O2Bionics.ChatService.Contract.WidgetAppearance.ChatWidgetLocation
  offsetX: number; // System.Int32
  offsetY: number; // System.Int32
  minStateTitle: string; // System.String
  customCssUrl: string; // System.String
  poweredByVisible: boolean; // System.Boolean
}

export interface CustomerInfo extends INamed {
  Id: number; // System.UInt32
  AddTimestampUtc: string; // System.DateTime
  UpdateTimestampUtc: string; // System.DateTime
  Status: ObjectStatus; // enum Com.O2Bionics.ChatService.Contract.ObjectStatus
  Name: string; // System.String
  Domains: string[]; // System.String[]
  CreateIp: string; // System.String
}

export class WidgetLoadRequest {
  public BeginDate: string; // System.DateTime
  public EndDate: string; // System.DateTime
  public BeginDateStr: string; // System.String
  public EndDateStr: string; // System.String
}

export interface WidgetViewStatisticsEntry {
  CustomerId: number; // System.UInt32
  Date: string; // System.DateTime
  Count: number; // System.Int64
  IsViewCountExceeded: boolean; // System.Boolean
}

export interface WidgetDailyViewCountExceededEvent extends ILimit {
  Total: number; // System.Int64
  Limit: number; // System.Int64
  Date: string; // System.DateTime
}

export interface WidgetUnknownDomain extends INamed {
  Domains: string; // System.String
  Name: string; // System.String
}

export interface WidgetUnknownDomainTooManyEvent extends ILimit {
  Domains: string; // System.String
  Limit: number; // System.Int32
  Date: string; // System.DateTime
}

export class Facet {
  public Id: string; // System.String
  public Name: string; // System.String
  public Count: number; // System.Int64
}

export class SearchPositionInfo {
  public Values: string[]; // System.Collections.Generic.List`1[System.String]
}

export class Filter {
  public ProductCode: string; // System.String
  public PageSize: number; // System.Int32
  public FromRow: number; // System.Int32
  public ChangedOnly: boolean; // System.Boolean
  public Substring: string; // System.String
  public CustomerId: string; // System.String
  public Operations: string[]; // System.Collections.Generic.List`1[System.String]
  public Statuses: string[]; // System.Collections.Generic.List`1[System.String]
  public SearchPosition: SearchPositionInfo; // Com.O2Bionics.Utils.SearchPositionInfo
  public AuthorIds: string[]; // System.Collections.Generic.List`1[System.String]
  public FromTime: string; // System.DateTime
  public ToTime: string; // System.DateTime
  public FromTimeStr: string; // System.String
  public ToTimeStr: string; // System.String
}

export class FacetResponse {
  public Count: number; // System.Int64
  public RawDocuments: string[]; // System.Collections.Generic.List`1[System.String]
  public Operations: Facet[]; // System.Collections.Generic.List`1[Com.O2Bionics.AuditTrail.Contract.Facet]
  public Statuses: Facet[]; // System.Collections.Generic.List`1[Com.O2Bionics.AuditTrail.Contract.Facet]
  public Authors: Facet[]; // System.Collections.Generic.List`1[Com.O2Bionics.AuditTrail.Contract.Facet]
}

export class OperationStatusNames extends IdToNameMap {
  constructor()
  {
    super();

    const m = this.idNameMap;

    m.set('AccessDenied', 'Access denied');
    m.set('ValidationFailed', 'Validation failed');
    m.set('OperationFailed', 'Operation failed');
    m.set('Success', 'Success');
    m.set('NotFound', 'Account not found');
    m.set('NotActive', 'Account not active');
    m.set('CustomerNotActive', 'Customer not active');
    m.set('LoginFailed', 'Login Failed');
  }
}

export class OperationKindNames extends IdToNameMap {
  constructor()
  {
    super();

    const m = this.idNameMap;

    m.set('UserChangePassword', 'Change user password');
    m.set('UserDelete', 'Delete user');
    m.set('UserInsert', 'Insert user');
    m.set('UserUpdate', 'Update user');
    m.set('DepartmentDelete', 'Delete department');
    m.set('DepartmentInsert', 'Insert department');
    m.set('DepartmentUpdate', 'Update department');
    m.set('CustomerInsert', 'Insert customer');
    m.set('CustomerUpdate', 'Update customer');
    m.set('WidgetAppearanceUpdate', 'Update widget appearance');
    m.set('Login', 'User login');
    m.set('WidgetOverload', 'Too many Widget loads');
    m.set('WidgetUnknownDomain', 'Unknown domain');
    m.set('WidgetUnknownDomainTooManyEvent', 'Too many unknown domains');
  }
}
