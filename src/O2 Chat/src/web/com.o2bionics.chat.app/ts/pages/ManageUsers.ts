import * as Contract from '../contract/Contract';
import { UserController } from '../contract/Controllers';

import EditUserModel from '../models/EditUserModel';
import UserStorage from '../models/UserStorage';
import DepartmentStorage from '../models/DepartmentStorage';
import VisitorStorage from '../models/VisitorStorage';

import AppBase from '../AppBase';
import { AjaxServerError } from '../ErrorHandler';

import SelectAvatarDialog from '../dialogs/SelectAvatarDialog';
import MainModelBase from '../MainModelBase';


export default class ManageUsersApp extends AppBase {

  private constructor(currentUserId: number)
  {
    super(
      currentUserId,
      new UserStorage(currentUserId, info => new EditUserModel(info, this)),
      new DepartmentStorage(),
      new VisitorStorage());
  }

  public createModel(): MainModelBase
  {
    return new ManageUsersMainModel(this);
  }
}

class ManageUsersMainModel extends MainModelBase {

  public readonly list = ko.pureComputed(
    () => this.app.userStorage.visible().sort(
      (x, y) =>
      {
        if (x.id === 0) return -1;
        if (y.id === 0) return 1;
        return y.id - x.id;
      }));
  public readonly count = ko.pureComputed(() => this.app.userStorage.visible().length);

  public readonly departments = ko.pureComputed(() => this.app.departmentStorage.visible());
  public readonly maxUsers = ko.observable<number>();
  public readonly editMode = ko.observable<string>(null);
  public readonly editItem = ko.observable<EditUserModel>(null);
  public readonly listErrorMessages = ko.observableArray<string>([]);
  public readonly areAvatarsAllowed = ko.observable<boolean>();
  public readonly dialogSelectAvatar: SelectAvatarDialog;

  constructor(private readonly app: ManageUsersApp)
  {
    super(app);

    this.dialogSelectAvatar = new SelectAvatarDialog(this.app);
  }

  public init(): void
  {
    this.load();
  }

  public run(): void
  {}

  public readonly isEditing = ko.pureComputed(() => this.editItem() !== null);

  public readonly showCountWarning = ko.pureComputed(() => this.count() >= this.maxUsers() - 5);
  public readonly canAdd = ko.pureComputed(() => !this.isEditing() && this.count() < this.maxUsers());

  public readonly isUserEditable = (user: EditUserModel) =>
    this.currentUser()
    && (this.currentUser().isOwner() || !user.isOwner());

  public readonly isUserDeletable = (user: EditUserModel) =>
    this.currentUser()
    && (this.currentUser().id !== user.id)
    && (this.currentUser().isOwner() || !user.isOwner());


  // knockout bindable

  public templateToUse = (item: EditUserModel) =>
  {
    return this.editItem() !== item ? 'rowTmpl' : `row${this.editMode()}Tmpl`;
  };

  public showCreateTemplate = () =>
  {
    if (!this.canAdd()) return;

    const x = this.app.userStorage.createNew() as EditUserModel;
    this.editMode('Edit');
    this.editItem(x);
  }

  public showEditTemplate = (item: EditUserModel) =>
  {
    if (this.editItem() || !this.isUserEditable(item)) return;

    this.editMode('Edit');
    this.editItem(item);
  }

  public showDeleteTemplate = (item: EditUserModel) =>
  {
    if (this.editItem()) return;
    if (!this.isUserDeletable(item))
    {
      this.app.toast('Error', '<p>Can\'t delete current logged in user</p>');
      return;
    }

    this.editMode('Delete');
    this.editItem(item);
  }

  public showSetPasswordTemplate = (item: EditUserModel) =>
  {
    if (this.editItem() || !this.isUserEditable(item)) return;

    this.editMode('SetPassword');
    this.editItem(item);
  }

  public cancelEdit = () =>
  {
    if (!this.editItem()) return;

    const ei = this.editItem();
    this.editItem(null);

    if (ei.id === 0)
    {
      this.app.userStorage.remove(ei);
    }
    else
    {
      ei.reset();
    }
  }

  public create = () =>
  {
    const item = this.editItem();
    if (!item) return;

    if (item.userPropertiesErrors().length > 0 || item.userPasswordErrors().length > 0)
    {
      item.userPropertiesErrors.showAllMessages();
      item.userPasswordErrors.showAllMessages();
      return;
    }

    this.serverCall(
      item,
      () => new UserController(this.app).create(item.getData(), item.password()),
      (model, info) =>
      {
        this.app.userStorage.remove(item);
        this.app.userStorage.createNew(info);
      });
  }

  public update = () =>
  {
    const item = this.editItem();
    if (!item) return;

    if (item.userPropertiesErrors().length > 0)
    {
      item.userPropertiesErrors.showAllMessages();
      return;
    }

    this.serverCall(
      item,
      () => new UserController(this.app).update(item.getData()),
      (model, info) => model.update(info));
  }

  public showSelectAvatarDialog = async () =>
  {
    if (!this.areAvatarsAllowed())
    {
      this.showAvatarsWarning();
      return;
    }

    const item = this.editItem();
    if (!item) return;

    const selected = await this.dialogSelectAvatar.open(item.avatar());
    item.avatar(selected);
  }

  public uploadAvatar = () =>
  {
    if (!this.areAvatarsAllowed())
    {
      this.showAvatarsWarning();
      return;
    }

    const item = this.editItem();
    if (!item) return;

    // ... show()
  }

  public showAvatarsWarning = () =>
  {
    this.app.toast('Warning', '<p>Feature disabled. Please upgrade your plan for enable selecting avatars.</p>');
  }

  public updatePassword = () =>
  {
    const item = this.editItem();
    if (!item) return;

    if (item.userPasswordErrors().length > 0)
    {
      item.userPasswordErrors.showAllMessages();
      return;
    }

    this.serverCall(
      item,
      () => new UserController(this.app).setPassword(item.id, item.password()),
      (model, info) => model.update(info));
  }

  public delete = () =>
  {
    const item = this.editItem();
    if (!item) return;

    this.serverCall(
      item,
      () => new UserController(this.app).delete(item.id),
      model => this.app.userStorage.remove(model));
  }

  public refresh = () =>
  {
    if (this.editItem()) return;
    this.load();
  }

  // end of knockout bindable

  private async load(): Promise<void>
  {
    this.listErrorMessages.removeAll();

    try
    {
      const r = await new UserController(this.app).getAll();
      this.app.departmentStorage.load(r.Departments);
      this.app.userStorage.load(r.Users);
      this.areAvatarsAllowed(r.AreAvatarsAllowed);
      this.maxUsers(r.MaxUsers);
      this.updateCurrentUser();
    }
    catch (x)
    {
      if (x instanceof AjaxServerError)
      {
        this.listErrorMessages((x as AjaxServerError).messages.map(y => y.Message));
      }
      else this.listErrorMessages(['Server call failed.']);
    }
  };

  private async serverCall(
    item: EditUserModel,
    serverCall: () => Promise<Contract.UpdateUserResult>,
    successCallback: (x: EditUserModel, info: Contract.UserInfo) => void): Promise<void>
  {
    this.listErrorMessages.removeAll();

    try
    {
      const r = await serverCall();

      this.editItem(null);
      successCallback(item, r.User);
    }
    catch (x)
    {
      if (x instanceof AjaxServerError)
      {
        item.serverMessages.removeAll();
        const messages = (x as AjaxServerError).messages;
        for (const message of messages)
        {
          if (message.Field && item.hasOwnProperty(message.Field))
            (item as any)[message.Field].setError(message.Message);
          else
            item.serverMessages.push(message.Message);
        }
      }
      else item.serverMessages(['Server call failed.']);
    }
  }
}