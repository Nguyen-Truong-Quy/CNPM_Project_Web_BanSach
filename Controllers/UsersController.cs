using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;
using CNPM_Project_web.Helpers;
using CNPM_Project_web.ViewModel;
using System.IO;

namespace CNPM_Project_web.Controllers
{
    public class UsersController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Users/Register
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Register (Step 1-3)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model, string otp, string newPassword)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Step 1: send OTP if only email
            if (string.IsNullOrEmpty(otp) && string.IsNullOrEmpty(newPassword))
            {
                if (EmailExists(model.Email))
                {
                    ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng, vui lòng chọn email khác.");
                    return View(model);
                }
                var code = GenerateOtp(model.Email);
                ViewBag.ShowOtpBox = true;
                ViewBag.SuccessMessage = "Mã OTP đã được gửi đến email của bạn.";
                TempData["RegisterEmail"] = model.Email.Trim().ToLower();
                return View(model);
            }

            // Step 2: verify OTP
            if (!string.IsNullOrEmpty(otp) && string.IsNullOrEmpty(newPassword))
            {
                var email = TempData["RegisterEmail"] as string;
                ViewBag.ShowOtpBox = true;
                if (!ValidateOtp(otp))
                {
                    ModelState.AddModelError(nameof(otp), "Mã OTP không đúng.");
                    return View(model);
                }
                ViewBag.ShowPasswordBox = true;
                TempData["OtpValid"] = true;
                TempData.Keep();
                return View(model);
            }

            // Step 3: set password and create user
            if (!string.IsNullOrEmpty(otp) && !string.IsNullOrEmpty(newPassword) && TempData["OtpValid"]?.ToString() == "True")
            {
                var email = TempData["RegisterEmail"] as string;
                ViewBag.ShowPasswordBox = true;
                if (!ValidatePassword(newPassword))
                {
                    ModelState.AddModelError(nameof(newPassword), "Mật khẩu phải có ít nhất 8 ký tự và chứa ít nhất 1 số.");
                    return View(model);
                }
                CreateUser(email, newPassword);
                TempData["SuccessMessage"] = "Đăng ký thành công. Bạn có thể đăng nhập ngay!";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // --- Register helpers ---

        private bool EmailExists(string email)
        {
            var key = email.Trim().ToLower();
            return db.USERS.Any(u => u.EMAIL.ToLower() == key);
        }

        private string GenerateOtp(string email)
        {
            var code = new Random().Next(100000, 999999).ToString();
            TempData["RegisterOtp"] = code;
            EmailService.SendOtp(email, code);
            return code;
        }

        private bool ValidateOtp(string otp)
        {
            return otp == (TempData["RegisterOtp"] as string);
        }

        private bool ValidatePassword(string password)
        {
            return password.Length >= 8 && System.Text.RegularExpressions.Regex.IsMatch(password, "\\d");
        }

        private void CreateUser(string email, string password)
        {
            var newUserId = GenerateNewUserId();
            var newMaKH = GenerateNewMaKH();

            db.USERS.Add(new USER
            {
                ID_User = newUserId,
                EMAIL = email,
                PASSWORD = password,
                ID_ROLE = 2
            });
            db.Khach_Hang.Add(new Khach_Hang
            {
                MA_KH = newMaKH,
                ID_User = newUserId,
                EMAIL = email
            });
            db.SaveChanges();
        }

        private string GenerateNewUserId()
        {
            var last = db.USERS.OrderByDescending(u => u.ID_User).Select(u => u.ID_User).FirstOrDefault();
            int next = 1;
            if (!string.IsNullOrEmpty(last) && last.StartsWith("USER_"))
                int.TryParse(last.Substring(5), out next);
            return $"USER_{++next:D3}";
        }

        private string GenerateNewMaKH()
        {
            var last = db.Khach_Hang.OrderByDescending(k => k.MA_KH).Select(k => k.MA_KH).FirstOrDefault();
            int next = 1;
            if (!string.IsNullOrEmpty(last) && last.StartsWith("KH"))
                int.TryParse(last.Substring(2), out next);
            return $"KH{++next:D3}";
        }

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["UserId"] != null)
            {
                int role = (int)Session["Role"];
                if (role == 5)
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                return RedirectToAction("TrangChu", "Home");
            }

            var cookie = Request.Cookies["UserInfo"];
            if (cookie != null && !string.IsNullOrEmpty(cookie["UserId"]))
            {
                var userIdFromCookie = cookie["UserId"];

                Session["UserId"] = userIdFromCookie;
                Session["Email"] = cookie["Email"];
                Session["Role"] = int.Parse(cookie["Role"] ?? "2");

                var kh = db.Khach_Hang.FirstOrDefault(k => k.ID_User == userIdFromCookie);
                if (kh != null)
                    Session["CustomerId"] = kh.MA_KH;

                int role = (int)Session["Role"];
                if (role == 5)
                    return RedirectToAction("Index", "San_Pham", new { area = "Admin" });

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


            var user = CheckUser(model.Email, model.Password);
            if (user == null)

            {
                DisplayLoginFailure();
                return View(model);
            }

            Session["UserId"] = user.ID_User;
            Session["Email"] = user.EMAIL;
            Session["Role"] = user.ID_ROLE;

            var kh = db.Khach_Hang.FirstOrDefault(k => k.ID_User == user.ID_User);
            if (kh != null)
                Session["CustomerId"] = kh.MA_KH;

            var loginCookie = new HttpCookie("UserInfo")
            {
                ["UserId"] = user.ID_User,
                ["Email"] = user.EMAIL,
                ["Role"] = user.ID_ROLE.ToString(),
                Expires = DateTime.Now.AddDays(7),
                Path = "/"
            };
            Response.Cookies.Add(loginCookie);

            // ✅ Redirect theo Role
            if (user.ID_ROLE == 5) // Quản trị viên
                return RedirectToAction("Index", "San_Pham", new { area = "Admin" });

            return RedirectToAction("TrangChu", "Home"); // Người dùng thường
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

            // Bước 0: kiểm tra email tồn tại
            if (user == null)
            {
                DisplayInvalidEmail();
                return View(model);
            }


            if (string.IsNullOrEmpty(model.OTP) && string.IsNullOrEmpty(model.NewPassword))
            {
                SendOtpToEmail(email);
                ViewBag.ShowOtpBox = true;
                ViewBag.SuccessMessage = "Mã OTP đã được gửi đến email của bạn.";
                return View(model);
            }


            if (!string.IsNullOrEmpty(model.OTP) && string.IsNullOrEmpty(model.NewPassword))
            {
                if (IsOtpValid(model.OTP))
                {
                    DisplayCreateNewPassForm();
                }
                else
                {
                    DisplayFalseOTP();
                }
                return View(model);
            }

       
            if (!string.IsNullOrEmpty(model.NewPassword) && TempData["OTP_Validated"]?.ToString() == "True")
            {
                user.PASSWORD = model.NewPassword.Trim();
                db.SaveChanges();
                return DisplayUpdateSuccess();
            }

            // Mọi trường hợp khác
            ModelState.AddModelError("", "Vui lòng thực hiện đúng quy trình.");
            return View(model);
        }



        [HttpGet]
        public ActionResult Chinh_Sua_Thong_Tin()
        {
            // 1) Kiểm tra đã login?
            var userId = Session["UserId"] as string;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Users");

            // 2) Lấy hồ sơ khách
            var khach = db.Khach_Hang.SingleOrDefault(k => k.ID_User == userId);
            if (khach == null)
            {
                // Nếu chưa có hồ sơ, chuyển sang tạo mới hoặc hiển thị lỗi
                return RedirectToAction("Create", "Users");
                // hoặc: return HttpNotFound("Chưa tìm thấy hồ sơ khách hàng.");
            }

            // 3) Map sang ViewModel
            var vm = new UserKhachHangViewModel1
            {
                ID_User = khach.ID_User,
                HO_TEN_KH = khach.HO_TEN_KH,
                SDT_KH = khach.SDT_KH,
                DIA_CHI = khach.DIA_CHI,
                ANH_DAI_DIEN = khach.ANH_DAI_DIEN
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Chinh_Sua_Thong_Tin(UserKhachHangViewModel1 model, HttpPostedFileBase AnhUpload)
        {
            CheckValues(model, true);
            if (!ModelState.IsValid)
                return View(model);

            // Chưa đăng nhập hoặc user không khớp
            if (Session["UserId"] == null || model.ID_User != Session["UserId"].ToString())
                return RedirectToAction("Login", "Users");

            // Kiểm tra có thực sự chỉnh sửa gì mới không
            if (!CheckValueUserEdit(model, AnhUpload))
                return View(model);

            // ... phần validate chi tiết và lưu như cũ ...
            var khach = db.Khach_Hang.FirstOrDefault(k => k.ID_User == model.ID_User);
            khach.HO_TEN_KH = model.HO_TEN_KH.Trim();
            khach.SDT_KH = model.SDT_KH.Trim();
            khach.DIA_CHI = model.DIA_CHI.Trim();
            if (AnhUpload != null && AnhUpload.ContentLength > 0)
            {
                var fileName = Path.GetFileName(AnhUpload.FileName);
                // Thư mục lưu: ~/Content/Uploads/KhachHang
                var folder = Server.MapPath("~/Content/Uploads/KhachHang");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                var fullPath = Path.Combine(folder, fileName);
                AnhUpload.SaveAs(fullPath);

                // Lưu vào DB _chỉ_ phần path ảo, không kèm "~"
                khach.ANH_DAI_DIEN = "/Content/Uploads/KhachHang/" + fileName;
                db.SaveChanges();
            }
            return RedirectToAction("Chinh_Sua_Thong_Tin");


            db.SaveChanges();
            TempData["SuccessMessage"] = "Cập nhật thành công.";
            // Redirect về GET Chinh_Sua_Thong_Tin, nơi bạn sẽ load lại khach và gán vào viewmodel
            return RedirectToAction("Chinh_Sua_Thong_Tin");
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
        private USER CheckUser(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var normalizedEmail = email.Trim().ToLower();
            var normalizedPassword = password.Trim();

            var user = db.USERS.FirstOrDefault(u => u.EMAIL.ToLower() == normalizedEmail);

            if (user != null && user.PASSWORD.Trim() == normalizedPassword)
                return user;

            return null;
        }
        private void DisplayLoginFailure()
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
        }
        // Gửi OTP về email
        private void SendOtpToEmail(string email)
        {
            var generatedOtp = new Random().Next(100000, 999999).ToString();
            TempData["OTP"] = generatedOtp;
            EmailService.SendOtp(email, generatedOtp);
        }

        private bool IsOtpValid(string otp)
        {
            return otp == (TempData["OTP"] as string);
        }
 
        private void CreateUserAndCustomer(RegisterViewModel model)
        {
            // Sinh ID_User mới
            var lastUserId = db.USERS.OrderByDescending(u => u.ID_User).Select(u => u.ID_User).FirstOrDefault();
            int nextUserNumber = 1;
            if (!string.IsNullOrEmpty(lastUserId) && lastUserId.StartsWith("USER_"))
                int.TryParse(lastUserId.Substring(5), out nextUserNumber);
            nextUserNumber++;
            string newIdUser = $"USER_{nextUserNumber:D3}";

            // Sinh MA_KH mới
            var lastMaKH = db.Khach_Hang.OrderByDescending(k => k.MA_KH).Select(k => k.MA_KH).FirstOrDefault();
            int nextMaKHNumber = 1;
            if (!string.IsNullOrEmpty(lastMaKH) && lastMaKH.StartsWith("KH"))
                int.TryParse(lastMaKH.Substring(2), out nextMaKHNumber);
            nextMaKHNumber++;
            string newMaKH = $"KH{nextMaKHNumber:D3}";

            // Thêm user
            db.USERS.Add(new USER
            {
                ID_User = newIdUser,
                EMAIL = model.Email.Trim(),
                PASSWORD = model.Password.Trim(),
                ID_ROLE = 2
            });

            // Thêm khách hàng
            db.Khach_Hang.Add(new Khach_Hang
            {
                MA_KH = newMaKH,
                ID_User = newIdUser,
                EMAIL = model.Email.Trim(),
                HO_TEN_KH = "",
                SDT_KH = "",
                DIA_CHI = "",
                ANH_DAI_DIEN = null
            });

            db.SaveChanges();
        }
        private void DisplayCreateNewPassForm()
        {
            ViewBag.ShowOtpBox = true;
            ViewBag.ShowPasswordBox = true;
            TempData["OTP_Validated"] = true;
            TempData.Keep();
        }
        private ActionResult DisplayUpdateSuccess()
        {
            TempData["SuccessMessage"] = "Mật khẩu đã được cập nhật.";
            return RedirectToAction("Login");
        }
        private void DisplayFalseOTP()
        {
            ViewBag.ShowOtpBox = true;
            ModelState.AddModelError("", "Mã OTP không chính xác.");
        }
        private void DisplayInvalidEmail()
{
    ModelState.AddModelError("", "Email không tồn tại.");
}
        private bool CheckValueUserEdit(UserKhachHangViewModel1 model, HttpPostedFileBase AnhUpload)
        {
            // Lấy record cũ
            var khach = db.Khach_Hang.FirstOrDefault(k => k.ID_User == model.ID_User);
            if (khach == null)
            {
                ModelState.AddModelError("", "Không tìm thấy hồ sơ khách hàng.");
                return false;
            }

            // So sánh từng trường
            bool sameName = model.HO_TEN_KH?.Trim() == khach.HO_TEN_KH;
            bool samePhone = model.SDT_KH?.Trim() == khach.SDT_KH;
            bool sameAddr = model.DIA_CHI?.Trim() == khach.DIA_CHI;
            bool noNewImage = AnhUpload == null || AnhUpload.ContentLength == 0;

            if (sameName && samePhone && sameAddr && noNewImage)
            {
                ModelState.AddModelError("", "Thông tin mới trùng với thông tin cũ, vui lòng thay đổi ít nhất một mục.");
                return false;
            }

            return true;
        }
      



    }
}
