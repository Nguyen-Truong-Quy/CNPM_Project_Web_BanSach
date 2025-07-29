using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;

namespace CNPM_Project_web.Areas.Admin.Controllers
{
    public class ThanhToanController : Controller
    {
        private Web_Entities db = new Web_Entities();
        // GET: Admin/ThanhToan
        public ActionResult DanhSach(string tuNgay, string denNgay, int? trangThai)
        {
            DateTime? start = string.IsNullOrEmpty(tuNgay) ? default(DateTime?) : DateTime.Parse(tuNgay);
            DateTime? end = string.IsNullOrEmpty(denNgay) ? default(DateTime?) : DateTime.Parse(denNgay).AddDays(1);

            var ds = db.ThanhToans
                .Where(t => !start.HasValue || t.ThoiGianThanhToan >= start)
                .Where(t => !end.HasValue || t.ThoiGianThanhToan < end)
                .Where(t => !trangThai.HasValue || t.ID_TRANG_THAI == trangThai)
                .OrderByDescending(t => t.ThoiGianThanhToan)
                .ToList();

            ViewBag.ListTrangThai = new SelectList(db.Trang_Thai.Where(t => t.LoaiTrangThai == "Thanh Toán"), "ID_TRANG_THAI", "TEN_TRANG_THAI");
            ViewBag.TrangThaiSelected = trangThai;

            return View(ds);
        }

        // GET: Chi tiết đơn
        public ActionResult ChiTiet(int id)
        {
            var hoaDon = db.ThanhToans.FirstOrDefault(t => t.ID_THANHTOAN == id);
            if (hoaDon == null)
            {
                return HttpNotFound();
            }
            return View(hoaDon);
        }

        // POST: Xác nhận
        [HttpPost]
        public ActionResult XacNhan(int id)
        {
            var thanhToan = db.ThanhToans.FirstOrDefault(t => t.ID_THANHTOAN == id);
            if (thanhToan != null)
            {
                // ✅ Cập nhật trạng thái Thanh Toán
                var trangThaiThanhToan = db.Trang_Thai.FirstOrDefault(t =>
                    t.TEN_TRANG_THAI == "Đã thanh toán" && t.LoaiTrangThai == "Thanh Toán");

                if (trangThaiThanhToan != null)
                    thanhToan.ID_TRANG_THAI = trangThaiThanhToan.ID_TRANG_THAI;

                // ✅ Cập nhật trạng thái Đơn Hàng sang "Hoàn thành"
                var donHang = db.Don_Hang.FirstOrDefault(d => d.ID_DON_HANG == thanhToan.ID_DON_HANG);
                if (donHang != null)
                {
                    var trangThaiDonHang = db.Trang_Thai.FirstOrDefault(t =>
                        t.TEN_TRANG_THAI == "Hoàn thành" && t.LoaiTrangThai == "Đơn Hàng");

                    if (trangThaiDonHang != null)
                        donHang.ID_TRANG_THAI = trangThaiDonHang.ID_TRANG_THAI;
                }

                db.SaveChanges();
            }

            return RedirectToAction("DanhSach");
        }


    }
}