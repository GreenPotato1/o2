import AppBase from '../AppBase';
import DialogBase from './DialogBase';

import DepartmentStorage from '../models/DepartmentStorage';
import DepartmentModel from '../models/DepartmentModel';
import EditDepartmentModel from '../models/EditDepartmentModel';

import { DepartmentController } from '../contract/Controllers';


export default class DialogDepartmentEdit extends DialogBase<DepartmentModel | null> {

  private static readonly templateId = 'dialog-department-edit-tmpl';

  public readonly editItem = ko.observable<EditDepartmentModel>();

  public constructor(
      app: AppBase,
      private readonly storage: DepartmentStorage,
      item: DepartmentModel,
    )
  {
    super(app, DialogDepartmentEdit.templateId, () => this.commands.close.execute());
    this.editItem(new EditDepartmentModel(item.asInfo()));
  }

  protected afterShow(): void
  {
    const i = $('#dialog-department-edit input')[0];
    console.log('first input', i);
    if (i) (i as HTMLElement).focus();
  }

  public open(): Promise<DepartmentModel | null>
  {
    return this.show();
  }

  public readonly commands = {
      close: ko.command(
        {
          execute: () => this.hide(null),
        }),
      save: ko.command(
        {
          canExecute: () => this.editItem() ? this.editItem().isValid() : false,
          execute: async () =>
          {
            const model = this.editItem();
            model.errorMessages.removeAll();
            console.debug('save', model);

            const controller = new DepartmentController(this.app);

            const d = model.asInfo();

            const r = await this.app.errorHandler.handleErrors(
              () => model.id() === 0
                    ? controller.create(d)
                    : controller.update(d),
              model);

            if (r)
            {
              this.storage.update(r.Department);
              const m = this.storage.get(r.Department.Id);

              console.debug('save model updated', m);

              this.hide(m === undefined ? null : m);
            }
          },
        }),
    };
}