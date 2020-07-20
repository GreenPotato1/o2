import AppBase from '../AppBase';
import Deferred from '../Deferred';

class JqueryDialogModel {

  public readonly modal = true;
  public readonly autoOpen = false;
  public readonly resizable = false;
  public readonly draggable = true;
  public readonly closeOnEscape = true;

  public readonly create =
    (event: any) => $(event.target).parent().css('position', 'fixed');
  public readonly open: (() => void) | undefined = undefined;

  public readonly isOpen = ko.observable(false);

  constructor(
      public readonly height: number,
      public readonly width: number,
      public readonly close: () => void,
      isChildDialog?: boolean | undefined,
    )
  {
    if (!isChildDialog)
      this.open = JqueryDialogModel.fixZindex;
  }

  private static readonly fixZindex = () =>
  {
    $('.ui-dialog').css('z-index', 7001);
    $('.ui-widget-overlay').css('z-index', 7000);
  }
}

export abstract class TemplatedDialogBase {
  public dialog: JqueryDialogModel;

  protected constructor(
      public readonly templateId: string,
      private readonly close: () => void,
    )
  {}

  public createDialogModel(isChildDialog: boolean)
  {
    const template = document.getElementById(this.templateId);
    if (!template)
    {
      console.error(`template id=${this.templateId} not found`);
      return;
    }
    const widthText = template.getAttribute('width');
    const heightText = template.getAttribute('height');
    if (!widthText || !heightText)
    {
      console.error(`template id=${this.templateId} should have specified dimensions (width and height attributes)`);
      return;
    }

    this.dialog = new JqueryDialogModel(
      +heightText,
      +widthText,
      this.close,
      isChildDialog);
  }
}

export default abstract class DialogBase<TResult> extends TemplatedDialogBase {

  private closePromise?: Deferred<TResult>;

  protected constructor(
      protected readonly app: AppBase,
      templateId: string,
      private readonly onCloseCallback: () => void,
    )
  {
    super(
      templateId,
      () =>
      {
        if (this.closePromise !== undefined)
          this.onCloseCallback();
      });
  }

  protected afterShow(): void
  {}

  protected show(): Promise<TResult>
  {
    if (this.closePromise !== undefined)
      throw new Error('Invalid state: closePromise is created in show()');

    this.app.model.pushDialog(this);

    this.dialog.isOpen(true);
    this.afterShow();
    this.closePromise = new Deferred<TResult>();
    return this.closePromise.promise;
  }

  protected hide(r: TResult): void
  {
    const promise = this.closePromise;
    if (!promise) throw new Error('Invalid state: closePromise is not defined in hide()');
    this.closePromise = undefined;

    this.dialog.isOpen(false);
    this.app.model.popDialog();

    promise.resolve(r);
  }
}