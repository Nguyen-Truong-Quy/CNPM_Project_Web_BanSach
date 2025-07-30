using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;
using CNPM_Project_web.ViewModel;

namespace CNPM_Project_web.Controllers
{
    public class Don_HangController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Don_Hang
        public ActionResult Index()
        {
            var list = db.Don_Hang
                .Include(d => d.Trang_Thai)
                .Include(d => d.Khach_Hang)
                .ToList();
            return View(list);
        }

        // GET: Don_Hang/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var dh = db.Don_Hang
                .Include(d => d.Trang_Thai)
                .Include(d => d.Khach_Hang)
                .Include(d => d.Chi_Tiet_Don_Hang.Select(ct => ct.San_Pham))
                .Include(d => d.ThanhToans.Select(t => t.Phuong_Thuc_Thanh_Toan))
                .FirstOrDefault(d => d.ID_DON_HANG == id.Value);

            if (dh == null)
                return HttpNotFound();

            return View(dh);
        }

        // POST: Don_Hang/MuaNgay
        [HttpPost, ValidateAntiForgeryToken]

        public ActionResult MuaNgay(string maSP, int soLuong = 1)
        {
            // 1. Kiểm tra đăng nhập
            var userId = Session["UserId"]?.ToString();
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Users");

            // 2. Lấy khách hàng
            var kh = db.Khach_Hang.FirstOrDefault(k => k.ID_User == userId);
            if (kh == null)
                return RedirectToAction("Create", "Users");

            // 3. Lấy sản phẩm, bắt buộc phải có giá và tồn kho đủ
            var sp = db.San_Pham.Find(maSP);
            if (sp == null)
                return HttpNotFound("Sản phẩm không tồn tại.");
            if (sp.GIA_BAN == null)
                return new HttpStatusCodeResult(400, "Sản phẩm chưa có giá bán.");
            if (sp.TON_KHO < soLuong)
                return new HttpStatusCodeResult(400, "Không đủ tồn kho.");

            // 4. Tính đơn giá và tổng tiền
            decimal giaBan = sp.GIA_BAN.Value;      // chắc chắn không null
            decimal tongTien = giaBan * soLuong;

            // 5. Tạo đơn
            var dh = new Don_Hang
            {
                MA_KH = kh.MA_KH,
                TG_DAT_HANG = DateTime.Now,
                ID_TRANG_THAI = db.Trang_Thai
                                    .First(t => t.TEN_TRANG_THAI == "Mới")
                                    .ID_TRANG_THAI,
                TONG_TIEN = tongTien
            };
            db.Don_Hang.Add(dh);
            db.SaveChanges();  // để có ID_DON_HANG

            // 6. Tạo chi tiết đơn, gán đúng giá bán
            var ct = new Chi_Tiet_Don_Hang
            {
                ID_DON_HANG = dh.ID_DON_HANG,
                MA_SP = sp.MA_SP,
                SO_LUONG = soLuong,
                GIA_BAN = giaBan   // ← dùng Value, bảo đảm nó không null
            };
            db.Chi_Tiet_Don_Hang.Add(ct);

            // 7. Giảm tồn kho và lưu
            sp.TON_KHO -= soLuong;
            db.SaveChanges();

            // 8. Chuyển sang trang xác nhận
            return RedirectToAction("XacNhanDonHang", new { id = dh.ID_DON_HANG });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatHangTuGio(string[] chonSP)
        {
            var userId = Session["UserId"]?.ToString();
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Users");

            var kh = db.Khach_Hang.FirstOrDefault(k => k.ID_User == userId);
            if (kh == null)
                return RedirectToAction("Create", "Users");

            if (chonSP == null || chonSP.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất 1 sản phẩm để đặt hàng.";
                return RedirectToAction("XemGioHang", "Cart");
            }

            var gioHang = db.Gio_Hang
                            .Include(g => g.San_Pham)
                            .Where(g => g.MA_KH == kh.MA_KH && chonSP.Contains(g.MA_SP))
                            .ToList();

            if (gioHang == null || !gioHang.Any())
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm nào hợp lệ trong giỏ hàng.";
                return RedirectToAction("XemGioHang", "Cart");
            }

            decimal tongTien = 0;
            foreach (var item in gioHang)
            {
                if (item.San_Pham.GIA_BAN == null || item.San_Pham.TON_KHO < item.SO_LUONG)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm {item.San_Pham.TEN_SP} không đủ tồn kho hoặc chưa có giá bán.";
                    return RedirectToAction("XemGioHang", "Cart");
                }
                tongTien += item.San_Pham.GIA_BAN.Value * item.SO_LUONG;
            }

            var dh = new Don_Hang
            {
                MA_KH = kh.MA_KH,
                TG_DAT_HANG = DateTime.Now,
                ID_TRANG_THAI = db.Trang_Thai.First(t => t.TEN_TRANG_THAI == "Mới").ID_TRANG_THAI,
                TONG_TIEN = tongTien
            };
            db.Don_Hang.Add(dh);
            db.SaveChanges();

            foreach (var item in gioHang)
            {
                db.Chi_Tiet_Don_Hang.Add(new Chi_Tiet_Don_Hang
                {
                    ID_DON_HANG = dh.ID_DON_HANG,
                    MA_SP = item.MA_SP,
                    SO_LUONG = item.SO_LUONG,
                    GIA_BAN = item.San_Pham.GIA_BAN.Value
                });

                item.San_Pham.TON_KHO -= item.SO_LUONG;
                db.Gio_Hang.Remove(item);
            }

            db.SaveChanges();
            return RedirectToAction("XacNhanDonHang", "Don_Hang", new { id = dh.ID_DON_HANG });
        }



        // GET: Don_Hang/XacNhanDonHang/5
        [HttpGet]
        public ActionResult XacNhanDonHang(int? id)
        {
            if (id == null)
                return RedirectToAction("DonHangCuaToi");

            var dh = db.Don_Hang
                .Include(d => d.Trang_Thai)
                .Include(d => d.Khach_Hang)
                .Include(d => d.Chi_Tiet_Don_Hang.Select(ct => ct.San_Pham))
                .FirstOrDefault(d => d.ID_DON_HANG == id.Value);

            if (dh == null)
                return HttpNotFound();

            // Trả về view XacNhanDonHang.cshtml với model Don_Hang
            return View(dh);
        }

        // POST: Don_Hang/XacNhanDonHang
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult XacNhanDonHang(int idDonHang, HttpPostedFileBase ImageUpload)
        {
            if (ImageUpload == null || ImageUpload.ContentLength == 0)
            {
                TempData["Error"] = "Vui lòng chọn ảnh xác nhận thanh toán.";
                return RedirectToAction("XacNhanDonHang", new { id = idDonHang });
            }

            // Lưu file ảnh
            var fileName = Guid.NewGuid() + Path.GetExtension(ImageUpload.FileName);
            var folder = Server.MapPath("~/Image/ThanhToan/");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            ImageUpload.SaveAs(Path.Combine(folder, fileName));

            // Lấy đơn hàng
            var donHang = db.Don_Hang.Find(idDonHang);
            if (donHang == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("DanhSachDonHang");
            }

            // Tạo bản ghi ThanhToan
            var tt = new ThanhToan
            {
                ID_DON_HANG = idDonHang,
                ID_PHUONG_THUC = 1,
                GIA_BAN = donHang.TONG_TIEN,
                ID_TRANG_THAI = db.Trang_Thai
                                   .First(t => t.TEN_TRANG_THAI == "Chờ xác nhận")
                                   .ID_TRANG_THAI,
                AnhThanhToan = "/Image/ThanhToan/" + fileName,
                ThoiGianThanhToan = donHang.TG_DAT_HANG // ✅ Gán đúng
            };
            db.ThanhToans.Add(tt);

            // Cập nhật trạng thái đơn
            donHang.ID_TRANG_THAI = tt.ID_TRANG_THAI;

            db.SaveChanges();

            TempData["Success"] = "Gửi xác nhận thanh toán thành công!";
            return RedirectToAction("ChiTietDonHang", new { id = idDonHang });
        }


        // GET: Don_Hang/ChiTietDonHang/5
        public ActionResult ChiTietDonHang(int id)
        {
            var dh = db.Don_Hang
                .Include(d => d.Trang_Thai)
                .Include(d => d.Khach_Hang)
                .Include(d => d.Chi_Tiet_Don_Hang.Select(ct => ct.San_Pham))
                .Include(d => d.ThanhToans.Select(t => t.Phuong_Thuc_Thanh_Toan))
                .FirstOrDefault(d => d.ID_DON_HANG == id);

            if (dh == null)
                return HttpNotFound();

            return View(dh);
        }

        // GET: Don_Hang/DonHangCuaToi
        public ActionResult DonHangCuaToi()
        {
            var userId = Session["UserId"] as string;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Users");

            var maKH = db.Khach_Hang
                         .Where(k => k.ID_User == userId)
                         .Select(k => k.MA_KH)
                         .FirstOrDefault();
            if (maKH == null)
                return RedirectToAction("Create", "Users");

            // 1) Lấy về List<Don_Hang> trước (LINQ-to-Entities → SQL)
            var donHangs = db.Don_Hang
    .Where(d => d.MA_KH == maKH)
    .Include(d => d.Trang_Thai)
    .Include(d => d.Khach_Hang) // ✅ thêm dòng này
    .OrderByDescending(d => d.TG_DAT_HANG)
    .ToList();

            // 2) Chuyển sang ViewModel (LINQ-to-Objects)
            var listVm = donHangs
                .Select((d, idx) => new DonHangViewModel
                {
                    STT = idx + 1,
                    ID_DON_HANG = d.ID_DON_HANG,
                    TenKhachHang = d.Khach_Hang.HO_TEN_KH,
                    NgayDat = d.TG_DAT_HANG,
                    TongTien = d.TONG_TIEN,
                    TrangThaiDonHang = d.Trang_Thai.TEN_TRANG_THAI,
                    TrangThaiThanhToan = d.ThanhToans.Any() ? "Đã gửi" : "Chưa gửi",
                    // Vì ở Index không cần chi tiết, ta để list rỗng
                    ChiTietSanPham = new List<ChiTietSPViewModel>()
                })
                .ToList();

            return View("DonHangCuaToi", listVm);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
