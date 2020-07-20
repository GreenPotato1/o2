import AppBase from '../AppBase';
import DialogBase from './DialogBase';

import { AccountController } from '../contract/Controllers';

export default class SelectAvatarDialog extends DialogBase<string | null> {

  private static readonly templateId = 'dialog-select-avatar';

  public predefinedAvatars = ko.observableArray<string>([]);
  public selected = ko.observable<string>();

  public constructor(app: AppBase)
  {
    super(app, SelectAvatarDialog.templateId, () => this.onCancel());
  }

  public onOk = () =>
  {
    this.hide(this.selected());
  }

  public onCancel = () =>
  {
    this.hide(null);
  }

  public onDblClick = () =>
  {
    this.onOk();
  }

  public open(currentAvatar: string): Promise<string | null>
  {
    this.loadAvatars();
    this.selected(currentAvatar);
    return this.show();
  }

  public selectAvatar = (a: string) =>
  {
    this.selected(a);
  }

  private async loadAvatars(): Promise<void>
  {
    const avatars = await new AccountController(this.app)
      .getAvatars();
    this.predefinedAvatars(avatars ? avatars : []);
  }
}