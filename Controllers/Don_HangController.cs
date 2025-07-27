using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;

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

            // Tạo bản ghi ThanhToan
            var tt = new ThanhToan
            {
                ID_DON_HANG = idDonHang,
                ID_PHUONG_THUC = 1,
                GIA_BAN = db.Don_Hang.Find(idDonHang).TONG_TIEN,
                ID_TRANG_THAI = db.Trang_Thai
                                   .First(t => t.TEN_TRANG_THAI == "Chờ xác nhận")
                                   .ID_TRANG_THAI,
                AnhThanhToan = "/Image/ThanhToan/" + fileName
            };
            db.ThanhToans.Add(tt);

            // Cập nhật trạng thái đơn
            var dh = db.Don_Hang.Find(idDonHang);
            dh.ID_TRANG_THAI = tt.ID_TRANG_THAI;

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
            var userId = Session["UserId"]?.ToString();
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Users");

            var kh = db.Khach_Hang.FirstOrDefault(k => k.ID_User == userId);
            if (kh == null)
                return HttpNotFound();

            var list = db.Don_Hang
                .Include(d => d.Trang_Thai)
                .Where(d => d.MA_KH == kh.MA_KH)
                .OrderByDescending(d => d.TG_DAT_HANG)
                .ToList();

            return View(list);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
