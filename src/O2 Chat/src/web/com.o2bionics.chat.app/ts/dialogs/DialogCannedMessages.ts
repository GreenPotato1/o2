import * as Common from '../Common';
import DialogBase from './DialogBase';
import AppBase from '../AppBase';
import CannedMessageModel from '../models/CannedMessageModel';
import CannedMessageStorage from '../models/CannedMessageStorage';
import DialogCannedMessageEdit from './DialogCannedMessageEdit';
import DialogDeleteObject from './DialogDeleteObject';

export default class DialogCannedMessages extends DialogBase<CannedMessageModel | null> {

  private static readonly templateId = 'dialog-canned-messages-tmpl';

  public readonly filterString = ko.observable('');

  private readonly list = ko.observable(ko.observableArray<CannedMessageModel>([]));

  private readonly trimmedFilterString = ko.pureComputed(
    () => this.filterString().trim().toLowerCase());

  public readonly filtered = ko.pureComputed(
    () =>
    {
      const fs = this.trimmedFilterString();
      return ko.utils.arrayFilter(this.list()(), x => !fs || x.matchSearchFilter(fs));
    });


  public constructor(
    app: AppBase,
    private readonly storage: CannedMessageStorage,
    private readonly canInsertText: boolean)
  {
    super(app, DialogCannedMessages.templateId, () => this.commands.close.execute());
  }


  public open(): Promise<CannedMessageModel | null>
  {
    this.list(this.storage.all);
    return this.show();
  }

  public readonly commands = {
      close: ko.command(
        {
          execute: () => this.hide(null),
        }),

      resetSearch: ko.command(
        {
          canExecute: () =>
          {
            console.log('crs', this.filterString().length, this.filterString());
            return this.filterString().length > 0;
          },
          execute: () =>
          {
            this.filterString('');
            $('#dialog-canned-messages-search').trigger('change');
          },
        }),

      addNew: ko.command(
        {
          execute: async () =>
          {
            const ed = new DialogCannedMessageEdit(this.app, this.storage, this.storage.createNew());
            const created = await ed.open();
            if (created)
            {
              const fs = this.trimmedFilterString();
              if (fs && !created.matchSearchFilter(fs))
                this.commands.resetSearch.execute();

              Common.scrollToTheEnd('dialog-canned-messages-list-container');
            }
          }
        }),
    };

  // #region item operations

  public canInsertMessageText = () =>
  {
    return this.canInsertText;
  };

  public readonly clickInsertMessageText = (item: CannedMessageModel) =>
  {
    if (!this.canInsertMessageText()) return;

    this.hide(item);
  }

  public readonly canEdit = (item: CannedMessageModel) =>
  {
    return this.storage.departmentId === item.departmentId();
  }

  public readonly clickEdit = async (item: CannedMessageModel) =>
  {
    if (!this.canEdit(item)) return;

    await new DialogCannedMessageEdit(this.app, this.storage, item).open();
  }

  public readonly clickDelete = async (item: CannedMessageModel) =>
  {
    if (!this.canEdit(item)) return;

    if (await new DialogDeleteObject(this.app, 'canned message', item.key()).open())
      await this.app.errorHandler.handleErrors(
        () => this.storage.delete(this.app, item.id()),
        this);
  }

  // #endregion
}