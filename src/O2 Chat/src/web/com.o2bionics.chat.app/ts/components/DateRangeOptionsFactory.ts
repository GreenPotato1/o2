import * as moment from '../typings/moment/moment';

export class DateRangeOptionsFactory {
  public static createDateRangeOptions()
  {
    const start = moment();
    const end = moment();

    const result = {
        startDate: start,
        endDate: end,
        alwaysShowCalendars: true,
        opens: 'center',
        ranges: {
            'Today': [moment(), moment()],
            'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
            'Last 7 Days': [moment().subtract(6, 'days'), moment()],
            'Last 30 Days': [moment().subtract(29, 'days'), moment()],
            'This Month': [moment().startOf('month'), moment().endOf('month')],
            'Last Month': [
                moment().subtract(1, 'month').startOf('month'),
                moment().subtract(1, 'month').endOf('month')
              ]
          },
      };

    return result;
  }
}