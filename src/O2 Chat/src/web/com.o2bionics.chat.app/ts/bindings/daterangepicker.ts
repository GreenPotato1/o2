
import * as moment from '../typings/moment/moment';

console.debug('loaded');

ko.bindingHandlers.daterangepicker = {
    init: (element, valueAccessor, allBindingsAccessor) =>
    {
      const options = allBindingsAccessor!().daterangepickerOptions || {};

      $(element).daterangepicker(
        options,
        (start: moment.Moment, end: moment.Moment) =>
        {
          const accessor = valueAccessor();
          console.debug('change event:', start, end, accessor);
          if (ko.isObservable(accessor.start))
            accessor.start(start);
          if (ko.isObservable(accessor.end))
            accessor.end(end);
        });
    },
    update: (element, valueAccessor) =>
    {
      const widget = $(element).data('daterangepicker');
      console.debug('update called:', widget);
      if (widget)
      {
        const accessor = valueAccessor();
        widget.setStartDate(ko.utils.unwrapObservable(accessor.start));
        widget.setEndDate(ko.utils.unwrapObservable(accessor.end));
      }
    }
  };