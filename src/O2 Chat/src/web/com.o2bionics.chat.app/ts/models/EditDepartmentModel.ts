import * as Contract from '../contract/Contract';
import DepartmentModel from './DepartmentModel';
import { IErrorMessagesSite } from '../ErrorHandler';

export default class EditDepartmentModel extends DepartmentModel implements IErrorMessagesSite {

  constructor(data?: Contract.DepartmentInfo)
  {
    super(data);

    this.name
      .extend(
        {
          maxLength: 256,
          required: { message: 'Please enter department Name' }
        });

    this.description
      .extend(
        {
          maxLength: 60,
          required: { message: 'Please enter Description' }
        });
  }

  public errorMessages = ko.observableArray<string>([]);

  public departmentPropertiesErrors = ko.validation.group(
    [
      this.status,
      this.isPublic,
      this.name,
      this.description
    ]);

  public isDisabled = ko.pureComputed(
    {
      read: () =>
      {
        return this.status() === 1;
      },
      write: (value) =>
      {
        var s = this.status();
        if (value)
          this.status(1);
        else
          this.status(s === 1 ? 0 : s);
      },
      owner: this
    });

  public isValid = () =>
  {
    if (this.departmentPropertiesErrors().length > 0)
    {
      this.departmentPropertiesErrors.showAllMessages();
      return false;
    }
    return true;
  }
}