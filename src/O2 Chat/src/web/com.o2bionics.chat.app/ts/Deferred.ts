export default class Deferred<T> {

  public readonly promise: Promise<T>;

  private aResolve: (value?: T | PromiseLike<T> | undefined) => void;

  public get resolve(): (value?: T | PromiseLike<T> | undefined) => void
  {
    return this.aResolve;
  }

  private aReject: (reason?: any) => void;

  public get reject(): (reason?: any) => void
  {
    return this.aReject;
  }

  constructor()
  {
    this.promise = new Promise<T>(
      (resolve, reject) =>
      {
        this.aResolve = resolve;
        this.aReject = reject;
      });
  }
}