using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;

namespace Com.O2Bionics.Utils.Web.ModelBinders
{
    /// <inheritdoc />
    /// <summary>
    /// Handles empty arrays as empty collections instead of nulls.
    /// see https://lostechies.com/jimmybogard/2013/11/07/null-collectionsarrays-from-mvc-model-binding/
    /// </summary>
    public class BetterDefaultModelBinder : DefaultModelBinder
    {
        public override object BindModel(
            ControllerContext controllerContext,
            ModelBindingContext bindingContext)
        {
            bindingContext.ModelMetadata.ConvertEmptyStringToNull = false;
            Binders = new ModelBinderDictionary { DefaultBinder = this };
            return base.BindModel(controllerContext, bindingContext);
        }

        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            var model = base.CreateModel(controllerContext, bindingContext, modelType);

            if (model == null || model is IEnumerable)
                return model;

            foreach (var property in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = property.GetValue(model);
                if (value != null)
                    continue;

                if (property.PropertyType.IsArray)
                {
                    value = Array.CreateInstance(property.PropertyType.GetElementType(), 0);
                    property.SetValue(model, value);
                }
                else if (property.PropertyType.IsGenericType)
                {
                    Type typeToCreate;
                    var genericTypeDefinition = property.PropertyType.GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(IDictionary<,>))
                    {
                        typeToCreate = typeof(Dictionary<,>).MakeGenericType(property.PropertyType.GetGenericArguments());
                    }
                    else if (genericTypeDefinition == typeof(IEnumerable<>) ||
                             genericTypeDefinition == typeof(ICollection<>) ||
                             genericTypeDefinition == typeof(IList<>))
                    {
                        typeToCreate = typeof(List<>).MakeGenericType(property.PropertyType.GetGenericArguments());
                    }
                    else if (genericTypeDefinition == typeof(HashSet<>))
                    {
                        typeToCreate = typeof(HashSet<>).MakeGenericType(property.PropertyType.GetGenericArguments());
                    }
                    else
                    {
                        continue;
                    }

                    value = Activator.CreateInstance(typeToCreate);
                    property.SetValue(model, value);
                }
            }

            return model;
        }
    }
}