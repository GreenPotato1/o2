import * as Contract from './contract/Contract';
import * as Common from './Common';

export class AjaxError extends Error {
  public errorMessage: string;

  public constructor(public readonly xhr: JQueryXHR)
  {
    super(
      xhr.status === 0
      ? 'Network error'
      : `Server error ${xhr.status}`);

    if (400 //Error codes that can be returned by us.
      === xhr.status
      && 'string' === typeof (xhr.responseText)
      && null != xhr.responseText
      && 0 < xhr.responseText.length)
      this.errorMessage = xhr.responseText;

    // see https://github.com/Microsoft/TypeScript/issues/12123
    Object.setPrototypeOf(this, AjaxError.prototype);
  }
}

export class AjaxServerError extends Error {
  public constructor(
    public readonly code: Contract.CallResultStatusCode,
    public readonly messages: Array<Contract.ValidationMessage>)
  {
    super('Server call failed');

    // see https://github.com/Microsoft/TypeScript/issues/12123
    Object.setPrototypeOf(this, AjaxServerError.prototype);
  }
}

export interface IErrorMessagesSite {
  errorMessages: KnockoutObservableArray<string>
}

export default class ErrorHandler {

  private toast(title: string, messageHtml: string): void
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

  private isIErrorMessagesSite(obj: any): obj is IErrorMessagesSite
  {
    return !!obj && 'errorMessages' in obj;
  }

  public async handleErrors<T>(func: () => Promise<T>, errorsTarget: any = null): Promise<T | undefined>
  {
    if (this.isIErrorMessagesSite(errorsTarget))
      errorsTarget.errorMessages.removeAll();

    try
    {
      return await func();
    }
    catch (x)
    {
      if (x instanceof AjaxServerError && errorsTarget)
      {
        switch (x.code)
        {
        case Contract.CallResultStatusCode.Failure:
        case Contract.CallResultStatusCode.AccessDenied:
        case Contract.CallResultStatusCode.NotFound:
          this.toast('Server call failed.', Common.escapeHtml(x.message));
          break;
        case Contract.CallResultStatusCode.Warning:
          this.toast('Warning', Common.escapeHtml(x.message));
          break;
        case Contract.CallResultStatusCode.ValidationFailed:
          this.setValidationMessages(x, errorsTarget);
          break;
        }
      }
      else
        this.toast('Server call failed.', '');
      return undefined;
    }
  }

  private setValidationMessages(x: AjaxServerError, errorsTarget: any): void
  {
    const messages = x.messages;
    for (const message of messages)
    {
      if (message.Field && errorsTarget.hasOwnProperty(message.Field))
        errorsTarget[message.Field].setError(message.Message);
      else
      {
        if (this.isIErrorMessagesSite(errorsTarget))
          errorsTarget.errorMessages.push(message.Message);
        else
          this.toast('Server call failed.', Common.escapeHtml(message.Message));
      }
    }
  }
}