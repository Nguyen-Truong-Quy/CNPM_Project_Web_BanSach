using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CNPM_Project_web.ViewModel
{

        public class MuaNgayViewModel
        {
            public int ID_DonHang { get; set; }    // ← thêm
            public string MaSP { get; set; }
            public string TenSanPham { get; set; }
            public decimal GiaBan { get; set; }    // không nullable
            public int SoLuong { get; set; }
            public string HoTen { get; set; }
            public string SDT { get; set; }
            public string DiaChi { get; set; }
        }
    

}
