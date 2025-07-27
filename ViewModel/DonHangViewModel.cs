using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CNPM_Project_web.ViewModel
{
    public class DonHangViewModel
    {
        public int STT { get; set; } // số thứ tự
        public int ID_DON_HANG { get; set; }
        public string TenKhachHang { get; set; }
        public DateTime? NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThaiDonHang { get; set; }
        public string TrangThaiThanhToan { get; set; }
        public List<ChiTietSPViewModel> ChiTietSanPham { get; set; }
    }


}