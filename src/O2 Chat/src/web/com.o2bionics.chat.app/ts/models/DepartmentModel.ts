import * as Contract from '../contract/Contract';

export default class DepartmentModel {
  public readonly id = ko.observable<number>(0);
  public readonly status = ko.observable<Contract.ObjectStatus>();
  public readonly isPublic = ko.observable<boolean>();
  public readonly name = ko.observable<string>();
  public readonly description = ko.observable<string>();

  public readonly isActive = ko.pureComputed(
    () => this.status() === Contract.ObjectStatus.Active);
  public readonly isDeleted = ko.pureComputed(
    () => this.status() === Contract.ObjectStatus.Deleted);

  public readonly statusText = ko.pureComputed(
    () =>
    {
      const status = this.status();
      return status === 0
               ? 'Active'
               : status === 1
               ? 'Disabled'
               : status === 2
               ? 'Deleted'
               : 'Unkonwn';
    });

  public constructor(info?: Contract.DepartmentInfo)
  {
    if (info)
    {
      this.id(info.Id);

      this.update(info);
    }
  }

  public update(info: Contract.DepartmentInfo): void
  {
    this.status(info.Status);
    this.isPublic(info.IsPublic);
    this.name(info.Name);
    this.description(info.Description);
  }

  public asInfo(): Contract.DepartmentInfo
  {
    const info = {
        Id: this.id(),
        CustomerId: 0 as number,
        Status: this.status(),
        IsPublic: this.isPublic(),
        Name: this.name(),
        Description: this.description(),
      };
    console.debug('asInfo()', info);
    return info;
  }
}