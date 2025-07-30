using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CNPM_Project_web.ViewModel
{
    public class ThongKeViewModel
    {
        public int TongDonHang { get; set; }
        public decimal TongDoanhThu { get; set; }
        public Dictionary<string, int> SoLuongTheoTrangThai { get; set; }
        public List<DoanhThuThang> DoanhThuTheoThang { get; set; }
        public List<SanPhamBanChay> TopSanPhamBanChay { get; set; }
    }
}