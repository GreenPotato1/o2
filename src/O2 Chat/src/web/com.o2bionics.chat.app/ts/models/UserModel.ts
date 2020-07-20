import * as Contract from '../contract/Contract';
import AvatarHelper from '../AvatarHelper';


export default class UserModel {

  public readonly id: number = 0;
  public readonly addTimestamp: Date | undefined = undefined;

  public readonly updateTimestamp = ko.observable<Date>(null);
  public readonly status = ko.observable<Contract.ObjectStatus>();
  public readonly firstName = ko.observable<string>();
  public readonly lastName = ko.observable<string>();
  public readonly email = ko.observable<string>();
  public readonly avatar = ko.observable<string>();
  public readonly isOwner = ko.observable<boolean>();
  public readonly isAdmin = ko.observable<boolean>();
  public readonly agentDepartments = ko.observableArray<number>();
  public readonly supervisorDepartments = ko.observableArray<number>();

  public readonly fullName = ko.pureComputed(
    () => this.firstName() + ' ' + this.lastName());
  public readonly avatarUrl = ko.pureComputed(() => AvatarHelper.toAvatarUrl(this.avatar()));

  public readonly isActive = ko.pureComputed(
    () => this.status() === Contract.ObjectStatus.Active);
  public readonly isDeleted = ko.pureComputed(
    () => this.status() === Contract.ObjectStatus.Deleted);

  public constructor(protected source: Contract.UserInfo | null)
  {
    if (source)
    {
      this.id = source.Id;
      this.addTimestamp = new Date(source.AddTimestampUtc);

      this.update(source);
    }
    else
    {
      this.addTimestamp = new Date();
      this.status(Contract.ObjectStatus.Active);
    }
  }

  public clone(): UserModel
  {
    return new UserModel(this.source);
  }

  public update(info: Contract.UserInfo): void
  {
    this.updateTimestamp(new Date(info.AddTimestampUtc));
    this.status(info.Status);
    this.firstName(info.FirstName);
    this.lastName(info.LastName);
    this.email(info.Email);
    this.avatar(info.Avatar);
    this.isOwner(info.IsOwner);
    this.isAdmin(info.IsAdmin);
    this.agentDepartments(info.AgentDepartments);
    this.supervisorDepartments(info.SupervisorDepartments);

    this.source = info;
  }

  public getData(): Contract.UserInfo
  {
    const email = this.email();
    const firstName = this.firstName();
    const lastName = this.lastName();

    const user = {
        CustomerId: 0 as number,
        AddTimestampUtc: '' as string,
        UpdateTimestampUtc: '' as string,

        Id: this.id,
        Status: this.status(),
        Email: email ? email.trim() : '',
        Avatar: this.avatar(),
        FirstName: firstName ? firstName.trim() : '',
        LastName: lastName ? lastName.trim() : '',
        IsOwner: this.isOwner(),
        IsAdmin: this.isAdmin(),
        AgentDepartments: this.agentDepartments().slice(0),
        SupervisorDepartments: this.supervisorDepartments().slice(0),
      };
    return user;
  }

  public reset(): void
  {
    const s = this.source;

    this.status(s ? s.Status : Contract.ObjectStatus.Active);
    this.email(s ? s.Email : '');
    this.avatar(s ? s.Avatar : null);
    this.firstName(s ? s.FirstName : '');
    this.lastName(s ? s.LastName : '');
    this.isOwner(s ? s.IsOwner : false);
    this.isAdmin(s ? s.IsAdmin : false);
    this.agentDepartments(s ? s.AgentDepartments.slice(0) : []);
    this.supervisorDepartments(s ? s.SupervisorDepartments.slice(0) : []);
  }
}