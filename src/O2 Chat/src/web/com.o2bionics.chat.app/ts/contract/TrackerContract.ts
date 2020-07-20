// This file was auto-generated. If necessary, make changes only to the source .t4 files.

// ReSharper disable InconsistentNaming

export interface Point {
  lat: number; // System.Double
  lon: number; // System.Double
}

export interface SearchPositionInfo {
  Values: string[]; // System.Collections.Generic.List`1[System.String]
}

export interface UserAgentInfo {
  Device: string; // System.String
  Os: string; // System.String
  UserAgent: string; // System.String
  UserAgentString: string; // System.String
}

export interface TimeZoneDescription {
  Offset: number; // System.Int32
  Description: string; // System.String
}

export interface GeoLocation {
  Country: string; // System.String
  City: string; // System.String
  Point: Point; // Com.O2Bionics.PageTracker.Contract.Point
}

export interface PageHistoryRecord {
  Id: string; // System.String
  TimestampUtc: string; // System.DateTime
  Url: string; // System.Uri
  CustomText: string; // System.String
}

export interface PageHistoryVisitorInfo {
  TimestampUtc: string; // System.DateTime
  VisitorExternalId: string; // System.String
  Ip: string; // System.Net.IPAddress
  IpLocation: GeoLocation; // Com.O2Bionics.PageTracker.Contract.GeoLocation
  TimeZone: TimeZoneDescription; // Com.O2Bionics.PageTracker.Contract.TimeZoneDescription
  UserAgent: UserAgentInfo; // Com.O2Bionics.PageTracker.Contract.UserAgentInfo
}

export interface GetHistoryResult {
  Visitor: PageHistoryVisitorInfo; // Com.O2Bionics.PageTracker.Contract.PageHistoryVisitorInfo
  Items: PageHistoryRecord[]; // System.Collections.Generic.List`1[Com.O2Bionics.PageTracker.Contract.PageHistoryRecord]
  HasMore: boolean; // System.Boolean
  SearchPosition: SearchPositionInfo; // Com.O2Bionics.Utils.SearchPositionInfo
}
