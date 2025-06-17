using System;
using System.Linq;
using System.Web.Mvc;
using CNPM_Project_web.Model;
using CNPM_Project_web.Helpers;

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
            if (ModelState.IsValid)
            {
                var existingUser = db.USERS.Find(model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                if (string.IsNullOrEmpty(Otp))
                {
                    string generatedOtp = new Random().Next(100000, 999999).ToString();
                    TempData["OTP"] = generatedOtp;
                    TempData["RegisterModel"] = model;
                    EmailService.SendOtp(model.Email, generatedOtp);
                    ViewBag.SuccessMessage = "Mã OTP đã được gửi đến email của bạn.";
                    ViewBag.ShowOtpBox = true;
                    return View(model);
                }

                string sentOtp = TempData["OTP"] as string;
                if (Otp != sentOtp)
                {
                    ViewBag.ErrorMessage = "Mã OTP không đúng.";
                    ViewBag.ShowOtpBox = true;
                    return View(model);
                }

                string maKH = GenerateCustomerCode();

                var khachHang = new Khach_Hang
                {
                    MA_KH = maKH,
                    HO_TEN_KH = model.Username,
                    EMAIL = model.Email
                };
                db.Khach_Hang.Add(khachHang);

                var newUser = new USER
                {
                    USERNAME = model.Username,
                    PASSWORD = model.Password, // Không mã hóa
                    EMAIL = model.Email,
                    ID_ROLE = 2,
                    MA_KH = maKH
                };
                db.USERS.Add(newUser);
                db.SaveChanges();

                ViewBag.SuccessMessage = "Đăng ký thành công!";
                return View();
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
                var user = db.USERS.FirstOrDefault(u => u.USERNAME == model.Username);

                if (user == null || user.PASSWORD != model.Password)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    return View(model);
                }

                // Đăng nhập thành công: lưu thông tin vào Session
                Session["Username"] = user.USERNAME;
                Session["Role"] = user.ID_ROLE;
                Session["CustomerId"] = user.MA_KH;

                return RedirectToAction("Contact", "Home");
            }
            catch (Exception)
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
