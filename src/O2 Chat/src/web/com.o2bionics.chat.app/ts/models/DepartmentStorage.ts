import * as Contract from '../contract/Contract';
import { DepartmentController } from '../contract/Controllers';
import IServerTransport from '../IServerTransport';
import DepartmentModel from './DepartmentModel'
import EditDepartmentModel from './EditDepartmentModel'

export default class DepartmentStorage {

  public readonly current = ko.observable<DepartmentModel>(null);
  public readonly all = ko.observableArray<DepartmentModel>([]);

  public readonly visible = ko.pureComputed(
    () => ko.utils.arrayFilter(
      this.all(),
      x =>
      {
        const s = x.status();
        return s !== Contract.ObjectStatus.Deleted;
      }));

  private readonly map = new Map<number, DepartmentModel>();

  public load(list: Array<Contract.DepartmentInfo>): DepartmentStorage
  {
    list.forEach(x => this.map.set(x.Id, new DepartmentModel(x)));
    this.all(Array.from(this.map.values()));
    return this;
  }

  public update(...list: Contract.DepartmentInfo[]): DepartmentStorage
  {
    for (const x of list)
    {
      const dept = this.map.get(x.Id);
      if (dept)
      {
        dept.update(x);
      }
      else
      {
        const m = new DepartmentModel(x);
        this.map.set(x.Id, m);
        this.all.push(m);
      }
    }
    return this;
  }

  public createNew(info?: Contract.DepartmentInfo): DepartmentModel
  {
    return new DepartmentModel(info);
  }

  public get(id: number | null): DepartmentModel | undefined
  {
    if (id === null) return undefined;
    return this.map.get(id);
  }

  private getName(id: number, x: DepartmentModel | undefined): string
  {
    return x ? x.name() : `Unknown ${id}`;
  }

  public name(id: number): string
  {
    return this.getName(id, this.get(id));
  }

  public names(ids: number[]): string[]
  {
    return ids.map(id => this.getName(id, this.get(id)));
  }

  public remove(x: DepartmentModel): void
  {
    this.all.remove(x);
    this.map.delete(x.id());
  }

  public async save(transport: IServerTransport, model: EditDepartmentModel): Promise<DepartmentModel>
  {
    console.debug('save', model);

    const controller = new DepartmentController(transport);

    const d = model.asInfo();
    const r = model.id() === 0
                ? await controller.create(d)
                : await controller.update(d);

    console.debug('save result', r);

    this.update(r.Department);
    const m = this.get(r.Department.Id);

    console.debug('save model updated', m);

    return m!;
  }
}