import { DepartmentController } from '../contract/Controllers';
import AppBase from '../AppBase';
import * as EH from '../ErrorHandler';
import MainModelBase from '../MainModelBase';

import UserModel from '../models/UserModel';
import DepartmentModel from '../models/DepartmentModel';
import EditDepartmentModel from '../models/EditDepartmentModel';
import UserStorage from '../models/UserStorage';
import DepartmentStorage from '../models/DepartmentStorage';
import VisitorStorage from '../models/VisitorStorage';
import CannedMessageStorage from '../models/CannedMessageStorage';

import DialogDepartmentEdit from '../dialogs/DialogDepartmentEdit';
import DialogDeleteObject from '../dialogs/DialogDeleteObject';
import DialogCannedMessages from '../dialogs/DialogCannedMessages';


export default class ManageDepartmentsApp extends AppBase {

  private constructor(currentUserId: number)
  {
    super(
      currentUserId,
      new UserStorage(currentUserId, info => new UserModel(info)),
      new DepartmentStorage(),
      new VisitorStorage());
  }

  public createModel(): MainModelBase
  {
    return new ManageDepartmentsMainModel(this);
  }

  public registerComponents(): void
  {
    super.registerComponents();
  }
}

class ManageDepartmentsMainModel extends MainModelBase implements EH.IErrorMessagesSite {

  public readonly list = ko.pureComputed(
    () => this.app.departmentStorage.visible().sort(
      (x, y) =>
      {
        if (x.id() === 0) return -1;
        if (y.id() === 0) return 1;
        return y.id() - x.id();
      }));
  public readonly count = ko.pureComputed(() => this.app.departmentStorage.visible().length);

  public readonly maxDepartments = ko.observable<number>();

  public readonly canAdd = ko.pureComputed(() => this.count() < this.maxDepartments());

  public readonly showCountWarning = ko.pureComputed(() => this.count() >= this.maxDepartments() - 5);

  public errorMessages = ko.observableArray<string>();

  constructor(private readonly app: ManageDepartmentsApp)
  {
    super(app);
  }

  public init(): void
  {
    this.load();
  }

  public run(): void {}

  public readonly commands = {
      create: ko.command(
        {
          canExecute: () => this.canAdd(),
          execute: async () =>
          {
            await new DialogDepartmentEdit(
                this.app,
                this.app.departmentStorage,
                this.app.departmentStorage.createNew())
              .open();
          }
        }),
      refresh: ko.command(
        {
          execute: () => this.load(),
        }),
    };

  public editItem = async (item: DepartmentModel) =>
  {
    await new DialogDepartmentEdit(
        this.app,
        this.app.departmentStorage,
        item)
      .open();
  }

  public deleteItem = async (item: EditDepartmentModel) =>
  {
    if (await new DialogDeleteObject(this.app, 'department', item.name()).open())
    {
      const r = await this.app.errorHandler.handleErrors(
        () => new DepartmentController(this.app).delete(item.id()),
        item);

      if (r)
        this.app.departmentStorage.remove(item);
    }
  }

  public showItemCannedMessages = async (department: EditDepartmentModel) =>
  {
    const departmentId = department.id();

    const r = await this.app.errorHandler.handleErrors(
      () => CannedMessageStorage.loadDepartmentMessages(this.app, departmentId));

    if (r)
      await new DialogCannedMessages(this.app, r, false).open();
  }

  // end of knockout bindable

  private async load(): Promise<void>
  {
    this.errorMessages.removeAll();

    const r = await this.app.errorHandler.handleErrors(
      () => new DepartmentController(this.app).getAll(),
      this);

    if (r)
    {
      this.app.departmentStorage.load(r.Departments);
      this.maxDepartments(r.MaxDepartments);
    }
  };
}