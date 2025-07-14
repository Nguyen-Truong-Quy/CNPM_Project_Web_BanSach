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
            [HttpPost]
            public ActionResult ThemGioHang(string maSP, int soLuong)
            {
                if (string.IsNullOrEmpty(maSP) || soLuong < 1)
                    return RedirectToAction("Index", "SanPham");

                string maKH = "KH001"; // Tạm hardcode vì chưa có login

                var gio = db.Gio_Hang.FirstOrDefault(g => g.MA_KH == maKH && g.MA_SP == maSP);
                var khachHang = db.Khach_Hang.FirstOrDefault(k => k.MA_KH == maKH);
                if (khachHang == null)
                {
                    db.Khach_Hang.Add(new Khach_Hang
                    {
                        MA_KH = maKH,
                        HO_TEN_KH = "Khách vãng lai",
                        SDT_KH = "0000000000",
                        DIA_CHI = "Chưa có",
                        EMAIL = "khach@example.com"
                    });
                    db.SaveChanges();
                }

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
            public ActionResult XemGioHang()
            {
                string maKH = "KH001";
                var gio = db.Gio_Hang.Where(g => g.MA_KH == maKH).ToList();
                return View(gio);
            }
            // Cập nhật số lượng
            [HttpPost]
            public ActionResult CapNhatGio(FormCollection form)
            {
                string maKH = "KH001";
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
                string maKH = "KH001";
                var item = db.Gio_Hang.FirstOrDefault(g => g.MA_KH == maKH && g.MA_SP == maSP);
                if (item != null)
                {
                    db.Gio_Hang.Remove(item);
                    db.SaveChanges();
                }
                return RedirectToAction("XemGioHang");
            }

            public ActionResult GoiYSanPham()
            {
                var goiY = db.San_Pham.OrderByDescending(x => x.TON_KHO).Take(4).ToList();
                return PartialView(goiY);
            }
        [HttpPost]
        public ActionResult XoaNhieuSP(FormCollection form)
        {
            string maKH = "KH001";
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

    }

}