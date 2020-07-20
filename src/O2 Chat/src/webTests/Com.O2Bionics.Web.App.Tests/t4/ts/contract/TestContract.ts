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

// This file is used to test the .ttinclude files.
// The generated .ts files should compile without errors.

export interface INamed {
  Name: string;
}

export interface BaseInterface1 {
  Name: string;
}

export interface BaseInterface2 {
  Count: number;
}

export class BaseClass {
  public OtherName: string;
}

export enum CallResultStatusCode {
  Success = 0,
  AccessDenied = 1,
  Warning = 2,
  Failure = 3,
  NotFound = 4,
  ValidationFailed = 5,
}

//Error: the code for type Com.O2Bionics.ChatService.Contract.CallResultStatusCode has already been generated.
//Error: Com.O2Bionics.ChatService.Contract.OnlineStatusInfo must be enum.
//Error: null returned for type Com.O2Bionics.ChatService.Contract.CallResultStatusCode_BadName.

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

//Error: the code for type Com.O2Bionics.ChatService.Contract.ObjectStatus has already been generated.
//Error: null returned for type Com.O2Bionics.ChatService.Contract.ObjectStatus_BadName.
export enum ResetPasswordCodeStatus {
  Success = 0,
  CodeNotFoundOrExpired = 1,
  AccountRemovedOrLocked = 2,
}

export class ResetPasswordCodeStatusNames extends IdToNameMap {
  constructor()
  {
    super();

    const m = this.idNameMap;

//Error: Com.O2Bionics.ChatService.Contract.ResetPasswordCodeStatus.Success must have the DisplayAttribute applied.
//Error: Com.O2Bionics.ChatService.Contract.ResetPasswordCodeStatus.CodeNotFoundOrExpired must have the DisplayAttribute applied.
//Error: Com.O2Bionics.ChatService.Contract.ResetPasswordCodeStatus.AccountRemovedOrLocked must have the DisplayAttribute applied.
  }
}


export interface DepartmentInfo extends INamed {
  Id: number; // System.UInt32
  CustomerId: number; // System.UInt32
  Status: ObjectStatus; // enum Com.O2Bionics.ChatService.Contract.ObjectStatus
  IsPublic: boolean; // System.Boolean
  Name: string; // System.String
  Description: string; // System.String
}

//Error: the code for type Com.O2Bionics.ChatService.Contract.DepartmentInfo has already been generated.
//Error: null returned for type Com.O2Bionics.ChatService.Contract.DepartmentInfo_BadName.

//Error: The class Com.O2Bionics.AuditTrail.Contract.Facet base 'bad base name' must be a valid identifier.
//Error: One of class Com.O2Bionics.AuditTrail.Contract.Facet bases '' must be a valid identifier.
//Error: One of class Com.O2Bionics.AuditTrail.Contract.Facet bases 'BaseInterface 2' must be a valid identifier.
export class Facet extends BaseClass implements BaseInterface1, BaseInterface2 {
  public Id: string; // System.String
  public Name: string; // System.String
  public Count: number; // System.Int64
}

//Error: the code for type Com.O2Bionics.AuditTrail.Contract.Facet has already been generated.
//Error: null returned for type Com.O2Bionics.AuditTrail.Contract.Facet_BadName.

export class Filter {
  public ProductCode: string; // System.String
  public PageSize: number; // System.Int32
  public FromRow: number; // System.Int32
  public ChangedOnly: boolean; // System.Boolean
  public Substring: string; // System.String
  public CustomerId: string; // System.String
  public Operations: string[]; // System.Collections.Generic.List`1[System.String]
  public Statuses: string[]; // System.Collections.Generic.List`1[System.String]
  public SearchPosition: any 
//Error: unknown type: SearchPositionInfo.
; // Com.O2Bionics.Utils.SearchPositionInfo
  public AuthorIds: string[]; // System.Collections.Generic.List`1[System.String]
  public FromTime: string; // System.DateTime
  public ToTime: string; // System.DateTime
  public FromTimeStr: string; // System.String
  public ToTimeStr: string; // System.String
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

//Error: the code for type Com.O2Bionics.ChatService.Contract.AuditTrail.OperationStatus has already been generated.
//Error: null returned for type Com.O2Bionics.ChatService.Contract.AuditTrail.OperationStatus_BadName.
export class ProductCodesNames extends IdToNameMap {
  constructor()
  {
    super();

    const m = this.idNameMap;

//Error: The type ProductCodes must have keys.
  }
}
