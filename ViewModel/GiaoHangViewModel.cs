using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CNPM_Project_web.ViewModel
{
    public class GiaoHangViewModel
    {
        public int ID_DON_HANG { get; set; }
        public string TenKhachHang { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }
        public List<SelectListItem> DanhSachTrangThai { get; set; }
    }
}