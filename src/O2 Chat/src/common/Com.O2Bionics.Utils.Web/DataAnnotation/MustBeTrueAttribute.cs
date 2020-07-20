using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Com.O2Bionics.Utils.Web.DataAnnotation
{
    public class MustBeTrueAttribute : ValidationAttribute, IClientValidatable
    {
        public override bool IsValid(object value)
        {
            return value is bool && (bool)value;
        }

        // requres 'jquery.validation.unobtrusive.mustbetrue.js'
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            yield return new ModelClientValidationRule
                {
                    ValidationType = "mustbetrue",
                    ErrorMessage = ErrorMessage,
                };
        }
    }
}