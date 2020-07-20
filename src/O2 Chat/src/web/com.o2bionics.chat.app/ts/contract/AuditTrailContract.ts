// ReSharper disable InconsistentNaming

import * as Contract from './Contract';

export interface StringDictionary {
  [key: string]: string;
}

export interface StringToIdNames {
  [key: string]: Array<Contract.IdName<string>>;
}

export class Facet2 extends Contract.Facet {
  //JS only.
  public DisplayName: string;
}

export interface AuditEvent {
  Id: string; //Guid
  Author: Contract.Facet;
  CustomValues?: StringDictionary;

  CustomObjects?: StringToIdNames; // Sent from the server.

  Timestamp: string; //DateTime
  Operation: string;
  Status: string;

  OldValue?: any;
  NewValue?: any;
}

export class AuditRecord {
  //JS only class to display AuditEvent in a grid.

  public Id: string; //Guid
  public Timestamp: string; //DateTime

  public Operation: string;
  public Status: string;
  public Author: string;
  public Name: string;

  //Raw history document, removed after the "Details" are set.
  public History?: AuditEvent;

  //Binded HTML.
  public IsDetailsVisible: KnockoutObservable<boolean>;
  public Details: KnockoutObservable<string>;
  public lazyFormDetails: () => void;
}

export interface FacetResponse {
  RawDocuments: Array<string>;
  Operations: Array<Facet2>;
  Statuses: Array<Facet2>;
  Authors: Array<Facet2>;

  //JS only.
  Documents: Array<AuditEvent>;
}


//JS only:
export class className {
  public static readonly deleted = 'deleted';
  public static readonly inserted = 'inserted';
}

export interface Formatter {
  format(history: AuditEvent): string;
}

export class AuditTrailMaps {
  public readonly operationKindNames = new Contract.OperationKindNames();
  public readonly operationStatusNames = new Contract.OperationStatusNames();
  public readonly objectStatusNames = new Contract.ObjectStatusNames();
}