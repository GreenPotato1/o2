import * as Contract from '../contract/Contract';

export default class CannedMessageModel {

  public readonly key = ko.observable<string>();
  public readonly value = ko.observable<string>();
  public readonly id = ko.observable<number>(0);
  public readonly departmentId = ko.observable<number>();
  public readonly userId = ko.observable<number>();


  public constructor(info?: Contract.CannedMessageInfo)
  {
    if (info)
    {
      this.id(info.Id);

      this.update(info);
    }
  }

  public matchSearchFilter(s: string): boolean
  {
    return this.key().toLowerCase().indexOf(s) >= 0
      || this.value().toLowerCase().indexOf(s) >= 0;
  }

  public keyStartsWith(s: string): boolean
  {
    return this.key().toLowerCase().indexOf(s.toLowerCase()) === 0;
  }


  public update(info: Contract.CannedMessageInfo): void
  {
    if (this.id() !== 0 && this.id() !== info.Id)
      throw `CannedMessage id mismatch ${this.id} != ${info.Id}`;

    this.key(info.Key);
    this.value(info.Value);
    this.departmentId(info.DepartmentId);
    this.userId(info.UserId);
  }

  public createNew(departmentId: number | null, userId: number | null): CannedMessageModel
  {
    this.departmentId(departmentId);
    this.userId(userId);

    return new CannedMessageModel(this.asInfo());
  }

  public static fromInfo(info: Contract.CannedMessageInfo)
  {
    return new CannedMessageModel(info);
  }

  public clone(): CannedMessageModel
  {
    var info = this.asInfo();
    info.Id = this.id();

    return new CannedMessageModel(info);
  }

  public asInfo(): Contract.CannedMessageInfo
  {
    return {
        AddTimestampUtc: '' as string,
        UpdateTimestampUtc: '' as string,
        Id: this.id(),
        DepartmentId: this.departmentId(),
        UserId: this.userId(),
        Key: this.key(),
        Value: this.value(),
      };
  }
}