import * as Contract from './Contract';
import * as AuditTrailContract from './AuditTrailContract';
import * as TrackerContract from './TrackerContract';
import IServerTransport from '../IServerTransport';
import { combineUrl } from '../Common';

export class UserController {
  constructor(readonly transport: IServerTransport)
  {}

  public getAll(): Promise<Contract.GetUsersResult>
  {
    return this.transport.call<Contract.GetUsersResult>(
      'GET',
      'User/GetAll');
  }

  public create(user: Contract.UserInfo, password: string): Promise<Contract.UpdateUserResult>
  {
    return this.transport.call<Contract.UpdateUserResult>(
      'POST',
      'User/Create',
      { user: user, password: password });
  }

  public update(user: Contract.UserInfo): Promise<Contract.UpdateUserResult>
  {
    return this.transport.call<Contract.UpdateUserResult>(
      'POST',
      'User/Update',
      { user: user });
  }

  public setPassword(userId: number, password: string): Promise<Contract.UpdateUserResult>
  {
    return this.transport.call<Contract.UpdateUserResult>(
      'POST',
      'User/SetPassword',
      { userId: userId, password: password });
  }

  public delete(userId: number): Promise<Contract.UpdateUserResult>
  {
    return this.transport.call<Contract.UpdateUserResult>(
      'POST',
      'User/Delete',
      { userId: userId });
  }
}

export class AccountController {
  constructor(readonly transport: IServerTransport)
  {}

  public getAvatars(): Promise<Array<string>>
  {
    return this.transport.call<Array<string>>(
      'GET',
      'Account/GetAvatars');
  }
}

export type SelectAuditFunc = (filter: Contract.Filter) => Promise<AuditTrailContract.FacetResponse | null>;

export interface IAuditTrailController {
  selectAuditTrailEvents: SelectAuditFunc;
  selectLoginEvents: SelectAuditFunc;
}

export class AuditTrailController implements IAuditTrailController {
  constructor(readonly transport: IServerTransport)
  {}

  public selectAuditTrailEvents(filter: Contract.Filter): Promise<AuditTrailContract.FacetResponse | null>
  {
    return this.transport.call<AuditTrailContract.FacetResponse>(
      'POST',
      'AuditTrail/AuditTrailEvents',
      { filter: filter });
  }

  public selectLoginEvents(filter: Contract.Filter): Promise<AuditTrailContract.FacetResponse | null>
  {
    return this.transport.call<AuditTrailContract.FacetResponse>(
      'POST',
      'AuditTrail/LoginEvents',
      { filter: filter });
  }
}

export class WidgetController {
  constructor(readonly transport: IServerTransport)
  {}

  public selectWidgetLoads(request: Contract.WidgetLoadRequest): Promise<
    Array<Contract.WidgetViewStatisticsEntry> | null>
  {
    return this.transport.call<Array<Contract.WidgetViewStatisticsEntry>>(
      'POST',
      'Widget/Loads',
      { request: request });
  }
}

export class SessionController {
  constructor(readonly transport: IServerTransport)
  {}

  public search(
      startDate: string,
      endDate: string,
      searchWord: string,
      agents: Array<number>,
      pageSize: number,
      pageNumber?: number | null,
    ): Promise<Contract.SessionSearchResult>
  {
    return this.transport.call<Contract.SessionSearchResult>(
      'GET',
      'Session/Search',
      {
        startDate: startDate,
        endDate: endDate,
        searchWord: searchWord,
        agents: agents,
        pageSize: pageSize,
        pageNumber: pageNumber,
      });
  }

  public get(
      sessionSkey: number,
      messagesPageSize: number,
    ): Promise<Contract.GetSessionResult>
  {
    return this.transport.call<Contract.GetSessionResult>(
      'GET',
      'Session/Get',
      {
        sid: sessionSkey,
        messagesPageSize: messagesPageSize,
      });
  }

  public messages(
      sessionSkey: number,
      messagesPageSize: number,
      pageNumber?: number | null,
    ): Promise<Contract.GetSessionMessagesResult>
  {
    return this.transport.call<Contract.GetSessionMessagesResult>(
      'GET',
      'Session/Messages',
      {
        sessionSkey: sessionSkey,
        messagesPageSize: messagesPageSize,
        pageNumber: pageNumber,
      });
  }
}

export class PageTrackerController {
  constructor(readonly transport: IServerTransport)
  {}

  public get(customerId: number,
      visitorUniqueId: number,
      pageSize: number,
      searchPosition: string | null = null,
    ): Promise<TrackerContract.GetHistoryResult>
  {
    return this.transport.call<TrackerContract.GetHistoryResult>(
      'GET',
      combineUrl(this.transport.getTrackerUrl(), '/g'),
      {
        cid: customerId,
        vid: visitorUniqueId,
        sz: pageSize,
        sp: searchPosition,
      },
      true);
  }
}

export class DepartmentController {
  constructor(readonly transport: IServerTransport)
  {}

  public getAll(): Promise<Contract.GetDepartmentsResult>
  {
    return this.transport.call<Contract.GetDepartmentsResult>(
      'GET',
      'Department/GetAll');
  }

  public create(dept: Contract.DepartmentInfo): Promise<Contract.UpdateDepartmentResult>
  {
    return this.transport.call<Contract.UpdateDepartmentResult>(
      'POST',
      'Department/Create',
      { dept: dept });
  }

  public update(dept: Contract.DepartmentInfo): Promise<Contract.UpdateDepartmentResult>
  {
    return this.transport.call<Contract.UpdateDepartmentResult>(
      'POST',
      'Department/Update',
      { dept: dept });
  }

  public delete(deptId: number): Promise<Contract.UpdateDepartmentResult>
  {
    return this.transport.call<Contract.UpdateDepartmentResult>(
      'POST',
      'Department/Delete',
      { deptId: deptId });
  }
}


export class CannedMessagesController {
  constructor(readonly transport: IServerTransport)
  {}

  public getUserMessages(): Promise<Contract.GetCannedMessagesResult>
  {
    return this.transport.call<Contract.GetCannedMessagesResult>(
      'GET',
      'CannedMessages/GetUserMessages');
  }

  public getDepartmentMessages(deptId: number): Promise<Contract.GetCannedMessagesResult>
  {
    return this.transport.call<Contract.GetCannedMessagesResult>(
      'GET',
      'CannedMessages/GetDepartmentMessages',
      { deptId: deptId });
  }

  public create(info: Contract.CannedMessageInfo): Promise<Contract.UpdateCannedMessageResult>
  {
    return this.transport.call<Contract.UpdateCannedMessageResult>(
      'POST',
      'CannedMessages/Create',
      { data: info });
  }

  public update(info: Contract.CannedMessageInfo): Promise<Contract.UpdateCannedMessageResult>
  {
    return this.transport.call<Contract.UpdateCannedMessageResult>(
      'POST',
      'CannedMessages/Update',
      { data: info });
  }

  public delete(id: number): Promise<Contract.UpdateCannedMessageResult>
  {
    return this.transport.call<Contract.UpdateCannedMessageResult>(
      'POST',
      'CannedMessages/Delete',
      { id: id });
  }
}