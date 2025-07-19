using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace CNPM_Project_web.Helpers
{
    public class ValidationHelper
    {
        public static void CheckEmailAndPassword(string email, string password, ModelStateDictionary modelState)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                modelState.AddModelError("EMAIL", "Email không được để trống");
            }
            else if (!email.EndsWith("@gmail.com"))
            {
                modelState.AddModelError("EMAIL", "Email phải có đuôi @gmail.com");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                modelState.AddModelError("PASSWORD", "Mật khẩu không được để trống");
            }
            else if (password.Length < 8 || !Regex.IsMatch(password, @"\d"))
            {
                modelState.AddModelError("PASSWORD", "Mật khẩu phải có ít nhất 8 ký tự và chứa ít nhất 1 số");
            }
        }
    }
}