using System;
using System.Collections.Generic;
using Com.O2Bionics.ChatService.Contract;

namespace Com.O2Bionics.ChatService.Objects
{
    public class BusinessObjectValidatorBase
    {
        protected static void ValidateStringField(
            List<ValidationMessage> messages,
            string name,
            string value,
            bool canBeNull,
            int maxLength,
            bool canBeEmpty = true)
        {
            if (value == null && !canBeNull)
                throw new ArgumentException(string.Format("{0} can't be null", name));
            if (value == null) return;

            if (!canBeEmpty && value.Length == 0)
                messages.Add(new ValidationMessage(name, "Can't be empty"));
            if (value.Length > maxLength)
                messages.Add(new ValidationMessage(name, string.Format("Can't be longer than {0} characters", maxLength)));
        }
    }
}