using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Publisher.Validation
{
    public class NotValueAttribute : ValidationAttribute, IClientValidatable
    {
        public string NotValue
        {
            get;
            private set;
        }

        public NotValueAttribute(string notValue)
        {
            NotValue = notValue;
        }

        protected override ValidationResult IsValid(object objValue, ValidationContext validationContext)
        {
            var otherValue = NotValue;
            if (object.Equals(objValue, otherValue))
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            return null;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = ErrorMessage,
                ValidationType = "notequalto",
            };
            rule.ValidationParameters["other"] = NotValue;
            yield return rule;
        }
    }
}