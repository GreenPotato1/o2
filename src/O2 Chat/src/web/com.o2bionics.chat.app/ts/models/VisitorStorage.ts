import * as Contract from '../contract/Contract';
import VisitorModel from './VisitorModel';

export default class VisitorStorage {
  private readonly map = new Map<number, VisitorModel>();

  public update(infos: Array<Contract.VisitorInfo>): void
  {
    for (const x of infos)
    {
      const v = this.map.get(x.UniqueId);
      if (v)
      {
        v.update(x);
      }
      else
      {
        const m = new VisitorModel(x);
        this.map.set(x.UniqueId, m);
      }
    }
  }

  public get(id: number | null): VisitorModel | undefined
  {
    if (id === null) return undefined;
    return this.map.get(id);
  }

  public name(id: number): string
  {
    const v = this.get(id);
    return v ? (v.email() || v.name() || '' + id) : `Unknown ${id}`;
  }
}