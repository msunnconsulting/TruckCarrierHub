using System;
using System.ComponentModel.DataAnnotations;

namespace PartnerCarrier.ViewModels.User
{
    public class MaxWordCountAttribute : ValidationAttribute
    {
        private readonly int _maxWords;

        public MaxWordCountAttribute(int maxWords)
        {
            _maxWords = maxWords;
            ErrorMessage = $"Response cannot exceed {_maxWords} words.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            var text = value.ToString();
            var wordCount = text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;

            return wordCount > _maxWords
                ? new ValidationResult(ErrorMessage)
                : ValidationResult.Success;
        }
    }
}
