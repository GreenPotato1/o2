import AppBase from './AppBase';
import UserModel from './models/UserModel';
import UserStorage from './models/UserStorage';
import DepartmentStorage from './models/DepartmentStorage';
import VisitorStorage from './models/VisitorStorage';
import { TemplatedDialogBase } from './dialogs/DialogBase';

abstract class MainModelBase {

  public readonly currentUser = ko.observable<UserModel>(null);

  public readonly dialogStack = ko.observableArray<TemplatedDialogBase>([]);
  public readonly dialogStackItemTemplate = (item: TemplatedDialogBase) => item.templateId;

  protected constructor(
    private readonly appBase: AppBase)
  {}

  public pushDialog(dialog: TemplatedDialogBase): void
  {
    dialog.createDialogModel(this.dialogStack.length > 0);
    this.dialogStack.push(dialog);
  }

  public popDialog(): void
  {
    this.dialogStack.pop();
  }


  public abstract init(): void;

  public abstract run(): void;

  protected updateCurrentUser()
  {
    const cu = this.appBase.userStorage.get(this.appBase.currentUserId);
    this.currentUser(cu ? cu.clone() : null);
  }

  protected updateCurrentUserInfo(firstName: string, lastName: string, avatar: string): void
  {
    const cu = this.currentUser();
    if (cu)
    {
      cu.firstName(firstName);
      cu.lastName(lastName);
      cu.avatar(avatar);
    }
  }
}

export default MainModelBase;