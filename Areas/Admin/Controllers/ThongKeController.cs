using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;
using CNPM_Project_web.ViewModel;

namespace CNPM_Project_web.Areas.Admin.Controllers
{
    public class ThongKeController : Controller
    {
        private Web_Entities db = new Web_Entities();

        public ActionResult Index()
        {
            var model = new ThongKeViewModel();

            // Tổng số đơn
            model.TongDonHang = db.Don_Hang.Count();

            // Tổng doanh thu (đơn đã hoàn thành)
            model.TongDoanhThu = db.Don_Hang
                .Where(d => d.ID_TRANG_THAI == 5) // ID 5 = Hoàn thành
                .Sum(d => (decimal?)d.TONG_TIEN) ?? 0;

            // Số đơn theo từng trạng thái
            model.SoLuongTheoTrangThai = db.Don_Hang
                .GroupBy(d => d.Trang_Thai.TEN_TRANG_THAI)
                .ToDictionary(g => g.Key, g => g.Count());

            // Doanh thu theo tháng (12 tháng gần nhất)
            model.DoanhThuTheoThang = db.Don_Hang
                .Where(d => d.TG_DAT_HANG.HasValue)
                .GroupBy(d => d.TG_DAT_HANG.Value.Month)
                .Select(g => new DoanhThuThang
                {
                    Thang = g.Key,
                    DoanhThu = g.Sum(d => d.TONG_TIEN)
                })
                .ToList();

            // Top 5 sản phẩm bán chạy
            model.TopSanPhamBanChay = db.Chi_Tiet_Don_Hang
                .GroupBy(c => c.San_Pham.TEN_SP)
                .OrderByDescending(g => g.Sum(c => c.SO_LUONG))
                .Take(5)
                .Select(g => new SanPhamBanChay
                {
                    TenSanPham = g.Key,
                    SoLuong = g.Sum(c => c.SO_LUONG)
                })
                .ToList();

            return View(model);
        }
    }
}