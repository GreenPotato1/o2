import * as Contract from './contract/Contract';

import IServerTransport from './IServerTransport'
import MainModelBase from './MainModelBase';
import UserStorage from './models/UserStorage';
import DepartmentStorage from './models/DepartmentStorage';
import VisitorStorage from './models/VisitorStorage';
import * as Common from './Common';
import ErrorHandler from './ErrorHandler';
import { AjaxServerError, AjaxError } from './ErrorHandler';


abstract class AppBase implements IServerTransport {

  public errorHandler: ErrorHandler;

  protected constructor(
    public readonly currentUserId: number,
    public readonly userStorage: UserStorage,
    public readonly departmentStorage: DepartmentStorage,
    public readonly visitorStorage: VisitorStorage)
  {
    this.errorHandler = new ErrorHandler();
  }

  private mainModel: MainModelBase | null = null;

  public get model(): MainModelBase
  {
    if (this.mainModel === null)
      throw 'MainModel is not created.';
    return this.mainModel;
  }


  public run(): void
  {
    this.registerComponents();

    this.initializeValidation();

    this.mainModel = this.createModel();
    this.mainModel.init();

    ko.applyBindings(this.mainModel, document.getElementById('all-body'));
    this.mainModel.run();
  }

  protected abstract createModel(): MainModelBase;

  protected registerComponents(): void
  {}

  protected initializeValidation(): void
  {
    ko.validation.init(
      {
        errorElementClass: 'error',
        decorateElementOnModified: true,
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: false,
        parseInputAttributes: true,
      },
      true);
  }

  public blockUi(): void
  {
    $('#objectTable').block({ message: null });
  }

  public unblockUi(): void
  {
    $('#objectTable').unblock();
  }

  public toast(title: string, messageHtml: string): void
  {
    $.gritter.add(
      {
        title: title,
        text: messageHtml,
        sticky: false,
        time: 7000,
        class_name: 'gritter-custom'
      });
  }

  public getTrackerUrl(): string
  {
    throw new Error('Not implemented');
  };


  public visitorHtml(id: number): string
  {
    return this.objectHtml(this.visitorStorage.name(id), '&#xE55A;');
  }

  public userHtml(skey: number): string
  {
    return this.objectHtml(this.userStorage.name(skey), '&#xE7FD;');
  }

  public departmentHtml(skey: number): string
  {
    return this.objectHtml(this.departmentStorage.name(skey), '&#xE7EF;');
  }

  private objectHtml(name: string, symbol: string): string
  {
    return `<span style='white-space: nowrap;'><i class='material-icons'>${symbol}</i> ${Common.escapeHtml(name)
      }</span>`;
  }

  public call<TResult>(method: string, url: string, data: any = null, crossDomain: boolean = false): Promise<TResult>
  {
    console.debug('call:', method, url, data);

    this.blockUi();

    return new Promise<TResult>(
      (resolve, reject) =>
      {
        const request: any = {
            type: method,
            url: url,
            dataType: 'json',
            data: data,
          };
        if ('GET' !== method)
        {
          request.contentType = 'application/json; charset=utf-8';
          request.data = JSON.stringify(data);
        }

        // TODO: task-387, task-390. Currently the Page Tracker cannot use the "token".
        // if (crossDomain) { request.crossDomain = true; request.xhrFields = { withCredentials: true}; }

        console.debug(request);

        $.ajax(request)
          .always(() => this.unblockUi())
          .done(
            x =>
            {
              console.debug('call done', url, x);

              if (x.Status !== undefined
                && x.Status.StatusCode !== undefined
                && x.Status.StatusCode !== Contract.CallResultStatusCode.Success)
              {
                console.error('call fail on the server', url, x);
                reject(new AjaxServerError(x.Status.StatusCode, x.Status.Messages));
                return;
              }

              resolve(x);
            })
          .fail(
            x =>
            {
              console.error('call fail', url, x);
              reject(new AjaxError(x));
            });
      });
  }
}

export default AppBase;