using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.Validators
{
    public class DateCheckAttribute : ValidationAttribute
    {
        private readonly int _maxReturnDays;

        public DateCheckAttribute(int maxReturnDays)
        {
            _maxReturnDays = maxReturnDays;
            ErrorMessage = $@"Invalid date range: Departure Date must be greater than today and Return Date must be within the next {maxReturnDays} days.";
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                if (value is not null)
                {
                    var departureProperty = validationContext.ObjectType.GetProperty("departureDate");
                    var returnProperty = validationContext.ObjectType.GetProperty("returnDate");

                    if (departureProperty == null || returnProperty == null)
                    {
                        return new ValidationResult("Departure Date and Return Date are required.");
                    }

                    var departureDate = (string)departureProperty.GetValue(validationContext.ObjectInstance);
                    var returnDate = (string)returnProperty.GetValue(validationContext.ObjectInstance);

                    if (departureDate != null)
                    {
                        DateTime deptdate = DateTime.ParseExact(departureDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                        if (deptdate <= DateTime.Now)
                        {
                            return new ValidationResult("Departure Date must be in the future.");
                        }
                    }
                    if (returnDate != null)
                    {
                        DateTime retdate = DateTime.ParseExact(returnDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                        if (retdate > DateTime.Now.AddDays(_maxReturnDays))
                        {
                            return new ValidationResult($"ReturnDate must be within {_maxReturnDays} days from today.");
                        }
                    }  
                    return ValidationResult.Success;
                }

                return new ValidationResult(ErrorMessage);
            }
            catch
            {

            }
            return ValidationResult.Success;
        }
    }
}
