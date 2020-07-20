using System;
using Com.O2Bionics.MailerService.Contract;
using JetBrains.Annotations;

namespace Com.O2Bionics.MailerService.Web
{
    public abstract class MailMessageViewBase<TModel> : System.Web.Mvc.WebViewPage<TModel>
    {
        [NotNull]
        public string Subject
        {
            get => (string)ViewContext.ViewData[MailerConstants.SubjectKey] ?? "";
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Can't be null or whitespace", nameof(Subject));
                ViewContext.ViewData[MailerConstants.SubjectKey] = value;
            }
        }

        protected string WriteUtcDate(object utc, object timezoneOffsetMinutes)
        {
            if (utc == null)
                throw new ArgumentNullException(nameof(utc));
            if (timezoneOffsetMinutes == null)
                throw new ArgumentNullException(nameof(timezoneOffsetMinutes));

            var date = Convert.ToDateTime(utc);
            var offset = Convert.ToInt32(timezoneOffsetMinutes);

            return date.AddMinutes(offset).ToString("ddd, yyyy MMM dd, hh:mm tt");
        }
    }
}