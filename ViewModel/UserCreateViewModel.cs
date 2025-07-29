using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CNPM_Project_web.ViewModel
{
    public class UserCreateViewModel
    {
        [Required, EmailAddress]
        public string EMAIL { get; set; }
        [Required, MinLength(8)]
        public string PASSWORD { get; set; }
    }

}