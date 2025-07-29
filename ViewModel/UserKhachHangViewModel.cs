using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CNPM_Project_web.ViewModel
{
    public class UserKhachHangViewModel
    {
        public string ID_User { get; set; }
        public string PASSWORD { get; set; }

        public string HO_TEN_KH { get; set; }
        public string SDT_KH { get; set; }
        public string DIA_CHI { get; set; }
        public string ANH_DAI_DIEN { get; set; }

        // File upload
        public HttpPostedFileBase AnhUpload { get; set; }
    }

}