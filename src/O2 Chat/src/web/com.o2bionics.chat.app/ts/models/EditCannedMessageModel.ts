import * as Contract from '../contract/Contract';
import CannedMessageModel from './CannedMessageModel';
import { IErrorMessagesSite } from '../ErrorHandler';

export default class EditCannedMessage extends CannedMessageModel implements IErrorMessagesSite {

  constructor(data?: Contract.CannedMessageInfo)
  {
    super(data);

    this.key.extend(
      {
        pattern: {
            message: 'The string must begin with a character and be at minimum 2 characters long',
            params: '^[a-zA-Z]{1}[a-zA-Z0-9]+$'
          },
        required: true,
      });
  }

  public errorMessages = ko.observableArray<string>([]);
}