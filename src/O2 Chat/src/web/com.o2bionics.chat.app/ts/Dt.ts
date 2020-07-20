import * as moment from './typings/moment/moment';


export default class Dt {
  private static readonly dateFormat = 'MM/DD/YYYY';
  private static readonly dateFormatHash = 'YYYYMMDD';
  private static readonly dateFormatRequest = 'YYYYMMDD';
  private static readonly format = 'llll';

  public static asDateText(d: Date): string
  {
    return moment(d).format(Dt.dateFormat);
  }

  public static fromDateText(s: string): Date
  {
    const m = moment(s, Dt.dateFormat);
    return m.isValid() ? m.toDate() : new Date();
  }

  public static asDateHashText(d: Date): string
  {
    return moment(d).format(Dt.dateFormatHash);
  }

  public static fromDateHashText(s: string): Date
  {
    const m = moment(s, Dt.dateFormatHash);
    return m.isValid() ? m.toDate() : new Date();
  }

  public static asDateRequestText(d: Date): string
  {
    return moment(d).format(Dt.dateFormatRequest);
  }

  public static asText(d: Date | null | undefined): string
  {
    if (!d) return 'n/a';
    return Dt.momentAsText(moment(d));
  }

  public static asHtml(d: Date | null | undefined): string
  {
    if (!d) return '<time>n/a</time>';
    const m = moment(d);
    return `<time datetime="${m.toISOString()}">${Dt.momentAsText(m)}</time>`;
  }

  public static asUtcDateHtml(d: Date | null | undefined): string
  {
    if (!d) return '<time>n/a</time>';
    const m = moment.utc(d);
    return `<time datetime="${m.toISOString()}">${Dt.momentAsDateText(m)}</time>`;
  }

  private static momentAsText(m: moment.Moment): string
  {
    const now = moment();

    if (m.isSame(now, 'day'))
    {
      return m.format('HH:mm:ss');
    }
    else if (m.isSame(now, 'year'))
    {
      return m.format('MM/DD HH:mm:ss');
    }
    return m.format('MM/DD/YYYY HH:mm');
  }

  private static momentAsDateText(m: moment.Moment): string
  {
    return m.format('MM/DD/YYYY');
  }

  public static from(to: Date, from?: Date | undefined): string
  {
    return moment(from).from(to);
  }

  public static fromNow(t: Date): string
  {
    return moment(t).fromNow();
  }

  public static to(to: Date, base?: Date | undefined): string
  {
    return moment(base).to(to);
  }

  public static dateTimeFormat(d: Date): string
  {
    return moment(d).format('YY-MM-DD HH:mm');
  }


  public static utcNow(): Date
  {
    const d = new Date();
    return new Date(
        d.getUTCFullYear(),
        d.getUTCMonth(),
        d.getUTCDate(),
        d.getUTCHours(),
        d.getUTCMinutes(),
        d.getUTCSeconds()
      );
  }
}