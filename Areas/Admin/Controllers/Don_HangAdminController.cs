using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;
using CNPM_Project_web.ViewModel;

namespace CNPM_Project_web.Areas.Admin.Controllers
{
    public class Don_HangAdminController : Controller
    {
        private Web_Entities db = new Web_Entities();


        public ActionResult Index()
        {
            var donHangs = db.Don_Hang
                .OrderByDescending(d => d.TG_DAT_HANG)
                .ToList();

            var listVm = donHangs.Select(d => new GiaoHangViewModel
            {
                ID_DON_HANG = d.ID_DON_HANG,
                TenKhachHang = d.Khach_Hang?.HO_TEN_KH,
                NgayDat = d.TG_DAT_HANG ?? DateTime.Now, // cast nếu nullable
                TongTien = d.TONG_TIEN,
                TrangThai = d.Trang_Thai.TEN_TRANG_THAI,
                DanhSachTrangThai = db.Trang_Thai
                    .Where(t => t.LoaiTrangThai == "Đơn Hàng")
                    .Select(t => new SelectListItem
                    {
                        Value = t.ID_TRANG_THAI.ToString(),
                        Text = t.TEN_TRANG_THAI
                    })
                    .ToList()
            }).ToList();

            return View(listVm);
        }

        [HttpPost]
        public ActionResult CapNhatTrangThai(int idDonHang, int idTrangThaiMoi)
        {
            var donHang = db.Don_Hang.Find(idDonHang);
            if (donHang != null)
            {
                donHang.ID_TRANG_THAI = idTrangThaiMoi;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật trạng thái thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
            }

            return RedirectToAction("Index");
        }

    }
}
