using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CNPM_Project_web.Controllers
{
    public class CartViewModel
    {
        public int ID_GIO_HANG { get; set; }
        public string MA_SP { get; set; }
        public string TEN_SP { get; set; }
        public string HINH_ANH { get; set; }
        public decimal GIA_BAN { get; set; }
        public int SO_LUONG { get; set; }
        public decimal ThanhTien => GIA_BAN * SO_LUONG;
    }
}