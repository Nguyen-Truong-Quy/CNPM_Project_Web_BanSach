using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CNPM_Project_web.Model
{
    public class CheckoutViewModel
    {
        public List<Gio_Hang> CartItems { get; set; }
        public decimal TongTien { get; set; }
        public int IdThanhToan { get; set; }
        public List<Phuong_Thuc_Thanh_Toan> PhuongThucThanhToans { get; set; }
        public Khach_Hang KhachHang { get; set; }
    }
}