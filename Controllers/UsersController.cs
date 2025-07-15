using System;
using System.Linq;
using System.Web.Mvc;
using CNPM_Project_web.Model;
using CNPM_Project_web.Helpers;
using System.Xml;

namespace CNPM_Project_web.Controllers
{
    public class UsersController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Users/Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model, string Otp)
        {
            if (TempData["RegisterModel"] != null)
            {
                model = TempData["RegisterModel"] as RegisterViewModel;
            }

            if (ModelState.IsValid)
            {
                //var existingUser = db.USERS.FirstOrDefault(u => u.EMAIL == model.Email);
                //if (existingUser != null)
                //{
                //    ModelState.AddModelError("", "Email đã tồn tại.");
                //    return View(model);
                //}

                // Gửi OTP nếu chưa có
                ValidationHelper.CheckEmailAndPassword(model.Email, model.Password, ModelState);
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (string.IsNullOrEmpty(Otp))
                {
                    string generatedOtp = new Random().Next(100000, 999999).ToString();
                    TempData["OTP"] = generatedOtp;
                    TempData["RegisterModel"] = model;

                    try
                    {
                        EmailService.SendOtp(model.Email, generatedOtp);
                        ViewBag.SuccessMessage = "Mã OTP đã được gửi đến email của bạn.";
                        ViewBag.ShowOtpBox = true;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Không thể gửi email OTP. Vui lòng kiểm tra kết nối hoặc cấu hình email.");
                        return View(model);
                    }

                    return View(model);
                }



                // Kiểm tra OTP
                string sentOtp = TempData["OTP"] as string;
                if (Otp != sentOtp)
                {
                    ViewBag.ErrorMessage = "Mã OTP không đúng.";
                    ViewBag.ShowOtpBox = true;
                    return View(model);
                }

                // === TẠO MÃ USER ===
                string lastUserId = db.USERS.OrderByDescending(u => u.ID_User).Select(u => u.ID_User).FirstOrDefault();
                int nextUserNumber = 1;
                if (!string.IsNullOrEmpty(lastUserId) && lastUserId.StartsWith("USER_"))
                {
                    int.TryParse(lastUserId.Substring(5), out nextUserNumber);
                    nextUserNumber++;
                }
                string newIdUser = $"USER_{nextUserNumber:D3}";

                // === TẠO MÃ KHÁCH HÀNG ===
                string lastMaKH = db.Khach_Hang.OrderByDescending(k => k.MA_KH).Select(k => k.MA_KH).FirstOrDefault();
                int nextMaKHNumber = 1;
                if (!string.IsNullOrEmpty(lastMaKH) && lastMaKH.StartsWith("KH"))
                {
                    int.TryParse(lastMaKH.Substring(2), out nextMaKHNumber);
                    nextMaKHNumber++;
                }
                string newMaKH = $"KH{nextMaKHNumber:D3}";

                // === TẠO USER ===
                var newUser = new USER
                {
                    ID_User = newIdUser,
                    EMAIL = model.Email,
                    PASSWORD = model.Password,
                    ID_ROLE = 2 // Giả sử 2 là ROLE Khách hàng
                };
                db.USERS.Add(newUser);

                // === TẠO KHÁCH HÀNG (có thể để trống thông tin) ===
                var khachHang = new Khach_Hang
                {
                    MA_KH = newMaKH,
                    ID_User = newIdUser,
                    HO_TEN_KH = "",
                    EMAIL = model.Email,
                    SDT_KH = "",       // Có thể để trống
                    DIA_CHI = "",      // Có thể để trống
                    ANH_DAI_DIEN = null
                };
                db.Khach_Hang.Add(khachHang);

                db.SaveChanges();

                ViewBag.SuccessMessage = "Đăng ký thành công. Bạn có thể đăng nhập ngay!";
                return RedirectToAction("Login", "Users");
            }

            return View(model);
        }


        private string GenerateCustomerCode()
        {
            int count = db.USERS.Count(u => u.ID_ROLE == 2) + 1;
            return "KH" + count.ToString("D4"); // VD: KH0001
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Tìm người dùng theo Email
                var user = db.USERS.FirstOrDefault(u => u.EMAIL == model.Email);

                if (user == null || user.PASSWORD != model.Password)
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                    return View(model);
                }

                // Tìm khách hàng gắn với user này (nếu có)
                var khachHang = db.Khach_Hang.FirstOrDefault(k => k.ID_User == user.ID_User);

                // Đăng nhập thành công: lưu thông tin vào Session
                Session["Email"] = user.EMAIL;
                Session["UserId"] = user.ID_User;
                Session["Role"] = user.ID_ROLE;
                Session["CustomerId"] = khachHang?.MA_KH;


                if (user.ID_ROLE == 1)
                {
                    // ROLE = 1: Quản trị viên
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else
                {
                    // ROLE khác (ví dụ 2 = khách hàng)
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi. Vui lòng thử lại sau.");
                return View(model);
            }
        }

        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
