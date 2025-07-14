using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;
using CNPM_Project_web.Lib;

namespace CNPM_Project_web.Controllers
{
    public class OrderController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // Bước 1: Hiển thị form nhập thông tin đặt hàng

        [HttpPost]
        public ActionResult DatHangTuChon(string[] chonSP)
        {
            if (chonSP == null || chonSP.Length == 0)
            {
                TempData["ThongBao"] = "Bạn chưa chọn sản phẩm nào để đặt hàng.";
                return RedirectToAction("XemGioHang", "Cart");
            }

            string maKH = "KH001"; // hardcode vì chưa có login
            var gioHangDaChon = db.Gio_Hang
                                  .Include(g => g.San_Pham)
                                  .Where(g => g.MA_KH == maKH && chonSP.Contains(g.MA_SP))
                                  .ToList();

            ViewBag.GioHang = gioHangDaChon;
            ViewBag.PhuongThuc = new SelectList(db.Phuong_Thuc_Thanh_Toan, "ID_THANH_TOAN", "TEN_PHUONG_THUC");
            ViewBag.MaKH = maKH;

            return View("NhapThongTinDatHang");
        }

        // Bước 2: Xử lý đặt hàng
        [HttpPost]
        public ActionResult XacNhanDatHang(string maKH, string diaChi, int idThanhToan, FormCollection form)
        {
            var dsSP = form.GetValues("chonSP");
            if (dsSP == null || dsSP.Length == 0)
                return RedirectToAction("XemGioHang", "Cart");

            var gioHang = db.Gio_Hang
                            .Where(g => g.MA_KH == maKH && dsSP.Contains(g.MA_SP))
                            .Include(g => g.San_Pham)
                            .ToList();

            if (!gioHang.Any())
                return RedirectToAction("XemGioHang", "Cart");

            // Tính tổng tiền
            decimal tongTien = gioHang.Sum(g => (g.San_Pham.GIA_BAN ?? 0) * g.SO_LUONG);

            // Tạo đơn hàng
            var don = new Don_Hang
            {
                MA_KH = maKH,
                ID_TRANG_THAI = 1, // Mặc định: chờ xử lý
                ID_THANH_TOAN = idThanhToan,
                TONG_TIEN = tongTien,
                TG_DAT_HANG = DateTime.Now
            };

            db.Don_Hang.Add(don);
            db.SaveChanges();

            // Thêm chi tiết đơn hàng + cập nhật tồn kho
            foreach (var item in gioHang)
            {
                db.Chi_Tiet_Don_Hang.Add(new Chi_Tiet_Don_Hang
                {
                    ID_DON_HANG = don.ID_DON_HANG,
                    MA_SP = item.MA_SP,
                    SO_LUONG = item.SO_LUONG,
                    GIA_BAN = item.San_Pham.GIA_BAN ?? 0
                });

                // Trừ tồn kho
                item.San_Pham.TON_KHO -= item.SO_LUONG;

                // Xoá sản phẩm trong giỏ
                db.Gio_Hang.Remove(item);
            }

            db.SaveChanges();

            if (idThanhToan == 2) // ví dụ ID = 2 là VNPay
            {
                return RedirectToAction("ThanhToanVNPay", new { idDonHang = don.ID_DON_HANG });
            }
            else
            {
                return RedirectToAction("XacNhanThanhToan", new { id = don.ID_DON_HANG });
            }

        }

        // Bước 3: Trang xác nhận đơn hàng đã đặt
        public ActionResult XacNhanThanhToan(int id)
        {
            var don = db.Don_Hang
                        .Include(d => d.Chi_Tiet_Don_Hang.Select(ct => ct.San_Pham))
                        .Include(d => d.Phuong_Thuc_Thanh_Toan)
                        .FirstOrDefault(d => d.ID_DON_HANG == id);

            return View(don);
        }
        [HttpGet]
        public ActionResult NhapThongTinDatHang()
        {
            string maKH = "KH001";

            var gioHang = db.Gio_Hang.Where(g => g.MA_KH == maKH).Include(g => g.San_Pham).ToList();
            var phuongThuc = db.Phuong_Thuc_Thanh_Toan.ToList();

            ViewBag.GioHang = gioHang;
            ViewBag.PhuongThuc = new SelectList(phuongThuc, "ID_THANH_TOAN", "TEN_PHUONG_THUC");

            return View(maKH);
        }
        [HttpPost]
        public ActionResult NhapThongTinDatHang(string[] chonSP)
        {
            string maKH = "KH001"; // hardcode tạm
            var gioHang = db.Gio_Hang
                            .Where(g => g.MA_KH == maKH && chonSP.Contains(g.MA_SP))
                            .Include(g => g.San_Pham)
                            .ToList();

            if (!gioHang.Any())
            {
                return RedirectToAction("XemGioHang", "Cart");
            }

            ViewBag.GioHang = gioHang;
            ViewBag.PhuongThuc = new SelectList(db.Phuong_Thuc_Thanh_Toan, "ID_THANH_TOAN", "TEN_PHUONG_THUC");
            return View();
        }
        public ActionResult ThanhToanVNPay(int idDonHang)
        {
            var don = db.Don_Hang.FirstOrDefault(d => d.ID_DON_HANG == idDonHang);
            if (don == null) return HttpNotFound();

            string vnp_Returnurl = Url.Action("KetQuaThanhToan", "Order", null, Request.Url.Scheme);
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];

            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((int)(don.TONG_TIEN * 100)).ToString()); // nhân 100 vì VNPay dùng đơn vị là xu
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Request.UserHostAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng #" + idDonHang);
            vnpay.AddRequestData("vnp_OrderType", "billpayment");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", idDonHang.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return Redirect(paymentUrl);
        }
        public ActionResult VNPayReturn()
        {
            var vnpay = new VnPayLibrary();
            var responseData = Request.QueryString;

            foreach (string key in responseData.AllKeys)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, responseData[key]);
                }
            }

            string vnp_SecureHash = responseData["vnp_SecureHash"];
            string hashSecret = VNPayConfig.vnp_HashSecret;

            bool isValid = vnpay.ValidateSignature(Request.QueryString, hashSecret);

            if (isValid)
            {
                string responseCode = responseData["vnp_ResponseCode"];
                string txnRef = responseData["vnp_TxnRef"];

                if (responseCode == "00")
                {
                    ViewBag.Message = "Thanh toán thành công cho đơn hàng #" + txnRef;
                }
                else
                {
                    ViewBag.Message = "Thanh toán thất bại. Mã lỗi: " + responseCode;
                }
            }
            else
            {
                ViewBag.Message = "Xác minh chữ ký không hợp lệ!";
            }

            return View(); // Tạo view VNPayReturn.cshtml để hiển thị thông báo
        }

        public ActionResult KetQuaThanhToan()
        {
            var vnpay = new VnPayLibrary();
            var responseData = Request.QueryString;

            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
            bool checkSignature = vnpay.ValidateSignature(responseData, vnp_HashSecret);

            int idDonHang = int.Parse(responseData["vnp_TxnRef"]);
            var don = db.Don_Hang.Find(idDonHang);

            if (checkSignature)
            {
                string vnp_ResponseCode = responseData["vnp_ResponseCode"];
                if (vnp_ResponseCode == "00")
                {
                    // Thành công
                    don.ID_TRANG_THAI = 2; // Ví dụ: Đã thanh toán
                }
                else
                {
                    // Giao dịch thất bại
                    don.ID_TRANG_THAI = 4; // Ví dụ: Thanh toán thất bại
                }
                db.SaveChanges();
            }

            return View(don); // hiển thị kết quả
        }

    }
}