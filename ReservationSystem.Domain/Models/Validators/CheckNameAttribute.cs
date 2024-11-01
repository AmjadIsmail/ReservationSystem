using Microsoft.AspNetCore.Components.Web;
using OfficeOpenXml.Export.HtmlExport.StyleCollectors.StyleContracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.Validators
{
    public class CheckNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                if (value is not null)
                {
                    var firstName = validationContext.ObjectType.GetProperty("firstName");
                    var surName = validationContext.ObjectType.GetProperty("surName"); 
                    if (firstName != null)
                    {
                        string _firstname = (string )firstName.GetValue(validationContext.ObjectInstance);
                        if ((IsAllLettersAtoZ(_firstname) == false))
                        {
                            return new ValidationResult("Invalid First Name, name must contains only Alphabets");
                        }
                    }
                    if (surName != null)
                    {
                        string _surName = (string)surName.GetValue(validationContext.ObjectInstance);

                        if ((IsAllLettersAtoZ(_surName.ToString())) == false)
                        {
                            return new ValidationResult("Invalid Sur Name, name must contains only Alphabets");
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
        public static bool IsValidMobileNumber(string mobile)
        {
            if (string.IsNullOrEmpty(mobile))
                return false;            
            string pattern = @"^\+?[1-9]\d{1,14}$";
            return Regex.IsMatch(mobile, pattern);
        }
        public static bool IsValidEmail(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return false;
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
           
        }
        public  static bool IsAllLettersAtoZ(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return false;
                string pattern = @"^[A-Za-z\s]+$";
                return Regex.IsMatch(input, pattern);
            }
            catch
            {
                return false;
            }        
        }

        public static bool IsDOBWithin24Months(string dob)
        {
            try
            {
                DateTime parsedDate;
                string[] formats = { "dd/MM/yyyy", "dd-MM-yyyy" };
                if (!DateTime.TryParseExact(dob, formats, null, System.Globalization.DateTimeStyles.None, out parsedDate))
                {
                    return false;
                }
                var monthsDifference = (DateTime.Today.Year - parsedDate.Year) * 12 + DateTime.Today.Month - parsedDate.Month;

                return monthsDifference <= 24;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsDOBWithin12Years(string dob)
        {
            try
            {
                DateTime parsedDate;
                string[] formats = { "dd/MM/yyyy", "dd-MM-yyyy" };
                if (!DateTime.TryParseExact(dob, formats, null, System.Globalization.DateTimeStyles.None, out parsedDate))
                {
                    return false;
                }
                TimeSpan difference = DateTime.Now.Subtract(parsedDate);
                int totalMonths = (parsedDate.Year - DateTime.Now .Year) * 12 + parsedDate.Month - DateTime.Now .Month;
                DateTime today = DateTime.Today;
                int ageYears = today.Year - parsedDate.Year;
                if (today < parsedDate.AddYears(ageYears))
                {
                    ageYears--;
                }
                return ageYears >= 2 && ageYears <= 12;
            }
            catch
            {
                return false;
            }
          
        }
    }

    public class CheckDOBAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                if (value is not null)
                {
                                 
                    
                    var type = validationContext.ObjectType.GetProperty("type");
                    var dob = validationContext.ObjectType.GetProperty("dob");
                    string _dob = (string)dob.GetValue(validationContext.ObjectInstance);
                    if (type != null && dob != null)
                    {
                        string _type = (string)type.GetValue(validationContext.ObjectInstance);                      
                        if (_type.ToString() == "INF")                        {
                           
                            if (_dob == null) { return new ValidationResult("Required Infant Date of birth."); }
                            if (CheckNameAttribute.IsDOBWithin24Months(_dob.ToString()) == false)
                            {
                                return new ValidationResult("Invalid Infant Date of birth. It should be less then 2 years...");
                            }
                        }
                        else if (_type == "CHD")
                        {

                           
                            if (_dob == null) { return new ValidationResult("Required Child Date of birth."); }
                            if (CheckNameAttribute.IsDOBWithin12Years(_dob.ToString()) == false)
                            {
                                return new ValidationResult("Invalid Child Date of birth. It should be greater then 2 years and less then or equal 12 years...");
                            }
                        }
                    }
                  
                    return ValidationResult.Success;                }

                return new ValidationResult(ErrorMessage);
            }
            catch
            {

            }
            return ValidationResult.Success;
        }
      
    }

    public class CheckEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                if (value is not null)
                {
                   
                    var email = validationContext.ObjectType.GetProperty("email");                   
                    var isLeadPassenger = validationContext.ObjectType.GetProperty("isLeadPassenger");

                    var type = validationContext.ObjectType;                   
                    var propertyInfo = type.GetProperty("isLeadPassenger");
                    var isLeadPassengerValue = propertyInfo.GetValue(validationContext.ObjectInstance, null);

                    if (isLeadPassengerValue != null)
                    {                      
                            
                            bool isLead = (bool)isLeadPassenger.GetValue(validationContext.ObjectInstance);
                            if (isLead)
                            {
                                if (email == null)
                                {
                                    return new ValidationResult("Email is required for Lead Passenger");
                                }
                                string _email = (string)email.GetValue(validationContext.ObjectInstance);
                                if ((CheckNameAttribute.IsValidEmail(_email.ToString())) == false)
                                {
                                    return new ValidationResult("Invalid Email address");
                                }
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
