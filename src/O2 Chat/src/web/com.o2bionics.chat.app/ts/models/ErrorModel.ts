import * as ErrorHandler from '../ErrorHandler';

export default class ErrorModel {
  public readonly errorMessages = ko.observableArray<string>([]);

  public wrapException(x: any): void
  {
    if (x instanceof ErrorHandler.AjaxServerError)
    {
      this.errorMessages((x as ErrorHandler.AjaxServerError).messages.map(y => y.Message));
    }
    else
    {
      const ajaxError = x as ErrorHandler.AjaxError;
      const has = (x instanceof ErrorHandler.AjaxError)
        && null != ajaxError
        && null != ajaxError.errorMessage
        && 0 < ajaxError.errorMessage.length;
      const error = null != ajaxError && has ? ajaxError.errorMessage : 'Server call failed.';
      this.errorMessages([error]);
    }
  }

  //Log an error to the Error Tracker.
  public logErrorSafe(e: any, message: string)
  {
    try
    {
      const name = 'O2Bionics';
      const logger = (window as any)[name];
      const funcName = 'saveError';
      if (null != logger)
      {
        const saveError = logger[funcName];
        if (null != saveError && 'function' === typeof(saveError))
        {
          const errorInfo: any = {
              'Message': message,
              'Url': document.location.href
            };
          //if (null != e.message && 0 < e.message.length)
          //  errorInfo.ExceptionMessage = e.message;
          errorInfo.ExceptionMessage = e.toString();

          if (null != e.stack && 0 < e.stack.length)
            errorInfo.ExceptionStack = e.stack;

          {
            const s = ErrorModel.getExceptionSource(e);
            if (null != s && 0 < s.length)
              errorInfo.ExceptionSource = s;
          }
          saveError(errorInfo);
          return;
        }
      }

      console.error(`Function ${funcName} was not found. Cannot log the error: ${e}`);
    }
    catch (e2)
    {
      console.error(`logErrorSafe error: "${e2}". Couldn't log an error: "${e}"`);
    }
  }

  private static getExceptionSource(e: any): string
  {
    try
    {
      let result = '';
      if (null != e.fileName && 0 < e.fileName.length)
        result += `fileName=${e.fileName}`;

      if (null != e.lineNumber && 'number' === typeof (e.lineNumber))
        result += ` line=${e.lineNumber}`;

      if (null != e.columnNumber && 'number' === typeof (e.columnNumber))
        result += ` column=${e.columnNumber}`;

      return result;
    }
    catch (e2)
    {
      const err = `getExceptionSource error: ${e2}`;
      console.error(err);
      return err;
    }
  }
}