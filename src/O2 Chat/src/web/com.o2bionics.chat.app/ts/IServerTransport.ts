
interface IServerTransport {
  getTrackerUrl(): string;

  call<TResult>(method: string, url: string, data?: any, crossDomain?: boolean | null): Promise<TResult>;
}

export default IServerTransport;