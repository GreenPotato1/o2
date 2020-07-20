import * as Contract from '../contract/Contract';
import { CannedMessagesController } from '../contract/Controllers';
import IServerTransport from '../IServerTransport';
import CannedMessageModel from '../models/CannedMessageModel';

export default class CannedMessageStorage {

  public readonly all = ko.observableArray<CannedMessageModel>([]);

  private constructor(
    public readonly departmentId: number | null,
    public readonly userId: number | null,
    messages: Contract.CannedMessageInfo[])
  {
    this.push(messages);
  }

  private push(infos: Array<Contract.CannedMessageInfo>): CannedMessageStorage
  {
    this.all.push.apply(this.all, infos.map(x => CannedMessageModel.fromInfo(x)));
    return this;
  }

  public static async loadDepartmentMessages(transport: IServerTransport, departmentId: number):
    Promise<CannedMessageStorage>
  {
    const r = await new CannedMessagesController(transport).getDepartmentMessages(departmentId);
    return new CannedMessageStorage(departmentId, null, r.CannedMessages);
  }

  public static async loadUserMessages(transport: IServerTransport, userId: number):
    Promise<CannedMessageStorage>
  {
    const r = await new CannedMessagesController(transport).getUserMessages();
    return new CannedMessageStorage(null, userId, r.CannedMessages);
  }

  public lookupKey(term: string): CannedMessageModel[]
  {
    const matches = $.grep(this.all(), x => x!.keyStartsWith(term));
    console.debug('search cm', term, this.all, matches);
    return matches;
  }

  public createNew(): CannedMessageModel
  {
    return new CannedMessageModel().createNew(this.departmentId, this.userId);
  }

  public async delete(transport: IServerTransport, id: number): Promise<void>
  {
    new CannedMessagesController(transport).delete(id);
    this.all.remove(x => x.id() === id);
  }

  public async save(transport: IServerTransport, model: CannedMessageModel): Promise<CannedMessageModel>
  {
    const controller = new CannedMessagesController(transport);

    const d = model.asInfo();
    const r = model.id() === 0
                ? await controller.create(d)
                : await controller.update(d);

    let m = this.all().find(x => x.id() === model.id());
    if (m)
    {
      m.update(r.CannedMessage);
    }
    else
    {
      m = CannedMessageModel.fromInfo(r.CannedMessage);
      this.all.push(m);
    }
    return m;
  }
}