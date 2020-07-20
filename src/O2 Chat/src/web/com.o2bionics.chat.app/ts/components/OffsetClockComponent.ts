import Dt from '../Dt';
import * as moment from '../typings/moment/moment';

export default class OffsetClockComponent {

  public readonly currentTimeHtml = ko.pureComputed(
    () =>
    {
      var offset = this.params.offset();
      if (offset === null)
        return 'Loading...';
      return Dt.asHtml(moment(Dt.utcNow()).add(offset, 'm').toDate());
    });

  public constructor(private readonly params: { offset: KnockoutObservable<number> })
  {}

  private readonly timerHandle = window.setInterval(() => this.currentTimeHtml());

  public static register(): void
  {
    ko.components.register(
      'offset-clock',
      {
        viewModel: OffsetClockComponent,
        template:
          // language=html
          `<span data-bind="html: currentTimeHtml"></span>`,
      });
  }

  public dispose(): void
  {
    window.clearInterval(this.timerHandle);
  }
}