using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Com.O2Bionics.Utils.Web.ModelBinders
{
    public class HashSetDecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var modelName = bindingContext.ModelName;

            var result = new HashSet<decimal>();

            if (bindingContext.ValueProvider is ValueProviderCollection providers)
            {
                var dictionaryValueProvider = providers
                    .OfType<DictionaryValueProvider<object>>()
                    .FirstOrDefault(vp => vp.ContainsPrefix(modelName));
                if (dictionaryValueProvider != null)
                {
                    var keys = dictionaryValueProvider.GetKeysFromPrefix(modelName);
                    foreach (var key in keys.Values)
                    {
                        var v = bindingContext.ValueProvider.GetValue(key).AttemptedValue;
                        result.Add(decimal.Parse(v));
                    }
                    return result;
                }
            }
            return null;
        }
    }
}