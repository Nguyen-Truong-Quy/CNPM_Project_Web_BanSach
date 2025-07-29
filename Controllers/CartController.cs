using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;

    namespace CNPM_Project_web.Controllers
    {
        public class CartController : Controller
        {
            private Web_Entities db = new Web_Entities();
        // GET: Cart
        private string GetCurrentCustomerId()
        {
            return Session["CustomerId"] as string;
        }

        // GET: Cart
        public ActionResult XemGioHang()
        {
            var maKH = Session["CustomerId"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                TempData["ErrorMessage"] = "Bạn chưa đăng nhập. (Session null)";
                return RedirectToAction("Login", "Users");
            }

            var gio = db.Gio_Hang.Where(g => g.MA_KH == maKH).ToList();
            return View(gio);
        }




        [HttpPost]
        public ActionResult ThemGioHang(string maSP, int soLuong)
        {
            string maKH = GetCurrentCustomerId();
            if (string.IsNullOrEmpty(maKH))
            {
                TempData["Message"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng.";
                return RedirectToAction("Login", "Users");
            }

            if (string.IsNullOrEmpty(maSP) || soLuong < 1)
                return RedirectToAction("Index", "SanPham");

            // No longer hardcoding maKH, using the one from session
            var gio = db.Gio_Hang.FirstOrDefault(g => g.MA_KH == maKH && g.MA_SP == maSP);
            // No need to check and create a new Khach_Hang here, as the user is logged in
            // and should already have a Khach_Hang entry.

            if (gio != null)
            {
                gio.SO_LUONG += soLuong;
            }
            else
            {
                db.Gio_Hang.Add(new Gio_Hang
                {
                    MA_KH = maKH,
                    MA_SP = maSP,
                    SO_LUONG = soLuong,
                    NGAY_TAO = DateTime.Now
                });
            }

            db.SaveChanges();
            return RedirectToAction("XemGioHang");
        }

        // Cập nhật số lượng
        [HttpPost]
        public ActionResult CapNhatGio(FormCollection form)
        {
            string maKH = GetCurrentCustomerId();
            if (string.IsNullOrEmpty(maKH))
            {
                TempData["Message"] = "Vui lòng đăng nhập để cập nhật giỏ hàng.";
                return RedirectToAction("Login", "Users");
            }

            var gioHang = db.Gio_Hang.Where(g => g.MA_KH == maKH).ToList();

            foreach (var item in gioHang)
            {
                string key = "soLuong[" + item.MA_SP + "]";
                if (form.AllKeys.Contains(key))
                {
                    int soLuong = int.Parse(form[key]);
                    item.SO_LUONG = soLuong;
                }
            }

            db.SaveChanges();
            return RedirectToAction("XemGioHang");
        }

        // Xóa sản phẩm
        public ActionResult XoaSP(string maSP)
        {
            string maKH = GetCurrentCustomerId();
            if (string.IsNullOrEmpty(maKH))
            {
                TempData["Message"] = "Vui lòng đăng nhập để xóa sản phẩm khỏi giỏ hàng.";
                return RedirectToAction("Login", "Users");
            }

            var item = db.Gio_Hang.FirstOrDefault(g => g.MA_KH == maKH && g.MA_SP == maSP);
            if (item != null)
            {
                db.Gio_Hang.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("XemGioHang");
        }

        [HttpPost]
        public ActionResult XoaNhieuSP(FormCollection form)
        {
            string maKH = GetCurrentCustomerId();
            if (string.IsNullOrEmpty(maKH))
            {
                TempData["Message"] = "Vui lòng đăng nhập để xóa sản phẩm khỏi giỏ hàng.";
                return RedirectToAction("Login", "Users");
            }

            var dsSP = form.GetValues("chonSP");
            if (dsSP != null)
            {
                foreach (var maSP in dsSP)
                {
                    var item = db.Gio_Hang.FirstOrDefault(g => g.MA_KH == maKH && g.MA_SP == maSP);
                    if (item != null)
                    {
                        db.Gio_Hang.Remove(item);
                    }
                }
                db.SaveChanges();
            }

            return RedirectToAction("XemGioHang");
        }

        public ActionResult GoiYSanPham()
        {
            var goiY = db.San_Pham.OrderByDescending(x => x.TON_KHO).Take(4).ToList();
            return PartialView(goiY);
        }
        public ActionResult ThongBaoChuaDangNhap()
        {
            return View();
        }


    }

}