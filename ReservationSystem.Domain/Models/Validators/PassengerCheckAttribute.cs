using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.Validators
{
    public  class PassengerCheckAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                if (value is not null)
                {
                    var adults = validationContext.ObjectType.GetProperty("adults");
                    var children = validationContext.ObjectType.GetProperty("children");
                    var infant = validationContext.ObjectType.GetProperty("infant");
                    int totAdt = 0;
                    int totChild = 0;
                    int totInf = 0;
                    if (adults == null)
                    {
                        return new ValidationResult("Atleast one adult is required.");
                    }
                    else
                    {
                        totAdt = (int)adults.GetValue(validationContext.ObjectInstance);
                    }
                    if( infant != null)
                    {
                        totInf = (int)infant.GetValue(validationContext.ObjectInstance);

                        var adt = (int)adults.GetValue(validationContext.ObjectInstance);
                        var inf = (int)infant.GetValue(validationContext.ObjectInstance);
                        if(inf > adt)
                        {
                            return new ValidationResult("Total number of Infants can not greater then total Adults.");
                        }
                    }
                    if(children != null)
                    {
                        totChild = (int)children.GetValue(validationContext.ObjectInstance);
                    }
                    if((totAdt+totChild) > 9)
                    {
                        return new ValidationResult("Total number of Passengers should be less or equal to 9.");
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
