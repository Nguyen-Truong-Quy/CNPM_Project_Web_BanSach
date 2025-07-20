using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;

namespace CNPM_Project_web.Areas.Admin.Controllers
{
    public class User_KhachHangController : Controller
    {
        private Web_Entities db = new Web_Entities();

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.RoleList = new SelectList(db.Roles.ToList(), "ID_ROLE", "TEN_ROLE");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserKhachHangViewModel model, HttpPostedFileBase AnhUpload)
        {
            CheckValues(model);

            ViewBag.RoleList = new SelectList(db.Roles.ToList(), "ID_ROLE", "TEN_ROLE");

            if (ModelState.IsValid)
            {
                // Xử lý lưu ảnh nếu có
                string filePath = null;
                if (AnhUpload != null && AnhUpload.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(AnhUpload.FileName);
                    string serverPath = Server.MapPath("~/Content/Uploads/KhachHang/");
                    if (!Directory.Exists(serverPath)) Directory.CreateDirectory(serverPath);

                    string fullPath = Path.Combine(serverPath, fileName);
                    AnhUpload.SaveAs(fullPath);

                    filePath = "/Content/Uploads/KhachHang/" + fileName;
                }

                // Tạo mã USER_...
                string lastUserId = db.USERS.OrderByDescending(u => u.ID_User).Select(u => u.ID_User).FirstOrDefault();
                int nextUserNumber = 1;
                if (!string.IsNullOrEmpty(lastUserId) && lastUserId.StartsWith("USER_"))
                {
                    int.TryParse(lastUserId.Substring(5), out nextUserNumber);
                    nextUserNumber++;
                }
                string newIdUser = $"USER_{nextUserNumber:D3}";

                // Tạo mã KH...
                string lastMaKH = db.Khach_Hang.OrderByDescending(k => k.MA_KH).Select(k => k.MA_KH).FirstOrDefault();
                int nextMaKHNumber = 1;
                if (!string.IsNullOrEmpty(lastMaKH) && lastMaKH.StartsWith("KH"))
                {
                    int.TryParse(lastMaKH.Substring(2), out nextMaKHNumber);
                    nextMaKHNumber++;
                }
                string newMaKH = $"KH{nextMaKHNumber:D3}";

                // Thêm user
                var user = new USER
                {
                    ID_User = newIdUser,
                    EMAIL = model.EMAIL,
                    PASSWORD = model.PASSWORD,
                    ID_ROLE = model.ID_ROLE
                };
                db.USERS.Add(user);

                // Thêm khách hàng
                var khach = new Khach_Hang
                {
                    MA_KH = newMaKH,
                    HO_TEN_KH = model.HO_TEN_KH,
                    SDT_KH = model.SDT_KH,
                    DIA_CHI = model.DIA_CHI,
                    EMAIL = model.EMAIL,
                    ANH_DAI_DIEN = filePath, // ảnh đã upload
                    ID_User = newIdUser
                };
                db.Khach_Hang.Add(khach);

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }


        public ActionResult Index()
        {
            var list = (from u in db.USERS
                        join k in db.Khach_Hang on u.ID_User equals k.ID_User
                        join r in db.Roles on u.ID_ROLE equals r.ID_ROLE
                        select new UserKhachHangViewModel
                        {
                            ID_User = u.ID_User,
                            EMAIL = u.EMAIL,
                            PASSWORD = u.PASSWORD,
                            ID_ROLE = u.ID_ROLE,
                            TEN_ROLE = r.TEN_ROLE,
                            MA_KH = k.MA_KH,
                            HO_TEN_KH = k.HO_TEN_KH,
                            SDT_KH = k.SDT_KH,
                            DIA_CHI = k.DIA_CHI,
                            ANH_DAI_DIEN = k.ANH_DAI_DIEN
                        }).ToList();

            return View(list); // <-- kiểu đúng: List<UserKhachHangViewModel>
        }


        public ActionResult Details(string id)
        {
            if (id == null) return HttpNotFound();

            var data = (from u in db.USERS
                        join k in db.Khach_Hang on u.ID_User equals k.ID_User
                        join r in db.Roles on u.ID_ROLE equals r.ID_ROLE
                        where u.ID_User == id
                        select new UserKhachHangViewModel
                        {
                            ID_User = u.ID_User,
                            EMAIL = u.EMAIL,
                            PASSWORD = u.PASSWORD,
                            ID_ROLE = u.ID_ROLE,
                            TEN_ROLE = r.TEN_ROLE,  // <-- THÊM DÒNG NÀY
                            MA_KH = k.MA_KH,
                            HO_TEN_KH = k.HO_TEN_KH,
                            SDT_KH = k.SDT_KH,
                            DIA_CHI = k.DIA_CHI,
                            ANH_DAI_DIEN = k.ANH_DAI_DIEN
                        }).FirstOrDefault();

            if (data == null) return HttpNotFound();

            return View(data);
        }


        [HttpGet]
        public ActionResult Edit(string id)
        {
            if (id == null) return HttpNotFound();

            var data = (from u in db.USERS
                        join k in db.Khach_Hang on u.ID_User equals k.ID_User
                        where u.ID_User == id
                        select new UserKhachHangViewModel
                        {
                            ID_User = u.ID_User,
                            EMAIL = u.EMAIL,
                            PASSWORD = u.PASSWORD,
                            ID_ROLE = u.ID_ROLE,
                            MA_KH = k.MA_KH,
                            HO_TEN_KH = k.HO_TEN_KH,
                            SDT_KH = k.SDT_KH,
                            DIA_CHI = k.DIA_CHI,
                            ANH_DAI_DIEN = k.ANH_DAI_DIEN
                        }).FirstOrDefault();

            if (data == null) return HttpNotFound();

            ViewBag.RoleList = new SelectList(db.Roles.ToList(), "ID_ROLE", "TEN_ROLE", data.ID_ROLE);
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserKhachHangViewModel model, HttpPostedFileBase AnhUpload)
        {
            CheckValues(model);
            ViewBag.RoleList = new SelectList(db.Roles.ToList(), "ID_ROLE", "TEN_ROLE", model.ID_ROLE);

            if (ModelState.IsValid)
            {
                var user = db.USERS.Find(model.ID_User);
                if (user != null)
                {
                    user.EMAIL = model.EMAIL;
                    user.PASSWORD = model.PASSWORD;
                    user.ID_ROLE = model.ID_ROLE;
                }

                var khach = db.Khach_Hang.FirstOrDefault(k => k.ID_User == model.ID_User);
                if (khach != null)
                {
                    khach.HO_TEN_KH = model.HO_TEN_KH;
                    khach.SDT_KH = model.SDT_KH;
                    khach.DIA_CHI = model.DIA_CHI;
                    khach.EMAIL = model.EMAIL;

                    if (AnhUpload != null && AnhUpload.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(AnhUpload.FileName);
                        string folderPath = Server.MapPath("~/Content/Uploads/KhachHang/");
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        string filePath = Path.Combine(folderPath, fileName);
                        AnhUpload.SaveAs(filePath);

                        khach.ANH_DAI_DIEN = "/Content/Uploads/KhachHang/" + fileName;
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }


        public ActionResult Delete(string id)
        {
            var khach = db.Khach_Hang.FirstOrDefault(k => k.ID_User == id);
            var user = db.USERS.Find(id);

            if (khach != null) db.Khach_Hang.Remove(khach);
            if (user != null) db.USERS.Remove(user);

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void CheckValues(UserKhachHangViewModel model)
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
            else if (model.PASSWORD.Length < 8 || !System.Text.RegularExpressions.Regex.IsMatch(model.PASSWORD, @"\d"))
            {
                ModelState.AddModelError("PASSWORD", "Mật khẩu phải có ít nhất 8 ký tự và chứa ít nhất 1 số");
            }

            if (model.ID_ROLE == 0)
            {
                ModelState.AddModelError("ID_ROLE", "Vui lòng chọn vai trò");
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

            // Tuỳ chọn bật lại nếu cần:
            // if (string.IsNullOrWhiteSpace(model.ANH_DAI_DIEN))
            // {
            //     ModelState.AddModelError("ANH_DAI_DIEN", "Ảnh đại diện không được để trống");
            // }
        }
    }
}