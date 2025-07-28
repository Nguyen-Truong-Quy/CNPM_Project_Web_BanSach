using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;
using CNPM_Project_web.Helpers;
using CNPM_Project_web.ViewModel;

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
                model = TempData["RegisterModel"] as RegisterViewModel;

            if (!ModelState.IsValid)
                return View(model);

            // Validate email & password
            ValidationHelper.CheckEmailAndPassword(model.Email, model.Password, ModelState);
            if (!ModelState.IsValid)
                return View(model);

            // Gửi OTP nếu chưa có
            if (string.IsNullOrEmpty(Otp))
            {
                var generatedOtp = new Random().Next(100000, 999999).ToString();
                TempData["OTP"] = generatedOtp;
                TempData["RegisterModel"] = model;

                try
                {
                    EmailService.SendOtp(model.Email, generatedOtp);
                    ViewBag.SuccessMessage = "Mã OTP đã được gửi đến email của bạn.";
                    ViewBag.ShowOtpBox = true;
                }
                catch
                {
                    ModelState.AddModelError("", "Không thể gửi email OTP. Vui lòng kiểm tra cấu hình email.");
                }
                return View(model);
            }

            // Kiểm tra OTP
            if (Otp != (TempData["OTP"] as string))
            {
                ViewBag.ErrorMessage = "Mã OTP không đúng.";
                ViewBag.ShowOtpBox = true;
                return View(model);
            }

            // Tạo ID_User mới
            var lastUserId = db.USERS
                               .OrderByDescending(u => u.ID_User)
                               .Select(u => u.ID_User)
                               .FirstOrDefault();
            int nextUserNumber = 1;
            if (!string.IsNullOrEmpty(lastUserId) && lastUserId.StartsWith("USER_"))
                int.TryParse(lastUserId.Substring(5), out nextUserNumber);
            nextUserNumber++;
            string newIdUser = $"USER_{nextUserNumber:D3}";

            // Tạo MA_KH mới
            var lastMaKH = db.Khach_Hang
                             .OrderByDescending(k => k.MA_KH)
                             .Select(k => k.MA_KH)
                             .FirstOrDefault();
            int nextMaKHNumber = 1;
            if (!string.IsNullOrEmpty(lastMaKH) && lastMaKH.StartsWith("KH"))
                int.TryParse(lastMaKH.Substring(2), out nextMaKHNumber);
            nextMaKHNumber++;
            string newMaKH = $"KH{nextMaKHNumber:D3}";

            // Thêm USER
            db.USERS.Add(new USER
            {
                ID_User = newIdUser,
                EMAIL = model.Email.Trim(),
                PASSWORD = model.Password.Trim(),
                ID_ROLE = 2
            });

            // Thêm Khach_Hang
            db.Khach_Hang.Add(new Khach_Hang
            {
                MA_KH = newMaKH,
                ID_User = newIdUser,
                HO_TEN_KH = "",
                EMAIL = model.Email.Trim(),
                SDT_KH = "",
                DIA_CHI = "",
                ANH_DAI_DIEN = null
            });

            db.SaveChanges();
            TempData["SuccessMessage"] = "Đăng ký thành công. Bạn có thể đăng nhập ngay!";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Login()
        {
            // Nếu đã có session, vào thẳng
            if (Session["UserId"] != null)
            {
                var role = (int)Session["Role"];
                if (role == 1)
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                return RedirectToAction("TrangChu", "Home");
            }

            // Nếu có cookie nhưng session mất (reload), thì refill session
            var cookie = Request.Cookies["UserInfo"];
            if (cookie != null && !string.IsNullOrEmpty(cookie["UserId"]))
            {
                Session["UserId"] = cookie["UserId"];
                Session["Email"] = cookie["Email"];
                Session["Role"] = int.Parse(cookie["Role"] ?? "2");

                var role = (int)Session["Role"];
                if (role == 1)
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                return RedirectToAction("TrangChu", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Chuẩn hóa đầu vào
            var emailInput = model.Email?.Trim().ToLower();
            var pwdInput = model.Password?.Trim();

            // Tìm user không phân biệt hoa–thường
            var user = db.USERS
                        .FirstOrDefault(u => u.EMAIL.ToLower() == emailInput);

            if (user == null || user.PASSWORD.Trim() != pwdInput)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            // Tạo session + cookie
            Session["UserId"] = user.ID_User;
            Session["Email"] = user.EMAIL;
            Session["Role"] = user.ID_ROLE;

            var loginCookie = new HttpCookie("UserInfo")
            {
                ["UserId"] = user.ID_User,
                ["Email"] = user.EMAIL,
                ["Role"] = user.ID_ROLE.ToString(),
                Expires = DateTime.Now.AddDays(7),
                Path = "/"
            };
            Response.Cookies.Add(loginCookie);

            // Redirect theo role
            if (user.ID_ROLE == 1)
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            return RedirectToAction("TrangChu", "Home");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            if (Request.Cookies["UserInfo"] != null)
            {
                var expired = new HttpCookie("UserInfo")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Path = "/"
                };
                Response.Cookies.Add(expired);
            }

            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
        public ActionResult Ho_So_Khach_Hang()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Users");

            var idUser = Session["UserId"].ToString();
            var khach = db.Khach_Hang.FirstOrDefault(k => k.ID_User == idUser);

            // Nếu chưa có khách hàng, redirect về form Create để bổ sung thông tin
            if (khach == null)
                return RedirectToAction("Create", "Users");

            return View(khach);
        }

    }
}
