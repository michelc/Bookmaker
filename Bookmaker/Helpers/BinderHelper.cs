using System.Web.Mvc;

namespace Bookmaker.Helpers
{
    public class StringModelBinder : IModelBinder
    {
        /// <summary>
        /// Supprime les espaces en début et en fin des chaines de caractères saisies
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            ModelState modelState = new ModelState { Value = valueResult };

            object actualValue = valueResult == null ? null : valueResult.AttemptedValue;

            if (actualValue != null)
            {
                actualValue = ((string)actualValue).Trim();
                if ((string)actualValue == string.Empty)
                {
                    actualValue = null;
                }
            }

            return actualValue;
        }

    }
}