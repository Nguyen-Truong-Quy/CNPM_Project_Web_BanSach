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
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            var email = model.Email?.Trim().ToLower();
            var user = db.USERS.FirstOrDefault(u => u.EMAIL.ToLower() == email);

            if (user == null)
            {
                ModelState.AddModelError("", "Email không tồn tại.");
                return View(model);
            }

            // Nếu chưa nhập OTP → gửi OTP
            if (string.IsNullOrEmpty(model.OTP) && string.IsNullOrEmpty(model.NewPassword))
            {
                var otp = new Random().Next(100000, 999999).ToString();
                TempData["OTP"] = otp;
                TempData["Email"] = email;
                TempData.Keep();

                try
                {
                    EmailService.SendOtp(email, otp);
                    ViewBag.ShowOtpBox = true;
                    ViewBag.SuccessMessage = "Mã OTP đã được gửi đến email của bạn.";
                }
                catch
                {
                    ModelState.AddModelError("", "Không thể gửi OTP. Vui lòng thử lại.");
                }

                return View(model);
            }

            // Nếu nhập OTP nhưng chưa có mật khẩu mới
            if (!string.IsNullOrEmpty(model.OTP) && string.IsNullOrEmpty(model.NewPassword))
            {
                var otpFromTemp = TempData["OTP"] as string;
                if (model.OTP == otpFromTemp)
                {
                    ViewBag.ShowOtpBox = true;
                    ViewBag.ShowPasswordBox = true;
                    TempData["OTP_Validated"] = true;
                    TempData.Keep();
                }
                else
                {
                    ModelState.AddModelError("", "Mã OTP không chính xác.");
                    ViewBag.ShowOtpBox = true;
                }

                return View(model);
            }

            // Nhập mật khẩu mới sau khi đúng OTP
            if (TempData["OTP_Validated"]?.ToString() == "True")
            {
                user.PASSWORD = model.NewPassword.Trim();
                db.SaveChanges();
                TempData["SuccessMessage"] = "Mật khẩu đã được cập nhật.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Vui lòng thực hiện đúng quy trình.");
            return View(model);
        }
        [HttpGet]
        public ActionResult Chinh_Sua_Thong_Tin()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Users");

            var idUser = Session["UserId"].ToString();
            var khach = db.Khach_Hang.FirstOrDefault(k => k.ID_User == idUser);

            if (khach == null)
                return RedirectToAction("Create", "Users");

            var model = new UserKhachHangViewModel1
            {
                ID_User = khach.ID_User,
                HO_TEN_KH = khach.HO_TEN_KH,
                SDT_KH = khach.SDT_KH,
                DIA_CHI = khach.DIA_CHI,
                ANH_DAI_DIEN = khach.ANH_DAI_DIEN
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Chinh_Sua_Thong_Tin(UserKhachHangViewModel1 model, HttpPostedFileBase AnhUpload)
        {
            CheckValues(model, true);
            if (!ModelState.IsValid)
                return View(model);

            if (Session["UserId"] == null || model.ID_User != Session["UserId"].ToString())
                return RedirectToAction("Login", "Users");

            // Kiểm tra hợp lệ các trường khách hàng
            if (string.IsNullOrWhiteSpace(model.HO_TEN_KH))
                ModelState.AddModelError("HO_TEN_KH", "Họ tên không được để trống");
            if (string.IsNullOrWhiteSpace(model.SDT_KH) || !System.Text.RegularExpressions.Regex.IsMatch(model.SDT_KH, @"^0\d{9}$"))
                ModelState.AddModelError("SDT_KH", "Số điện thoại phải gồm 10 số và bắt đầu bằng 0");
            if (string.IsNullOrWhiteSpace(model.DIA_CHI))
                ModelState.AddModelError("DIA_CHI", "Địa chỉ không được để trống");

            if (!ModelState.IsValid)
                return View(model);

            var khach = db.Khach_Hang.FirstOrDefault(k => k.ID_User == model.ID_User);
            if (khach == null)
                return HttpNotFound();

            khach.HO_TEN_KH = model.HO_TEN_KH;
            khach.SDT_KH = model.SDT_KH;
            khach.DIA_CHI = model.DIA_CHI;

            if (AnhUpload != null && AnhUpload.ContentLength > 0)
            {
                var fileName = System.IO.Path.GetFileName(AnhUpload.FileName);
                var path = System.IO.Path.Combine(Server.MapPath("~/Content/Uploads"), fileName);
                AnhUpload.SaveAs(path);
                khach.ANH_DAI_DIEN = "~/Content/Uploads/" + fileName;
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Cập nhật thông tin thành công.";
            return RedirectToAction("Ho_So_Khach_Hang");
        }



        private void CheckValues(UserKhachHangViewModel1 model, bool isEdit = false)
        {
            if (!isEdit) // Kiểm tra khi tạo mới (tức là cần check email + password)
            {
                if (string.IsNullOrWhiteSpace(model.EMAIL))
                {
                    ModelState.AddModelError("EMAIL", "Email không được để trống");
                }
                else if (!model.EMAIL.EndsWith("@gmail.com"))
                {
                    ModelState.AddModelError("EMAIL", "Email phải có đuôi @gmail.com");
                }

                if (string.IsNullOrWhiteSpace(model.PASSWORD))
                {
                    ModelState.AddModelError("PASSWORD", "Mật khẩu không được để trống");
                }
                else if (model.PASSWORD.Length < 8 || !System.Text.RegularExpressions.Regex.IsMatch(model.PASSWORD, @"\\d"))
                {
                    ModelState.AddModelError("PASSWORD", "Mật khẩu phải có ít nhất 8 ký tự và chứa ít nhất 1 số");
                }

                if (model.ID_ROLE == 0)
                {
                    ModelState.AddModelError("ID_ROLE", "Vui lòng chọn vai trò");
                }
            }

            if (string.IsNullOrWhiteSpace(model.HO_TEN_KH))
            {
                ModelState.AddModelError("HO_TEN_KH", "Họ tên không được để trống");
            }

            if (string.IsNullOrWhiteSpace(model.SDT_KH))
            {
                ModelState.AddModelError("SDT_KH", "Số điện thoại không được để trống");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(model.SDT_KH, @"^0\d{9}$"))
            {
                ModelState.AddModelError("SDT_KH", "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0");
            }

            if (string.IsNullOrWhiteSpace(model.DIA_CHI))
            {
                ModelState.AddModelError("DIA_CHI", "Địa chỉ không được để trống");
            }

            // Nếu cần ảnh bắt buộc:
            // if (string.IsNullOrWhiteSpace(model.ANH_DAI_DIEN))
            // {
            //     ModelState.AddModelError("ANH_DAI_DIEN", "Ảnh đại diện không được để trống");
            // }
        }

    }
}
