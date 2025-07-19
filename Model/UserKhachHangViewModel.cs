using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CNPM_Project_web.Model
{
    public class UserKhachHangViewModel
    {
        // USERS
        public string ID_User { get; set; }
        public string EMAIL { get; set; }
        public string PASSWORD { get; set; }
        public int ID_ROLE { get; set; }
        public string TEN_ROLE { get; set; }

        // KHACH_HANG
        public string MA_KH { get; set; }
        public string HO_TEN_KH { get; set; }
        public string SDT_KH { get; set; }
        public string DIA_CHI { get; set; }
        public string ANH_DAI_DIEN { get; set; }
    }
}