using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MyAppChecker.Infrastructure
{
    public class DisplayNameValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.ErrorCount > 0)
            {
                var modelType = context.ActionDescriptor.Parameters
                    .FirstOrDefault(p =>
                        p.BindingInfo.BindingSource.Id.Equals("Body", StringComparison.InvariantCultureIgnoreCase) ||
                        p.BindingInfo.BindingSource.Id.Equals("Custom", StringComparison.InvariantCultureIgnoreCase)
                        )
                    ?.ParameterType; //Get model type  

                var expandoObj = new ExpandoObject();
                var expandoObjCollection =
                    (ICollection<KeyValuePair<String, Object>>)
                    expandoObj; //Cannot convert IEnumrable to ExpandoObject  

                var dictionary = context.ModelState.ToDictionary(k => k.Key, v => v.Value)
                    .Where(v => v.Value.ValidationState == ModelValidationState.Invalid)
                    .ToDictionary(
                        k =>
                        {
                            if (modelType != null)
                            {
                                var property = modelType.GetProperties().FirstOrDefault(p =>
                                    p.Name.Equals(k.Key, StringComparison.InvariantCultureIgnoreCase));
                                if (property != null)
                                {
                                    //Try to get the attribute  
                                    var displayName = property.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                                        .Cast<DisplayNameAttribute>().SingleOrDefault()?.DisplayName;
                                    return displayName ?? property.Name;
                                }
                            }

                            return k.Key; //Nothing found, return original vaidation key  
                        },
                        v => v.Value.Errors.Select(e => e.ErrorMessage).ToList() as Object); //Box String collection  
                foreach (var keyValuePair in dictionary)
                {
                    expandoObjCollection.Add(keyValuePair);
                }

                dynamic eoDynamic = expandoObj;
                context.Result = new BadRequestObjectResult(eoDynamic);
            }

            base.OnActionExecuting(context);
        }
    }

}