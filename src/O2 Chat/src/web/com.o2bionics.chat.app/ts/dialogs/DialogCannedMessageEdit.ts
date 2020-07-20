import AppBase from '../AppBase';
import DialogBase from './DialogBase';
import CannedMessageModel from '../models/CannedMessageModel';
import CannedMessageStorage from '../models/CannedMessageStorage';

import EditCannedMessageModel from '../models/EditCannedMessageModel';

export default class DialogCannedMessageEdit extends DialogBase<CannedMessageModel | null> {

  private static readonly templateId = 'dialog-canned-message-edit-tmpl';

  public readonly editItem = ko.observable<EditCannedMessageModel>();

  private readonly errors = ko.validation.group(this);

  public constructor(app: AppBase,
    private readonly storage: CannedMessageStorage,
    item: CannedMessageModel)
  {
    super(app, DialogCannedMessageEdit.templateId, () => this.commands.close.execute());

    this.editItem(new EditCannedMessageModel(item.asInfo()));
  }

  protected afterShow(): void
  {
    ($('#dialog-edit-canned-message input')[0] as HTMLElement).focus();
  }


  public open(): Promise<CannedMessageModel | null>
  {
    $('#dialog-edit-canned-message-key').trigger('change');
    $('#dialog-edit-canned-message-value').trigger('change');

    return this.show();
  }

  public readonly commands = {
      close: ko.command(
        {
          execute: () => this.hide(null),
        }),
      save: ko.command(
        {
          canExecute: () =>
          {
            if (this.errors().length > 0)
            {
              this.errors.showAllMessages();
              return false;
            }
            return true;
          },
          execute: async () =>
          {
            var model = this.editItem();

            const modified = model.clone();

            const saved = await this.app.errorHandler.handleErrors(
              () => this.storage.save(this.app, modified),
              model);

            if (saved)
              this.hide(saved);
          },
        }),
    };
}