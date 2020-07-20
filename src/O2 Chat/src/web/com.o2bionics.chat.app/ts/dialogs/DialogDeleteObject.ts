import AppBase from '../AppBase';
import DialogBase from './DialogBase';

export default class DialogDeleteObject extends DialogBase<boolean> {

  private static readonly templateId = 'dialog-delete-object-tmpl';

  public readonly objectType = ko.observable<string>(null);
  public readonly objectName = ko.observable<string>(null);

  private readonly errors = ko.validation.group(this);

  public constructor(app: AppBase, objectType: string, objectName: string)
  {
    super(app, DialogDeleteObject.templateId, () => this.commands.close.execute());

    this.objectType(objectType);
    this.objectName(objectName);
  }

  public open(): Promise<boolean>
  {
    return this.show();
  }

  public readonly commands = {
      close: ko.command(
        {
          execute: () => this.hide(false),
        }),
      delete: ko.command(
        {
          execute: () => this.hide(true),
        }),
    };
}