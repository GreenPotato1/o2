import * as Contract from '../contract/Contract';
import UserModel from './UserModel';

export default class UserStorage {

  private readonly map = new Map<number, UserModel>();

  public readonly all = ko.observableArray<UserModel>([]);

  public readonly visible = ko.pureComputed(
    () => ko.utils.arrayFilter(
      this.all(),
      x =>
      {
        const s = x.status();
        return s === Contract.ObjectStatus.Active || s === Contract.ObjectStatus.Disabled;
      }));

  public constructor(
    private readonly currentUserId: number,
    private readonly toModel: (info: Contract.UserInfo | null) => UserModel)
  {}

  public load(list: Array<Contract.UserInfo>): UserStorage
  {
    this.map.clear();
    list.forEach(x => this.map.set(x.Id, this.toModel(x)));
    this.all(Array.from(this.map.values()));

    return this;
  }

  public update(list: Array<Contract.UserInfo>): UserStorage
  {
    for (const x of list)
    {
      const user = this.map.get(x.Id);
      if (user)
      {
        user.update(x);
      }
      else
      {
        const m = this.toModel(x);
        this.map.set(x.Id, m);
        this.all.push(m);
      }
    }
    return this;
  }

  public createNew(info: Contract.UserInfo | null = null): UserModel
  {
    const x = this.toModel(info);
    this.all.unshift(x);
    return x;
  }

  public remove(x: UserModel): void
  {
    const index = this.all.indexOf(x);
    if (index >= 0) this.all.splice(index, 1);
  }

  public get(skey: number | null): UserModel | undefined
  {
    if (skey === null) return undefined;
    return this.map.get(skey);
  }

  public name(skey: number): string
  {
    if (skey === this.currentUserId) return 'Me';
    const user = this.get(skey);
    return user ? user.fullName() : `Unknown ${skey}`;
  }

  public avatar(skey: number): string | null
  {
    const user = this.get(skey);
    if (!user) return null;
    return user.avatar();
  }
}