import * as Contract from '../contract/Contract';
import UserModel from './UserModel';
import AppBase from '../AppBase';

export default class EditUserModel extends UserModel {

  public readonly serverMessages = ko.observableArray(<string[]>[]);

  public readonly isDisabled = ko.pureComputed(
    {
      read: () => this.status() === Contract.ObjectStatus.Disabled,
      write: (value: boolean) =>
      {
        const old = this.status();
        if (value)
          this.status(Contract.ObjectStatus.Disabled);
        else
          this.status(old === Contract.ObjectStatus.Disabled ? Contract.ObjectStatus.Active : old);
      },
    });

  public readonly statusText = ko.pureComputed(
    () =>
    {
      const status = this.status();
      return typeof Contract.ObjectStatus[status] !== 'undefined' ? Contract.ObjectStatus[status] : 'Unknown';
    });

  // base class properties validation
  public readonly userPropertiesErrors = ko.validation.group(
    [
      this.status,
      this.email,
      this.firstName,
      this.lastName,
      this.isOwner,
      this.isAdmin,
      this.isDisabled,
      this.agentDepartments,
      this.supervisorDepartments
    ]);

  public readonly password = ko.observable('');
  public readonly password2 = ko.observable('');

  public readonly userPasswordErrors = ko.validation.group(
    [
      this.password,
      this.password2
    ]);

  public readonly agentDepartmentNames = ko.pureComputed(
    () => this.app.departmentStorage.names(this.agentDepartments()).join('\n'));
  public readonly supervisorDepartmentNames = ko.pureComputed(
    () => this.app.departmentStorage.names(this.supervisorDepartments()).join('\n'));


  public constructor(
    source: Contract.UserInfo | null,
    private readonly app: AppBase)
  {
    super(source);

    this.email
      .extend(
        {
          maxLength: 256,
          email: { message: 'Please enter valid Email' },
          required: { message: 'Please enter user Email' }
        });

    this.firstName
      .extend(
        {
          maxLength: 60,
          required: { message: 'Please enter First Name' }
        });
    this.lastName
      .extend(
        {
          maxLength: 60,
        });
    this.isOwner
      .extend(
        {
          validation: {
              validator: (val: boolean) => val || !this.isLastActiveOwner(),
              message: 'You are the last active Owner here'
            }
        });
    this.isDisabled
      .extend(
        {
          validation: {
              validator: (val: boolean) => !val || !this.isLastActiveOwner(),
              message: 'You are the last active Owner here'
            }
        });

    this.password
      .extend(
        {
          required: { message: 'Please enter Password' }
        });
    this.password2
      .extend(
        {
          equal: this.password
        });
    this.supervisorDepartments
      .extend(
        {
          validation: {
              validator: (val: string) =>
                val.length > 0
                || this.isOwner()
                || this.isAdmin()
                || this.agentDepartments().length > 0,
              message: 'Please provide any of access rights',
            }
        });
  }

  private isLastActiveOwner(): boolean
  {
    return !this.app.userStorage.all().some(
      x =>
      x.id !== this.id
      && x.status() === Contract.ObjectStatus.Active
      && x.isOwner());
  }

  public update(info: Contract.UserInfo): void
  {
    super.update(info);
  }

  public reset(): void
  {
    super.reset();

    this.password('');
    this.password2('');
    this.serverMessages([]);
  }
}